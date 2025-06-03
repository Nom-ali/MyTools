using MyBox;
using MyTools;
using MyTools.TextWritter;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupPanel : MonoBehaviour, IInitialization
{
    [SerializeField] private AddListeners listeners;

    [Separator("********** Popup Panel **********")]
    [SerializeField] internal TextMeshProUGUI TitleText;
    [SerializeField] internal TextMeshProUGUI MessageText;

    [Space]
    [SerializeField] internal Button FirstBtn = null;
    [SerializeField] internal Button SecondBtn = null;
    

    private AnimationBase panel = null;
    private GameObject panelObj = null;

    public void Init(UIManagerBase uIManager)
    {
        listeners.Init(uIManager);
    }

    private void Show()
    {
        if(panel)
            panel.Show();
        else if (panelObj)
            panelObj.SetActive(true);

        Init(GetComponentInParent<UIManagerBase>());
    }

    private void Hide()
    {
        if (panel)
            panel.Hide();
        else if (panelObj)
            panelObj.SetActive(false);
    }

    public async void ShowPopup(PopupSetting popupSetting, UIManagerBase uIManager)
    {
        Init(uIManager);

        if (!panel && TryGetComponent(out AnimationBase animationBase))
        { 
            panel = animationBase; 
        }

        if (!panel)
            panelObj = gameObject;

        FirstBtn.gameObject.SetActive(false);
        SecondBtn.gameObject.SetActive(false);

        //setting tile
        if (TitleText)
            TitleText.text = popupSetting.Title;

        //showing panel
        Show();

        // chechk if popup is already active ir not
        while (!MessageText.gameObject.activeInHierarchy)
            await Task.Yield();

        //writing text letter by letter
        if (MessageText)
            MessageText.WriteLetter(popupSetting.Message, popupSetting.TextDuration)
                .OnComplete(async () =>
                {
                    //enabling or disabling btn
                    FirstBtn.gameObject.SetActive(popupSetting.EnableFirstBtn);
                    SecondBtn.gameObject.SetActive(popupSetting.EnableSecondBtn);

                    // assigning 1st btn actions
                    if (popupSetting.EnableFirstBtn && FirstBtn)
                    {
                        if (FirstBtn.transform.GetChild(0).TryGetComponent(out Text text))
                            text.text = popupSetting.FirstBtnText;

                        FirstBtn.onClick.RemoveAllListeners();
                        FirstBtn.onClick.AddListener(() =>
                        {
                            AudioPlayer.instance?.PlayButtonSound();
                            popupSetting.FirstBtnAction?.Invoke();
                            UIManager.Instance.OnButtonClicked(popupSetting.m_FirstBtnAction);
                            Hide();
                        });
                    }

                    // assigning 2nd btn actions
                    if (popupSetting.EnableSecondBtn && SecondBtn)
                    {
                        if (popupSetting.EnableSecondBtn && SecondBtn.transform.GetChild(0).TryGetComponent(out TextMeshProUGUI text))
                            text.text = popupSetting.SecondBtnText;

                        SecondBtn.onClick.RemoveAllListeners();
                        SecondBtn.onClick.AddListener(() =>
                        {
                            AudioPlayer.instance?.PlayButtonSound();
                            popupSetting.SecondBtnAction?.Invoke();
                            UIManager.Instance.OnButtonClicked(popupSetting.m_SecondBtnAction);
                            Hide();
                        });
                    }
                    // On Complete Action callback
                    popupSetting.OnComplete?.Invoke();

                    if (popupSetting.AutoClosePopup)
                    {
                        await Task.Delay((int)((popupSetting.TextDuration + popupSetting.AutoCloseDelay) * 1000));
                        Hide();
                    }
                })
                .Start();   // Starting method from here
    }

    private void OnDisable()
    {
        listeners.OnDisableRemoveAll();
    }
}

[System.Serializable]
public class PopupSetting
{
    [SerializeField] internal string Title = "";
    [SerializeField] internal string Message = "";
    [SerializeField] internal float TextDuration = 1;
    [SerializeField] internal bool AutoClosePopup = false;
    [ConditionalField(nameof(AutoClosePopup), false)]
    [SerializeField] internal float AutoCloseDelay = 1f;
    [SerializeField] internal bool EnableFirstBtn = true;
    [SerializeField] internal string FirstBtnText = "OK";
    [SerializeField] internal System.Action FirstBtnAction = null;
    [SerializeField] internal ButtonActionSimple m_FirstBtnAction;
    [SerializeField] internal bool EnableSecondBtn = false;
    [SerializeField] internal string SecondBtnText = "";
    [SerializeField] internal System.Action SecondBtnAction = null;
    [SerializeField] internal ButtonActionSimple m_SecondBtnAction;
    [SerializeField] internal bool AdsIcon = false;
    [SerializeField] internal System.Action OnComplete = null;

    // Parameterless constructor
    public PopupSetting()
    {
    }

    //public PopupSetting(string message, string title = "", string firstBtnText = "", System.Action firstAction = null,
    //    bool enableSecondbtn = false, string secondBtnText = "", Action secondAction = null, bool adsIcon = false, Action onComplete = null)
    //{
    //    Title = title;
    //    Message = message;
    //    FirstBtnText = firstBtnText;
    //    FirstBtnAction = firstAction;
    //    EnableSecondBtn = enableSecondbtn;
    //    SecondBtnText = secondBtnText;
    //    SecondBtnAction = secondAction;
    //    AdsIcon = adsIcon;
    //    OnComplete = onComplete;
    //}
}