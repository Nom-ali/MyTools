using MyTools;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AddListeners
{
    [SerializeField] private ButtonAction[] AddButtons;

    public Button GetButton(int ButtonIndex)
    {
        return AddButtons[ButtonIndex].button;
    }

    public virtual void Init(UIManagerBase manager)
    {
        foreach (var button in AddButtons)
        {
            ButtonAction btn = button;
            UIManagerBase managerBase = manager;
            btn.button.onClick.RemoveAllListeners();
            btn.button.onClick.AddListener(() =>
            {
                //if (btn.AdsOnClick)
                //    AdsManager.Instance.ShowInterAds();
                 
                Debug.Log($"Btn {btn.button.name} pressed", btn.button);
                managerBase.OnButtonClicked(btn);
            });
            btn.button.interactable = button.Interactable;
        }
    }

    internal void OnDisableRemoveAll()
    {
        foreach (var button in AddButtons)
        {
            if(button.button.onClick.GetPersistentEventCount() > 0)
                button.button.onClick.RemoveAllListeners();
        }
    }
}
