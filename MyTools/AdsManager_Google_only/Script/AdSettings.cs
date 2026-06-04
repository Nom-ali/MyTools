using GoogleMobileAds.Api;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AdSettings", menuName = "Ads/AdSettings", order = 1)]
public class AdSettings : ScriptableObject
{
    [Header("********** Admob ********** ")]
    [SerializeField] private AdPosition Admob_MedBannerPosition = AdPosition.Center;
    [SerializeField] private AdmobIds AdmobAndroidLiveID;
    [SerializeField] private AdmobIds AdmobIosLiveID;
    
    [Header("Usage Tracking Description")]
    [SerializeField] private string userTrackingUsageDescription = "Allow tracking to help us show ads that match your interests and keep the game free for everyone.";
    
    //Admob ADs Setting
    public AdPosition AdmobMedBannerPos => Admob_MedBannerPosition;
    public   AdmobIds AdmobAndroidID => AdmobAndroidLiveID;
    public   AdmobIds AdmobIosID => AdmobIosLiveID;
    public string UserTrackingUsageDescription => userTrackingUsageDescription;
    
}

[Serializable]
public struct AdmobBannerSetting
{
    public string BannerID;
    public AdPosition bannerPosition;
}

[Serializable]
public struct AdmobIds
{
    public string AppID;
    
    
    [Space]
    public string BigBannerID;
    public string InterID;
    public string RewardedID;
    public string AppOpenID;
    public AdmobBannerSetting[] BannerID;
}
