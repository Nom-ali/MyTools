using System.Collections;
using UnityEngine;

public class TimerAds : MonoBehaviour
{
    [SerializeField] private float Delay = 60;
    [SerializeField] private AnimationBase Popup;
    // Start is called before the first frame update
    void Start()
    {
            StartCoroutine(TimerCoroutine());
    }

    private IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(1);
        
        //if (GameManager.Instance.CurrentLevelNo <= 1)
        //    yield break;

        while (true)
        {   
            if(GameManager.Instance.CurrentState() == GameStatus.GameState.None) 
            { 
                yield return new WaitForSeconds(Delay - 3); // Wait for 60 seconds
                
                Popup.Show();
                yield return new WaitForSeconds(3);
                
                Popup.Hide();
                yield return new WaitForSeconds(0.3f);
                
                RunMethod();
            }
            yield return null;
        }
    }

    private void RunMethod()
    {
        // Your method logic here
        //if (GameManager.Instance.CurrentState() == GameStatus.GameState.None)
            //AdsManager.Instance?.ShowInterAds();
    }
}
