﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using StarkSDKSpace;
using System.Collections.Generic;

public class MoneyController : MonoBehaviour {

	///*************************************************************************///
	/// Main CoinPack purchase Controller.
	/// This class handles all touch events on coin packs.
	/// You can easily integrate your own (custom) IAB system to deliver a nice 
	/// IAP options to the player.
	///*************************************************************************///

	private float buttonAnimationSpeed = 9;	//speed on animation effect when tapped on button
	private bool  canTap = true;			//flag to prevent double tap
	public AudioClip coinsCheckout;				//purchase sound
	public AudioClip tapSfx;					//purchase sound

	//Reference to GameObjects
	public GameObject playerMoney;			//UI 3d text object
	private int availableMoney;				//UI 3d text object

	//*****************************************************************************
	// Init. Updates the 3d texts with saved values fetched from playerprefs.
	//*****************************************************************************
	void Awake (){
		availableMoney = PlayerPrefs.GetInt("PlayerMoney");
		playerMoney.GetComponent<TextMeshPro>().text = "" + availableMoney;
	}

	//*****************************************************************************
	// FSM
	//*****************************************************************************
	void Update (){	
		if(canTap) {
			StartCoroutine(tapManager());
		}
	}

	//*****************************************************************************
	// This function monitors player touches on menu buttons.
	// detects both touch and clicks and can be used with editor, handheld device and 
	// every other platforms at once.
	//*****************************************************************************
	private RaycastHit hitInfo;
	private Ray ray;
    public string clickid;
    private StarkAdManager starkAdManager;
    IEnumerator tapManager (){

		//Mouse of touch?
		if(	Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended)  
			ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
		else if(Input.GetMouseButtonUp(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			yield break;
			
		if (Physics.Raycast(ray, out hitInfo)) {
			GameObject objectHit = hitInfo.transform.gameObject;
			switch(objectHit.name) {
			
				case "coinPack_1":
					//Here you should implement your own in-app purchase routines.
					//But for simplicity, we add the basic functions.
					
					//** Required steps **
					//Lead the player to the in-app gate and after the purchase is done, go to next line.
					//You should open the pay gateway, make the transaction, close the gateway, get the response and then consume the purchased item.
					//Then you can grant the user access to the item.
					//For security, you can avoid having money or similar purchasable items in plant text (string) and encode them with custom hash.
					
					//animate the button
					StartCoroutine(animateButton(objectHit));
					
					//add the purchased coins to the available user money
					availableMoney += 200;
					
					//save new amount of money
					PlayerPrefs.SetInt("PlayerMoney", availableMoney);
					
					//play sfx
					playSfx(coinsCheckout);
					
					//Wait
					yield return new WaitForSeconds(1.5f);
					
					//Reload the level
					SceneManager.LoadScene(SceneManager.GetActiveScene().name);
					
					break;
					
				case "coinPack_2":
					StartCoroutine(animateButton(objectHit));
					availableMoney += 500;
					PlayerPrefs.SetInt("PlayerMoney", availableMoney);
					playSfx(coinsCheckout);
					yield return new WaitForSeconds(1.5f);
					SceneManager.LoadScene(SceneManager.GetActiveScene().name);
					break;
					
				case "coinPack_3":
                    ShowVideoAd("fb29oqfcps5hp6303g",
            (bol) => {
                if (bol)
                {
					Debug.Log("objectHit->" + objectHit);
					if (objectHit != null)
					{
						StartCoroutine(animateButton(objectHit));
					}
                    availableMoney += 100;
					Debug.Log("availableMoney->"+ availableMoney);
					PlayerPrefs.SetInt("PlayerMoney", availableMoney);

					Debug.Log("coinsCheckout->" + coinsCheckout);

					if (coinsCheckout != null)
					{
						//playSfx(coinsCheckout);
					}

					Debug.Log("--playSfx--");

					clickid = "";
                    getClickid();

					Debug.Log("clickid->" + clickid);

					apiSend("game_addiction", clickid);
                    apiSend("lt_roi", clickid);


                }
                else
                {
                    StarkSDKSpace.AndroidUIManager.ShowToast("观看完整视频才能获取奖励哦！");
                }
            },
            (it, str) => {
                Debug.LogError("Error->" + str);
                //AndroidUIManager.ShowToast("广告加载异常，请重新看广告！");
            });
                    yield return new WaitForSeconds(1.5f);
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    break;
				
				case "Btn-Back":
					StartCoroutine(animateButton(objectHit));
					playSfx(tapSfx);
					yield return new WaitForSeconds(1.0f);
					SceneManager.LoadScene("Menu-c#");
					break;
				
			}	
		}
	}

	//*****************************************************************************
	// This function animates a button by modifying it's scales on x-y plane.
	// can be used on any element to simulate the tap effect.
	//*****************************************************************************
	IEnumerator animateButton ( GameObject _btn  ){
		canTap = false;
		Vector3 startingScale = _btn.transform.localScale;	//initial scale	
		Vector3 destinationScale = startingScale * 1.1f;		//target scale
		
		//Scale up
		float t = 0.0f; 
		while (t <= 1.0f) {
			t += Time.deltaTime * buttonAnimationSpeed;
			_btn.transform.localScale = new Vector3( Mathf.SmoothStep(startingScale.x, destinationScale.x, t),
			                                        Mathf.SmoothStep(startingScale.y, destinationScale.y, t),
			                                        _btn.transform.localScale.z);
			yield return 0;
		}
		
		//Scale down
		float r = 0.0f; 
		if(_btn.transform.localScale.x >= destinationScale.x) {
			while (r <= 1.0f) {
				r += Time.deltaTime * buttonAnimationSpeed;
				_btn.transform.localScale = new Vector3( Mathf.SmoothStep(destinationScale.x, startingScale.x, r),
				                                        Mathf.SmoothStep(destinationScale.y, startingScale.y, r),
				                                        _btn.transform.localScale.z);
				yield return 0;
			}
		}
		
		if(r >= 1)
			canTap = true;
	}

	//*****************************************************************************
	// Play sound clips
	//*****************************************************************************
	void playSfx ( AudioClip _clip  ){
		GetComponent<AudioSource>().clip = _clip;
		if(!GetComponent<AudioSource>().isPlaying) {
			GetComponent<AudioSource>().Play();
		}
	}
    public void apiSend(string eventname, string clickid)
    {
        TTRequest.InnerOptions options = new TTRequest.InnerOptions();
        options.Header["content-type"] = "application/json";
        options.Method = "POST";
        options.DataType = "JSON";
        options.ResponseType = "text";

        JsonData data1 = new JsonData();

        data1["event_type"] = eventname;
        data1["context"] = new JsonData();
        data1["context"]["ad"] = new JsonData();
        data1["context"]["ad"]["callback"] = clickid;

        Debug.Log("<-data1-> " + data1.ToJson());

        options.Data = data1.ToJson();

        TT.Request("https://analytics.oceanengine.com/api/v2/conversion", options,
           response => { Debug.Log(response); },
           response => { Debug.Log(response); });
    }


    /// <summary>
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="closeCallBack"></param>
    /// <param name="errorCallBack"></param>
    public void ShowVideoAd(string adId, System.Action<bool> closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            starkAdManager.ShowVideoAdWithId(adId, closeCallBack, errorCallBack);
        }
    }
    public void getClickid()
    {
        var launchOpt = StarkSDK.API.GetLaunchOptionsSync();
        if (launchOpt.Query != null)
        {
            foreach (KeyValuePair<string, string> kv in launchOpt.Query)
                if (kv.Value != null)
                {
                    Debug.Log(kv.Key + "<-����-> " + kv.Value);
                    if (kv.Key.ToString() == "clickid")
                    {
                        clickid = kv.Value.ToString();
                    }
                }
                else
                {
                    Debug.Log(kv.Key + "<-����-> " + "null ");
                }
        }
    }

}