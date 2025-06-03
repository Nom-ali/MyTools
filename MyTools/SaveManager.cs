/// Save Manager System
/// this script is using Newtonsoft.Json Lib
/// make sure you import this lib
/// you can add by name from package manager "com.unity.nuget.newtonsoft-json"

using Newtonsoft.Json;
using UnityEngine;
using System.IO;
using System;
using TMPro;

namespace MyTools.SaveManager
{
    public static class SaveManager
    {
        public static ObservableObject<int> Currency = new();

        //Converting Cash digits to Alpahbet-digits
        public static string FormatEveryThirdPower(long target)
        {
            string[] ShortNotationList = new string[12] { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };

            double value = target;
            int baseValue = 0;
            string notationValue = "";
            string toStringValue;
            if (value >= 1000) // I start using the first notation at 10k
            {
                value /= 1000;
                baseValue++;
                while (Mathf.Round((float)value) >= 1000)
                {
                    value /= 1000;
                    baseValue++;
                }

                if (baseValue < 2)
                    toStringValue = "N1"; // display 1 decimal while under 1 million
                else
                    toStringValue = "N2"; // display 2 decimals for 1 million and higher

                if (baseValue > ShortNotationList.Length) return null;
                else notationValue = ShortNotationList[baseValue];
            }
            else toStringValue = ""; // string formatting at low numbers
            return value.ToString(toStringValue) + notationValue;
        }

        public static int GetLevel(TextMeshProUGUI levelText = null, bool debug = false)
        {
            int levelNo = Prefs.GetInt(SharedVariables.CurrentLevelNo, 0, debug);

            if (Prefs.GetBool(SharedVariables.GameCompleted, false, debug) == true)
            {
                levelNo = Prefs.GetInt(SharedVariables.LastPlayedLevel, 0, debug);
                if (levelText) levelText.text = "Level : " + Prefs.GetInt(SharedVariables.RandomLevel).ToString();
            }
            else
                if (levelText) levelText.text = "Level : " + (levelNo + 1);

            return levelNo;
        }

        public static bool GetLevelModule(int moduleBy, int currentLevel, bool debug = false)
        {
            int levelNo;
            if (currentLevel > 1)
                levelNo = currentLevel;
            else
                return false;

            if (Prefs.GetBool(SharedVariables.GameCompleted, false, debug) == true)
            {
                return (Prefs.GetInt(SharedVariables.RandomLevel) % moduleBy) == 0 ? true : false;
            }
            else
                return (levelNo % moduleBy) == 0 ? true : false;
        }

        public static void SaveNextLevel(bool debug = false)
        {
            int levelIndex = Prefs.GetInt(SharedVariables.CurrentLevelNo, 0, debug);
            Prefs.SetInt(SharedVariables.CurrentLevelNo, levelIndex + 1, debug);
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
                    if (Prefs.GetInt(SharedVariables.CurrentLevelNo, 0) == Prefs.GetInt(SharedVariables.UnlockedLevels))
                        Prefs.SetInt(SharedVariables.UnlockedLevels, Prefs.GetInt(SharedVariables.UnlockedLevels, 0) + 1, true);
                    Prefs.SetInt(SharedVariables.CurrentLevelNo, Prefs.GetInt(SharedVariables.CurrentLevelNo) + 1, true);

                    if (Prefs.GetInt(SharedVariables.CurrentLevelNo, 0, true) == totalLevels)
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
            if (SaveRandomLevel) Prefs.SetInt(SharedVariables.RandomLevel, randomLevel);
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
                        if (debug) Debug.Log($"SaveManager: Successfully deleted {fileName}.json");
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

            public static void Save()
            {
                PlayerPrefs.Save();
            }
            public static T Get<T>(string baseKey, T defaultValue = default(T), bool debug = false)
            {
                var value = JsonConvert.DeserializeObject<T>(PlayerPrefs.GetString(baseKey, JsonConvert.SerializeObject(defaultValue)));
                if (debug) Debug.Log($"<color=yellow>SaveManager: Getting Value </color> of {baseKey}: {value}");
                return value;
            }

            public static string Set<T>(string baseKey, T value, bool debug = false)
            {
                var serialized = JsonConvert.SerializeObject(value);
                PlayerPrefs.SetString(baseKey, serialized);
                if (debug) Debug.Log($"<color=yellow>SaveManager: Setting Value </color> of {baseKey}: {value}");
                return serialized;
            }

            public static void DeleteKey(string baseKey, bool debug = false)
            {
                PlayerPrefs.DeleteKey(baseKey);
                if (debug) Debug.Log($"<color=yellow>SaveManager: Deleted Value </color> of {baseKey}");
            }

            public static bool Has(string baseKey, bool debug = false)
            {
                var haskey = PlayerPrefs.HasKey(baseKey);
                if (debug) Debug.Log($"<color=yellow>SaveManager: Has key </color> Value of {baseKey}: {haskey}");
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