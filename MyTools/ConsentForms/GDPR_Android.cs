using UnityEngine;
using System.Collections.Generic;
using GoogleMobileAds.Ump.Api;
using UnityEngine.SceneManagement;
using System.Collections;

public class GDPR_Android : MonoBehaviour
{
    private ConsentForm _consentForm;

    void Start()
    {
        var debugSettings = new ConsentDebugSettings
        {
            // Simulates EEA region for testing purposes.
            DebugGeography = DebugGeography.EEA,
            TestDeviceHashedIds = new List<string>
            {
                "965E4A26737DF85475A353251709C315"
            }
        };

        ConsentRequestParameters request = new ConsentRequestParameters
        {
            TagForUnderAgeOfConsent = false,
            ConsentDebugSettings = debugSettings,
        };

        // Requests the latest consent info.
        ConsentInformation.Update(request, OnConsentInfoUpdated);
    }

    private void OnConsentInfoUpdated(FormError error)
    {
        if (error != null)
        {
            Debug.LogError($"Consent Info Update Error: {error.Message}");
            StartCoroutine(LoadNextScene());
            return;
        }

        if (ConsentInformation.IsConsentFormAvailable())
        {
            LoadConsentForm();
        }
        else
        {
            StartCoroutine(LoadNextScene());
        }
    }

    private void LoadConsentForm()
    {
        ConsentForm.Load(OnConsentFormLoaded);
    }

    private void OnConsentFormLoaded(ConsentForm consentForm, FormError error)
    {
        if (error != null)
        {
            Debug.LogError($"Consent Form Load Error: {error.Message}");
            StartCoroutine(LoadNextScene());
            return;
        }

        _consentForm = consentForm;

        if (ConsentInformation.ConsentStatus == ConsentStatus.Required)
        {
            _consentForm.Show(OnConsentFormShown);
        }
        else
        {
            StartCoroutine(LoadNextScene());
        }
    }

    private void OnConsentFormShown(FormError error)
    {
        if (error != null)
        {
            Debug.LogError($"Consent Form Show Error: {error.Message}");
            // Continue regardless of error; don't block the flow.
        }

        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {

        FirebaseManager.Instance.Init();
        yield return new WaitForSeconds(3);
        // Move to the next scene (assumes linear build order).
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
