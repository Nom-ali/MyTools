using System.Collections;
using UnityEngine;
using MyBox;
using RNA;
using System;
using RNA.SaveManager;
using TMPro;
using System.Collections.Generic;
using RNA.LoadingManager;

public class GameManager : UIManagerBase
{
    public static GameManager Instance;
    [Separator("********** Additional UI **********")]
    [SerializeField] private TextMeshProUGUI LevelText;

    [Separator("********** Testing **********")]
    [SerializeField] private bool TestLevel = false;
    [ConditionalField(nameof(TestLevel), false)]
    [SerializeField] private int TestLevelNo = 0;

    [Separator("********** Gameplay **********")]
    [SerializeField] private GameStatus gameState;

    [Separator("********** Levels **********")]
    [SerializeField] private LevelBase CurrentLevel = null;
    [SerializeField] private LevelBase[] LevelsList;

    [SerializeField] private ParticleSystem particle;

    [SerializeField] private int LevelNo;
    public int CurrentLevelNo => LevelNo;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    internal override IEnumerator Start()
    {
        yield return base.Start();

        yield return LoadLevel();
    }

    IEnumerator LoadLevel()
    {
        LevelNo = SaveManager.Prefs.GetInt(SharedVariables.CurrentLevelNo);
        int? levelNo = null;

        if (TestLevel)
            levelNo = TestLevelNo;
        else
            levelNo = SaveManager.GetLevel(LevelText, false);

        yield return new WaitUntil(() => levelNo != null);

        CurrentLevel = LevelsList[(int)levelNo];
        CurrentLevel.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);

        if(LoadingScript.Instance)
            LoadingScript.Instance.FadeOutLoadingScreen();
    }

    public override void GameplaySetting()
    {
        base.GameplaySetting();

        if (gameplay.HintBtn)
        {
            gameplay.HintBtn.onClick.RemoveAllListeners();
            gameplay.HintBtn.onClick.AddListener(() =>
            {
                //TODO Rewarded Ads here
                AdsManager.Instance?.ShowRewardedAds(() => ShowHintPopup(CurrentLevel.HintSprite, 5));
            });
        }
        
        if (gameplay.SkipLevelBtn)
        {
            gameplay.SkipLevelBtn.onClick.RemoveAllListeners();
            gameplay.SkipLevelBtn.onClick.AddListener(() =>
            {
                //TODO Rewarded Ads here\
                AdsManager.Instance?.ShowRewardedAds(() =>
                {
                    SaveManager.SaveNextLevel(LevelsList.Length);
                    Restart(true);
                });
            });
        }
    }

    public void CheckLevelComplete()
    {
        if (CurrentLevel)
        {
            bool Check = Array.TrueForAll(CurrentLevel.objectList, Item => Item.ItemPlaced == true);

            if (Check)
            {
                Debug.Log("********** <color=yellow> LEVEL COMPLETE </color>**********");
                LevelComplete();
            }
        }
    }

    void LevelComplete()
    {
        if (gameState.CurrentGameState == GameStatus.GameState.None)
        {
            ShowPanel(PanelType.LevelComplete);
            gameState.SetGameStatus(GameStatus.GameState.LevelComplete);
            SaveManager.SaveNextLevel(LevelsList.Length, true);
            this.InvokeAfterDelay(1, () =>
            {
                AdsManager.Instance?.ShowInterAds();
            });
        }
    }
    
    //void LevelFail()
    //{
    //    if (gameState.CurrentGameState == GameStatus.GameState.None)
    //    {
    //        ShowPanel(PanelType.LevelFail);
    //        gameState.SetGameStatus(GameStatus.GameState.LevelFail);
    //        this.InvokeAfterDelay(1, () =>
    //        {
    //            AdsManager.Instance?.ShowInterAds();
    //        });
    //    }
    //}

    public GameStatus.GameState CurrentState()
    {
        return gameState.CurrentGameState;
    }

    public void ShowParticle(Vector3 pos)
    {
        particle.transform.position = new Vector3(pos.x, pos.y, -1);
        particle.Play();
    }
}
