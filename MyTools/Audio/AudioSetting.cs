using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioSettings", menuName = "Audio/Audio Settings")]
public class AudioSettings : ScriptableObject
{
    [Header("General Settings")]
    public bool enabled = true;
    [Range(0f, 1f)]
    public float volume = 1.0f;
    public string defaultClipId;

    [Header("Clips")]
    public AudioClipData[] clips;

    public AudioClipData GetClipById(AudioType audioType)
    {
        if (clips == null || clips.Length == 0)
            return null;

        foreach (var clip in clips)
        {
            if (clip.audioType == audioType)
                return clip;
        }

        return null;
    }

    private void OnValidate()
    {
        if (clips == null || clips.Length == 0)
            return;
        foreach (var clip in clips)
        {
            if (clip.displayName != clip.audioType.ToString())
                clip.displayName = clip.audioType.ToString();

            if (clip.clip == null)
                Debug.LogError($"Audio clip is missing for {clip.displayName}", this);
        }
    }
}

[Serializable]
public class AudioClipData
{
    public string displayName;
    public AudioType audioType = AudioType.None;
    public AudioClip clip;
    public AudioClip[] variations;

    public bool loop = false;

    [Range(0f, 1f)]
    public float volumeMultiplier = 1.0f;

    [Range(0f, 1f)]
    public float spatialBlend = 0f;

    [Range(0f, 256)]
    public int priority = 128;

    [Range(0.5f, 1.5f)]
    public float minPitch = 0.5f;

    [Range(0.5f, 1.5f)]
    public float maxPitch = 1.0f;

    public AudioClipData()
    {
        displayName = string.Empty;
        audioType = AudioType.None;
        clip = null;
        variations = null;
        loop = false;
        volumeMultiplier = 1.0f;
        spatialBlend = 0f;
        priority = 128;
        minPitch = 0.5f;
        maxPitch = 1.5f;
    }
}