using Firebase.Extensions;
using System.Collections;
using UnityEngine;

public class CustomAnalytics : MonoBehaviour
{

    private static System.Action<string> LogEvent_;

    public bool isFireBaseOkToUse;
    char[] specialChars = new char[] { '!', '?', '@', '#', '$', '%', '^', '&', '*', ':', ';', '"', '<', '>', '|', '[', ']', '(', ')', '-' }; // Your array of special characters

    private void Awake()
    {
        StartCoroutine(Custom_Start());
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LogEvent_ += m_LogEvent;
    }

    public static void LogEvent(params string[] messages)
    {
        string combinedEvent = string.Join("_", messages);

        LogEvent_?.Invoke(combinedEvent);
    }

    string ReplaceSpecialCharacters(string input)
    {
        char[] inputArray = input.ToCharArray();

        // Loop through each character and replace it with "_" if it's a special character or space
        for (int i = 0; i < inputArray.Length; i++)
        {
            if (System.Array.Exists(specialChars, element => element == inputArray[i]) || inputArray[i] == ' ')
            {
                inputArray[i] = '_'; // Replace with underscore
            }
        }

        return new string(inputArray); // Convert the array back to a string
    }

    private IEnumerator Custom_Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                // app = Firebase.FirebaseApp.DefaultInstance;
                isFireBaseOkToUse = true;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
                Debug.Log("Analytics: Firebase is ready to Use!!!");
            }
            else
            {
                Debug.LogError(string.Format("Analytics: Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });

        yield return null;
    }

    private void m_LogEvent(string eventNames)
    {
        if (!isFireBaseOkToUse || string.IsNullOrEmpty(eventNames))
        {
            Debug.Log("Analytics: Message is empty or Firebase is not Ok To Use ");
            return;
        }

        string newString = ReplaceSpecialCharacters(eventNames);

        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(newString);

            Debug.Log("Analytics: " + newString);

        }
        catch (System.Exception e)
        {
            Debug.Log("Analytics: Error in Analytics: " + e.ToString());

        }
    }

    private void OnDisable()
    {
        LogEvent_ -= m_LogEvent;
    }
}
