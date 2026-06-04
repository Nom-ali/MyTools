/// <summary>
/// This script is created using aDMOB 10.0.0 Version
/// </summary>
using System.Collections.Generic;
using GoogleMobileAds.Api;
using System.Collections;
using MyTools.SaveManager;
using UnityEngine;
using System;

public class IAdmobAD : MonoBehaviour
{
    #region Variables
    [SerializeField] private Platform m_Platform = Platform.Android;

    private bool IsAd_Removed => SaveManager.Prefs.GetBool(SharedVariables.RemoveAds, false) == true;

    [Header("----- Test Mode ----------------")]
    [SerializeField] private bool EnableTestIds = false;
    [SerializeField] AdPosition BannerPosition;
    [SerializeField] AdPosition BigBannerPosition;

    [Header("----- Admob -------------------")]
    [SerializeField] private bool ShowAppOpenAdOnLoad = false;
    [SerializeField] private bool ShowBannerOnLoad = false;

    [Tooltip("Don't Change or set , if you don't know")]
    private readonly AdmobIds AdmobAndroidTestingID = new()
    {
        BannerID = "ca-app-pub-3940256099942544/6300978111",
        MedBannerID = "ca-app-pub-3940256099942544/6300978111",
        InterID = "ca-app-pub-3940256099942544/1033173712",
        RewardedID = "ca-app-pub-3940256099942544/5224354917",
        AppOpenID = "ca-app-pub-3940256099942544/9257395921",
    };
    private readonly AdmobIds AdmobIOSTestingID = new()
    {
        BannerID = "ca-app-pub-3940256099942544/2934735716",
        MedBannerID = "ca-app-pub-3940256099942544/2934735716",
        InterID = "ca-app-pub-3940256099942544/4411468910",
        RewardedID = "ca-app-pub-3940256099942544/1712485313",
        AppOpenID = "ca-app-pub-3940256099942544/5575463023",
    };

    [Space(2)]
    [Header("----- AD Ids-------------------")]
    [SerializeField] AdmobIds AdmobAndroidLiveID;
    [SerializeField] AdmobIds AdmobIosLiveID;

    /// Private variables
    /// Don't change these if you don't know
    [HideInInspector] public bool AdmobInitialized = false;
    private BannerView bannerView;
    private BannerView medBannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    private AppOpenAd appOpenAd;

    /// Private variables
    /// Don't change these if you don't know
    private string BannerAdsID, MedBannerAdsID, InterstitialAdsID, RewardedAdsID, AppOpenAdsID;
    private bool isAdmobBanner = false;
    private bool isAdmobBannerMed = false;
    private bool m_ShowAppOpenAd = true;

    [SerializeField] private AdsStatus BannerStatus = AdsStatus.None;
    public AdsStatus CurrentBannerStatus { get { return BannerStatus; } set { BannerStatus = value; } }


    //Rewarded Ads Reward Call delegate
    private Action RewardAction;

    private int RewardretryAttempt = 0;
    private const int RewardmaxRetryAttempts = 3;
    #endregion

    #region Initialization
    public IEnumerator Initialize_Admob(AdSettings adSettings, Platform platform, bool showBannerOnLoad, bool appOpenOnLoad, bool Testing)
    {
        AdmobAndroidLiveID = adSettings.AdmobAndroidID;
        AdmobIosLiveID = adSettings.AdmobIosID;
        BannerPosition = adSettings.AdmobBannerPos;
        BigBannerPosition = adSettings.AdmobMedBannerPos;

        m_Platform = platform;
        EnableTestIds = Testing;
        ShowAppOpenAdOnLoad = appOpenOnLoad;
        ShowBannerOnLoad = showBannerOnLoad;

        // get IDs before initializing
        yield return Set_Ads_IDs_Before_Initializing();

        // Initialize the Google Mobile Ads SDK.
        yield return InitializeAdmob();
    }

    private IEnumerator InitializeAdmob()
    {
        Debug.Log("SDK: ADMOB, Initializing...");

        if (Application.platform == RuntimePlatform.IPhonePlayer)
            MobileAds.SetiOSAppPauseOnBackground(true);

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();
            foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
            {
                string className = keyValuePair.Key;
                AdapterStatus status = keyValuePair.Value;
                switch (status.InitializationState)
                {
                    case AdapterState.NotReady:
                        // The adapter initialization did not complete.
                        AdmobInitialized = false;
                        Debug.LogError("Admob: Adapter: " + className + " not ready.");
                        break;
                    
                    case AdapterState.Ready:
                        // The adapter was successfully initialized.
                        Debug.Log("Admob: Adapter: " + className + " is initialized.");

                        AdmobInitialized = true;

                        if (IsAd_Removed == false)
                        {
                            RequestAdmobBanner();
                            RequestAdmobMedBanner();
                            RequestInterstitial();
                        }

                        RequestAdmobAppOpenAds();
                        RequestAdmobRewardedAds();
                        break;
                }
            }
        });
        yield return null;
    }

    private IEnumerator Set_Ads_IDs_Before_Initializing()
    {
        // Initializing Admob IDs
        if (EnableTestIds)
        {
            if (m_Platform.Equals(Platform.Android))
            {
                BannerAdsID = AdmobAndroidTestingID.BannerID.Trim();
                MedBannerAdsID = AdmobAndroidTestingID.MedBannerID.Trim();
                InterstitialAdsID = AdmobAndroidTestingID.InterID.Trim();
                RewardedAdsID = AdmobAndroidTestingID.RewardedID.Trim();
                AppOpenAdsID = AdmobAndroidTestingID.AppOpenID.Trim();
            }
            else if (m_Platform.Equals(Platform.IOS))
            {
                BannerAdsID = AdmobIOSTestingID.BannerID.Trim();
                MedBannerAdsID = AdmobIOSTestingID.MedBannerID.Trim();
                InterstitialAdsID = AdmobIOSTestingID.InterID.Trim();
                RewardedAdsID = AdmobIOSTestingID.RewardedID.Trim();
                AppOpenAdsID = AdmobIOSTestingID.AppOpenID.Trim();
            }
        }
        else
        {
            if (m_Platform.Equals(Platform.Android))
            {
                BannerAdsID = AdmobAndroidLiveID.BannerID.Trim();
                MedBannerAdsID = AdmobAndroidLiveID.MedBannerID.Trim();
                InterstitialAdsID = AdmobAndroidLiveID.InterID.Trim();
                RewardedAdsID = AdmobAndroidLiveID.RewardedID.Trim();
                AppOpenAdsID = AdmobAndroidLiveID.AppOpenID.Trim();
            }
            else if (m_Platform.Equals(Platform.IOS))
            {
                BannerAdsID = AdmobIosLiveID.BannerID.Trim();
                MedBannerAdsID = AdmobIosLiveID.MedBannerID.Trim();
                InterstitialAdsID = AdmobIosLiveID.InterID.Trim();
                RewardedAdsID = AdmobIosLiveID.RewardedID.Trim();
                AppOpenAdsID = AdmobIosLiveID.AppOpenID.Trim();
            }
        }
        yield return null;
    }

    #endregion Initialization
    
    /***************************************************************************************************************************************
                                                             Admob Section
    //**************************************************************************************************************************************/

    #region ADMOB Section

    #region Admob Banner

    private void RequestAdmobBanner()
    {
        if (IsAd_Removed == true)
            return;

        // Requesting Banner ADS
        Debug.Log("Admob: Requesting Banner with ID: " + BannerAdsID);

        // If the banner is already running then destroy it first and call again
        bannerView?.Destroy();

        bannerView = new BannerView(BannerAdsID, AdSize.Banner, BannerPosition);

        // Called when an ad request has successfully loaded.
        bannerView.OnBannerAdLoaded += () =>
        {
            BannerStatus = AdsStatus.Ready;
            isAdmobBanner = true; // Set banner flag
           
            if(ShowBannerOnLoad)
                AdsManager.Instance.ShowBanner();
           
            Debug.Log("Admob: Banner ad loaded.");
        };

        // Called when an ad request failed to load.
        bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            BannerStatus = AdsStatus.NotReady;
            isAdmobBanner = false;
            Debug.Log("Admob: Banner ads failed to load: " + error.GetMessage());
        };

        // Called when an ad is clicked.
        bannerView.OnAdClicked += () =>
        {
            Debug.Log("Admob: Banner Ads Clicked");
        };

        // Create an empty ad request.
        // Load the banner with the request.
        AdRequest request = new();
        bannerView.LoadAd(request);
        bannerView.Hide();
    }

    private void RequestAdmobMedBanner()
    {
        if (IsAd_Removed == true)
            return;

        // Requesting Medium Rectangle Banner ADS
        Debug.Log("Admob: Requesting Medium Rectangle Banner with ID: " + MedBannerAdsID);

        // If the medium banner is already running then destroy it first and call again
        medBannerView?.Destroy();

        medBannerView = new BannerView(MedBannerAdsID, AdSize.MediumRectangle, BigBannerPosition);

        // Called when an ad request has successfully loaded.
        medBannerView.OnBannerAdLoaded += () =>
        {
            isAdmobBannerMed = true; // Set medium banner flag
            Debug.Log("Admob: Medium Rectangle Banner ad loaded.");
        };

        // Called when an ad request failed to load.
        medBannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.Log("Admob: Medium Rectangle Banner ads failed to load: " + error.GetMessage());
        };

        // Called when an ad is clicked.
        medBannerView.OnAdClicked += () =>
        {
            Debug.Log("Admob: Medium Rectangle Banner Ads Clicked");
        };

        // Create an empty ad request.
        AdRequest request = new();
        // Load the medium banner with the request.
        medBannerView.LoadAd(request);
        medBannerView.Hide();
    }

    /// <summary>
    /// Show admob banner ads
    /// </summary>
    public void ShowAdmobBanner()
    {
        Debug.Log("Admob: Showing Banner");
        if (isAdmobBanner) // Check if banner is loaded
        {
            bannerView?.Show();
        }
        else
        {
            Debug.LogError("Admob: Banner is not ready yet.");
        }
    }

    /// <summary>
    /// Show admob MediumRectangle banner ad
    /// </summary>
    public void ShowMedAdmobBanner()
    {
        Debug.Log("Admob: Showing Medium Rectangle Banner");
        if (isAdmobBannerMed) // Check if medium banner is loaded
        {
            medBannerView?.Show();
        }
        else
        {
            Debug.LogError("Admob: Medium Rectangle Banner is not ready yet.");
        }
    }
   
    public void AdmobBannerHide()
    {
        Debug.Log("Admob: Hiding Banner");
        if (bannerView != null)
        {
            bannerView.Hide(); // Hides the banner
        }
        else
        {
            Debug.LogError("Admob: Banner view is null, cannot hide.");
        }
    }

    public void DestroyBanner()
    {
        Debug.Log("Admob: Destroying Banner");
        if (bannerView != null)
        {
            bannerView.Destroy(); // Destroys the banner
            bannerView = null; // Set banner view to null
            isAdmobBanner = false; // Set banner flag to false
            BannerStatus = AdsStatus.NotReady;


        }
        else
        {
            Debug.LogError("Admob: Banner view is null, cannot destroy.");
        }
    }

    /// <summary>
    /// Hides the AdMob Medium Rectangle Banner ad.
    /// </summary>
    public void AdmobMedBannerHide()
    {
        Debug.Log("Admob: Hiding Medium Rectangle Banner");
        if (medBannerView != null)
        {
            medBannerView.Hide(); // Hides the medium rectangle banner
        }
        else
        {
            Debug.LogError("Admob: Medium Rectangle Banner view is null, cannot hide.");
        }
    }

    public void DestroyMedBanner()
    {
        Debug.Log("Admob: Destroying Medium Rectangle Banner");
        if (medBannerView != null)
        {
            medBannerView.Destroy(); // Destroys the medium rectangle banner
            medBannerView = null; // Set medium rectangle banner view to null
            isAdmobBannerMed = false; // Set medium banner flag to false
        }
        else
        {
            Debug.LogError("Admob: Medium Rectangle Banner view is null, cannot destroy.");
        }
    }
    #endregion

    #region Admob Interstitial
    private void RequestInterstitial()
    {
        if (IsAd_Removed == true)
            return;

        Debug.Log("Admob: Requesting Interstitial Ads with ID: " + InterstitialAdsID);

        this.interstitialAd?.Destroy();

        //Create an empty ad request.
        AdRequest request = new();
        // Load the interstitial with the request.
        InterstitialAd.Load(InterstitialAdsID, request, (InterstitialAd InterAds, LoadAdError error) =>
        {
            if (error != null || InterAds == null)
            {
                Debug.LogError("Admob: Interstitial Ads Failed to load: " + error);
                return;
            }
            else
            {
                Debug.Log("Admob: Interstitial Ads loaded with response: " + InterAds.GetResponseInfo());
                this.interstitialAd = InterAds;
            }

            //ad callbackss
            InterAds.OnAdFullScreenContentOpened += () =>
            {
                //showing loading ads popup
                // Delegates.ShowLoadingAdsPopup?.Invoke(false);
                Debug.Log("Admob: Interstitial ad opening.");
            };
            InterAds.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Admob: Interstitial ad closed.");
                this.RequestInterstitial();
            };
            InterAds.OnAdClicked += () =>
            {
                Debug.Log("Admob: Interstitial ad recorded a click.");
            };
            InterAds.OnAdFullScreenContentFailed += (AdError error) =>
            {
                //showing loading ads popup
                //   Delegates.ShowLoadingAdsPopup?.Invoke(false);
                Debug.Log("Admob: Interstitial ad failed to show with error: " +
                            error.GetMessage());
            };
        });
    }


    /// <summary>
    /// show admob interstitial ads
    /// </summary>    
    public void ShowAdmobInterAds()
    {
        Debug.Log("Admob: Showing Admob Inter Ads");
        if (interstitialAd.CanShowAd())
            interstitialAd.Show();
        else
        {
            //showing loading ads popup
            // Delegates.ShowLoadingAdsPopup?.Invoke(false);
            Debug.LogError("Admob: Interstitial Ads not ready");
        }
    }

    public void DestroyInterAds()
    {
        Debug.Log("Admob: Destroying Interstitial Ads");
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }
        else
        {
            Debug.LogError("Admob: Interstitial ad is null, cannot destroy.");
        }
    }

    #endregion

    #region Admob Rewarded
    private void RequestAdmobRewardedAds()
    {
        // Check if retry limit is reached
        if (RewardretryAttempt >= RewardmaxRetryAttempts)
        {
            Debug.LogError("Admob: Max retry attempts reached. Stopping ad requests.");
            return; // Exit if max attempts reached
        }

        // Requesting Rewarded ADS
        Debug.Log("Admob: Requesting Rewarded Ads with ID: " + RewardedAdsID);

        this.rewardedAd?.Destroy();

        // Create an empty ad request.
        AdRequest request = new();

        // Load the rewarded ad with the request.
        RewardedAd.Load(RewardedAdsID, request, (RewardedAd RewardedAds, LoadAdError error) =>
        {
            if (error != null || RewardedAds == null)
            {
                Debug.Log("Admob: Rewarded Ads failed to load with response: " + error.GetResponseInfo());
                // Increment retry attempt count
                RewardretryAttempt++; 
                if (RewardretryAttempt < RewardmaxRetryAttempts)
                {
                    // Retry requesting rewarded ads
                    this.RequestAdmobRewardedAds();
                }
                return;
            }
            else
            {
                Debug.Log("Admob: Rewarded Ads loaded successfully with response: " + RewardedAds.GetResponseInfo());
                this.rewardedAd = RewardedAds;
                RewardretryAttempt = 0; // Reset retry count on success
            }

            // ads callbacks
            RewardedAds.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Admob: Rewarded ad opening.");
            };
            RewardedAds.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Admob: Rewarded ad closed.");
                // Reloading rewarded ads again
                this.RequestAdmobRewardedAds();
            };
            RewardedAds.OnAdClicked += () =>
            {
                Debug.Log("Admob: Rewarded ad recorded a click.");
            };
            RewardedAds.OnAdFullScreenContentFailed += (AdError adError) =>
            {
                Debug.LogError("Admob: Rewarded ad failed to show. Error: " + adError.GetMessage());
                // Retry requesting rewarded ads again
                this.RequestAdmobRewardedAds();
            };
        });         
    }

    /// <summary>
    /// show admob rewarded ads
    /// </summary>
    public void ShowAdmobRewardedAds(Action action)
    {
        Debug.Log("Admob: Showing Admob Rewarded Video Ads");
        RewardAction = action;
        if (rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log("Admob: Rewarded ad granted a reward.");

                //todo grant a reward here
                //Giving reward here if you have assigned method in show rewarded ads as a parameter
                if (RewardAction != null)
                    RewardAction?.Invoke();
                else Debug.LogError("Admob: Rewarded Action missing");
            });
        }
        else
        {
            Debug.LogError("Admob: Rewarded ad is not ready yet.");
            RewardretryAttempt = 0;
            this.RequestAdmobRewardedAds();
        }
    }

    public void DestroyRewardedAds()
    {
        Debug.Log("Admob: Destroying Rewarded Ads");
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
        else
        {
            Debug.LogError("Admob: Rewarded ad is null, cannot destroy.");
        }
    }

    #endregion ADMOB REWARDED

    #region ADMON AppOpen ADS
    void RequestAdmobAppOpenAds()
    {
        Debug.Log("Admob: Requesting AppOpen Ads with ID: " + AppOpenAdsID);


        // destroy ads if not empty
        appOpenAd?.Destroy();

        // creating new request
        AdRequest request = new();

        //loading ads
        AppOpenAd.Load(AppOpenAdsID, request,
            (AppOpenAd ad, LoadAdError loadError) =>
            {
                // if ad failed to load
                if (loadError != null)
                {
                    Debug.LogError("Admob: AppOpen ad failed to load with error: " +
                        loadError.GetMessage());
                    return;
                }
                else if (ad == null)
                {
                    Debug.LogError("Admob: AppOpen ad failed to load.");
                    return;
                }
                else
                {
                    this.appOpenAd = ad;
                    m_ShowAppOpenAd = true;

                    if (ShowAppOpenAdOnLoad)
                    {
                        ShowAppOpenAd();
                        ShowAppOpenAdOnLoad = false;
                    }

                    Debug.Log("Admob: AppOpen ad loaded."); // ad loaded
                }

                //this.appOpenExpireTime = DateTime.Now + APPOPEN_TIMEOUT;

                // ad callbacks
                ad.OnAdFullScreenContentOpened += () =>
                {
                    Debug.Log("Admob: App open ad opened.");
                    //ShowAppOpenAdsTrueFalse(false);
                };
                ad.OnAdFullScreenContentClosed += () =>
                {
                    //ShowAppOpenAdsTrueFalse(false);
                    Debug.Log("Admob: App open ad closed.");
                    //Fixme remove this if you want to run this ONCE
                    this.RequestAdmobAppOpenAds();
                };
                ad.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    Debug.LogError("Admob: App open ad failed to show with error: " +
                        error.GetMessage());
                };
            });
    }

    public void DestroyAppOpenAd()
    {
        Debug.Log("Admob: Destroying AppOpen Ads");
        if (appOpenAd != null)
        {
            appOpenAd.Destroy();
            appOpenAd = null;
        }
        else
        {
            Debug.LogError("Admob: AppOpen ad is null, cannot destroy.");
        }
    }
  
    public void ShowAppOpenAd()
    {
        Debug.Log("Admob: Showing AppOpen ads Ads");
        if (m_ShowAppOpenAd && appOpenAd.CanShowAd())
        {
            appOpenAd.Show();
            m_ShowAppOpenAd = false;
        }
        else
            RequestAdmobAppOpenAds();
    }

    //public void ShowAppOpenAdsTrueFalse(bool IsTrue)
    //{
    //    m_ShowAppOpenAd = IsTrue;
    //    Debug.Log("Show appOpenOnLoad Ads True False: " + IsTrue);
    //}
    #endregion

    #endregion ADMOB

    #region AD Avalibality Check

    // method for checking if banner ad is ready
    public bool IsBannerReady => isAdmobBanner;

    // method for checking if Big banner ad is ready
    public bool IsMedBannerReady => isAdmobBannerMed;

    // method for checking if Interstitial ad is ready
    public bool IsInterstitialReady()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            return true;
        }
        else
        {
            RequestInterstitial();
            return false;
        }
    }

    public bool IsRewardedAdReady()// method for checking if RewardedAd ad is ready
    {
        if(rewardedAd != null && rewardedAd.CanShowAd())
        {
            return true;
        }
        else
        {
            RequestAdmobRewardedAds();
            return false;
        }
    }

    // method for checking if appOpenOnLoad ad is ready
    public bool IsAppOpenAdReady => appOpenAd != null ? appOpenAd.CanShowAd() : false;
    
    #endregion
}

