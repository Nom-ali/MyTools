using DG.Tweening;
using System;
using UnityEngine;

public class DoScale : AnimationBase
{
    [MyBox.ReadOnly, SerializeField] private Vector3 TargetScale;

    public override void Awake()
    {     
        TargetScale = rectTransform.localScale;
    }

    internal override void OnEnable()
    {

        if(PlayOnEnable)
        Show();
    }

    public override void Show()
    {
        OnShow(null);
    }

    public override void Hide()
    {
       OnHide(null);
    }

    public override void Show(Action action)
    {
       OnShow(action);
    }

    public override void Hide(Action action)
    {
        OnHide(action);
    }

    internal override void OnShow(Action action)
    {
        //Debug.Log("Show Called", gameObject);

        if (gameObject.activeSelf == false)
            gameObject.SetActive(true);

        rectTransform.localScale = Vector3.zero;

        if (SelectShowEvents.AddEvents.HasFlag(EventTypes.OnStart))
            SelectShowEvents.m_OnStart?.Invoke();
        
        if (Callings.ShowCall.CallingTime.Equals(CallTime.OnStart) && Callings.CallThis.HasFlag(Delays.Show))
            ShowCalls();

        rectTransform.DOScale(TargetScale, ShowDuration).SetEase(ShowEase).SetUpdate(isTimeIndependent)
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
        //Debug.Log("Hide Called", gameObject);
        rectTransform.localScale = TargetScale;

        if (SelectHideEvents.AddEvents.HasFlag(EventTypes.OnStart))
            SelectHideEvents.m_OnStart?.Invoke();

        if (Callings.HideCall.CallingTime.Equals(CallTime.OnStart) && Callings.CallThis.HasFlag(Delays.Hide))
            HideCalls();

        rectTransform.DOScale(Vector3.zero, HideDuration).SetEase(HideEase).SetUpdate(isTimeIndependent)
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (SetDefaultEase)
        {
            ShowEase = Ease.OutBack;
            HideEase = Ease.InBack;
            Debug.Log("Default Value Set");
            SetDefaultEase = false;
        }
    }
#endif
}
