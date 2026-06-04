using MyTools.SaveManager;
using System;
using System.Collections;
using UnityEngine;

namespace GoogleAds
{
    public class AdsManager : Singleton<AdsManager>
    {
        #region Variables

        [Header("========== Debugger ==========")]
        [SerializeField]
        bool ShowDebugLogs = false;

        [Header("========== Fram Limit ==========")]
        [SerializeField]
        bool SetFrames = true;
        [SerializeField] private int Frames = 60;

        [Header("========== Internet Check ==========")]
        [SerializeField] internal bool checkInternet = true;
        internal InternetConnectivity internetConnectivity = null;

        [Header("========== Test Mode ==========")]
        [SerializeField] private bool ENABLE_TEST_ADS = false;

        [Header("========== Setting ==========")]
        [SerializeField] private bool ENABLE_ADS = true;

        [Header("========== ADS SETTING ==========")]
        private Platform m_Platform = Platform.None;
      
        [Tooltip("Create scriptable object [ADSETTING], Copy Past ID and Drag it here")]
        [SerializeField] private AdSettings adSettings; // Reference to the ScriptableObject for Ad configuration

        [Space]
        [SerializeField] private bool NeverSleepMode = true;
        [SerializeField] private bool ShowBannerOnLoad = false;
        [SerializeField] private bool ShowAppOpenAdOnLoad = false;
        [SerializeField] private bool ShowAppOpenInBackground = false;
        [SerializeField] private bool ShowLoadingPanel = false;

        [Header("========== UI Components ==========")]
        [SerializeField] private GameObject LoadingPopup;
        [SerializeField] internal GameObject InternetPopup = null;

        // dont destroy these bool
        private IAdmobAD admobAd = null;
        //private IUnityAD unityAd = null;

        private bool admobBannerShowing;

        private bool admobMedBannerShowing;
        private bool isFirstTimeOpen = true; // Starts as true to indicate the app is opening for the first time
        private bool isADLoading;

        private bool IsAd_Removed => SaveManager.Prefs.GetBool(SharedVariables.RemoveAds, false, true);

        #endregion
         
        #region Awake
        // Creating Instance

        protected override void Awake()
        {
            base.Awake();
            Debug.unityLogger.logEnabled = ShowDebugLogs;
            if (SetFrames) Application.targetFrameRate = Frames;
            if (NeverSleepMode) Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        #endregion

        #region Start\Initializing
        private IEnumerator Start()
        {
            if (checkInternet)
            {
                internetConnectivity = new InternetConnectivity
                {
                    checkInternet = this.checkInternet,
                    OnInternetLost = () => InternetPopup?.SetActive(true),
                    OnInternetRestored = () => InternetPopup?.SetActive(false),
                };
                StartCoroutine(internetConnectivity.CheckInternet());
            }

            yield return PlatformDetection();

            if (m_Platform == Platform.None) yield break;

            yield return InitializeSelectedSDKs();
        }
        IEnumerator PlatformDetection()
        {
#if UNITY_ANDROID
            m_Platform = Platform.Android;
#elif UNITY_IOS
        m_Platform = Platform.IOS;
#else
        m_Platform = Platform.None; // add this enum value
#endif
            Debug.Log($"<color=yellow>Runtime Platform Detected: {m_Platform}</color>");
            yield return null;
        }
      
        /// <summary>
        /// Initializes the selected SDKs based on the user's choice in adSDK flags and IDs
        /// </summary>
        private IEnumerator InitializeSelectedSDKs()
        {
            if (ENABLE_ADS == false)
            {
                Debug.Log("ADS are disabled.");
                yield break;
            }

            // Check if AdPriorityOrder is empty
           
            bool? anySDKInitialized = null; // Flag to check if any SDK was initialized

            if (adSettings == null)
                adSettings = Resources.Load<AdSettings>("AdSettings"); // Load the AdSettings ScriptableObject

            if (adSettings == null)
            {
                Debug.Log("<color=red>AdSettings not found in Resources/AdSettings</color>");
                yield break;
            }

            // Check and initialize AdMob
           
                // Create GameObjects for each ad manager
                if (admobAd == null)
                {
                    GameObject admobGO = new("AdmobAd");
                    admobGO.transform.SetParent(transform);
                    admobAd = admobGO.AddComponent<IAdmobAD>();  // Assuming AdmobAd is the class that implements IAdmobAD
                }

                if (admobAd != null)
                {
                    Debug.Log("Initializing AdMob ID...");
                    yield return admobAd.Initialize_Admob(adSettings, m_Platform, ShowBannerOnLoad, ShowAppOpenAdOnLoad, ENABLE_TEST_ADS);
                    anySDKInitialized = true; // Set flag to true if AdMob is initialized
                }
           
            // Check if no SDKs were initialized
            if (anySDKInitialized == null || !(bool)anySDKInitialized)
            {
                Debug.LogError("Error: No Ad SDK has been initialized. Please check your settings and SDK assignments.");
            }
        }
        #endregion Start\Initializing

        /***************************************************************************************************************************************
                                                               Show Methods
       //**************************************************************************************************************************************/

        IEnumerator Waithelper()
        {
            float timer = 0f;
            while ((admobAd == null || admobAd.CurrentBannerStatus == AdsStatus.None) && timer < 5f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }

        #region Show Methods
        /// <summary>
        /// Show Banner ads method.
        /// This is based on selected Platform and priority
        /// </summary>
        #region Banner
        public void ShowBanner()
        {
            StartCoroutine(ShowBannerAds());
        }

        private IEnumerator ShowBannerAds()
        {
            if (ENABLE_ADS == false)
            {
                Debug.Log("Remote Config: ADS are disabled.");
                if (admobAd && admobAd.IsBannerReady)
                    admobAd.DestroyBanner(); // Hide AdMob banner if it is ready

                yield break;
            }

            if (IsAd_Removed)
            {
                Debug.LogError("Ads are removed");
                yield break; // Ads are disabled, exit the method
            }
            
            yield return Waithelper();
            if (admobAd == null || admobAd.CurrentBannerStatus == AdsStatus.None)
            {
                Debug.LogError("Admob: Banner is null or Not Ready");
                yield break;
            }

            if (admobAd != null && admobAd.AdmobInitialized)
            {
                // Show AdMob banner
                Debug.Log("Showing AdMob banner.");
                admobAd.ShowAdmobBanner();
                admobBannerShowing = true;
                yield break;
            }
              
        }

        #endregion Banner

        #region Big Banner
        public void ShowBigBannerAds()
        {
            if (ENABLE_ADS == false)
            {
                Debug.Log("Remote Config: ADS are disabled.");
                if (admobAd && admobAd.IsMedBannerReady)
                    admobAd.DestroyMedBanner(); // Hide AdMob banner if it is ready

                return;
            }

            if (IsAd_Removed)
            {
                Debug.LogError("Ads are removed");
                return; // Ads are disabled, exit the method
            }
           
            if (admobAd != null && admobAd.AdmobInitialized && admobAd.IsMedBannerReady) // Check if AdMob Med banner is ready
            {
                Debug.Log("Showing AdMob Med banner.");
                admobMedBannerShowing = true;
                admobAd.ShowMedAdmobBanner(); // Show AdMob banner
                return;
            }
        }
        #endregion Big Banner

        #region Destroy Banner
        /// <summary>
        /// hide banner
        /// </summary>
        public void DestroyBanner()
        {
            // Check and destroy AdMob banner if it is showing
            if (admobBannerShowing)
            {
                admobAd?.DestroyBanner(); // Call the method to destroy the AdMob banner
                admobBannerShowing = false; // Update the state to indicate banner is no longer showing
            }
        }

        public void HideBigBanner()
        {
            if (admobMedBannerShowing)
            {
                admobAd?.HideBigBanner();
                admobMedBannerShowing = false;
            }
        }

        public void DestroyBigBanner()
        {
            // Check and destroy AdMob Med banner if it is showing
            if (admobMedBannerShowing)
            {
                admobAd?.DestroyMedBanner(); // Call the method to destroy the AdMob Med banner
                admobMedBannerShowing = false; // Update the state to indicate Med banner is no longer showing
            }
        }
        #endregion Destroy Banner

        #region Inter
        public void ShowInterAds()
        {
            StartCoroutine(ShowInterAds_(null));
        }

        public void ShowInterAds(Action action)
        {
            StartCoroutine(ShowInterAds_(action));
        }

        /// <summary>
        /// Show Interstitial ads method.
        /// This is based on selected Platform and priority
        /// </summary>
        private IEnumerator ShowInterAds_(Action action)
        {
            if (ENABLE_ADS == false)
            {
                Debug.Log("Remote Config: ADS are disabled.");
                if (admobAd && admobAd.IsInterstitialReady())
                    admobAd.DestroyInterAds(); // Destroy AdMob banner if it is ready

                yield break;
            }

            if (IsAd_Removed)
            {
                Debug.LogError("Ads are removed");
                yield break; // Ads are disabled, exit the method
            }

            //Show Loading Ads Panel
            if (ShowLoadingPanel && LoadingPopup)
            {
                LoadingPopup.SetActive(true);
                yield return new WaitForSeconds(3);
                LoadingPopup.SetActive(false);
                yield return new WaitUntil(() => LoadingPopup.gameObject.activeSelf == false);
            }

          
            if (admobAd != null && admobAd.AdmobInitialized && admobAd.IsInterstitialReady()) // Check if AdMob interstitial is ready
            {
                Debug.Log("Showing AdMob interstitial.");

                isADLoading = true;

                admobAd.ShowAdmobInterAds(); // Show AdMob interstitial
                StartCoroutine(Reset_AppOpen_ADLoading(2));
                action?.Invoke(); // Trigger callback after showing ad
                yield break;
            }

            ////Any action/event you want to perform after ads failed to show
            action?.Invoke();
        }
        #endregion Inter

        #region Rewarded 
        public void ShowRewardedAds()
        {
            ShowRewardedAds(null, null);
        }

        public void ShowRewardedAds(Action rewardedAction, Action rewardNotReady = null)
        {
            StartCoroutine(ShowRewardedAds_(rewardedAction, rewardNotReady));
        }

        /// <summary>
        /// Show rewarded ads method.
        /// This is based on selected Platform and priority
        /// </summary>
        /// <param name="rewardedAction"> You can assign reward method as a parameter and it will auto assign after ads completion</param>
        private IEnumerator ShowRewardedAds_(Action rewardedAction, Action RewardNotReady = null)
        {
            if (admobAd && admobAd.IsRewardedAdReady() == false)
            {
                PopupSetting popup = new PopupSetting
                {
                    Title = "Not Ready",
                    Message = "Rewarded Ads are not ready yet. Please try again later.",
                    AutoClosePopup = true,
                    AutoCloseDelay = 1.5f,
                    EnableFirstBtn = false,
                    EnableSecondBtn = false
                };
                UIManager.Instance?.ShowPopup(popup);
                RewardNotReady?.Invoke();
                yield break;
            }


            if (ENABLE_ADS == false)
            {
                Debug.Log("Remote Config: ADS are disabled.");
                if (admobAd && admobAd.IsRewardedAdReady())
                    admobAd.DestroyRewardedAds(); // Hide AdMob banner if it is ready

                yield break;
            }


            //Show Loading Ads Panel
            if (ShowLoadingPanel && LoadingPopup)
            {
                LoadingPopup.SetActive(true);
                yield return new WaitForSeconds(3);
                LoadingPopup.SetActive(false);
                yield return new WaitUntil(() => LoadingPopup.gameObject.activeSelf == false);
            }

            if (admobAd != null && admobAd.AdmobInitialized && admobAd.IsRewardedAdReady()) // Check if AdMob Rewarded is ready
            {
                Debug.Log("Showing AdMob Rewarded.");

                isADLoading = true;

                admobAd.ShowAdmobRewardedAds(rewardedAction);
                StartCoroutine(Reset_AppOpen_ADLoading(2));
                yield break;
            }
              
        }
        #endregion Rewarded

        #region AppOpen
        IEnumerator Reset_AppOpen_ADLoading(float wait)
        {
            yield return new WaitForSeconds(wait);
            isADLoading = false;
        }

        public bool CanShowADOpenAD()
        {
            return admobAd && admobAd.IsAppOpenAdReady;
        }
        public void ShowAppOpen()
        {
            if (ENABLE_ADS == false)
            {
                Debug.Log("Remote Config: ADS are disabled.");
                if (admobAd && admobAd.IsAppOpenAdReady)
                    admobAd.DestroyAppOpenAd(); // Hide AdMob banner if it is ready

                return;
            }

            if (IsAd_Removed)
            {
                // Ads are disabled, exit the method
                Debug.LogError("Ads are removed");
                return;
            }

            if (admobAd && admobAd.AdmobInitialized && admobAd.IsAppOpenAdReady)
            {
                admobAd.ShowAppOpenAd();
            }
        }
        #endregion AppOpen

        #region Application Paused
        private void OnApplicationPause(bool pauseStatus)
        {
            if (IsAd_Removed)
            {
                Debug.LogError("Remove Ads Is active: AppOpen");
                return; // Ads are disabled, exit the method
            }
            if (!ShowAppOpenInBackground)
            {
                //Debug.LogError("AppOpen Ad is not available for BG");
                return; // Ads are disabled, exit the method
            }

            // If this is the first time the app has opened, skip showing the AppOpen ad
            if (isFirstTimeOpen)
            {
                // Debug.Log("Skipping App Open ad because it's the first time the app has opened.");
                isFirstTimeOpen = false; // Mark that the first time has passed
                return;
            }

            if (!pauseStatus && !isADLoading && admobAd && admobAd.AdmobInitialized && admobAd.IsAppOpenAdReady)
            {
                // App has resumed from the background
                Debug.Log("App resumed from the background. Attempting to show App Open Ad.");
                admobAd.ShowAppOpenAd();
            }
        }
        #endregion Application Paused
        #endregion Show Methods

    }
}

    /***************************************************************************************************************************************
                                                             Extras
    //**************************************************************************************************************************************/

    #region Extras
   
    [Serializable]
    public enum Platform
    {
        None, Android, IOS
    }

    [Serializable]
    public enum AdsStatus
    {
        None,
        Ready,
        NotReady,
    }
    #endregion
