/// Save Manager System
/// this script is using Newtonsoft.Json Lib
/// make sure you import this lib
/// you can add by name from package manager "com.unity.nuget.newtonsoft-json"

using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;

namespace RNA.SaveManager
{
    public static class SaveManager
    {
        public static ObservableObject<int> Currency = new();


        public static int GetLevel(TextMeshProUGUI levelText, bool debug = false)
        {
            int levelNo = Prefs.GetInt(SharedVariables.CurrentLevelNo, 0, debug); ;
            
            if (Prefs.GetBool(SharedVariables.GameCompleted, false, debug) == true)
            {
                levelNo = Prefs.GetInt(SharedVariables.LastPlayedLevel, 0, debug);
                levelText.text = "Level : " + Prefs.GetInt(SharedVariables.RandomLevel).ToString();
            }
            else
                levelText.text = "Level : " + (levelNo + 1);

            return levelNo;
        }

        /// <summary>
        /// Use this only when there is no level selection
        /// And levels gonna repeat over and over
        /// Also This method will also assign random levels when all levels has been played.
        /// </summary>
        /// <param name="totalLevels">total numer of level</param>
        /// <param name="debug">should debug?</param>
        public static void SaveNextLevel(int totalLevels, bool debug = false)
        {
            int levelIndex = Prefs.GetInt(SharedVariables.CurrentLevelNo, 0, debug);
            if (Prefs.GetBool(SharedVariables.GameCompleted, false, debug) == false && levelIndex < totalLevels - 1)
            {
                Prefs.SetInt(SharedVariables.CurrentLevelNo, levelIndex + 1, debug);
            }
            else if (Prefs.GetBool(SharedVariables.GameCompleted, false, debug) == false && levelIndex == totalLevels - 1)
            {
                Prefs.SetInt(SharedVariables.CurrentLevelNo, 0, debug);
                Prefs.SetBool(SharedVariables.GameCompleted, true, debug);
                if (debug) Debug.Log("SaveManager: ----- <color=yellow> Game Completed </color> -----");
            }

            if (Prefs.GetBool(SharedVariables.GameCompleted, false, debug) == true)
            {
                while (true)
                {
                    int LastLevel = Prefs.GetInt(SharedVariables.LastPlayedLevel, totalLevels, debug);
                    int levelNum = UnityEngine.Random.Range(0, totalLevels);

                    if (levelNum != LastLevel)
                    {
                        Prefs.SetInt(SharedVariables.LastPlayedLevel, levelNum, debug);
                        break;
                    }
                }
                Prefs.SetInt(SharedVariables.RandomLevel, Prefs.GetInt(SharedVariables.RandomLevel, totalLevels, debug) + 1, debug);
            }
        }
           
        public static void SaveNextLevel_(int totalLevels)
        {
            if (Prefs.GetBool(SharedVariables.FreeMode, false))
            {
                while (true) 
                {
                    int randomLevel = UnityEngine.Random.Range(0, totalLevels);
                    if (Prefs.GetInt(SharedVariables.RandomLevel, 3) != randomLevel)
                    {
                        Prefs.SetInt(SharedVariables.RandomLevel, randomLevel);
                        break;
                    } 
                }
            }
            else
            {
                if (Prefs.GetInt(SharedVariables.UnlockedLevels, 0, true) < totalLevels)
                {
                    if(Prefs.GetInt(SharedVariables.CurrentLevelNo, 0) == Prefs.GetInt(SharedVariables.UnlockedLevels))
                        Prefs.SetInt(SharedVariables.UnlockedLevels, Prefs.GetInt(SharedVariables.UnlockedLevels, 0) + 1, true);
                    Prefs.SetInt(SharedVariables.CurrentLevelNo, Prefs.GetInt(SharedVariables.CurrentLevelNo) + 1, true);

                   if(Prefs.GetInt(SharedVariables.CurrentLevelNo, 0, true) == totalLevels )
                    {
                        Prefs.SetInt(SharedVariables.GameCompleted, 1);
                        Prefs.SetBool(SharedVariables.FreeMode, true, true);
                        GetRandomLevel(totalLevels);
                    }
                }
                else
                {
                    Prefs.SetInt(SharedVariables.GameCompleted, 1);
                    Prefs.SetBool(SharedVariables.FreeMode, true, true);
                    GetRandomLevel(totalLevels);
                }
            }
        }

        public static void InCaseOfRestart()
        {
            Prefs.SetInt(SharedVariables.CurrentLevelNo, Prefs.GetInt(SharedVariables.CurrentLevelNo) - 1, true);
        }

        public static int GetRandomLevel(int totalLevels, bool SaveRandomLevel = true)
        {
            int randomLevel = UnityEngine.Random.Range(3, totalLevels);
            if(SaveRandomLevel) Prefs.SetInt(SharedVariables.RandomLevel, randomLevel);
            return randomLevel;
        }

        public static class FileSystem
        {
            private static readonly string SaveFolder = Application.persistentDataPath + "/SaveFiles/";

            static FileSystem()
            {
                if (!Directory.Exists(SaveFolder))
                {
                    Directory.CreateDirectory(SaveFolder);
                }
            }

            public static void DeleteSaveFile(string fileName, bool debug = false)
            {
                try
                {
                    string filePath = SaveFolder + fileName + ".json";
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                       if(debug) Debug.Log($"SaveManager: Successfully deleted {fileName}.json");
                    }
                    else
                    {
                        if (debug) Debug.LogWarning($"SaveManager: File {fileName}.json not found.");
                    }
                }
                catch (Exception e)
                {
                    if (debug) Debug.LogError($"SaveManager: Failed to delete {fileName}.json: {e.Message}");
                }
            }
        }

        public static class Prefs
        {
            #region Where Magic Happens
            public static T Get<T>(string baseKey, T defaultValue = default(T), bool debug = false)
            {
                var value = JsonConvert.DeserializeObject<T>(PlayerPrefs.GetString(baseKey, JsonConvert.SerializeObject(defaultValue)));
                if (debug) Debug.Log($"SaveManager: Getting Value of {baseKey}: {value}");
                return value;
            }

            public static string Set<T>(string baseKey, T value, bool debug = false)
            {
                var serialized = JsonConvert.SerializeObject(value);
                PlayerPrefs.SetString(baseKey, serialized);
                if (debug) Debug.Log($"SaveManager: Setting Value of {baseKey}: {value}");
                return serialized;
            }

            public static void DeleteKey(string baseKey, bool debug = false)
            {
                PlayerPrefs.DeleteKey(baseKey);
                if (debug) Debug.Log($"SaveManager: Deleted Value of {baseKey}");
            } 
            
            public static bool Has(string baseKey, bool debug = false)
            {
                var haskey = PlayerPrefs.HasKey(baseKey);
                if (debug) Debug.Log($"SaveManager: Has key Value of {baseKey}");
                return haskey;
            }

            #endregion

            #region Methods
            public static void SetInt(string baseKey, int value, bool debug = false) => Set(baseKey, value, debug);
            public static void SetFloat(string baseKey, float value, bool debug = false) => Set(baseKey, value, debug);
            public static void SetObject<T>(string baseKey, T value, bool debug = false) => Set(baseKey, value, debug);
            public static void SetColor(string baseKey, Color value, bool debug = false) => Set(baseKey, value, debug);
            public static void SetString(string baseKey, string value, bool debug = false) => Set(baseKey, value, debug);
            public static void SetVector3(string baseKey, Vector3 value, bool debug = false) => Set(baseKey, new SerializableVector3(value), debug);
            public static void SetVector2(string baseKey, Vector2 value, bool debug = false) => Set(baseKey, new SerializableVector2(value), debug);
            public static void SetBool(string baseKey, bool value, bool debug = false) => Set(baseKey, value, debug);
            #endregion

            #region Properties
            public static int GetInt(string baseKey, int defaultValue = 0, bool debug = false) => Get(baseKey, defaultValue, debug);
            public static float GetFloat(string baseKey, float defaultValue = 0, bool debug = false) => Get(baseKey, defaultValue, debug);
            public static T GetObject<T>(string baseKey, T defaultValue = default, bool debug = false) => Get(baseKey, defaultValue, debug);
            public static Color GetColor(string baseKey, Color defaultValue, bool debug = false) => Get(baseKey, defaultValue, debug);
            public static string GetString(string baseKey, string defaultValue, bool debug = false) => Get(baseKey, defaultValue, debug);
            public static Vector3 GetVector3(string baseKey, Vector3 defaultValue, bool debug = false) => Get(baseKey, new SerializableVector3(defaultValue), debug).ToVector3();
            public static Vector2 GetVector2(string baseKey, Vector2 defaultValue, bool debug = false) => Get(baseKey, new SerializableVector2(defaultValue), debug).ToVector2();
            public static bool GetBool(string baseKey, bool defaultValue, bool debug = false) => Get(baseKey, defaultValue, debug);
            public static bool HasKey(string baseKey, bool debug = false) => Has(baseKey, debug);
            #endregion
        }
    }

    [Serializable]
    public struct SerializableVector3
    {
        public float x, y, z;

        public SerializableVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public readonly Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    [Serializable]
    public struct SerializableVector2
    {
        public float x, y;

        public SerializableVector2(Vector2 vector)
        {
            x = vector.x;
            y = vector.y;
        }

        public readonly Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
    }

}