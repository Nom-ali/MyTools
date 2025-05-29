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

        Application.targetFrameRate = 60; // Set target frame rate
    }
    #endregion Singleton

    internal override IEnumerator Start()
    {
        yield return base.Start();

        FakeLoadScene();
    }
   
    internal IEnumerator ShowLoadingPopup(float disableAfter = 3)
    {
        GameObject panel = ShowPanel_Ind(PanelType.LoadingPopup);
        float delay = panel.GetComponent<AnimationBase>().GetDelays();
        yield return new WaitForSeconds(disableAfter + delay);
        HidePanel_Ind();
        yield return new WaitUntil(() => GetPanel_Ind(PanelType.LoadingPopup).Panel.activeSelf == false);
    }
}
