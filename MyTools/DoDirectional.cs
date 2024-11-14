using UnityEngine;
using System;
using DG.Tweening;

public class DoDirectional : AnimationBase
{
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public enum ScaleOption
    {
        None,
        ScaleIn,
        ScaleOut
    }

    [Header("********** Directional **********")]
    [SerializeField] private Direction OnShowDirection;
    [SerializeField] private Direction OnHideDirection;
    [SerializeField] private float multiplier = 1.0f;
    [SerializeField] private bool Scaling = false;
    private Vector3 TargetScale;
    private Vector3 OrignalPosition;

    public override void Awake()
    {
        TargetScale = rectTransform.localScale;
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
        if (gameObject.activeSelf == false)
            gameObject.SetActive(true);

        Vector2 targetPosition = GetStartPosition(OnShowDirection);
        OrignalPosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = targetPosition;
      

        if (Scaling) rectTransform.localScale = Vector3.zero;

        if (SelectShowEvents.AddEvents.HasFlag(EventTypes.OnStart))
            SelectShowEvents.m_OnStart?.Invoke();

        if (Callings.ShowCall.CallingTime.Equals(CallTime.OnStart) && Callings.CallThis.HasFlag(Delays.Show))
            ShowCalls();

        if(Scaling) rectTransform.DOScale(TargetScale, ShowDuration).SetEase(ShowEase).SetUpdate(isTimeIndependent).SetDelay(GetDelays());
        rectTransform.DOAnchorPos(OrignalPosition, ShowDuration).SetEase(ShowEase).SetUpdate(isTimeIndependent)
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
        Vector2 targetPosition = GetStartPosition(OnHideDirection);
        if (Scaling) rectTransform.localScale = TargetScale;

        if (SelectHideEvents.AddEvents.HasFlag(EventTypes.OnStart))
            SelectHideEvents.m_OnStart?.Invoke();

        if (Callings.HideCall.CallingTime.Equals(CallTime.OnStart) && Callings.CallThis.HasFlag(Delays.Hide))
            HideCalls();

        if (Scaling) rectTransform.DOScale(Vector3.zero, HideDuration).SetEase(HideEase).SetUpdate(isTimeIndependent).SetDelay(GetDelays());
        rectTransform.DOAnchorPos(targetPosition, HideDuration).SetEase(HideEase).SetUpdate(isTimeIndependent)
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
                       {
                           rectTransform.anchoredPosition = OrignalPosition;
                           gameObject.SetActive(false);
                       }
                   });
               }
               else if (DisableOnHide)
               {
                   rectTransform.anchoredPosition = OrignalPosition;
                   gameObject.SetActive(false);
               }

               action?.Invoke();
           });
    }

    Vector2 GetStartPosition(Direction moveDirection)
    {
     
        Vector2 startPos = rectTransform.anchoredPosition;
        switch (moveDirection)
        {
            case Direction.Left:
                startPos.x -= Screen.width * multiplier;
                break;
            case Direction.Right:
                startPos.x += Screen.width * multiplier;
                break;
            case Direction.Up:
                startPos.y += Screen.height * multiplier;
                break;
            case Direction.Down:
                startPos.y -= Screen.height * multiplier;
                break;
            case Direction.TopLeft:
                startPos.x -= Screen.width * multiplier;
                startPos.y += Screen.height * multiplier;
                break;
            case Direction.TopRight:
                startPos.x += Screen.width * multiplier;
                startPos.y += Screen.height * multiplier;
                break;
            case Direction.BottomLeft:
                startPos.x -= Screen.width * multiplier;
                startPos.y -= Screen.height * multiplier;
                break;
            case Direction.BottomRight:
                startPos.x += Screen.width * multiplier;
                startPos.y -= Screen.height * multiplier;
                break;
        }
        return startPos;
    }
}
