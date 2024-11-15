using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using RNA.LoadingManager;
using System.Collections;
using UnityEngine.Events;
using RNA.TextWritter;
using UnityEngine.UI;
using UnityEngine;
using System;
using MyBox;
using TMPro;

namespace RNA
{
    public abstract class UIManagerBase : MonoBehaviour
    {
        #region Variables
        [Foldout("---------------- UI Manager ----------------", true)]
        [ReadOnly]
        [SerializeField] internal GameObject currentPanel;

        [Header("---------- Panels ----------")]
        [SerializeField] internal PanelSettings[] PanelsList;

        [Header("---------- Select Panel ----------")]
        [SerializeField] internal PanelSelection AddPanels;

        [Space]
        [ConditionalField(nameof(AddPanels), false, PanelSelection.MainMenu)]
        [SerializeField] internal MainMenu mainMenu;
      
        [ConditionalField(nameof(AddPanels), false, PanelSelection.Gameplay)]
        [SerializeField] internal Gameplay gameplay;
      
        [ConditionalField(nameof(AddPanels), false, PanelSelection.LevelComplete)]
        [SerializeField] internal LevelComplete levelComplete;
       
        [ConditionalField(nameof(AddPanels), false, PanelSelection.LevelFail)]
        [SerializeField] internal LevelFail levelFail; 
       
        [ConditionalField(nameof(AddPanels), false, PanelSelection.GamePause)]
        [SerializeField] internal GamePause gamePause;
     
        [ConditionalField(nameof(AddPanels), false, PanelSelection.Popup)]
        [SerializeField] internal Popup popup;

        [ConditionalField(nameof(AddPanels), false, PanelSelection.HintPopup)]
        [SerializeField] internal HintPopup hintPopup; 

        [ConditionalField(nameof(AddPanels), false, PanelSelection.QuitPanel)]
        [SerializeField] internal Quit quit;
        
        [ConditionalField(nameof(AddPanels), false, PanelSelection.RewardPanel)]
        [SerializeField] internal RewardADPanel rewardADPanel;

        [Header("---------- Additional UI ----------")]
        [SerializeField] internal TextMeshProUGUI CoinsText;
        #endregion


        #region Start
        internal virtual IEnumerator Start()
        {
            Application.targetFrameRate = 60;

            if (CoinsText)
            {
                SaveManager.SaveManager.Currency = new();
                SaveManager.SaveManager.Currency.Initialize(SharedVariables.Coins, CoinsText, 10);
            }

            if (AddPanels.HasFlag(PanelSelection.MainMenu))
                MainMenuSetting();

            if (AddPanels.HasFlag(PanelSelection.Gameplay))
                GameplaySetting(); 
            yield return null;
        }
        #endregion


        #region Panel Setting
        public PanelSettings GetPanel(PanelType panelType)
        {
            return Array.Find(PanelsList, panel => panel.panelType == panelType);
        }

        GameObject OldPanel = null;
        private GameObject Show_Panel(PanelType panelType, bool disableOldPanel = true, Action OnComplete = null)
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
                animationBase.Show(OnShow);
            else
            {
                panel.Panel?.SetActive(true);
                OnShow?.Invoke();
            }

            currentPanel = panel.Panel;
            return panel.Panel;
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
                    Debug.Log($"Setting ON Panel: {PanelsList[i].Panel.name}", PanelsList[i].Panel);
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

        /// <summary>
        /// Show Hint Popup 
        /// </summary>
        /// <param name="hintSprite">Sprite, that will be shown in Popup</param>
        /// <param name="autoCloseDelay">Hint Popup Auto Closes after given time</param>
        /// <param name="title">title (Optional)</param>
        public async void ShowHintPopup(Sprite hintSprite, float autoCloseDelay = 4, string title = "")
        {
            if (hintSprite == null)
                return;

            PanelSettings panelSetting = Array.Find(PanelsList, panel => panel.panelType == PanelType.HintPopup);
            if(panelSetting.Panel != null)
            {
                if (panelSetting.Panel.TryGetComponent(out AnimationBase animationBase))
                    animationBase.Show();

                if(title.NotNullOrEmpty())
                    hintPopup.Title.text = title;

                hintPopup.HintImage.sprite = hintSprite;

                await Task.Delay((int)(autoCloseDelay * 1000));
                animationBase.Hide();
            }
        }

        public GameObject ShowPanel(PanelType panelType)
        {
            Show_Panel(panelType);
        }
        
        public GameObject ShowPanel(PanelType panelType, bool disableOldPanel = false)
        {
            Show_Panel(panelType, disableOldPanel);
        } 
        
        public GameObject ShowPanel(PanelType panelType, Action OnComplete)
        {
            Show_Panel(panelType, OnComplete: OnComplete);
        }

        public void AssignPanelSetting(PanelType panelType, Action OnComplete = null)
        {
            switch (panelType)
            {

                case PanelType.MainMenu:
                    MainMenuSetting();
                    break;

                case PanelType.Gameplay:
                    GameplaySetting();
                    break;

                case PanelType.LevelComplete:
                    LevelCompleteSetting();
                    break;

                case PanelType.LevelFail:
                    LevelFailSetting();
                    break; 
                
                case PanelType.GamePause:
                    GamePasueSetting();
                    break;   
                
                case PanelType.QuitPanel:
                    QuitSetup();
                    break;    

                case PanelType.RewardPanel:
                    RewardAdPanelSetup();
                    break;
            }

            //action on Complete
            OnComplete?.Invoke();
        }

        #endregion


        #region General 
        internal void Restart(bool fadeLoadingScreen = false)
        {
            if (LoadingScript.Instance)
                LoadingScript.Instance.LoadingAsync(SceneManager.GetActiveScene().buildIndex, fadeLoadingScreen);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        internal void GotoHome(bool fadeLoadingScreen = false)
        {
                if (LoadingScript.Instance)
                    LoadingScript.Instance.LoadingAsync(SceneManager.GetActiveScene().buildIndex - 1, fadeLoadingScreen);
                else
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
        void GoToGamePlay(bool fadeLoadingScreen = false)
        {
            if (LoadingScript.Instance)
                LoadingScript.Instance.LoadingAsync(SceneManager.GetActiveScene().buildIndex + 1, fadeLoadingScreen);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
                    URLs.PrivacyPolicy => SharedVariables.PrivacyPolicyLink,
                    _ => ""
                };
                Application.OpenURL(url);
            }
            else if (onButton.OnBtnClick.Equals(OnClickAction.Popup))
            {
                ShowPopup(onButton.popupSetting);
            }
            else if(onButton.OnBtnClick.Equals(OnClickAction.LoadScene))
            {
                onClick = onButton.LoadScene switch
                {
                    LoadSceneBy.Restart => () => Restart(onButton.ManuallyFadeLoading),
                    LoadSceneBy.LoadNextScene => () => LoadingScript.Instance.LoadingAsync(SceneManager.GetActiveScene().buildIndex + 1, onButton.ManuallyFadeLoading),
                    LoadSceneBy.loadPreviousScene => () => LoadingScript.Instance.LoadingAsync(SceneManager.GetActiveScene().buildIndex - 1, onButton.ManuallyFadeLoading),
                    LoadSceneBy.MainMenu => () => GotoHome(onButton.ManuallyFadeLoading),
                    LoadSceneBy.Gameplay => () => GoToGamePlay(onButton.ManuallyFadeLoading),
                    LoadSceneBy.ByName => () => LoadingScript.Instance.LoadingAsync(onButton.SceneName, onButton.ManuallyFadeLoading),
                    LoadSceneBy.ByID => () => LoadingScript.Instance.LoadingAsync(onButton.SceneID, onButton.ManuallyFadeLoading),
                    _ => null
                };
            }
            else
            {
                onClick = onButton.OnBtnClick switch
                {
                    OnClickAction.ShowPanel => () => ShowPanel(onButton.Panel),
                    //OnClickAction.HidePanel => () => HidePanel(onButton.Panel),
                    OnClickAction.HideCurrentPanel => () => HideCurrentPanel(),
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
            if (onButton.OnBtnClick.Equals(OnClickAction.URL))
            {
                string url = onButton.URL switch
                {
                    URLs.RateUs => OpenRateUs(),
                    URLs.MoreGames => OpenMoreGames(),
                    URLs.PrivacyPolicy => SharedVariables.PrivacyPolicyLink,
                    _ => ""
                };
                Application.OpenURL(url);
            }
            else if (onButton.OnBtnClick.Equals(OnClickAction.LoadScene))
            {
                onClick = onButton.LoadScene switch
                {
                    LoadSceneBy.Restart => () => Restart(),
                    LoadSceneBy.LoadNextScene => () => LoadingScript.Instance.LoadingAsync(SceneManager.GetActiveScene().buildIndex + 1),
                    LoadSceneBy.loadPreviousScene => () => LoadingScript.Instance.LoadingAsync(SceneManager.GetActiveScene().buildIndex - 1),
                    LoadSceneBy.MainMenu => () => GotoHome(),
                    LoadSceneBy.Gameplay => () => GoToGamePlay(),
                    LoadSceneBy.ByName => () => LoadingScript.Instance.LoadingAsync(onButton.SceneName),
                    LoadSceneBy.ByID => () => LoadingScript.Instance.LoadingAsync(onButton.SceneID),
                    _ => null
                };
            }
            else
            {
                onClick = onButton.OnBtnClick switch
                {
                    OnClickAction.ShowPanel => () => ShowPanel(onButton.Panel),
                    //OnClickAction.HidePanel => () => HidePanel(onButton.Panel),
                    OnClickAction.HideCurrentPanel => () => HideCurrentPanel(),
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


        #region Panels Assignements
        public virtual void MainMenuSetting()
        {
            if (mainMenu.PlayBtn.button)
            {
                mainMenu.PlayBtn.button.onClick.RemoveAllListeners();
                mainMenu.PlayBtn.button.onClick.AddListener(() =>
                {
                    OnButtonClicked(mainMenu.PlayBtn);
                });
            }

            if (mainMenu.SettingBtn.button)
            {
                mainMenu.SettingBtn.button.onClick.RemoveAllListeners();
                mainMenu.SettingBtn.button.onClick.AddListener(() => 
                {
                    OnButtonClicked(mainMenu.SettingBtn);
                });    
            }    
               
            if(mainMenu.PrivacyBtn.button)
            {
                mainMenu.PrivacyBtn.button.onClick.RemoveAllListeners();
                mainMenu.PrivacyBtn.button.onClick.AddListener(() =>
                {
                    OnButtonClicked(mainMenu.PrivacyBtn);
                });    
            }    
                      
            if(mainMenu.RateUsBtn.button)
            {
                mainMenu.RateUsBtn.button.onClick.RemoveAllListeners();
                mainMenu.RateUsBtn.button.onClick.AddListener( () =>
                {
                    OnButtonClicked(mainMenu.RateUsBtn);
                });
            }

            if (mainMenu.MoreGamesBtn.button)
            {
                mainMenu.MoreGamesBtn.button.onClick.RemoveAllListeners();
                mainMenu.MoreGamesBtn.button.onClick.AddListener(() =>
                {
                    OnButtonClicked(mainMenu.MoreGamesBtn);
                });
            }


            if (mainMenu.ExitBtn.button)
            {
                mainMenu.ExitBtn.button.onClick.RemoveAllListeners();
                mainMenu.ExitBtn.button.onClick.AddListener( () =>
                {
                    OnButtonClicked(mainMenu.ExitBtn);
                });
            }  
        }


        public virtual void GameplaySetting()
        {
            if (gameplay.PauseBtn.button)
            { 
                gameplay.PauseBtn.button.onClick.RemoveAllListeners();
                gameplay.PauseBtn.button.onClick.AddListener(() => 
                {
                    //Time.timeScale = 0;
                    OnButtonClicked(gameplay.PauseBtn);
                });
            }
        }
           
        void LevelCompleteSetting()
        {
            if (levelComplete.HomeBtn.button)
            {
                levelComplete.HomeBtn.button.onClick.RemoveAllListeners();
                levelComplete.HomeBtn.button.onClick.AddListener(() => {
                
                    OnButtonClicked(levelComplete.HomeBtn);
                });
            }
            
            if (levelComplete.NextBtn.button)
            {
                levelComplete.NextBtn.button.onClick.RemoveAllListeners();
                levelComplete.NextBtn.button.onClick.AddListener(() => 
                {
                    OnButtonClicked(levelComplete.NextBtn);  
                });
            }
        }  
        
        void LevelFailSetting()
        {
            if (levelFail.HomeBtn.button)
            {
                levelFail.HomeBtn.button.onClick.RemoveAllListeners();
                levelFail.HomeBtn.button.onClick.AddListener(() => {
               
                    OnButtonClicked(levelFail.HomeBtn);
                });
            }
            
            if (levelFail.RestartBtn.button)
            {
                levelFail.RestartBtn.button.onClick.RemoveAllListeners();
                levelFail.RestartBtn.button.onClick.AddListener(() => 
                {
                  
                    OnButtonClicked(levelFail.RestartBtn);
                });
            }
        }


        void GamePasueSetting()
        {
            if (gamePause.HomeBtn.button)
            {
                gamePause.HomeBtn.button.onClick.RemoveAllListeners();
                gamePause.HomeBtn.button.onClick.AddListener(() => {
                    Time.timeScale = 1;
                    OnButtonClicked(gamePause.HomeBtn);
                });
            }

            if (gamePause.RestartBtn.button)
            {
                gamePause.RestartBtn.button.onClick.RemoveAllListeners();
                gamePause.RestartBtn.button.onClick.AddListener(() =>
                {
                    Time.timeScale = 1;
                    OnButtonClicked(gamePause.RestartBtn);
                });
            } 
            
            if (gamePause.Resume.button)
            {
                //gamePause.Resume.button.onClick.RemoveAllListeners();
                gamePause.Resume.button.onClick.AddListener(() =>
                {
                    Time.timeScale = 1;
                    OnButtonClicked(gamePause.Resume);
                });
            }
        }

        void QuitSetup()
        {
            if(quit.YesButton.button)
            {
                quit.YesButton.button.onClick.RemoveAllListeners();
                quit.YesButton.button.onClick.AddListener(() =>
                {
                    OnButtonClicked(quit.YesButton);
                });
            }
            
            if(quit.NoButton.button)
            {
                quit.NoButton.button.onClick.RemoveAllListeners();
                quit.NoButton.button.onClick.AddListener(() =>
                {
                    OnButtonClicked(quit.NoButton);
                });
            }
        }

        void RewardAdPanelSetup()
        {
            //if (rewardADPanel.YesBtn.button)
            //{
            //    rewardADPanel.YesBtn.button.onClick.RemoveAllListeners();
            //    rewardADPanel.YesBtn.button.onClick.AddListener(() =>
            //    {
            //        OnButtonClicked(rewardADPanel.YesBtn);
            //    });
            //}  
            if (rewardADPanel.NoBtn.button)
            {
                rewardADPanel.NoBtn.button.onClick.RemoveAllListeners();
                rewardADPanel.NoBtn.button.onClick.AddListener(() =>
                {
                    OnButtonClicked(rewardADPanel.NoBtn);
                });
            }
        }

        #endregion


        #region Show Popup
        public async void ShowPopup(PopupSetting popupSetting)
        {
            popup.FirstBtn.gameObject.SetActive(false);
            popup.SecondBtn.gameObject.SetActive(false);

            //setting tile
            if (popup.Title)
                popup.Title.text = popupSetting.Title;

            //showing panel
            ShowPanel(PanelType.Popup, false);

            // chechk if popup is already active ir not
            while (!popup.Message.gameObject.activeInHierarchy)
                await Task.Yield();

            //writing text letter by letter
            if (popup.Message)
                popup.Message.WriteLetter(popupSetting.Message, popupSetting.TextDuration)
                    .OnComplete(async () =>
                        {
                            //enabling or disabling btn
                            popup.FirstBtn.gameObject.SetActive(popupSetting.EnableFirstBtn);
                            popup.SecondBtn.gameObject.SetActive(popupSetting.EnableSecondBtn);

                            // assigning 1st btn actions
                            if (popupSetting.EnableFirstBtn && popup.FirstBtn)
                            {
                                if (popup.FirstBtn.transform.GetChild(0).TryGetComponent(out Text text))
                                    text.text = popupSetting.FirstBtnText;
           
                                popup.FirstBtn.onClick.RemoveAllListeners();
                                popup.FirstBtn.onClick.AddListener(() =>
                                {
                                    AudioPlayer.instance?.PlayButtonSound();
                                    popupSetting.FirstBtnAction?.Invoke();
                                    OnButtonClicked(popupSetting.m_FirstBtnAction);
                                    HideCurrentPanel();
                                });
                            }

                            // assigning 2nd btn actions
                            if (popupSetting.EnableSecondBtn && popup.SecondBtn)
                            {
                                if (popupSetting.EnableSecondBtn &&  popup.SecondBtn.transform.GetChild(0).TryGetComponent(out Text text))
                                  text.text = popupSetting.SecondBtnText;
                            
                                popup.SecondBtn.onClick.RemoveAllListeners();
                                popup.SecondBtn.onClick.AddListener(() =>
                                {
                                    AudioPlayer.instance?.PlayButtonSound();
                                    popupSetting.SecondBtnAction?.Invoke();
                                    OnButtonClicked(popupSetting.m_SecondBtnAction);
                                    HideCurrentPanel();
                                });
                            }
                            // On Complete Action callback
                            popupSetting.OnComplete?.Invoke();

                            if (popupSetting.AutoClosePopup)
                            {
                                await Task.Delay((int)((popupSetting.TextDuration + popupSetting.AutoCloseDelay) * 1000));
                                HideCurrentPanel();
                            }
                        })
                    .Start();   // Starting method from here


        }
        #endregion
    }

    #region Defined Variables
    [Serializable]
    public enum PanelType
    {
        None, Loading, MainMenu, ModeSelection, Customization, LevelSelection, MMSetting, GPSetting, Gameplay, GamePause, Restart, LevelComplete, LevelFail, Popup, QuitPanel, Warning, RewardPanel, HintPopup
    }    
    
    [Serializable, Flags]
    public enum PanelSelection
    {
        None                    = 0, 
        Loading                 = 1 << 0,
        MainMenu                = 1 << 1,
        ModeSelection           = 1 << 2,
        Customization           = 1 << 3,
        LevelSelection          = 1 << 4,
        MMSetting               = 1 << 5,
        GPSetting               = 1 << 6,
        Gameplay                = 1 << 7,
        GamePause               = 1 << 8,
        Restart                 = 1 << 9,
        LevelComplete           = 1 << 10,
        LevelFail               = 1 << 11,
        Popup                   = 1 << 12,
        QuitPanel               = 1 << 13,
        Warning                 = 1 << 14,
        RewardPanel             = 1 << 15,
        HintPopup               = 1 << 16,
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
        //HidePanel,
        LoadScene,
        AddClickEvent,
        HideCurrentPanel,
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
        ByID
    }

    [Serializable]
    public struct ButtonAction
    {
        public Button button;
        [ConditionalField(nameof(button))]
        public OnClickAction OnBtnClick;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.ShowPanel /*, OnClickAction.HidePanel*/)]
        public PanelType Panel;
        //[ConditionalField(nameof(Panel), false, PanelType.None)]
        //public GameObject PanelObject;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.LoadScene)]
        public LoadSceneBy LoadScene;
        [ConditionalField(nameof(LoadScene), false, LoadSceneBy.ByName)]
        public string SceneName;
        [ConditionalField(nameof(LoadScene), false, LoadSceneBy.ByID)]
        public int SceneID;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.LoadScene)]
        public bool ManuallyFadeLoading;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.URL)]
        public URLs URL;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.Popup)]
        public PopupSetting popupSetting;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.AddClickEvent)]
        public UnityEvent OnClickEvent;
        public Action OnClickButton;
    }  
    
    [Serializable]
    public struct ButtonActionSimple
    {
        public OnClickAction OnBtnClick;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.ShowPanel)]
        public PanelType Panel;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.LoadScene)]
        public LoadSceneBy LoadScene;
        [ConditionalField(nameof(LoadScene), false, LoadSceneBy.ByName)]
        public string SceneName;
        [ConditionalField(nameof(LoadScene), false, LoadSceneBy.ByID)]
        public string SceneID;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.URL)]
        public URLs URL;
        [ConditionalField(nameof(OnBtnClick), false, OnClickAction.AddClickEvent)]
        public UnityEvent OnClickEvent;
        public Action OnClickButton;
    }


    [Serializable]
    public struct Gameplay
    {
        [Space]
        public ButtonAction PauseBtn;
        [Space]
        public Button HintBtn;
        public Button SkipLevelBtn;
    }
    
    [Serializable]
    public struct MainMenu
    {
        public ButtonAction PlayBtn;
        public ButtonAction SettingBtn;
        public ButtonAction PrivacyBtn;
        public ButtonAction RateUsBtn;
        public ButtonAction MoreGamesBtn;
        public ButtonAction ExitBtn;
    }
    
    [Serializable]
    public struct LevelComplete
    {
        [Space]
        public ButtonAction NextBtn;
        //public ButtonAction RestartBtn;
        public ButtonAction HomeBtn;
        //public ButtonAction DoubleReward;
    }
         
    [Serializable]
    public struct LevelFail
    {
        [Space]
        public ButtonAction HomeBtn;
        public ButtonAction RestartBtn;
        //public ButtonAction ReviveBtn;
    }  
    
    [Serializable]
    public struct GamePause
    {
        [Space]
        public ButtonAction Resume;
        public ButtonAction HomeBtn;
        public ButtonAction RestartBtn;
    }

    [Serializable]
    public struct Quit
    {
        [Space]
        public ButtonAction YesButton;
        public ButtonAction NoButton;
    }

    [Serializable]
    public struct RewardADPanel
    {
        [Space]
        public ButtonAction YesBtn;
        public ButtonAction NoBtn;
    }  
    
    [Serializable]
    public struct HintPopup
    {
        [Space]
        public TextMeshProUGUI Title;
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

    [Serializable]
    public class PopupSetting
    {
        public string Title = "";
        public string Message = "";
        public float TextDuration = 1;
        public bool AutoClosePopup = false;
        [ConditionalField(nameof(AutoClosePopup), false)]
        public float AutoCloseDelay = 1f;
        public bool EnableFirstBtn = true;
        public string FirstBtnText = "OK";
        public Action FirstBtnAction = null;
        [SerializeField] internal protected ButtonActionSimple m_FirstBtnAction;
        public bool EnableSecondBtn = false;
        public string SecondBtnText = "";
        public Action SecondBtnAction = null;
        [SerializeField] internal protected ButtonActionSimple m_SecondBtnAction;
        public bool AdsIcon = false;
        public Action OnComplete = null;

        // Parameterless constructor
        public PopupSetting()
        {
        }

        public PopupSetting(string message, string title = "", string firstBtnText = "", Action firstAction = null,
            bool enableSecondbtn = false , string secondBtnText = "", Action secondAction = null, bool adsIcon = false, Action onComplete = null)
        {
            Title = title;
            Message = message;
            FirstBtnText = firstBtnText;
            FirstBtnAction = firstAction;
            EnableSecondBtn = enableSecondbtn;
            SecondBtnText = secondBtnText;
            SecondBtnAction = secondAction;
            AdsIcon = adsIcon;
            OnComplete = onComplete;
        }
    }
    #endregion
}
