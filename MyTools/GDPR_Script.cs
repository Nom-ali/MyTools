using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GoogleMobileAds.Ump;
using GoogleMobileAds.Ump.Api;

public class GDPR_Script : MonoBehaviour
{
    public static bool InitilizeAds = false;
    ConsentForm _consentForm;
    // Start is called before the first frame update
    void Start()
    {
        var debugSettings = new ConsentDebugSettings
        {
            // Geography appears as in EEA for debug devices.
            DebugGeography = DebugGeography.EEA,
            TestDeviceHashedIds = new List<string>
            {
                "965E4A26737DF85475A353251709C315"
            }
        };

        // Here false means users are not under age.
        ConsentRequestParameters request = new ConsentRequestParameters
        {
            TagForUnderAgeOfConsent = false,
            ConsentDebugSettings = debugSettings,
        };

        // CheckIfNPCTypeVehicleAvailable the current consent information status.
        ConsentInformation.Update(request, OnConsentInfoUpdated);
    }

    void OnConsentInfoUpdated(FormError error)
    {
        if (error != null)
        {
            // Handle the error.
            Debug.LogError(error);
            ATT_PopupHandler.instance.SendTrackingRequest();
            return;
        }

        if (ConsentInformation.IsConsentFormAvailable())
        {
            LoadConsentForm();
        }
        else
        {
            ATT_PopupHandler.instance.SendTrackingRequest();
        }
        // If the error is null, the consent information state was updated.
        // You are now ready to check if a form is available.
    }

    void LoadConsentForm()
    {
        // Loads a consent form.
        ConsentForm.Load(OnLoadConsentForm);
    }

    void OnLoadConsentForm(ConsentForm consentForm, FormError error)
    {
        if (error != null)
        {
            // Handle the error.
            Debug.LogError(error);
            ATT_PopupHandler.instance.SendTrackingRequest();
            return;
        }

        // The consent form was loaded.
        // Save the consent form for future requests.
        _consentForm = consentForm;

        // You are now ready to show the form.
        if (ConsentInformation.ConsentStatus == ConsentStatus.Required)
        {
            _consentForm.Show(OnShowForm);
        }
        else
        {
            ATT_PopupHandler.instance.SendTrackingRequest();
        }
    }


    void OnShowForm(FormError error)
    {
        if (error != null)
        {
            // Handle the error.
            Debug.LogError(error);
            return;
        }

        ATT_PopupHandler.instance.SendTrackingRequest();
        // Handle dismissal by reloading form.
        LoadConsentForm();
    }


}
