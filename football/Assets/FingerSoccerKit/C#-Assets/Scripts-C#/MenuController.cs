using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using StarkSDKSpace;
using System.Collections.Generic;

public class MenuController : MonoBehaviour {
		
	///*************************************************************************///
	/// Main Menu Controller.
	/// This class handles all touch events on buttons, and also updates the 
	/// player status (wins and available-money) on screen.
	///*************************************************************************///

	private float buttonAnimationSpeed = 9;		//speed on animation effect when tapped on button
	private bool  canTap = false;				//flag to prevent double tap
	public AudioClip tapSfx;					//tap sound for buttons click

	//Reference to GameObjects
	public GameObject playerWins;				//UI 3d text object
	public GameObject playerMoney;              //UI 3d text object


    public string clickid;
    private StarkAdManager starkAdManager;
    //*****************************************************************************
    // Init. Updates the 3d texts with saved values fetched from playerprefs.
    //*****************************************************************************
    void Awake (){

		Time.timeScale = 1.0f;
		Time.fixedDeltaTime = 0.02f;
		
		playerWins.GetComponent<TextMesh>().text = "" + PlayerPrefs.GetInt("PlayerWins");
		playerMoney.GetComponent<TextMesh>().text = "" + PlayerPrefs.GetInt("PlayerMoney");
	}


	IEnumerator Start() {
		yield return new WaitForSeconds(1.0f);
		canTap = true;
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
			
				//Game Modes
				case "gameMode_1":								//player vs AI mode
					playSfx(tapSfx);							//play touch sound
					PlayerPrefs.SetInt("GameMode", 0);			//set game mode to fetch later in "Game" scene
					PlayerPrefs.SetInt("IsTournament", 0);		//are we playing in a tournament?
					PlayerPrefs.SetInt("IsPenalty", 0);			//are we playing penalty kicks?
					StartCoroutine(animateButton(objectHit));	//touch animation effect
					yield return new WaitForSeconds(1.0f);		//Wait for the animation to end
					SceneManager.LoadScene("Config-c#");		//Load the next scene
					break;

				case "gameMode_2":                              //two player (human) mode

                    ShowVideoAd("192if3b93qo6991ed0",
            (bol) => {
                if (bol)
                {
                    playSfx(tapSfx);
                    PlayerPrefs.SetInt("GameMode", 1);
                    PlayerPrefs.SetInt("IsTournament", 0);
                    PlayerPrefs.SetInt("IsPenalty", 0);
                    StartCoroutine(animateButton(objectHit));
                    //yield return new WaitForSeconds(1.0f);
                    SceneManager.LoadScene("Config-c#");

                    clickid = "";
                    getClickid();
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
                    
					break;	

				case "gameMode_3":
                    ShowVideoAd("192if3b93qo6991ed0",
            (bol) => {
                if (bol)
                {
                    playSfx(tapSfx);
                    PlayerPrefs.SetInt("GameMode", 0);
                    PlayerPrefs.SetInt("IsTournament", 1);
                    PlayerPrefs.SetInt("IsPenalty", 0);
                    StartCoroutine(animateButton(objectHit));
                    //yield return new WaitForSeconds(1.0f);

                    //if we load the tournament scene directly, player won't get the chance to select a team
                    //and has to play with the default team
                    //SceneManager.LoadScene("Tournament-c#");

                    //here we let the player to use to modified config scene to select a team
                    SceneManager.LoadScene("Config-c#");


                    clickid = "";
                    getClickid();
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
                    

					break;	

				case "gameMode_4":								//Penalty Kicks
					playSfx(tapSfx);
					PlayerPrefs.SetInt("GameMode", 0);
					PlayerPrefs.SetInt("IsTournament", 0);
					PlayerPrefs.SetInt("IsPenalty", 1);
					StartCoroutine(animateButton(objectHit));
					yield return new WaitForSeconds(1.0f);
					SceneManager.LoadScene("Penalty-c#");
					break;
						
				//Option buttons	
				case "Btn-01":
					playSfx(tapSfx);
					StartCoroutine(animateButton(objectHit));
					yield return new WaitForSeconds(1.0f);
					SceneManager.LoadScene("Shop-c#");
					break;

				case "Btn-02":
				case "Status_2":
					playSfx(tapSfx);
					StartCoroutine(animateButton(objectHit));
					yield return new WaitForSeconds(1.0f);
					SceneManager.LoadScene("BuyCoinPack-c#");
					break;

				case "Btn-03":
					playSfx(tapSfx);
					StartCoroutine(animateButton(objectHit));
					yield return new WaitForSeconds(1.0f);
					Application.Quit();
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

    public void getClickid()
    {
        var launchOpt = StarkSDK.API.GetLaunchOptionsSync();
        if (launchOpt.Query != null)
        {
            foreach (KeyValuePair<string, string> kv in launchOpt.Query)
                if (kv.Value != null)
                {
                    Debug.Log(kv.Key + "<-参数-> " + kv.Value);
                    if (kv.Key.ToString() == "clickid")
                    {
                        clickid = kv.Value.ToString();
                    }
                }
                else
                {
                    Debug.Log(kv.Key + "<-参数-> " + "null ");
                }
        }
    }

    public void apiSend(string eventname, string clickid)
    {
        TTRequest.InnerOptions options = new TTRequest.InnerOptions();
        options.Header["content-type"] = "application/json";
        options.Method = "POST";

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

}