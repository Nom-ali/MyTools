/// <summary>
/// This script is created using these SDK versions
/// unity Ads SDK 4.12.0
/// </summary>


using RNA.SaveManager;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public class IUnityAD : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    private bool IsAd_Removed => SaveManager.Prefs.GetBool(SharedVariables.RemoveAds, false) == true;

    #region Variables

    public bool EnableTestIds;
    public bool DoShowUnityBanner;
    [SerializeField] private Platform m_Platform = Platform.Android;
   
    [Header("----- Unity Ads ----------------")]
    [SerializeField] string UnityID_Adroid;
    [SerializeField] string UnityID_iOS;
    public BannerPosition bannerPositionUnity = BannerPosition.TOP_CENTER;

    //Unity Section
    private string unityGameId;
    private string BannerPlacementID;
    private string InterPlacementID;
    private string RewardedPlacementID;
    private bool unityInterAdsLoaded = false;
    private bool unityRewardAdsLoaded = false;
    private bool unityBannerAdsLoaded = false;

    #endregion

    //Rewarded Ads Reward Call delegate
    private Action RewardAction;

    IEnumerator Set_Ads_IDs_Before_Initializing()
    { 
        if (m_Platform.Equals(Platform.Android))
        {
            unityGameId = UnityID_Adroid;
            BannerPlacementID = "Banner_Android";
            InterPlacementID = "Interstitial_Android";
            RewardedPlacementID = "Rewarded_Android";
        }
        else if (m_Platform.Equals(Platform.IOS))
        {
            unityGameId = UnityID_iOS;
            BannerPlacementID = "Banner_iOS";
            InterPlacementID = "Interstitial_iOS";
            RewardedPlacementID = "Rewarded_iOS";
        }

        Debug.Log("Unity Ads Game ID: " + unityGameId);
        yield return null;
    }

    public IEnumerator InitializeUnity()
    {
        Debug.Log("SDK: UNITY, Initializing ...");
        Debug.Log("Unity: Ads ID: " + unityGameId);
        Advertisement.Initialize(unityGameId, EnableTestIds, this);
        yield return null;
    }

    public IEnumerator InitializeUnity_ID(AdSettings adSettings,Platform platform, bool testing)
    {
        this.UnityID_Adroid = adSettings.UnityAndroidID;
        this.UnityID_iOS = adSettings.UnityIosID;
        this.bannerPositionUnity = adSettings.UnityBannerPos;

        m_Platform = platform;
        EnableTestIds = testing;

        yield return Set_Ads_IDs_Before_Initializing();

        yield return InitializeUnity();

        yield return null;
    }
    /***************************************************************************************************************************************
                                                           Unity Section
   //**************************************************************************************************************************************/

    #region UNITY ADS
    /// <summary>
    /// loading unity banner
    /// </summary>
    private void LoadUnityBanner()
    {
        Debug.Log(" UnityAds: banner Loading");
        Advertisement.Load(BannerPlacementID, this);
    }


    /// <summary>
    /// loading unity interstitial ads
    /// </summary>
    private void LoadUnityInter()
    {

        Debug.Log(" UnityAds: inter Loading");
        Advertisement.Load(InterPlacementID, this);
    }


    /// <summary>
    /// loading unity rewarded ads 
    /// </summary>
    private void LoadUnityRewarded()
    {
        Debug.Log(" UnityAds: rewarded Loading");
        Advertisement.Load(RewardedPlacementID, this);
        //CustomAnalytics.LogEvent("Reward: UNITY, Ad Unit SENT");
    }


    /// <summary>
    /// show unity banner
    /// </summary>

    [ContextMenu("Show Banner")]
    public void ShowUnityBanner()
    {
        if (Advertisement.isInitialized)
        {
            Debug.Log(" UnityAds: banner show");
            BannerOptions bannerOptions = new BannerOptions();
            bannerOptions.showCallback += () => { DoShowUnityBanner = true; };
            bannerOptions.hideCallback += () => { DoShowUnityBanner = false; };
            Advertisement.Banner.SetPosition(bannerPositionUnity);
            Advertisement.Banner.Show(BannerPlacementID, bannerOptions);
        }
    }


    /// <summary>
    /// show unity interstitial ads
    /// </summary>
    [ContextMenu("Show Inter")]
    public void ShowUnityInter()
    {
        //showing loading ads popup
        // Delegates.ShowLoadingAdsPopup?.Invoke(false);
        Debug.Log(" UnityAds: inter show");
        if (Advertisement.isInitialized)
        {
            Advertisement.Show(InterPlacementID, this);
        }
    }


    /// <summary>
    /// show unity rewarded ads
    /// </summary>
    [ContextMenu("Show Rewarded")]
    public void ShowUnityRewarded(Action action)
    {
        //showing loading ads popup
        //  Delegates.ShowLoadingAdsPopup?.Invoke(false);
        Debug.Log(" UnityAds: rewarded show");
        if (Advertisement.isInitialized)
        {
            RewardAction = action;
            Advertisement.Show(RewardedPlacementID, this);
        }
    }


    /// <summary>
    /// hide unity banner
    /// </summary>
    [ContextMenu("Show BannerClose")]
    public void UnityBannerClose()
    {
        Debug.Log(" UnityAds: banner hiding");
        Advertisement.Banner.Hide();
    }

    #endregion UNITY ADS


    /***************************************************************************************************************************************
                                                           Unity CallBacks Methods
   //**************************************************************************************************************************************/
    #region Unity Callbacks events
    /// <summary>
    /// Unity ads callbak
    /// </summary>
    public void OnInitializationComplete()
    {
        Debug.Log(" Unity Ads initialization complete.");
        if (Advertisement.isInitialized)
        {
            Debug.Log(" Loading Unity Ads");
            //LoadUnityBanner();

            if (IsAd_Removed == false)
            {
                LoadUnityInter();
            }
            LoadUnityRewarded();
        }
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError(" Unity Ads Initialization Failed. \nError: " + error + "\n Message: " + message);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log(" UnityAds: Ads Loaded: " + placementId);
        if (placementId.Equals(InterPlacementID))
            unityInterAdsLoaded = true;
        else if (placementId.Equals(RewardedPlacementID))
        {
            unityRewardAdsLoaded = true;
            //CustomAnalytics.LogEvent("Reward: UNITY, Ad Unit LOADED");
        }
        else if (placementId.Equals(BannerPlacementID))
            unityBannerAdsLoaded = true;
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError(" Unity Ads failed to Load: " + placementId);
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log(" Unity Ads failed to show: " + placementId);
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log(" Unity Ads show start: " + placementId);
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log(" Unity Ads show clicked: " + placementId);
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log(" Unity Ads show Completed: " + placementId);
        if (placementId.Equals(InterPlacementID) && IsAd_Removed == false)
        {
            /// after completion reload inter ads here
            LoadUnityInter();
        }
        else if (placementId.Equals(RewardedPlacementID))
        {
            Debug.Log(" Rewarded ads completed: Assigning reward.");
            if (showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
                if (RewardAction != null)
                {
                    //Giving reward here if you have assigned method in show rewarded ads as a parameter
                    RewardAction?.Invoke();
                }
                else Debug.LogError(" Rewarded Action missing");
            unityRewardAdsLoaded = false;
            // reloading rewarded ads here
            LoadUnityRewarded();
        }
    }
    #endregion callback

    /***************************************************************************************************************************************
                                                          Unity AD Check Methods
  //**************************************************************************************************************************************/

    #region AD Avalibality Check
    public bool IsBannerReady() // method for checking if banner ad is ready
    {
        return unityBannerAdsLoaded;
    }
    public bool IsInterstitialReady()// method for checking if Interstitial ad is ready
    {
        return unityInterAdsLoaded;
    }
    public bool IsRewardedAdReady()// method for checking if RewardedAd ad is ready
    {
        return unityRewardAdsLoaded;
    }

    #endregion
}
