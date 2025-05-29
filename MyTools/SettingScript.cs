using MyTools;
using MyTools.SaveManager;
using UnityEngine;
using UnityEngine.UI;

public class SettingScript : MonoBehaviour
{
    public AddListeners addListeners;
    [Space]
    [SerializeField] private setting MusicSetting;
    [SerializeField] private setting SoundSetting;
    [SerializeField] private setting VibrationSetting;

    private void OnEnable()
    {
        UIManagerBase uIManager = FindFirstObjectByType<UIManagerBase>();
        addListeners.Init(uIManager);
    }

    private void Start()
    {
        SetAtStart();
        AddListerners();
    }

    void AddListerners()
    {
        //Music
        if (MusicSetting.OnBtn)
        {
            MusicSetting.OnBtn.onClick.RemoveAllListeners();
            MusicSetting.OnBtn.onClick.AddListener(() =>
            {
                AudioPlayer.instance.ToggleMusic(false);
                SetAtStart();
            });
        }
        if (MusicSetting.OffBtn)
        {
            MusicSetting.OffBtn.onClick.RemoveAllListeners();
            MusicSetting.OffBtn.onClick.AddListener(() => 
            {
                AudioPlayer.instance.ToggleMusic(true);
                SetAtStart();
            });
        }

        //Sound
        if (SoundSetting.OnBtn)
        {
            SoundSetting.OnBtn.onClick.RemoveAllListeners();
            SoundSetting.OnBtn.onClick.AddListener(() =>
            {
                AudioPlayer.instance.ToggleSound(false);
                SetAtStart();
            });
        }
        if (SoundSetting.OffBtn)
        {
            SoundSetting.OffBtn.onClick.RemoveAllListeners();
            SoundSetting.OffBtn.onClick.AddListener(() =>
            {
                AudioPlayer.instance.ToggleSound(true);
                SetAtStart();
            });
        }

        //Vibration
        if (VibrationSetting.OnBtn)
        {
            VibrationSetting.OnBtn.onClick.RemoveAllListeners();
            VibrationSetting.OnBtn.onClick.AddListener(() =>
            {
                AudioPlayer.instance.ToggleVibration(false);
                SetAtStart();
            });

        }
        if (VibrationSetting.OffBtn)
        {
            VibrationSetting.OffBtn.onClick.RemoveAllListeners();
            VibrationSetting.OffBtn.onClick.AddListener(() =>
            {
                AudioPlayer.instance.ToggleVibration(false);
                SetAtStart();
            });
        }
    }

    void SetAtStart()
    {
        bool checkMusic = SaveManager.Prefs.GetBool(SharedVariables.Music, true) == true;
        MusicSetting.OnBtn?.gameObject.SetActive(checkMusic);
        MusicSetting.OffBtn?.gameObject.SetActive(!checkMusic);

        bool checkSound = SaveManager.Prefs.GetBool(SharedVariables.Sound, true) == true;
        SoundSetting.OnBtn?.gameObject.SetActive(checkSound);
        SoundSetting.OffBtn?.gameObject.SetActive(!checkSound);

        bool checkVibration = SaveManager.Prefs.GetBool(SharedVariables.Vibration, true) == true;
        VibrationSetting.OnBtn?.gameObject.SetActive(checkVibration);
        VibrationSetting.OffBtn?.gameObject.SetActive(!checkVibration);
    }

}

[System.Serializable]
public struct setting
{
    public Button OnBtn;
    public Button OffBtn;
}
