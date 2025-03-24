using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using StarkSDKSpace;
using System.Collections.Generic;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;

public class PauseManager : MonoBehaviour {
		
	//***************************************************************************//
	// This class manages pause and unpause states.
	//***************************************************************************//

	//static bool  soundEnabled;
	internal bool isPaused;
	private float savedTimeScale;
	public GameObject pausePlane;
    public string clickid;
    private StarkAdManager starkAdManager;
    enum Page {
		PLAY, PAUSE
	}
	private Page currentPage = Page.PLAY;

	//*****************************************************************************
	// Init.
	//*****************************************************************************
	void Awake (){		
		//soundEnabled = true;
		isPaused = false;
		
		Time.timeScale = 1.0f;
		Time.fixedDeltaTime = 0.02f;
		
		if(pausePlane)
	    	pausePlane.SetActive(false); 
	}

	//*****************************************************************************
	// FSM
	//*****************************************************************************
	void Update (){

		//touch control
		touchManager();
		
		//optional pause in Editor & Windows (just for debug)
		if(Input.GetKeyDown(KeyCode.P) || Input.GetKeyUp(KeyCode.Escape)) {
			//PAUSE THE GAME
			switch (currentPage) {
	            case Page.PLAY: 
	            	PauseGame(); 
	            	break;
	            case Page.PAUSE: 
	            	UnPauseGame(); 
	            	break;
	            default: 
	            	currentPage = Page.PLAY;
	            	break;
	        }
		}
		
		//debug restart
		if(Input.GetKeyDown(KeyCode.R)) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}

	//*****************************************************************************
	// This function monitors player touches on menu buttons.
	// detects both touch and clicks and can be used with editor, handheld device and 
	// every other platforms at once.
	//*****************************************************************************
	void touchManager (){
		if(Input.GetMouseButtonDown(0)) {
			RaycastHit hitInfo;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hitInfo)) {
				string objectHitName = hitInfo.transform.gameObject.name;
				switch(objectHitName) {
					case "PauseBtn":
						switch (currentPage) {
				            case Page.PLAY: 
				            	PauseGame();
				            	break;
				            case Page.PAUSE: 
				            	UnPauseGame(); 
				            	break;
				            default: 
				            	currentPage = Page.PLAY;
				            	break;
				        }
						break;
					
					case "ResumeBtn":
						switch (currentPage) {
				            case Page.PLAY: 
				            	PauseGame();
				            	break;
				            case Page.PAUSE: 
				            	UnPauseGame(); 
				            	break;
				            default: 
				            	currentPage = Page.PLAY;
				            	break;
				        }
						break;
					
					case "RestartBtn":
                        ShowVideoAd("fb29oqfcps5hp6303g",
            (bol) => {
                if (bol)
                {
                    UnPauseGame();
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);



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
						
					case "MenuBtn":
						UnPauseGame();
						SceneManager.LoadScene("Menu-c#");
						break;

					//if tournament mode is on
					case "ContinueTournamentBtn":
						UnPauseGame();
						SceneManager.LoadScene("Tournament-c#");
						break;
					case "ContinueBtn":
						GlobalGameManager.instance.gameStatusPlane.SetActive(false);
                        GlobalGameManager.gameTimer += 10;
						break;

                }
			}
		}
	}

	void PauseGame (){
		print("Game in Paused...");
		isPaused = true;
		savedTimeScale = Time.timeScale;
	    Time.timeScale = 0;
	    AudioListener.volume = 0;
	    if(pausePlane)
	    	pausePlane.SetActive(true);
	    currentPage = Page.PAUSE;


		ShowInterstitialAd("9i5a42ijd18g2482d4",
		   () => {

		   },
		   (it, str) => {
			   Debug.LogError("Error->" + str);
		   });

	}


	/// <summary>
	/// 播放插屏广告
	/// </summary>
	/// <param name="adId"></param>
	/// <param name="errorCallBack"></param>
	/// <param name="closeCallBack"></param>
	public void ShowInterstitialAd(string adId, System.Action closeCallBack, System.Action<int, string> errorCallBack)
	{
		starkAdManager = StarkSDK.API.GetStarkAdManager();
		if (starkAdManager != null)
		{
			var mInterstitialAd = starkAdManager.CreateInterstitialAd(adId, errorCallBack, closeCallBack);
			mInterstitialAd.Load();
			mInterstitialAd.Show();
		}
	}

	void UnPauseGame (){
		print("Unpause");
	    isPaused = false;
	    Time.timeScale = savedTimeScale;
	    AudioListener.volume = 1.0f;
		if(pausePlane)
	    	pausePlane.SetActive(false);   
	    currentPage = Page.PLAY;
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
}