using RNA;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AddListeners
{
    [SerializeField] private ButtonAction[] AddButtons;
    //[Space(10)]
    //public UnityEvent OnInitComplete = null;

    public Button GetButton(int ButtonIndex)
    {
        return AddButtons[ButtonIndex].button;
    }

    public virtual void Init(UIManagerBase manager)
    {
        //OnInitComplete?.Invoke();

        foreach (var button in AddButtons)
        {
            ButtonAction btn = button;
            UIManagerBase managerBase = manager;
            btn.button.onClick.RemoveAllListeners();
            btn.button.onClick.AddListener(() =>
            {
                Debug.Log($"Btn {btn.button.name} pressed", btn.button);
                //this.InvokeAfterDelay(.5f,() => managerBase.OnButtonClicked(btn));
                managerBase.OnButtonClicked(btn);
            });
            btn.button.interactable = button.Interactable;
        }
    }



    internal void OnDisableRemoveAll()
    {
        foreach (var button in AddButtons)
        {
            button.button.onClick.RemoveAllListeners();
        }
    }
}
