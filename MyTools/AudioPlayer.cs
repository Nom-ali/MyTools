//using Lofelt.NiceVibrations;
using RNA.SaveManager;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer instance;

    public bool PlayMusicAtStart = true;
    public float InitialDelay = 0;
    public MusicAssets BGMusicSetting;
    public AudioAssets SoundSetting;

    //private float SavedMusicValue => SaveManager.Prefs.GetFloat(SharedVariables.Music, BGMusicSetting.Volume);
    //private float SavedSoundValue => SaveManager.Prefs.GetFloat(SharedVariables.Sound, SoundSetting.Volume);

    #region Instance
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion


    private IEnumerator Start()
    {
        
        if (SaveManager.Prefs.GetBool(SharedVariables.DefaultSetting, false) == false)
            yield return SetDefaultControls();

        //BGMusicSetting.Volume = SavedMusicValue;
        //SoundSetting.Volume = SavedSoundValue;

        //bool checkSound = SaveManager.Prefs.GetBool(SharedVariables.Sound, true, true);
        //if (checkSound)
        //{
        //    SaveManager.Prefs.SetBool(SharedVariables.Sound, true);
        //    AudioListener.volume = 1;
        //}
        //else
        //{
        //    AudioListener.volume = 0;
        //    yield break;
        //}

        if (PlayMusicAtStart)
        {
            yield return new WaitForSeconds(InitialDelay);
            PlayBGMusic();
        }
    }

    IEnumerator SetDefaultControls()
    {
        var firstCheck = SaveManager.Prefs.GetBool(SharedVariables.DefaultSetting, false);
        if (firstCheck)
        {
            //SaveManager.Prefs.SetFloat(SharedVariables.Music, BGMusicSetting.Volume);
            //SaveManager.Prefs.SetFloat(SharedVariables.Sound, SoundSetting.Volume);  
            
            SaveManager.Prefs.SetBool(SharedVariables.Music, true);
            SaveManager.Prefs.SetBool(SharedVariables.Sound, true);
            SaveManager.Prefs.GetBool(SharedVariables.DefaultSetting, true);
        }
        yield return null;
    }

    public void PlayOneShot(AudioType type)
    {
        if (SaveManager.Prefs.GetBool(SharedVariables.Sound, true) == false)
            return;

        if (SoundSetting.ClipsList.Length <= 0)
        {
            Debug.LogError("Sound Error Found");
            return;
        }

        //Finding Clip from array of given type
        int SoundIndex = System.Array.FindIndex(SoundSetting.ClipsList, item => item.type.Equals(type));
        if (SoundIndex < 0)
            return;

        if (SoundSetting.audioSource == null)
            SoundSetting.audioSource = CreateSource("OneShot");

        SoundSetting.audioSource.volume = SoundSetting.Volume;
        int ClipIndex = SoundSetting.ClipsList[SoundIndex].clip.Length > 0 ? Random.Range(0, SoundSetting.ClipsList[SoundIndex].clip.Length) : 0;
        SoundSetting.audioSource.PlayOneShot(SoundSetting.ClipsList[SoundIndex].clip[ClipIndex]);
    }


    public void PlayBGMusic()
    {
        if (SaveManager.Prefs.GetBool(SharedVariables.Music, true) == false)
            return;

        if (BGMusicSetting.audioSource == null)
            BGMusicSetting.audioSource = CreateSource("BG_Music");

        if (BGMusicSetting.audioSource == null 
            || BGMusicSetting.audioSource.isPlaying 
            || BGMusicSetting.ClipsList.Length <= 0)
            return;

        int index = 0;
        //Play random Clip file from list
        if (BGMusicSetting.ClipsList.Length > 0)
            index = Random.Range(0, BGMusicSetting.ClipsList.Length);

        if (BGMusicSetting.ClipsList[index] == null)
            return;


        BGMusicSetting.audioSource.volume = BGMusicSetting.Volume;
        BGMusicSetting.audioSource.clip = BGMusicSetting.ClipsList[index];
        BGMusicSetting.audioSource.loop = true;
        BGMusicSetting.audioSource.Play();
    }

    public void StopMusic()
    {
        if (BGMusicSetting.audioSource.isPlaying)
        {
            BGMusicSetting.audioSource.Stop();
        }
    }

    public void ChangeMusicVolume(float value)
    {
        Debug.Log(value);
        BGMusicSetting.Volume = value;
        BGMusicSetting.audioSource.volume = value;
        SaveManager.Prefs.SetFloat(SharedVariables.Music, value);
    }   

    public void ChangeSoundVolume(float value)
    {
        Debug.Log(value);
        SoundSetting.Volume = value;
        SoundSetting.audioSource.volume = value;
        SaveManager.Prefs.SetFloat(SharedVariables.Sound, value);
    }



    AudioSource CreateSource(string name = "AudioSource")
    {
        GameObject Source = new(name);
        Source.transform.parent = transform;
        var source = Source.AddComponent<AudioSource>();
        source.loop = false;
        source.playOnAwake = false;
        return source;
    }

    public void Vibrate()
    {
        if (SaveManager.Prefs.GetInt(SharedVariables.Vibration, 0) == 1)
            return;

         //HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
    }

    public void PlayButtonSound()
    {
        PlayOneShot(AudioType.Button);
    }

}

[System.Serializable]
public struct AudioAssets
{
    public AudioSource audioSource;
    [Range(0, 1)]
    public float Volume;

    [Header("Clips")]
    public AudioClips[] ClipsList;
}              
[System.Serializable]
public struct MusicAssets
{
    public AudioSource audioSource;
    [Range(0, 1)]
    public float Volume;

    [Header("Clips")]
    public AudioClip[] ClipsList;
}

[System.Serializable]
public enum AudioType
{
    None,
    Button,
    Coins,
    GamePlay,
    MainMenu,
    LevelComplete,
    LevelFail,
    Droppable,
    PickUps,
    Died

}

[System.Serializable]
public struct AudioClips
{
    public AudioType type;
    public AudioClip[] clip;
}