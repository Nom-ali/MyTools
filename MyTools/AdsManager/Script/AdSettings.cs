using GoogleMobileAds.Api;
using System;
using UnityEngine;
using UnityEngine.Advertisements;

[CreateAssetMenu(fileName = "AdSettings", menuName = "Ads/AdSettings", order = 1)]
public class AdSettings : ScriptableObject
{
    [Header("********** Admob ********** ")]
    [SerializeField] private AdPosition Admob_BannerPosition = AdPosition.Top;
    [SerializeField] private AdPosition Admob_MedBannerPosition = AdPosition.Center;
    [SerializeField] private AdmobIds AdmobAndroidLiveID;
    [SerializeField] private AdmobIds AdmobIosLiveID;

    [Header("********** Applovin Settings ********** ")]
    [SerializeField] private string MaxSdkKey = "VMMPumfMteM7GKKd4xAspNa3QUHqX9HcKbzaBf4cHwqSLvoZgFznUV5_UzncZasQwb7FZL3TlOEfz2OJkbEmyt";
    [SerializeField] private MaxSdkBase.BannerPosition ApplovinBannerPosition = MaxSdkBase.BannerPosition.TopCenter;
    [SerializeField] private MaxSdkBase.AdViewPosition Applovin_BigBannerPosition = MaxSdkBase.AdViewPosition.Centered;
    [SerializeField] private ApplovinIds ApplovinAndroidLiveID;
    [SerializeField] private ApplovinIds ApplovinIosLiveID;

    [Header("********** Unity Settings ********** ")]
    [SerializeField] private string UnityID_Adroid;
    [SerializeField] private string UnityID_iOS;
    private BannerPosition UnityBannerPosition = BannerPosition.TOP_CENTER;


    //Admob ADs Setting
    public AdPosition AdmobBannerPos => Admob_BannerPosition;
    public AdPosition AdmobMedBannerPos => Admob_MedBannerPosition;
    public   AdmobIds AdmobAndroidID => AdmobAndroidLiveID;
    public   AdmobIds AdmobIosID => AdmobIosLiveID;

    //AppLoving ADs Setting
    public string MaxSDKKey => MaxSdkKey;
    public MaxSdkBase.BannerPosition ApplovinBannerPos => ApplovinBannerPosition;
    public MaxSdkBase.AdViewPosition ApplovinBigBannerPos => Applovin_BigBannerPosition;
    public ApplovinIds ApplovinAndroidID => ApplovinAndroidLiveID;
    public ApplovinIds ApplovinIosID => ApplovinIosLiveID;

    // Unity ADs Setting
    public string UnityAndroidID => UnityID_Adroid;
    public string UnityIosID =>  UnityID_iOS;
    public BannerPosition UnityBannerPos =>  UnityBannerPosition;
}


[Serializable]
public struct AdmobIds
{
    public string BannerID;
    public string MedBannerID;
    public string InterID;
    public string RewardedID;
    public string AppOpenID;
}

[Serializable]
public struct ApplovinIds
{
    public string BannerAdUnitId;
    public string MRecAdUnitId;
    public string InterstitialAdUnitId;
    public string RewardedAdUnitId;
}