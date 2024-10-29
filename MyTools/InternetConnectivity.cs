using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternetConnectivity : MonoBehaviour
{

    [SerializeField] private AnimationBase Popup;


    enum Status
    {
        None, 
        Shown, 
        Hidden
    }

    private Status status = Status.Hidden;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(CheckInternet());
    }

    IEnumerator CheckInternet()
    {
        while (true)
        {
            Debug.Log("Intnet Connectivity: " + (Application.internetReachability == NetworkReachability.NotReachable));
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                if (status == Status.Hidden)
                {
                    Popup.Show();
                    status = Status.Shown;
                }
            }
            else
            {
                if (status == Status.Shown)
                {
                    Popup.Hide();
                    status = Status.Hidden;
                }
            }

            Time.timeScale = Application.internetReachability == NetworkReachability.NotReachable ? 0 : 1;
            yield return new WaitForSecondsRealtime(1);
        }
         
    }
}


