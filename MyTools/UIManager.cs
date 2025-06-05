using MyTools;
using System.Collections;
using UnityEngine;

public class UIManager : UIManagerBase
{
    public static UIManager Instance;

    #region Singleton
    public override void Awake()
    {
        base.Awake();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion Singleton

    internal override IEnumerator Start()
    {
        yield return base.Start();
        FakeLoadScene(new ButtonActionSimple() { OnBtnClick = OnClickAction.ShowPanel, Panel = PanelType.MainMenu });
    }
   
    internal IEnumerator ShowLoadingPopup(float disableAfter = 3)
    {
        GameObject panel = ShowPanel_Ind(PanelType.LoadingPopup);
        float delay = panel.GetComponent<AnimationBase>().GetDelays();
        yield return new WaitForSeconds(disableAfter + delay);
        HidePanel_Ind();
        yield return new WaitUntil(() => GetPanel_Ind(PanelType.LoadingPopup).Panel.activeSelf == false);
    }

    public void GamePause()
    {
        GameManager.Instance.gameStatus.SetGameStatus(GameStatus.GameState.GamePause);
    }

    public void GameUnPause()
    {
        GameManager.Instance.gameStatus.SetGameStatus(GameStatus.GameState.None);
    }

    public void Restore_purchase()
    {
        MyTools.SaveManager.SaveManager.Prefs.SetBool(SharedVariables.RemoveAds, true);
        if (AdsManager.Instance)
            AdsManager.Instance.DestroyBanner();
    }

   
}
