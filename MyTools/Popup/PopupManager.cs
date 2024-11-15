using UnityEngine;

public class PopupManager : MonoBehaviour
{
    [SerializeField] private Popup Prefab;
    [SerializeField] private RectTransform ParentRect;

    private void Start()
    {
        InvokeRepeating(nameof(Testing), 1, 0.7f);
    }

    void Testing()
    {
        ShowPopup(Random.Range(0, 10001).ToString());
    }

    public void ShowPopup(string message, float duration)
    {
        Popup popup = Instantiate(Prefab, ParentRect);
        popup.SetupMessage(message, duration);
    }

    public void ShowPopup(string message)
    {
        Popup popup = Instantiate(Prefab, ParentRect);
        popup.SetupMessage(message);
    }
}
