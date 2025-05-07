using Firebase;
using Firebase.Extensions;
using System.Collections;
using UnityEngine;

public class CustomAnalytics : MonoBehaviour
{

    public bool isFireBaseOkToUse = false;
    char[] specialChars = new char[] { '.',',','!', '?', '@', '#', '$', '%', '^', '&', '*', ':', ';', '"', '<', '>', '|', '[', ']', '(', ')', '-' }; // Your array of special characters

    internal void LogEvent(params string[] messages)
    {
        string combinedEvent = string.Join("_", messages);
        m_LogEvent(combinedEvent);
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

    internal void Initialize(DependencyStatus dependencyStatus)
    {
        if (dependencyStatus == DependencyStatus.Available)
        {
            isFireBaseOkToUse = true;
            m_LogEvent("Game Started");
        }
        else
        {
            Debug.LogError(string.Format("Analytics: Dependencies resolve error: {0}", dependencyStatus));
            // Firebase Unity SDK is not safe to use here.
        }
        
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
}
