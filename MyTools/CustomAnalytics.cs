using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnalytics : MonoBehaviour
{

    public static System.Action<string> LogEvent;

    public bool isFireBaseOkToUse;
    char[] specialChars = new char[] { '!', '?', '@', '#', '$', '%', '^', '&', '*' }; // Your array of special characters

    private void Awake()
    {
        StartCoroutine(Custom_Start());
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LogEvent += m_LogEvent;
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
                Debug.Log(" Firebase is ready to Use!!!");
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });

        yield return null;
    }
    public void m_LogEvent(string eventName)
    {
        if (!isFireBaseOkToUse)
        {
            Debug.Log("Firebase is not Ok To Use");
        }

        string newString = ReplaceSpecialCharacters(eventName);

        eventName = eventName.Replace(" ", "_");

        try
        {
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);

            Debug.Log("Analytics: " + eventName);

        }
        catch (System.Exception e)
        {
            Debug.Log("Analytics: Error in Analytics: " + e.ToString());

        }


    }

    private void OnDisable()
    {
        LogEvent -= m_LogEvent;
    }
}
