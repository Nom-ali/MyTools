using DG.Tweening;
using MyBox;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DoFade : AnimationBase
{
    [ReadOnly, SerializeField] private Image FadeObject;
    [ReadOnly, SerializeField]  private float orignalAlpha;

    public override void Awake()
    {
        if(orignalAlpha > 0f)
            FadeObject.SetAlpha(orignalAlpha);

        if (FadeObject == null)
            FadeObject = rectTransform.GetComponent<Image>();
        orignalAlpha = FadeObject.color.a;

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
        Debug.Log("Show Called", gameObject);

        //set initial value

        if (gameObject.activeSelf == false)
            gameObject.SetActive(true);

        FadeObject.DOFade(0,0.01f);

        if (SelectShowEvents.AddEvents.HasFlag(EventTypes.OnStart))
            SelectShowEvents.m_OnStart?.Invoke();

        if (Callings.ShowCall.CallingTime.Equals(CallTime.OnStart) && Callings.CallThis.HasFlag(Delays.Show))
            ShowCalls();

        //Animating
        FadeObject.DOFade(orignalAlpha, ShowDuration).SetEase(ShowEase).SetUpdate(isTimeIndependent)
            .SetDelay(InitialDelay.AddIn.HasFlag(Delays.Show) ? GetDelays() : 0)
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
        Debug.Log("Hide Called", gameObject);

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