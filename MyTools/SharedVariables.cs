using UnityEngine;

public class SharedVariables
{

    public static string Tutorial_1 = "Tutorial_1";
    public static string Tutorial_2 = "Tutorial_2";
    public static string DefaultSetting = "DefaultSetting";

    public static string Skin = "Skin_";
    public static string SkinMale = "SkinMale_";
    public static string SkinFemale = "SkinFemale_";
    public static string SelectedSkin = "SelectedSkin_";

    public static string FreeMode = "FreeMode";
    public static string ModeIndex = "ModeIndex";

    public static string Level_ = "Level_";
    public static string RepeatingLevels = "RepeatingLevels";
    public static string UnlockedLevels = "UnlockedLevels";
    public static string CurrentLevelNo = "CurrentLevelNo";
    public static string RandomLevel = "RandomLevel";
    public static string GameCompleted = "GameCompleted";
    public static string LastPlayedLevel = "LastPlayedLevel";
    public static string BoltPopup = "BoltPopup";
    public static string Hammerpopup = "Hammerpopup";

    public static string Coins = "Coins";
    public static string HammerAmount = "HammerAmount";

    public static string Music = "Music";
    public static string Sound = "Sound";
    public static string Vibration = "Vibration";
    
    public static string RemoveAds = "RemoveAds";

    public static string WarningPopup = "WarningPopup";

    public static string SelectedPlayer = "SelectedPlayer_";
    public static string Player_Female = "Player_Female";

    public static string MainMenu = "2_MainMenu";
    public static string Gameplay = "3_Gameplay";

    public static string Revive = "Revive";
    public static string ReviveChances = "ReviveChances_";

    public static string NearestSpawnPoint = "NearestSpawnPoint_";

    /***************************************************************************************************************************************
                                                                Links
    //**************************************************************************************************************************************/
    public static string IOSRateUsLink = "https://apps.apple.com/us/app/seat-bus-jam-tap-away-car-out/id6711346404";
    public static string AndroidRateUsLink = "";
    
    public static string AndroidMoreGamesLink = "";
    public static string IOSMoreGamesLink = "https://apps.apple.com/us/developer/shamaila-qadeer/id1765671504";
    
    public static string PrivacyPolicyLink = "https://shehlaqadeer0.blogspot.com/2024/08/privacy-policy.html";


    public enum Tags
    {
        None, Screw, Body,
        Player,
        DropPoint,
    }

    public static Color GetColor(ColorType colorType)
    {
        return colorType switch
        {
            ColorType.Red => Color.red,
            ColorType.Yellow => Color.yellow,
            ColorType.Blue => Color.blue,
            ColorType.Green => Color.green,
            ColorType.Purple => GetColorFromHex("#A020F0"),
            ColorType.Pink => new Color(252f / 255f, 15f / 255f, 192f / 255f), // Adjusted values
            ColorType.Orange => new Color(0.9568628f, 0.4392157f, 0.2470588f), // Adjusted values
            ColorType.Gray => Color.gray, // Adjusted values
            ColorType.LightBlue => new Color(0.372549f, 1f, 0.8862746f),
            ColorType.Parrot => new Color(0.7098039f, 1f, 0.2509804f),
            _ => Color.white,
        }; ;
    }

    public static Color GetColorFromHex(string hex)
    {
        Color color = Color.white;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }

}

[System.Serializable]
public enum ColorType
{
    None, Yellow, Green, Blue, Red, Purple, Pink, Orange, Gray, LightBlue, Parrot
}


