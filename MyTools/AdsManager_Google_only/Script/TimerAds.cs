using System.Collections;
using UnityEngine;

public class TimerAds : MonoBehaviour
{
    [SerializeField] private float Delay = 60;
    [SerializeField] private GameObject Popup;
    // Start is called before the first frame update
    void Start()
    {
            StartCoroutine(TimerCoroutine());
    }

    private IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(1);
        
        while (true)
        {   
            yield return new WaitForSeconds(Delay - 3); // Wait for 60 seconds
                
            Popup.SetActive(true);
            yield return new WaitForSeconds(3);

            Popup.SetActive(false);
            yield return new WaitForSeconds(0.3f);
                
            RunMethod();
            yield return null;
        }
    }

    private void RunMethod()
    {
        // Your method logic here
        GoogleAds.AdsManager.Instance?.ShowInterAds();
    }
}
