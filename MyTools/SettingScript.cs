using RNA;
using RNA.LoadingManager;
using RNA.SaveManager;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingScript : MonoBehaviour
{
    [SerializeField] private setting MusicSetting;
    [SerializeField] private setting SoundSetting;
    [SerializeField] private setting VibrationSetting;

    [Space(7)]
    [SerializeField] private ButtonAction CloseBtn;
    [SerializeField] private ButtonAction HomeBtn;

    [SerializeField] private UIManagerBase _UIManager;


    // Start is called before the first frame update
    void Start()
    {
        SetAtStart();
        AddListerners();
       
        if (_UIManager == null)
        {
            _UIManager = FindObjectOfType<UIManagerBase>();
        }
    }


    void AddListerners()
    {
        //Music
        if (MusicSetting.OnBtn)
        {
            MusicSetting.OnBtn.onClick.RemoveAllListeners();
            MusicSetting.OnBtn.onClick.AddListener(() => SetMusic(false));
        }
        if (MusicSetting.OffBtn)
        {
            MusicSetting.OffBtn.onClick.RemoveAllListeners();
            MusicSetting.OffBtn.onClick.AddListener(() => SetMusic(true));
        }

        //Sound
        if (SoundSetting.OnBtn)
        {
            SoundSetting.OnBtn.onClick.RemoveAllListeners();
            //SoundSetting.OnBtn.onClick.AddListener(() => SetSound(false));
            SoundSetting.OnBtn.onClick.AddListener(() =>
                ToggleListener());
        }
        if (SoundSetting.OffBtn)
        {
            SoundSetting.OffBtn.onClick.RemoveAllListeners();
            //SoundSetting.OffBtn.onClick.AddListener(() => SetSound(true));
            SoundSetting.OffBtn.onClick.AddListener(() =>
                ToggleListener());
        }

        //Vibration
        if (VibrationSetting.OnBtn)
        {
            VibrationSetting.OnBtn.onClick.RemoveAllListeners();
            VibrationSetting.OnBtn.onClick.AddListener(() => SetVibration());
        }
        if (VibrationSetting.OffBtn)
        {
            VibrationSetting.OffBtn.onClick.RemoveAllListeners();
            VibrationSetting.OffBtn.onClick.AddListener(() => SetVibration());
        }

        if (CloseBtn.button)
        {
            CloseBtn.button.onClick.RemoveAllListeners();
            CloseBtn.button.onClick.AddListener(() =>
            {
                _UIManager.OnButtonClicked(CloseBtn);
            });
        }

        if (HomeBtn.button)
        {
            HomeBtn.button.onClick.RemoveAllListeners();
            HomeBtn.button.onClick.AddListener(() =>
            {
                _UIManager.OnButtonClicked(HomeBtn);
                //if (AdsManager.Instance)
                //    AdsManager.Instance.ShowInterAds(() =>
                //    {
                //        if (LoadingScript.Instance)
                //            LoadingScript.Instance.LoadingAsync(SceneManager.GetActiveScene().buildIndex - 1);
                //        else
                //            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
                //    });
            });
        }
    }



    void ToggleListener()
    {
        bool checkSound = SaveManager.Prefs.GetBool(SharedVariables.Sound, true) == true;
        SoundSetting.OnBtn.gameObject.SetActive(checkSound);
        SoundSetting.OffBtn.gameObject.SetActive(!checkSound);

        if (checkSound)
        {
            SaveManager.Prefs.SetBool(SharedVariables.Sound, false);
            //AudioPlayer.instance.StopMusic();
            //AudioListener.volume = 0;
        }
        else
        {
            SaveManager.Prefs.SetBool(SharedVariables.Sound, true);
            //AudioListener.volume = 1;
            //AudioPlayer.instance.PlayBGMusic();
        }
        SetAtStart();
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

    void SetMusic(bool check)
    {
        SaveManager.Prefs.SetBool(SharedVariables.Music, check == true ? true : false);
        if (check)
        {
            AudioPlayer.instance?.PlayOneShot(AudioType.Button);
            AudioPlayer.instance?.PlayBGMusic();
        }
        else
        {
            AudioPlayer.instance?.StopMusic();
        }

        SetAtStart();
    }

    void SetSound(bool check)
    {
        if (check)
            AudioPlayer.instance?.PlayOneShot(AudioType.Button);
        SaveManager.Prefs.SetBool(SharedVariables.Sound, check == true ? true : false);

        SetAtStart();
    }    

    void SetVibration()
    {
        bool check = SaveManager.Prefs.GetBool(SharedVariables.Vibration, true);
        if (!check)
           AudioPlayer.instance?.Vibrate();

        SaveManager.Prefs.SetBool(SharedVariables.Vibration, !check);

        SetAtStart();
    }

}

[System.Serializable]
public struct setting
{
    public Button OnBtn;
    public Button OffBtn;
}
