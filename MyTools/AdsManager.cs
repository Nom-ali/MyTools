///// <summary>
///// This script is created using these SDK versions
///// Admob 9.0.0 or above
///// unity Ads SDK 4.3.0
///// </summary>

//#if UNITY_IPHONE
//using Unity.Advertisement.IosSupport;
//#endif
//using UnityEngine.Advertisements;
//using System.Collections.Generic;
//using GoogleMobileAds.Api;
//using System.Collections;
//using UnityEngine;
//using System;

//public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
//{
//    public static AdsManager Instance;

//    #region Variables
//    [Header("----- Test Mode ----------------")]
//    [SerializeField] private bool EnableTestIds = false;
//    [SerializeField] private bool DontLoadAds = false;
//    [SerializeField] private List<string> TestDeviceIDs = new();

//    [Header("----- Debugger ------------------")]
//    [SerializeField]
//    bool ShowDebugLogs = false;

//    [Header("----- Fram Limit ------------------")]
//    [SerializeField]
//    bool SetFrames = true;
//    [SerializeField] private int Frames = 60;

//    [Header("----- Setting ------------------")]
//    [SerializeField] private bool NeverSleepMode = true;
//    [SerializeField] private Platform m_Platform = Platform.Android;
//    [SerializeField] private SDKs SDK = SDKs.Both;
//    [SerializeField] private Priority priority = Priority.Admob;
//    [SerializeField] private bool ShowLoadingPanel = false;
//    [SerializeField] private AnimationBase LoadingPanel;

//    [Header("----- Admob -------------------")]
//    [SerializeField] private bool ShowBannerAtStart = false;
//    [SerializeField] private bool RequestAppOpenAd = false;
//    [SerializeField] private bool ShowAppOpenAdOnLoad = false;

//    [Tooltip("Don't Change or set , if you don't know")]
//    private readonly AdmobIds AdmobAndroidTestingID = new()
//    {
//        BannerID = "ca-app-pub-3940256099942544/6300978111",
//        MedBannerID = "ca-app-pub-3940256099942544/6300978111",
//        InterID = "ca-app-pub-3940256099942544/1033173712",
//        RewardedID = "ca-app-pub-3940256099942544/5224354917",
//        AppOpenID = "ca-app-pub-3940256099942544/9257395921",
//        NativeID = "ca-app-pub-3940256099942544/2247696110"
//    };
//    private readonly AdmobIds AdmobIOSTestingID = new()
//    {
//        BannerID = "ca-app-pub-3940256099942544/2934735716",
//        MedBannerID = "ca-app-pub-3940256099942544/2934735716",
//        InterID = "ca-app-pub-3940256099942544/4411468910",
//        RewardedID = "ca-app-pub-3940256099942544/1712485313",
//        AppOpenID = "ca-app-pub-3940256099942544/5575463023",
//        NativeID = "ca-app-pub-3940256099942544/3986624511"
//    };
//    [Space(2)]
//    [SerializeField] AdmobIds AdmobAndroidLiveID;
//    [SerializeField] AdmobIds AdmobIosLiveID;

//    /// Private variables
//    /// Don't change these if you don't know
//    [HideInInspector] public bool AdmobInitialized = false;
//    private BannerView bannerView;
//    private BannerView medBannerView;
//    private InterstitialAd interstitialAd;
//    private RewardedAd rewardedAd;
//    private AppOpenAd appOpenAd;

//    [Header("----- Unity Ads ----------------")]
//    [SerializeField] string UnityID_Adroid;
//    [SerializeField] string UnityID_iOS;
//    bool EnableUnityBanner = false;
//    BannerPosition bannerPositionUnity = BannerPosition.TOP_CENTER;

//    /// Placement IDs
//    private readonly PlacementIDs UnityAndroidPlacemenntIDs = new()
//    {
//        BannerPlacementID = "Banner_Android",
//        InterPlacementID = "Interstitial_Android",
//        RewardedPlacementID = "Rewarded_Android"
//    };
//    private readonly PlacementIDs UnityIOSPlacemenntIDs = new()
//    {
//        BannerPlacementID = "Banner_iOS",
//        InterPlacementID = "Interstitial_iOS",
//        RewardedPlacementID = "Rewarded_iOS"
//    };

//    /// Private variables
//    /// Don't change these if you don't know
//    private string BannerAdsID,MedBannerAdsID, InterstitialAdsID, RewardedAdsID, AppOpenAdsID;
//    private bool isAdmobBanner = false;
//    [MyBox.ReadOnly, SerializeField] private bool isAdmobBannerMed = false;
//    private bool isUnityBanner = false;

//    //Unity Section
//    private string unityGameId;
//    private string BannerPlacementID;
//    private string InterPlacementID;
//    private string RewardedPlacementID;
//    [MyBox.ReadOnly, SerializeField] private bool unityInterAdsLoaded = false;
//    [MyBox.ReadOnly, SerializeField] private bool unityRewardAdsLoaded = false;
//    [MyBox.ReadOnly, SerializeField] private bool m_ShowAppOpenAd = true;

//    //Rewarded Ads Reward Call delegate
//    private Action RewardAction;

//    private bool RemoveAds => PlayerPrefs.GetInt("RemoveAds", 0) == 1;

//    #endregion

//    #region Awake
//    // Creating Instance
//    private void Awake()
//    {
//        if (SetFrames)
//            Application.targetFrameRate = Frames;

//        if (ShowDebugLogs)
//            Debug.unityLogger.logEnabled = true;
//        else
//            Debug.unityLogger.logEnabled = false;


//        if (NeverSleepMode)
//            Screen.sleepTimeout = SleepTimeout.NeverSleep;

//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(this);
//        }
//        else Destroy(this);

//        if (LoadingPanel != null && LoadingPanel.gameObject.activeSelf)
//            LoadingPanel.gameObject.SetActive(false);
//    }
//    #endregion

//    #region Initializing
//    private IEnumerator Start()
//    {
//        if (m_Platform.Equals(Platform.IOS))
//        {
//#if UNITY_IPHONE
//            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
//            yield return new WaitUntil(() => status == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED);
//#endif
//            // if //.Instance.Get_Value(ConsVariables.NoAds) == 0)

//            // Setting IDs
//            yield return Set_Ads_IDs_Before_Initializing();
//            // Initializing SDKs
//            yield return InitilizeSDK();

//        }
//        else
//        {
//            yield return Set_Ads_IDs_Before_Initializing();
//            // Initializing SDKs
//            yield return InitilizeSDK();
//        }
//        yield break;
//    }


//    /// <summary>
//    /// Initializing SDKs (admob and unity based on priority
//    /// </summary>
//    /// <returns></returns>
//    private IEnumerator InitilizeSDK()
//    {
//        //Initializing SDKs
//        // If Platform is null then it will return null Exception
//        if (SDK.Equals(SDKs.None))
//        {
//            Debug.LogError("No SDK selected.");
//            yield break;
//        }
//        /// if Admob is selected only
//        if (SDK.Equals(SDKs.Admob))
//        {
//            // If Platform is Admob then it will run ADMOB SDK
//            yield return InitializeAdmob();
//        }
//        /// if Unity is selected only
//        else if (SDK.Equals(SDKs.Unity))
//        {
//            /// todo If Platform is unity then it will run unity SDK
//            yield return InitializeUnity();
//        }
//        /// if both Admob and unity are selected
//        else if (SDK.Equals(SDKs.Both))
//        {
//            // If Platform is both selected then it will run Both SDK
//            yield return InitializeAdmob();
//            yield return InitializeUnity();
//        }
//        yield return null;
//    }


//    /// <summary>
//    /// Setting Ads IDs here + placement IDs
//    /// </summary>
//    /// <returns></returns>
//    private IEnumerator Set_Ads_IDs_Before_Initializing()
//    {
//        // Initializing Admob IDs
//        if (EnableTestIds)
//        {
//            if (m_Platform.Equals(Platform.Android))
//            {
//                BannerAdsID = AdmobAndroidTestingID.BannerID.Trim();
//                MedBannerAdsID = AdmobAndroidTestingID.MedBannerID.Trim();
//                InterstitialAdsID = AdmobAndroidTestingID.InterID.Trim();
//                RewardedAdsID = AdmobAndroidTestingID.RewardedID.Trim();
//                AppOpenAdsID = AdmobAndroidTestingID.AppOpenID.Trim();
//            }
//            else if (m_Platform.Equals(Platform.IOS))
//            {
//                BannerAdsID = AdmobIOSTestingID.BannerID.Trim();
//                MedBannerAdsID = AdmobIOSTestingID.MedBannerID.Trim();
//                InterstitialAdsID = AdmobIOSTestingID.InterID.Trim();
//                RewardedAdsID = AdmobIOSTestingID.RewardedID.Trim();
//                AppOpenAdsID = AdmobIOSTestingID.AppOpenID.Trim();
//            }
//        }
//        else
//        {
//            if (m_Platform.Equals(Platform.Android))
//            {
//                BannerAdsID = AdmobAndroidLiveID.BannerID.Trim();
//                MedBannerAdsID = AdmobAndroidLiveID.MedBannerID.Trim();
//                InterstitialAdsID = AdmobAndroidLiveID.InterID.Trim();
//                RewardedAdsID = AdmobAndroidLiveID.RewardedID.Trim();
//                AppOpenAdsID = AdmobAndroidLiveID.AppOpenID.Trim();
//            }
//            else if (m_Platform.Equals(Platform.IOS))
//            {
//                BannerAdsID = AdmobIosLiveID.BannerID.Trim();
//                MedBannerAdsID = AdmobIosLiveID.MedBannerID.Trim();
//                InterstitialAdsID = AdmobIosLiveID.InterID.Trim();
//                RewardedAdsID = AdmobIosLiveID.RewardedID.Trim();
//                AppOpenAdsID = AdmobIosLiveID.AppOpenID.Trim();
//            }
//        }

//        //Initalizing Unity Ads SDk   
//        if (m_Platform.Equals(Platform.Android))
//        {
//            unityGameId = UnityID_Adroid;
//            BannerPlacementID = UnityAndroidPlacemenntIDs.BannerPlacementID.Trim();
//            InterPlacementID = UnityAndroidPlacemenntIDs.InterPlacementID.Trim();
//            RewardedPlacementID = UnityAndroidPlacemenntIDs.RewardedPlacementID.Trim();
//        }
//        else if (m_Platform.Equals(Platform.IOS))
//        {
//            unityGameId = UnityID_iOS;
//            BannerPlacementID = UnityIOSPlacemenntIDs.BannerPlacementID.Trim();
//            InterPlacementID = UnityIOSPlacemenntIDs.InterPlacementID.Trim();
//            RewardedPlacementID = UnityIOSPlacemenntIDs.RewardedPlacementID.Trim();
//        }
//        yield return null;
//    }

//    public string Get_NativeAdsIDs()
//    {
//        if (EnableTestIds)
//        {
//            if (m_Platform.Equals(Platform.Android))
//                return AdmobAndroidTestingID.NativeID.Trim();
//            else if (m_Platform.Equals(Platform.IOS))
//                return AdmobIOSTestingID.NativeID.Trim();
//        }
//        else
//        {
//            if (m_Platform.Equals(Platform.Android))
//                return AdmobAndroidLiveID.NativeID.Trim();
//            else if (m_Platform.Equals(Platform.IOS))
//                return AdmobIosLiveID.NativeID.Trim();
//        }
//        return "";
//    }

//    /// <summary>
//    /// Initalizing admob SDk
//    /// </summary>
//    private IEnumerator InitializeAdmob()
//    {

//        //if (EnableTestIds)
//        //{
//        //    RequestConfiguration requestConfiguration = new RequestConfiguration();
//        //    requestConfiguration.TestDeviceIds = TestDeviceIDs;
//        //    MobileAds.SetRequestConfiguration(requestConfiguration);
//        //}

//        if (m_Platform.Equals(Platform.IOS))
//            MobileAds.SetiOSAppPauseOnBackground(true);

//        // Initialize the Google Mobile Ads SDK.
//        MobileAds.Initialize((InitializationStatus initStatus) =>
//        {
//            Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();
//            foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
//            {
//                string className = keyValuePair.KeyName;
//                AdapterStatus status = keyValuePair.Value;
//                switch (status.InitializationState)
//                {
//                    case AdapterState.NotReady:
//                        // The adapter initialization did not complete.
//                        AdmobInitialized = false;
//                        Debug.LogError(" Admob: Adapter: " + className + " not ready.");
//                        break;
//                    case AdapterState.Ready:
//                        // The adapter was successfully initialized.
//                        Debug.Log(" Admob: Adapter: " + className + " is initialized.");

//                        ///sending ads request
//                        if (DontLoadAds.Equals(false))
//                        {
//                            AdmobInitialized = true;
//                            if (!RemoveAds)
//                            {
//                                if (ShowBannerAtStart)
//                                    ShowAdmobBanner();
//                                if (RequestAppOpenAd)
//                                    this.RequestAdmobAppOpenAds();
//                                this.RequestInterstitial();
//                            }
//                            this.RequestAdmobRewardedAds();
//                        }
//                        break;
//                }
//            }
//        });
//        yield return null;
//    }


//    /// <summary>
//    /// Initalizing Unity SDk
//    /// </summary>
//    /// <returns></returns>
//    private IEnumerator InitializeUnity()
//    {
//        Debug.Log(" Unity Ads IDs: " + unityGameId);
//        Advertisement.Initialize(unityGameId, EnableTestIds, this);
//        yield return null;
//    }
//    #endregion


//    /***************************************************************************************************************************************
//                                                                Admob Section
//    //**************************************************************************************************************************************/

//    #region ADMOB Section
//    #region Admob banner
//    private void RequestAdmobBanner(string BannerAdsID, ref BannerView bannerView, AdSize adSize, AdPosition bannerPosition)
//    {
//        if (RemoveAds) return;

//        // Requesting Banner ADS
//        Debug.Log(" Admob: Requesting Banner with ID: " + BannerAdsID);

//        //if banner is already running then destrouy it first and call again
//        bannerView?.Destroy();

//        bannerView = new BannerView(BannerAdsID, adSize, bannerPosition);

//        // Called when an ad request has successfully loaded.
//        bannerView.OnBannerAdLoaded += () =>
//        {
//            if (adSize.Equals(AdSize.Banner))
//                isAdmobBanner = true;
//            else if (adSize.Equals(AdSize.MediumRectangle))
//                isAdmobBannerMed = true;
//            Debug.Log(" Admob: Banner add loaded.");
//        };
//        // Called when an ad request failed to load.
//        bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
//        {
//            Debug.Log(" Admob: Banner ads failed to load: " + error.GetMessage());
//        };
//        // Called when an ad is clicked.
//        bannerView.OnAdFullScreenContentOpened += () =>
//        {
//            Debug.Log(" Admob: Banner Ads Opening");
//        };
//        bannerView.OnAdClicked += () =>
//        {
//            Debug.Log(" Admob: Banner Ads Clicked");
//        };
//        bannerView.OnAdFullScreenContentClosed += () =>
//        {
//            if (adSize.Equals(AdSize.Banner))
//                this.bannerView = null;
//            else if (adSize.Equals(AdSize.MediumRectangle))
//                this.medBannerView = null;
//        };

//        // Create an empty ad request.
//        AdRequest request = new();
//        // Load the banner with the request.
//        bannerView.LoadAd(request);

//    }


//    /// <summary>
//    /// Show admob banner ads
//    /// </summary>
//    private void ShowAdmobBanner()
//    {
//        Debug.Log(" Admob: Rectangle Banner show");
//        this.RequestAdmobBanner(BannerAdsID, ref bannerView, AdSize.Banner, AdPosition.Bottom);
//    }


//    /// <summary>
//    /// Show admob MediumRectangle banner ad
//    /// </summary>
//    public void ShowMedAdmobBanner()
//    {
//        Debug.Log(" Admob: MediumRectangle Banner show");
//        this.RequestAdmobBanner(MedBannerAdsID, ref medBannerView, AdSize.MediumRectangle, AdPosition.BottomLeft);
//    }


//    /// <summary>
//    /// close or destroy admob banner
//    /// </summary>
//    private void AdmobBannerHide()
//    {
//        Debug.Log(" Admob: Banner closing");
//        bannerView?.Hide();
//        isAdmobBanner = false;
//    }

//    /// <summary>
//    /// close or destroy admob banner
//    /// </summary>
//    public IEnumerator AdmobMedBannerHide()
//    {
//        medBannerView?.Hide();
//        Debug.Log("Med banner is: " + medBannerView);
//        isAdmobBannerMed = false;
//        yield return null;

//    }
//    #endregion


//    #region Admob Interstitial
//    private void RequestInterstitial()
//    {
//        Debug.Log(" Admob: Requesting Interstitial Ads with ID: " + InterstitialAdsID);

//        if (RemoveAds)
//            return;

//        this.interstitialAd?.Destroy();

//        //Create an empty ad request.
//        AdRequest request = new();
//        // Load the interstitial with the request.
//        InterstitialAd.Load(InterstitialAdsID, request, (InterstitialAd InterAds, LoadAdError error) =>
//        {
//            if (error != null || InterAds == null)
//            {
//                Debug.LogError(" Admob: Interstitial Ads Failed to load: " + error);
//                return;
//            }
//            else
//            {
//                Debug.Log(" Admob: Interstitial Ads loaded with response: " + InterAds.GetResponseInfo());
//                this.interstitialAd = InterAds;
//            }

//            //ad callbackss
//            InterAds.OnAdFullScreenContentOpened += () =>
//            {
//                //showing loading ads popup
//                // Delegates.ShowLoadingAdsPopup?.Invoke(false);
//                Debug.Log(" Admob: Interstitial ad opening.");
//            };
//            InterAds.OnAdFullScreenContentClosed += () =>
//            {
//                Debug.Log(" Admob: Interstitial ad closed.");
//                this.RequestInterstitial();
//            };
//            InterAds.OnAdClicked += () =>
//            {
//                Debug.Log(" Admob: Interstitial ad recorded a click.");
//            };
//            InterAds.OnAdFullScreenContentFailed += (AdError error) =>
//            {
//                //showing loading ads popup
//                //   Delegates.ShowLoadingAdsPopup?.Invoke(false);
//                Debug.Log(" Admob: Interstitial ad failed to show with error: " +
//                            error.GetMessage());
//            };
//        });
//    }


//    /// <summary>
//    /// show admob interstitial ads
//    /// </summary>
//    private void ShowAdmobInterAds()
//    {
//        Debug.Log(" Admob: Showing Admob Inter Ads");
//        if (interstitialAd.CanShowAd())
//            interstitialAd.Show();
//        else
//        {
//            //showing loading ads popup
//            // Delegates.ShowLoadingAdsPopup?.Invoke(false);
//            Debug.LogError(" Admob: Interstitial Ads not ready");
//        }
//    }
//    #endregion


//    #region Admob Rewarded
//    private void RequestAdmobRewardedAds()
//    {
//        //Requesting Rewarded ADS
//        Debug.Log(" Admob: Requesting Rewarded Ads with ID: " + RewardedAdsID);

//        this.rewardedAd?.Destroy();

//        // Create an empty ad request.
//        AdRequest request = new();
//        // Load the rewarded ad with the request.
//        RewardedAd.Load(RewardedAdsID, request, (RewardedAd RewardedAds, LoadAdError error) =>
//        {
//            if (error != null || RewardedAds == null)
//            {
//                Debug.LogError(" Admob: Rewarded Ads Failed to load: " + error);
//                return;
//            }
//            else
//            {
//                Debug.Log(" Admob: Rewarded Ads loaded with response: " + RewardedAds.GetResponseInfo());
//                this.rewardedAd = RewardedAds;
//            }

//            // ads callbacks
//            RewardedAds.OnAdFullScreenContentOpened += () =>
//            {
//                //showing loading ads popup
//                //    Delegates.ShowLoadingAdsPopup?.Invoke(false);
//                Debug.Log(" Admob: Rewarded ad opening.");
//            };
//            RewardedAds.OnAdFullScreenContentClosed += () =>
//            {
//                Debug.Log(" Admob: Rewarded ad closed.");
//                // Reloading rewarded ads again
//                if (RewardAction != null)
//                    RewardAction?.Invoke();

//                this.RequestAdmobRewardedAds();
//            };
//            RewardedAds.OnAdClicked += () =>
//            {
//                Debug.Log(" Admob: Rewarded ad recorded a click.");
//            };
//            RewardedAds.OnAdFullScreenContentFailed += (AdError error) =>
//            {
//                //showing loading ads popup
//                //  Delegates.ShowLoadingAdsPopup?.Invoke(false);
//                Debug.Log(" Admob: Rewarded ad failed to show with error: " + error.GetMessage());
//                // Reloading rewarded ads if failed
//            };
//        });
//    }


//    /// <summary>
//    /// show admob rewarded ads
//    /// </summary>
//    private void ShowAdmobRewardedAds()
//    {
//        Debug.Log(" Admob: Showing Admob Rewarded Video Ads");

//        if (rewardedAd.CanShowAd())
//        {
//            rewardedAd.Show((Reward reward) =>
//            {
//                Debug.Log(" Admob: Rewarded ad granted a reward.");

//                //todo grant a reward here
//                //Giving reward here if you have assigned method in show rewarded ads as a parameter
//                //if (RewardAction != null)
//                //    RewardAction?.Invoke();
//                //else Debug.LogError(" Admob: Rewarded Action missing");
//            });
//        }
//        else
//        {
//            //showing loading ads popup
//            //  Delegates.ShowLoadingAdsPopup?.Invoke(false);
//            Debug.LogError(" Admob: Rewarded ad is not ready yet.");
//            //this.RequestAdmobRewardedAds();
//        }
//    }
//    #endregion ADMOB REWARDED


//    #region ADMON AppOpen ADS
//    void RequestAdmobAppOpenAds()
//    {
//        Debug.Log("Admob: Requesting AppOpen Ads with ID: " + AppOpenAdsID);

//        if (RemoveAds)
//            return;

//        // destroy ads if not empty
//        appOpenAd?.Destroy();

//        // creating new request
//        AdRequest request = new();

//        //loading ads
//        AppOpenAd.Load(AppOpenAdsID, request,
//            (AppOpenAd ad, LoadAdError loadError) =>
//            {
//                // if ad failed to load
//                if (loadError != null)
//                {
//                    Debug.LogError("Admob: AppOpen ad failed to load with error: " +
//                        loadError.GetMessage());
//                    return;
//                }
//                else if (ad == null)
//                {
//                    Debug.LogError("Admob: AppOpen ad failed to load.");
//                    return;
//                }
//                else
//                {
//                    this.appOpenAd = ad;
//                    m_ShowAppOpenAd = true;

//                    if (ShowAppOpenAdOnLoad)
//                        ShowAppOpenAd();

//                    Debug.Log("Admob: AppOpen ad loaded."); // ad loaded
//                }

//                //this.appOpenExpireTime = DateTime.Now + APPOPEN_TIMEOUT;

//                // ad callbacks
//                ad.OnAdFullScreenContentOpened += () =>
//                {
//                    Debug.Log("Admob: App open ad opened.");
//                    //ShowAppOpenAdsTrueFalse(false);
//                };
//                ad.OnAdFullScreenContentClosed += () =>
//                {
//                    //ShowAppOpenAdsTrueFalse(false);
//                    Debug.Log("Admob: App open ad closed.");
//                    //Fixme remove this if you want to run this ONCE
//                    //this.RequestAdmobAppOpenAds();
//                };
//                ad.OnAdFullScreenContentFailed += (AdError error) =>
//                {
//                    Debug.LogError("Admob: App open ad failed to show with error: " +
//                        error.GetMessage());
//                };
//            });
//    }

//    void DestroyAppOpenAd()
//    {
//        if (this.appOpenAd != null)
//        {
//            this.appOpenAd.Destroy();
//            this.appOpenAd = null;
//        }
//    }

//    public void ShowAppOpenAd()
//    {
//        Debug.Log("m_ShowAppOpenAd: " + m_ShowAppOpenAd);
//        if (m_ShowAppOpenAd && appOpenAd.CanShowAd())
//        {
//            Debug.Log("Admob: Showing AppOpen ads Ads");
//            appOpenAd.Show();
//            m_ShowAppOpenAd = false;
//        }
//    }

//    //public void ShowAppOpenAdsTrueFalse(bool IsTrue)
//    //{
//    //    m_ShowAppOpenAd = IsTrue;
//    //    Debug.Log("Show AppOpen Ads True False: " + IsTrue);
//    //}
//    #endregion
//    #endregion ADMOB


//    /***************************************************************************************************************************************
//                                                            Unity Section
//    //**************************************************************************************************************************************/

//    #region UNITY ADS
//    /// <summary>
//    /// loading unity banner
//    /// </summary>
//    private void LoadUnityBanner()
//    {
//        if (RemoveAds)
//            return;

//        Debug.Log(" UnityAds: banner Loading");
//        Advertisement.Load(BannerPlacementID, this);
//    }


//    /// <summary>
//    /// loading unity interstitial ads
//    /// </summary>
//    private void LoadUnityInter()
//    {
//        if (RemoveAds)
//            return;

//        Debug.Log(" UnityAds: inter Loading");
//        Advertisement.Load(InterPlacementID, this);
//    }


//    /// <summary>
//    /// loading unity rewarded ads 
//    /// </summary>
//    private void LoadUnityRewarded()
//    {
//        Debug.Log(" UnityAds: rewarded Loading");
//        Advertisement.Load(RewardedPlacementID, this);
//    }


//    /// <summary>
//    /// show unity banner
//    /// </summary>
//    private void ShowUnityBanner()
//    {
//        if (Advertisement.isInitialized)
//        {
//            Debug.Log(" UnityAds: banner show");
//            BannerOptions bannerOptions = new BannerOptions();
//            bannerOptions.showCallback += () => { isUnityBanner = true; };
//            bannerOptions.hideCallback += () => { isUnityBanner = false; };
//            Advertisement.Banner.SetPosition(bannerPositionUnity);
//            Advertisement.Banner.Show(BannerPlacementID, bannerOptions);
//        }
//    }


//    /// <summary>
//    /// show unity interstitial ads
//    /// </summary>
//    private void ShowUnityInter()
//    {
//        //showing loading ads popup
//        // Delegates.ShowLoadingAdsPopup?.Invoke(false);
//        Debug.Log(" UnityAds: inter show");
//        if (Advertisement.isInitialized)
//        {
//            Advertisement.Show(InterPlacementID, this);
//        }
//    }


//    /// <summary>
//    /// show unity rewarded ads
//    /// </summary>
//    private void ShowUnityRewarded()
//    {
//        //showing loading ads popup
//        //  Delegates.ShowLoadingAdsPopup?.Invoke(false);
//        Debug.Log(" UnityAds: rewarded show");
//        if (Advertisement.isInitialized)
//            Advertisement.Show(RewardedPlacementID, this);
//    }


//    /// <summary>
//    /// hide unity banner
//    /// </summary>
//    private void UnityBannerClose()
//    {
//        Debug.Log(" UnityAds: banner hiding");
//        Advertisement.Banner.Hide();
//    }

//    #endregion UNITY ADS


//    /***************************************************************************************************************************************
//                                                            Show Methods
//    //**************************************************************************************************************************************/
//    #region Show Methods
//    /// <summary>
//    /// Show Banner ads method.
//    /// This is based on selected Platform and priority
//    /// </summary>
//    public void ShowBannerAds()
//    {
//        if (RemoveAds || isAdmobBanner)
//            return;

//        /// if platform is admob only
//        if (SDK.Equals(SDKs.Admob))
//        {
//            if (AdmobInitialized)
//                ShowAdmobBanner();
//        }
//        /// if platform is unity only
//        else if (EnableUnityBanner && SDK.Equals(SDKs.Unity))
//        {
//            if (Advertisement.isInitialized)
//                ShowUnityBanner();
//        }
//        /// if platform is both
//        else if (SDK.Equals(SDKs.Both))
//        {
//            if (priority.Equals(Priority.Admob))
//            {
//                Debug.Log(" Priority: " + priority.ToString());
//                if (AdmobInitialized)
//                    ShowAdmobBanner();
//                else if (EnableUnityBanner && Advertisement.isInitialized)
//                    ShowUnityBanner();
//            }
//            else if (priority.Equals(Priority.Unity))
//            {
//                Debug.Log(" Priority: " + priority.ToString());
//                if (EnableUnityBanner && Advertisement.isInitialized)
//                    ShowUnityBanner();
//                else if (AdmobInitialized)
//                {
//                    ShowAdmobBanner();
//                }
//            }
//        }
//    }

//    public void ShowInterAds(Action action = null)
//    {
//        StartCoroutine(ShowInterAds_(action));
//    }

//    /// <summary>
//    /// Show Interstitial ads method.
//    /// This is based on selected Platform and priority
//    /// </summary>
//    private IEnumerator ShowInterAds_(Action action)
//    {
//        var result = CheckInterAdsAvailability(SDK, action);
//        Debug.Log("Inter Ads Availablity: " + result);
//        if (result.Equals(false))
//        {
//            action?.Invoke();
//            yield break;
//        }


//        //Show Loading Ads Panel
//        if (ShowLoadingPanel)
//        {
//            LoadingPanel.Show();
//            yield return new WaitForSeconds(2);
//            LoadingPanel.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
//            LoadingPanel.Hide();
//            yield return new WaitUntil(() => LoadingPanel.gameObject.activeSelf == false);
//        }

//        if (SDK.Equals(SDKs.Admob))
//        {
//            if (AdmobInitialized && interstitialAd.CanShowAd())
//                ShowAdmobInterAds();
//        }
//        /// if platform is unity only
//        else if (SDK.Equals(SDKs.Unity))
//        {
//            if (Advertisement.isInitialized && unityInterAdsLoaded)
//                ShowUnityInter();
//        }
//        /// if platform is both
//        else if (SDK.Equals(SDKs.Both))
//        {
//            if (priority.Equals(Priority.Admob))
//            {
//                Debug.Log("Priority: " + priority.ToString());
//                if (AdmobInitialized && interstitialAd != null && interstitialAd.CanShowAd())
//                    ShowAdmobInterAds();
//                else if (Advertisement.isInitialized && unityInterAdsLoaded)
//                    ShowUnityInter();
//            }
//            else if (priority.Equals(Priority.Unity))
//            {
//                Debug.Log(" Priority: " + priority.ToString());
//                if (Advertisement.isInitialized && unityInterAdsLoaded)
//                    ShowUnityInter();
//                else if (interstitialAd.CanShowAd())
//                    ShowAdmobInterAds();
//            }
//        }

//        //Any action/event you want perform after calling the ads show
//        action?.Invoke();
//    }

//    public void ShowRewardedAds(Action rewardedAction, Action rewardNotReady = null)
//    {
//        StartCoroutine(ShowRewardedAds_(rewardedAction, rewardNotReady));
//    }

//    /// <summary>
//    /// Show rewarded ads method.
//    /// This is based on selected Platform and priority
//    /// </summary>
//    /// <param name="rewardedAction"> You can assign reward method as a parameter and it will auto assign after ads completion</param>
//    private IEnumerator ShowRewardedAds_(Action rewardedAction, Action RewardNotReady = null)
//    {
//        var result = CheckRewardedAdsAvailability(RewardNotReady);
//        Debug.Log("Rewarded Ads Availablity: " + result);
//        if (result.Equals(false))
//            yield break;

//        if (ShowLoadingPanel)
//        {
//            LoadingPanel.Show();
//            yield return new WaitForSeconds(2);
//            //LoadingPanel.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
//            LoadingPanel.Hide();
//            yield return new WaitUntil(() => LoadingPanel.gameObject.activeSelf == false);
//        }

//        /// if platform is admob only
//        if (SDK.Equals(SDKs.Admob))
//        {
//            if (AdmobInitialized && rewardedAd.CanShowAd())
//                ShowAdmobRewardedAds();
//        }
//        /// if platform is unity only
//        else if (SDK.Equals(SDKs.Unity))
//        {
//            if (Advertisement.isInitialized && unityRewardAdsLoaded)
//                ShowUnityRewarded();
//        }
//        /// if platform is both
//        else if (SDK.Equals(SDKs.Both))
//        {
//            if (priority.Equals(Priority.Admob))
//            {
//                Debug.Log(" Priority: " + priority.ToString());
//                if (rewardedAd.CanShowAd())
//                    ShowAdmobRewardedAds();
//                else if (Advertisement.isInitialized && unityRewardAdsLoaded)
//                    ShowUnityRewarded();
//            }
//            else if (priority.Equals(Priority.Unity))
//            {
//                Debug.Log(" Priority: " + priority.ToString());
//                if (Advertisement.isInitialized && unityRewardAdsLoaded)
//                    ShowUnityRewarded();
//                else if (rewardedAd.CanShowAd())
//                    ShowAdmobRewardedAds();
//            }
//        }
//        RewardAction = rewardedAction;
//    }

//    bool CheckRewardedAdsAvailability(Action RewardNotReady = null)
//    {
//        try
//        {
//            bool canShowAdmobAd = rewardedAd != null ? rewardedAd.CanShowAd() : false;
//            bool canShowUnityAd = unityRewardAdsLoaded;

//            switch (SDK)
//            {
//                case SDKs.Admob:
//                    if (!canShowAdmobAd)
//                    {
//                        RewardNotReady?.Invoke();
//                        return false;
//                    }
//                    return true;

//                case SDKs.Unity:
//                    if (!canShowUnityAd)
//                    {
//                        RewardNotReady?.Invoke();
//                        return false;
//                    }
//                    return true;

//                case SDKs.Both:
//                    if (!canShowAdmobAd && !canShowUnityAd)
//                    {
//                        RewardNotReady?.Invoke();
//                        return false;
//                    }
//                    return true;

//                default:
//                    RewardNotReady?.Invoke();
//                    return false;
//            }
//        }
//        catch (Exception e)
//        {
//            Debug.LogException(e);
//            RewardNotReady?.Invoke();
//            return false;
//        }
//    }
//    bool CheckInterAdsAvailability(SDKs SDK, Action AdsNotReady = null)
//    {
//        try
//        {
//            bool canShowAdmobAd = interstitialAd != null ? interstitialAd.CanShowAd() : false;
//            bool canShowUnityAd = unityInterAdsLoaded;

//            switch (SDK)
//            {
//                case SDKs.Admob:
//                    if (!canShowAdmobAd)
//                    {
//                        AdsNotReady?.Invoke();
//                        return false;
//                    }
//                    return true;

//                case SDKs.Unity:
//                    if (!canShowUnityAd)
//                    {
//                        AdsNotReady?.Invoke();
//                        return false;
//                    }
//                    return true;

//                case SDKs.Both:
//                    if (!canShowAdmobAd && !canShowUnityAd)
//                    {
//                        AdsNotReady?.Invoke();

//                        return false;
//                    }
//                    return true;

//                default:
//                    AdsNotReady?.Invoke();
//                    return false;
//            }
//        }
//        catch (Exception e)
//        {
//            Debug.LogException(e);
//            AdsNotReady?.Invoke();
//            return false;
//        }
//    }


//    /// <summary>
//    /// hide banner
//    /// </summary>
//    public void DestroyBanner()
//    {
//        if (AdmobInitialized && isAdmobBanner)
//        {
//            AdmobBannerHide();
//        }
//        else if (EnableUnityBanner && Advertisement.isInitialized && isUnityBanner)
//        {
//            UnityBannerClose();
//        }
//    }

//    #endregion Show Methods


//    /***************************************************************************************************************************************
//                                                            Unity CallBacks Methods
//    //**************************************************************************************************************************************/
//    #region Unity Callbacks events
//    /// <summary>
//    /// Unity ads callbak
//    /// </summary>
//    public void OnInitializationComplete()
//    {
//        Debug.Log(" Unity Ads initialization complete.");
//        if (Advertisement.isInitialized)
//        {
//            Debug.Log(" Loading Unity Ads");

//            ///Loading ads here, when unity SDK is initialized
//            if (DontLoadAds.Equals(false))
//            {
//                if (!RemoveAds)
//                {
//                    LoadUnityInter();
//                    if (EnableUnityBanner)
//                        LoadUnityBanner();
//                }
//                LoadUnityRewarded();
//            }
//        }
//    }

//    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
//    {
//        Debug.LogError(" Unity Ads Initialization Failed. \nError: " + error + "\n Message: " + message);
//    }

//    public void OnUnityAdsAdLoaded(string placementId)
//    {
//        Debug.Log(" UnityAds: Ads Loaded: " + placementId);
//        if (placementId.Equals(InterPlacementID))
//            unityInterAdsLoaded = true;
//        else if (placementId.Equals(RewardedPlacementID))
//            unityRewardAdsLoaded = true;
//    }

//    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
//    {
//        Debug.LogError(" Unity Ads failed to Load: " + placementId);
//    }

//    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
//    {
//        Debug.Log(" Unity Ads failed to show: " + placementId);
//    }

//    public void OnUnityAdsShowStart(string placementId)
//    {
//        Debug.Log(" Unity Ads show start: " + placementId);
//    }

//    public void OnUnityAdsShowClick(string placementId)
//    {
//        Debug.Log(" Unity Ads show clicked: " + placementId);
//    }

//    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
//    {
//        Debug.Log(" Unity Ads show Completed: " + placementId);
//        if (placementId.Equals(InterPlacementID))
//        {
//            /// after completion reload inter ads here
//            LoadUnityInter();
//        }
//        else if (placementId.Equals(RewardedPlacementID))
//        {
//            Debug.Log(" Rewarded ads completed: Assigning reward.");
//            if (showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
//                if (RewardAction != null)
//                {
//                    //Giving reward here if you have assigned method in show rewarded ads as a parameter
//                    RewardAction?.Invoke();
//                }
//                else Debug.LogError(" Rewarded Action missing");
//            // reloading rewarded ads here
//            LoadUnityRewarded();
//        }
//    }
//    #endregion callback

//    //private void OnApplicationPause(bool pause)
//    //{
//    //    if (PlayerPrefs.GetInt("Removeads", 0) == 1)
//    //    {
//    //        return;

//    //    }
//    //    //   Debug.Log("On application Pause: " + pause);
//    //    if (SceneManager.GetActiveScene().name.Equals("GameOnScene") || SceneManager.GetActiveScene().name.Equals("MyPenalty"))
//    //        return;
//    //    else if (m_ShowAppOpenAd && !pause)
//    //        ShowAppOpenAd();
//    //    // StartCoroutine(MakeAppOpenAdsTrue());
//    //}
//}


///***************************************************************************************************************************************
//                                                            Enum + Struct
////**************************************************************************************************************************************/
//#region Fields
//[Serializable]
//public enum SDKs
//{
//    None,
//    Unity,
//    Admob,
//    Both
//}

//[Serializable]
//public enum Priority
//{
//    None, Admob, Unity
//}

//[Serializable]
//public enum Platform
//{
//    Android, IOS
//}

//[Serializable]
//public struct AdmobIds
//{
//    public string BannerID;
//    public string MedBannerID;
//    public string InterID;
//    public string RewardedID;
//    public string AppOpenID;
//    public string NativeID;
//}
//[Serializable]
//public struct PlacementIDs
//{
//    public string BannerPlacementID;
//    public string InterPlacementID;
//    public string RewardedPlacementID;
//}

//#endregion