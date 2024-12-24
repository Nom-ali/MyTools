using RNA;
using UnityEngine;
using UnityEngine.Events;

public class AddListeners : MonoBehaviour
{
    [SerializeField] private ButtonAction[] AddButtons;
    [Space]
    [SerializeField] private UnityEvent OnInitComplete = null;

    public void Init(UIManagerBase manager)
    {
        foreach (var button in AddButtons)
        {
            ButtonAction btn = button;
            UIManagerBase managerBase = manager;
            btn.button.onClick.RemoveAllListeners();
            btn.button.onClick.AddListener(() =>
            {
                Debug.Log($"Btn {btn.button.name} pressed", btn.button);
                managerBase.OnButtonClicked(btn);
            });
        }

        OnInitComplete?.Invoke();
    }

    private void OnDisable()
    {
        foreach (var button in AddButtons)
        {
            button.button.onClick.RemoveAllListeners();
        }
    }
}
