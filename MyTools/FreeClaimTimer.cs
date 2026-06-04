using System;
using UnityEngine;

public static class FreeClaimTimer
{
    private const string FreeClaimTimeKey = "FREE_CLAIM_READY_TIME";

    public static bool IsFreeClaimReady(string key)
    {
        string _key = FreeClaimTimeKey + key;
        if (!PlayerPrefs.HasKey(_key))
            return true; // first time, claim is available

        string savedTime = PlayerPrefs.GetString(_key, "");

        if (string.IsNullOrEmpty(savedTime))
            return true;

        if (!DateTime.TryParse(savedTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime readyTime))
            return true;

        return DateTime.UtcNow >= readyTime;
    }

    public static void StartFreeClaimCooldown(string key, int cooldownMinutes)
    {
        string _key = FreeClaimTimeKey + key;
        DateTime readyTime = DateTime.UtcNow.AddMinutes(cooldownMinutes);
        PlayerPrefs.SetString(_key, readyTime.ToString("o"));
        PlayerPrefs.Save();
    }

    public static TimeSpan GetRemainingTime(string key)
    {
        if (IsFreeClaimReady(key))
            return TimeSpan.Zero;


        string _key = FreeClaimTimeKey + key;
        string savedTime = PlayerPrefs.GetString(_key, "");
        DateTime readyTime = DateTime.Parse(savedTime, null, System.Globalization.DateTimeStyles.RoundtripKind);

        return readyTime - DateTime.UtcNow;
    }
}