using UnityEngine.SceneManagement;
using MyTools.LoadingManager;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using System;
using MyBox;
using TMPro;

namespace MyTools
{
    public abstract class UIManagerBase : MonoBehaviour
    {
        #region Variables
        [Foldout("******************** UI Manager ********************", true)]
        [ReadOnly]
        [SerializeField] protected GameObject currentPanel;

        [Header("---------- Internet ----------")]
        [SerializeField] internal bool checkInternet = true;
        internal InternetConnectivity internetConnectivity = null;

        [Header("---------- Panels ----------")]
        [SerializeField] internal LoadingScript LoadingPanel = null;
        [SerializeField] internal PanelSettings[] PanelsList;
        [SerializeField] internal PanelSettings[] IndPanelsList;

        [Header("---------- Coin System ----------")]
        [SerializeField] internal bool EnableCoinSystem = false;
        [ConditionalField(nameof(EnableCoinSystem), false)]
        [SerializeField] internal TextMeshProUGUI CoinsText;


        private PopupPanel popupPanel = null;
        #endregion

        #region Awake
        public virtual void Awake()
        {
            if(checkInternet)
            {
                internetConnectivity = new InternetConnectivity
                {
                    checkInternet = this.checkInternet,
                    OnInternetLost = () => ShowPanel_Ind(PanelType.LoadingPopup),
                    OnInternetRestored = () => HidePanel_Ind(),
                };
            }              
        }
        #endregion

        #region Start
        internal virtual IEnumerator Start()
        {
            // Set Frame Rate
            Application.targetFrameRate = 60;

            //Get loading panel
            LoadingPanel ??= LoadingPanel = FindFirstObjectByType<LoadingScript>();
            
            if (EnableCoinSystem && CoinsText)
            {
                SaveManager.SaveManager.Currency.Initialize(SharedVariables.Coins, 10, CoinsText);
            }

            yield return null;
        }
        #endregion


        #region Panel Setting
        public PanelSettings GetPanel(PanelType panelType)
        {
            return Array.Find(PanelsList, panel => panel.panelType == panelType);
        }  
        public PanelSettings GetPanel_Ind(PanelType panelType)
        {
            return Array.Find(IndPanelsList, panel => panel.panelType == panelType);
        }

        GameObject OldPanel = null;
        private GameObject Show_Panel(PanelType panelType, bool disableOldPanel = true, Action OnShow = null, Action OnHide = null)
        {
            OldPanel = currentPanel;
            AnimationBase animationBase;

            if (disableOldPanel)
            {
                PanelSettings[] activePanels = Array.FindAll(PanelsList, panels => panels.Panel.activeInHierarchy == true);
                foreach (var item in activePanels)
                {
                    Debug.Log($"<color=yellow> Panel: {item.panelType}</color> is hiding", item.Panel);
                    if (item.Panel && item.Panel.TryGetComponent(out animationBase))
                        animationBase.Hide(OnHide);
                    else
                    {
                        item.Panel?.SetActive(false);
                        OnHide?.Invoke();
                    }
                }
            }

            PanelSettings panel = Array.Find(PanelsList, panels => panels.panelType == panelType);
            Debug.Log($"<color=yellow>Panel: {panel.panelType}</color> is Showing", panel.Panel);
            if (panel.Panel && panel.Panel.TryGetComponent(out animationBase))
            {
                animationBase.Show(OnShow);
                if (animationBase.TryGetComponent(out IInitialization init))
                    init.Init(this);
            }
            
            else
            {
                panel.Panel?.SetActive(true);
                if (panel.Panel && panel.Panel.TryGetComponent(out IInitialization init))
                    init.Init(this);
                OnShow?.Invoke();
            }

            currentPanel = panel.Panel;
            return panel.Panel;
        }

        internal GameObject ShowPanel_Ind(PanelType panelType, Action OnShow = null)
        {
            PanelSettings panel = Array.Find(IndPanelsList, panels => panels.panelType == panelType);
            Debug.Log($"<color=yellow>Panel: {panel.panelType}</color> is Showing", panel.Panel);
            if (panel.Panel && panel.Panel.TryGetComponent(out AnimationBase animationBase))
            {
                animationBase.Show(OnShow);
                if(animationBase.TryGetComponent(out IInitialization init))
                    init.Init(this);
            }
            else
            {
                panel.Panel?.SetActive(true);
                if(panel.Panel && panel.Panel.TryGetComponent(out IInitialization init))
                    init.Init(this);
                OnShow?.Invoke();
            }
            return panel.Panel;
        }

        internal void HidePanel_Ind(Action OnHide = null)
        {
            PanelSettings[] activePanels = Array.FindAll(IndPanelsList, panels => panels.Panel.activeInHierarchy == true);
            foreach (var item in activePanels)
            {
                Debug.Log($"<color=yellow> Panel: {item.panelType}</color> is hiding", item.Panel);
                if (item.Panel && item.Panel.TryGetComponent(out AnimationBase animationBase))
                    animationBase.Hide(OnHide);
                else
                {
                    item.Panel?.SetActive(false);
                    OnHide?.Invoke();
                }
            }
        }

        public void HidePanel(PanelType panelType, Action OnHide = null)
        {
            for (int i = 0; i < PanelsList.Length; i++)
            {
                if (PanelsList[i].panelType == panelType)
                {
                    if (PanelsList[i].Panel && PanelsList[i].Panel.TryGetComponent(out AnimationBase animationBase))
                        animationBase.Hide(OnHide);
                    else
                        PanelsList[i].Panel?.SetActive(false);
                    Debug.Log($"Setting OFF Panel: {PanelsList[i].Panel.name}", PanelsList[i].Panel);
                }
                else if(PanelsList[i].Panel && PanelsList[i].Panel.activeSelf)
                {
                    currentPanel = PanelsList[i].Panel;
                }
            }
        }

        public void HideCurrentPanel()
        {
            if (!currentPanel)
            {
                Debug.LogError("There is no panel");
                return;
            }

            if (currentPanel.TryGetComponent(out AnimationBase animation))
                animation.Hide();
            else
                currentPanel.SetActive(false);

            if (currentPanel.name.ToLower().Contains("genericpopup"))
                currentPanel = OldPanel;
        }

        public GameObject ShowPanel(PanelType panelType)
        {
             return Show_Panel(panelType);
        }
        
        public GameObject ShowPanel(PanelType panelType, bool disableOldPanel = false)
        {
            return Show_Panel(panelType, disableOldPanel);
        } 
        
        public GameObject ShowPanel(PanelType panelType, Action OnComplete)
        {
            return Show_Panel(panelType, OnShow: OnComplete);
        }

        internal IEnumerator ShowLoadingPopup(float disableAfter = 3)
        {
            GameObject panel = ShowPanel_Ind(PanelType.LoadingPopup);
            float delay = panel.GetComponent<AnimationBase>().GetDelays();
            yield return new WaitForSeconds(disableAfter + delay);
            HidePanel_Ind();
            yield return new WaitUntil(() => GetPanel_Ind(PanelType.LoadingPopup).Panel.activeSelf == false);
        }
        #endregion


        #region General 
        internal void Restart(LoadSceneMode mode, bool fadeLoadingScreen = false, ButtonActionSimple onComplete = null)
        {
            if (LoadingPanel)
                LoadingPanel.LoadingAsync(SceneManager.GetActiveScene().buildIndex, mode, fadeLoadingScreen, onComplete);
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, mode);
                OnButtonClicked(onComplete);
            }
        }

        internal void GotoHome(LoadSceneMode mode, bool fadeLoadingScreen = false, ButtonActionSimple onComplete = null)
        {
            if (LoadingPanel)
                LoadingPanel.LoadingAsync(SceneManager.GetActiveScene().buildIndex - 1, mode, fadeLoadingScreen, onComplete);
            else 
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1, mode);
                OnButtonClicked(onComplete);
            } 
        }
        void GoToGamePlay(LoadSceneMode mode, bool fadeLoadingScreen = false, ButtonActionSimple onComplete = null)
        {
            if (LoadingPanel)
                LoadingPanel.LoadingAsync(SceneManager.GetActiveScene().buildIndex + 1, mode, fadeLoadingScreen, onComplete);
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, mode);
                OnButtonClicked(onComplete);
            }
        }

        public string OpenMoreGames()
        {
            if (Application.platform == RuntimePlatform.Android)
                return SharedVariables.AndroidMoreGamesLink;
            else
                return SharedVariables.IOSMoreGamesLink;
        }
     
        public string OpenRateUs()
        {
            if (Application.platform == RuntimePlatform.Android)
                return SharedVariables.AndroidRateUsLink;
            else
                return SharedVariables.IOSRateUsLink;
        }    
        
        public string OpenPrivacyPolicy()
        {
            if (Application.platform == RuntimePlatform.Android)
                return SharedVariables.AndroidPrivacyPolicyLink;
            else
                return SharedVariables.IOSPrivacyPolicyLink;
        }
        #endregion


        #region Loading

        internal void FakeLoadScene(ButtonActionSimple onComplete)
        {
            LoadingPanel?.FakeLoading(onComplete);
        } 
        
        internal void FakeLoadScene(Action onComplete)
        {              
            LoadingPanel?.FakeLoading(onComplete);
        }

        internal void LoadScene<T>(T scene)
        {            
            if (LoadingPanel)
            {
                if (typeof(T) == typeof(int))
                {
                    LoadingPanel.LoadScene((int)(object)scene);
                }
                else if (typeof(T) == typeof(string))
                {
                    LoadingPanel.LoadScene((string)(object)scene);
                }
                else
                {
                    Debug.LogError("Invalid scene type");
                }
            }
        }

        internal void ManualFadeLoading()
        {
            LoadingPanel.FadeOutLoadingScreen();
        }
        #endregion


        #region ButtonActions
        public void OnButtonClicked(ButtonAction onButton)
        {
            Action onClick = null;
            if (onButton.OnBtnClick.Equals(OnClickAction.URL))
            {
                string url = onButton.URL switch
                {
                    URLs.RateUs => OpenRateUs(),
                    URLs.MoreGames => OpenMoreGames(),
                    URLs.PrivacyPolicy => OpenPrivacyPolicy(),
                    _ => ""
                };
                Application.OpenURL(url);
            }
            else if (onButton.OnBtnClick.Equals(OnClickAction.Popup))
            {
                ShowPopup(onButton.popupSetting);
            }
            else if (onButton.OnBtnClick.Equals(OnClickAction.LoadScene))
            {
                onClick = onButton.LoadScene switch
                {
                    LoadSceneBy.Restart => () => LoadingPanel.ReloadScene(),/* Restart(onButton.LoadSceneMode, onButton.ManuallyFadeLoading, onButton.onComplete),*/
                    LoadSceneBy.LoadNextScene => () => LoadingPanel.LoadingAsync(SceneManager.GetActiveScene().buildIndex + 1, onButton.LoadSceneMode, onButton.ManuallyFadeLoading, onButton.onComplete),
                    LoadSceneBy.loadPreviousScene => () => LoadingPanel.LoadingAsync(SceneManager.GetActiveScene().buildIndex - 1, onButton.LoadSceneMode, onButton.ManuallyFadeLoading, onButton.onComplete),
                    LoadSceneBy.MainMenu => () => GotoHome(onButton.LoadSceneMode, onButton.ManuallyFadeLoading, onButton.onComplete),
                    LoadSceneBy.Gameplay => () => GoToGamePlay(onButton.LoadSceneMode, onButton.ManuallyFadeLoading, onButton.onComplete),
                    LoadSceneBy.ByName => () => LoadingPanel.LoadingAsync(onButton.SceneName, onButton.LoadSceneMode, onButton.ManuallyFadeLoading, onButton.onComplete),
                    LoadSceneBy.ByID => () => LoadingPanel.LoadingAsync(onButton.SceneID, onButton.LoadSceneMode, onButton.ManuallyFadeLoading, onButton.onComplete),
                    LoadSceneBy.SceneAsset => () => LoadingPanel.LoadingAsync(onButton.m_Scene.SceneName, onButton.LoadSceneMode, onButton.ManuallyFadeLoading, onButton.onComplete),
                    _ => null
                };
            }
            else if (onButton.OnBtnClick.Equals(OnClickAction.UnloadScene))
            {
                onClick = onButton.UnloadScene switch
                {
                    UnloadScene.MainMenu => () => LoadingPanel.UnloadAdditiveScene(SharedVariables.MainMenu, onButton.onComplete),
                    UnloadScene.Gameplay => () => LoadingPanel.UnloadAdditiveScene(SharedVariables.Gameplay, onButton.onComplete),
                    UnloadScene.Splash => () => LoadingPanel.UnloadAdditiveScene(SharedVariables.Splash, onButton.onComplete),
                    _ => null
                };
            }
            else
            {
                onClick = onButton.OnBtnClick switch
                {
                    OnClickAction.ShowPanel => () => ShowPanel(onButton.Panel),
                    OnClickAction.ShowPanelInd => () => ShowPanel_Ind(onButton.Panel),
                    OnClickAction.HideCurrentPanel => () => HideCurrentPanel(),
                    OnClickAction.HideIndPanel => () => HidePanel_Ind(),
                    OnClickAction.ExitGame => () => Application.Quit(),
                    OnClickAction.AddClickEvent => () => onButton.OnClickEvent?.Invoke(),
                    _ => null
                };
            }

            onButton.OnClickButton?.Invoke();
            onClick?.Invoke();
            AudioPlayer.instance?.PlayButtonSound();
        }

        public void OnButtonClicked(ButtonActionSimple onButton)
        {
            Action onClick = null;
            if (onButton == null || onButton.OnBtnClick == OnClickAction.None)
            {
                return;
            }
            else if (onButton.OnBtnClick.Equals(OnClickAction.URL))
            {
                string url = onButton.URL switch
                {
                    URLs.RateUs => OpenRateUs(),
                    URLs.MoreGames => OpenMoreGames(),
                    URLs.PrivacyPolicy => OpenPrivacyPolicy(),
                    _ => ""
                };
                Application.OpenURL(url);
            }
            else if (onButton.OnBtnClick.Equals(OnClickAction.LoadScene))
            {
                onClick = onButton.LoadScene switch
                {
                    LoadSceneBy.Restart => () => LoadingPanel.ReloadScene(), /*, Restart(onButton.LoadSceneMode),*/
                    LoadSceneBy.LoadNextScene => () => LoadingPanel.LoadingAsync(SceneManager.GetActiveScene().buildIndex + 1, onButton.LoadSceneMode),
                    LoadSceneBy.loadPreviousScene => () => LoadingPanel.LoadingAsync(SceneManager.GetActiveScene().buildIndex - 1, onButton.LoadSceneMode),
                    LoadSceneBy.MainMenu => () => GotoHome(onButton.LoadSceneMode),
                    LoadSceneBy.Gameplay => () => GoToGamePlay(onButton.LoadSceneMode),
                    LoadSceneBy.ByName => () => LoadingPanel.LoadingAsync(onButton.SceneName, onButton.LoadSceneMode),
                    LoadSceneBy.ByID => () => LoadingPanel.LoadingAsync(onButton.SceneID, onButton.LoadSceneMode),
                    _ => null
                };
            }
            else if (onButton.OnBtnClick.Equals(OnClickAction.UnloadScene))
            {
                onClick = onButton.UnloadScene switch
                {
                    UnloadScene.MainMenu => () => LoadingPanel.UnloadAdditiveScene(SharedVariables.MainMenu),
                    UnloadScene.Gameplay => () => LoadingPanel.UnloadAdditiveScene(SharedVariables.Gameplay),
                    UnloadScene.Splash => () => LoadingPanel.UnloadAdditiveScene(SharedVariables.Splash),
                    _ => null
                };
            }
            else
            {
                onClick = onButton.OnBtnClick switch
                {
                    OnClickAction.ShowPanel => () => ShowPanel(onButton.Panel),
                    OnClickAction.ShowPanelInd => () => ShowPanel_Ind(onButton.Panel),
                    OnClickAction.HideCurrentPanel => () => HideCurrentPanel(),
                    OnClickAction.HideIndPanel => () => HidePanel_Ind(),
                    OnClickAction.ExitGame => () => Application.Quit(),
                    OnClickAction.AddClickEvent => () => onButton.OnClickEvent?.Invoke(),
                    _ => null
                };
            } 

            onButton.OnClickButton?.Invoke();
            onClick?.Invoke();
            AudioPlayer.instance?.PlayButtonSound();
        }
        #endregion


        #region Show Popup

        internal void ShowPopup(PopupSetting popupSetting)
        {
            if (!popupPanel)
            {
                var popup = GetPanel_Ind(PanelType.Popup);
                popupPanel = popup.Panel.GetComponent<PopupPanel>();
            }

            if (popupPanel)
            {
                popupPanel.ShowPopup(popupSetting, this);
            }
            else
            {
                Debug.LogError("PopupPanel is not assigned or found in the scene.");
            }
        }      
        #endregion
    }

    #region Defined Variables
    [Serializable]
    public enum PanelType
    {
        None, LoadingScene, LoadingPopup, MainMenu, ModeSelection, Customization, LevelSelection, Setting, GPSetting, Gameplay,
        GamePause, Restart, LevelComplete, LevelFail, Popup, QuitPanel, Warning, RewardPanel
    }

    [Serializable]
    public struct PanelSettings
    {
        public PanelType panelType;
        public GameObject Panel;
    }

    [Serializable]
    public enum OnClickAction
    {
        None, 
        URL,
        Popup,
        ExitGame,
        ShowPanel,
        LoadScene,
        UnloadScene,
        ShowPanelInd,
        AddClickEvent,
        HideCurrentPanel,
        HideIndPanel,
    }

    [Serializable]
    public enum URLs
    {
        None, MoreGames, RateUs, PrivacyPolicy
    }

    public enum LoadSceneBy
    {
        None,
        Restart,
        LoadNextScene,
        loadPreviousScene,
        MainMenu,
        Gameplay,
        ByName,
        ByID,
        SceneAsset
    }

    public enum UnloadScene
    {
        None, 
        Splash,
        MainMenu,
        Gameplay
    }

    [Serializable]
    public class ButtonAction
    {
        public Button button = null;
        [ConditionalField(nameof(button))]
        public bool Interactable = true;
        [ConditionalField(nameof(button))]
        public bool AdsOnClick = false;
        [ConditionalField(nameof(button))]
        public OnClickAction OnBtnClick;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.ShowPanel, OnClickAction.ShowPanelInd)]
        public PanelType Panel = PanelType.None;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.LoadScene)]
        public LoadSceneBy LoadScene = LoadSceneBy.None;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.UnloadScene)]
        public UnloadScene UnloadScene = UnloadScene.None;
        [ConditionalField(nameof(LoadScene), false, LoadSceneBy.ByName)]
        public string SceneName = "";
        [ConditionalField(nameof(LoadScene), false, LoadSceneBy.ByID)]
        public int SceneID = -1;
        [ConditionalField(nameof(LoadScene), false, LoadSceneBy.SceneAsset)]
        public SceneReference m_Scene;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.LoadScene)]
        public LoadSceneMode LoadSceneMode = LoadSceneMode.Single;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.LoadScene)]
        public bool ManuallyFadeLoading = false;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.URL)]
        public URLs URL = URLs.None;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.Popup)]
        public PopupSetting popupSetting = null;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.AddClickEvent)]
        public UnityEvent OnClickEvent = null;
        public Action OnClickButton = null;
        public ButtonActionSimple onComplete = null;
    }

    [Serializable]
    public class ButtonActionSimple
    {
        public OnClickAction OnBtnClick;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.ShowPanel, OnClickAction.ShowPanelInd)]
        public PanelType Panel;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.LoadScene)]
        public LoadSceneBy LoadScene;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.UnloadScene)]
        public UnloadScene UnloadScene = UnloadScene.None;
        [ConditionalField(nameof(LoadScene), false, LoadSceneBy.ByName)]
        public string SceneName;
        [ConditionalField(nameof(LoadScene), false, LoadSceneBy.ByID)]
        public string SceneID;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.LoadScene)]
        public LoadSceneMode LoadSceneMode = LoadSceneMode.Single;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.URL)]
        public URLs URL;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.AddClickEvent)]
        public UnityEvent OnClickEvent;
        public Action OnClickButton;
    }

    [Serializable]
    public struct HintPopup
    {
        [Space]
        public AddListeners Buttons;
        public Text Title;
        public Image HintImage;
    }

    [Serializable]
    public struct Popup
    {
        [Space]
        public TextMeshProUGUI Title;
        public TextMeshProUGUI Message;
        public Button FirstBtn;
        public Button SecondBtn;
    }
    #endregion
}
