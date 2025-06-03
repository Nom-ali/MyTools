using DG.Tweening;
using MyBox;
using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class DoFadeCanvas : AnimationBase
{
    [ReadOnly, SerializeField] private CanvasGroup FadeObject;
    [ReadOnly, SerializeField]  private float orignalAlpha = 0;

    public override void Awake()
    {
        if (FadeObject == null)
            FadeObject = rectTransform.GetComponent<CanvasGroup>();
        orignalAlpha = FadeObject.alpha;

        if(orignalAlpha > 0f)
            FadeObject.DOFade(orignalAlpha, 0.01f);

        if(ChildList.Count > 0)
            ChildList.ForEach(child => child.gameObject.SetActive(false));
    }

    internal override void OnEnable()
    {        
        if (PlayOnEnable)
        Show();
    }

    public override void Hide()
    {
        OnHide(null);
    }

    public override void Hide(Action action)
    {
       OnHide(action);
    }

    public override void Show()
    {
        OnShow(null);
    }

    public override void Show(Action action)
    {
       OnShow(action);  
    }

    internal override void OnShow(Action action)
    {
        Debug.Log("Show Called: " + gameObject.name, gameObject);
        if (gameObject.activeSelf == false)
            gameObject.SetActive(true);

        FadeObject.DOFade(0,0.01f);

        if (SelectShowEvents.AddEvents.HasFlag(EventTypes.OnStart))
            SelectShowEvents.m_OnStart?.Invoke();

        if (Callings.ShowCall.CallingTime.Equals(CallTime.OnStart) && Callings.CallThis.HasFlag(Delays.Show))
            ShowCalls();

        //Animating
        FadeObject.DOFade(orignalAlpha, ShowDuration).SetEase(ShowEase).SetUpdate(isTimeIndependent)
            .SetDelay(GetDelays())
            .OnComplete(() =>
            {
                if (SelectShowEvents.AddEvents.HasFlag(EventTypes.OnComplete))
                    SelectShowEvents.m_OnComplete?.Invoke();

                if (Callings.ShowCall.CallingTime.Equals(CallTime.OnComplete) && Callings.CallThis.HasFlag(Delays.Show))
                {
                    ShowCalls();
                }
                action?.Invoke();
            });
    }

    internal override void OnHide(Action action)
    {
        Debug.Log("Hide Called: " + gameObject.name, gameObject);

        FadeObject.DOFade(orignalAlpha, 0.01f);

        if (SelectHideEvents.AddEvents.HasFlag(EventTypes.OnStart))
            SelectHideEvents.m_OnStart?.Invoke();

        if (Callings.HideCall.CallingTime.Equals(CallTime.OnStart) && Callings.CallThis.HasFlag(Delays.Hide))
            HideCalls();

        //Animating
        FadeObject.DOFade(0, ShowDuration).SetEase(HideEase).SetUpdate(isTimeIndependent)
            .SetDelay(GetDelays())
            .OnComplete(() =>
            {
                if (SelectHideEvents.AddEvents.HasFlag(EventTypes.OnComplete))
                    SelectHideEvents.m_OnComplete?.Invoke();

                if (Callings.HideCall.CallingTime.Equals(CallTime.OnComplete) && Callings.CallThis.HasFlag(Delays.Hide))
                {
                    HideCalls(() =>
                    {
                        if (DisableOnHide)
                            gameObject.SetActive(false);
                    });
                }
                else if (DisableOnHide)
                    gameObject.SetActive(false);

                action?.Invoke();
            });
    }
}