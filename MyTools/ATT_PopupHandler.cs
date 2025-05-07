using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

public class ATT_PopupHandler : MonoBehaviour
{
    public static ATT_PopupHandler instance;

    private void Awake()
    {
        instance = this;
    }

    public void SendTrackingRequest()
    {
        StartCoroutine(RequestTracking_Auth());
    }

    private IEnumerator RequestTracking_Auth()
    {
#if UNITY_IOS
        // check with iOS to see if the user has accepted or declined tracking
        var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

        if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            yield return new WaitForSeconds(3);
            ATTrackingStatusBinding.RequestAuthorizationTracking();
            Debug.Log("RequestAuthorizationTracking");
        }

#else
        Debug.Log("Unity iOS Support: App Tracking Transparency status not checked, because the platform is not iOS.");
#endif
        StartCoroutine(LoadingScreen());
        yield return null;
    }

    IEnumerator LoadingScreen()
    {
#if UNITY_IOS && !UNITY_EDITOR
                        var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                        while (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                        {
                            status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                            yield return null;
                        }
#endif
        Debug.Log("ATT Req is send and Show");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void OnDestroy()
    {
        if (instance != null)
        {
            instance = null;
        }
    }
}
