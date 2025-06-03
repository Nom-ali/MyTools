using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Linq;
using System;


public abstract class AnimationBase : MonoBehaviour
{
    [Header("********** Children ********** ")]
    [ReadOnly ,SerializeField] internal List<AnimationBase> ChildList = new();
    internal RectTransform rectTransform => GetComponent<RectTransform>();

    [Header("********** Setting ********** ")]
    [SerializeField] internal bool PlayOnEnable = false;
    [SerializeField] internal bool DisableOnHide = true;
    [SerializeField] internal InitialDelays InitialDelay;
    [SerializeField] internal Calls Callings;

    [Header("********** Duration ********** ")]
    [SerializeField] internal float ShowDuration = 0.3f;
    [SerializeField] internal float HideDuration = 0.3f;

    [Header("********** Loop ********** ")]
    [SerializeField] internal int Loops = 1;
    [ConditionalField(nameof(Loops), true, 0, 1)]
    [SerializeField] internal LoopType LoopType = LoopType.Restart;

    [Header("**********  Ease ********** ")]
    [SerializeField] internal bool SetDefaultEase = false;
    [SerializeField] internal Ease ShowEase = Ease.Unset;
    [SerializeField] internal Ease HideEase = Ease.Unset;

    [Header("********** Events ********** ")]
    [SerializeField] internal DefineEvents SelectShowEvents;
    [SerializeField] internal DefineEvents SelectHideEvents;

    public bool isTimeIndependent = false;

    public virtual void Awake()
    {
        
    }

    public abstract void Show();
    public abstract void Hide();

    public abstract void Show(Action action);
    public abstract void Hide(Action action);
          
    internal abstract void OnShow(Action action);
    internal abstract void OnHide(Action action);

    internal virtual void OnEnable()
    {

    }
    internal virtual void OnDisable()
    {

    }
   
    internal void ShowCalls(Action OnComplete = null)
    { 
        if (Callings.CallThis.HasFlag(Delays.Show))
        {
            if (Callings.ShowCall.Call.Equals(RunBy.RunOneByOne))
                StartCoroutine(ShowOneByOne(OnComplete));
            else if (Callings.ShowCall.Call.Equals(RunBy.RunAllAtOnce))
                StartCoroutine(ShowAtOnce(OnComplete));
            else
                return;
        }
    }  
    
    internal void HideCalls(Action OnComplete = null)
    { 
        if (Callings.CallThis.HasFlag(Delays.Hide))
        {
            if (Callings.HideCall.Call.Equals(RunBy.RunOneByOne))
                StartCoroutine(HideOneByOne(OnComplete));
            else if (Callings.HideCall.Call.Equals(RunBy.RunAllAtOnce))
                StartCoroutine(HideAtOnce(OnComplete));
            else
                return;
        }
    }

    internal IEnumerator ShowAtOnce(Action OnComplete = null)
    {
        for (int i = 0; i < ChildList.Count; i++)
        {
            ChildList[i].Show();
            yield return null;
        }
        OnComplete?.Invoke();
    }   

    internal IEnumerator ShowOneByOne(Action OnComplete = null)
    {
        for (int i = 0; i < ChildList.Count; i++)
        {
            ChildList[i].Show(); 
            //Debug.Log("Calling no: " + i);
            yield return new WaitForSecondsRealtime(ChildList[i].ShowDuration);
        }
        OnComplete?.Invoke();
    } 
    
    internal IEnumerator HideAtOnce(Action OnComplete = null)
    {
        for (int i = 0; i < ChildList.Count; i++)
        {
            ChildList[i].Hide();
            yield return null;
        }
        OnComplete?.Invoke();
    }   

    internal IEnumerator HideOneByOne(Action OnComplete = null)
    {
        for (int i = ChildList.Count - 1; i >= 0; i--)
        {
            ChildList[i].Hide();
            yield return new WaitForSecondsRealtime(ChildList[i].HideDuration);
        }
        OnComplete?.Invoke();
    }

    internal float GetDelays()
    {
        if (InitialDelay.DelayValue.Equals(Delays.None))
            return 0;

        if (InitialDelay.InitialDelay)
        {
            if (InitialDelay.DelayValue.Equals(DelayFrom.StaticValue))
                return InitialDelay.StaticValue;
            else if (ChildList.Count > 0 && InitialDelay.DelayValue.Equals(DelayFrom.FromChild))
                return ChildList.Sum(item => item.HideDuration);
            else return 0;
        }
        else
            return 0;
    }


#if UNITY_EDITOR
    [ButtonMethod(ButtonMethodDrawOrder.BeforeInspector)]
    public void GetChildComponent()
    {
        AnimationBase[] chilList = GetComponentsInChildren<AnimationBase>(true);
        ChildList = chilList.Where(child => child.gameObject != gameObject).ToList();
        Debug.Log($"{chilList.Length} Children found.");

        if(ChildList.Count > 0)
        {
            InitialDelay.InitialDelay = true;
            InitialDelay.DelayValue = DelayFrom.FromChild;
            InitialDelay.AddIn = Delays.Hide;

            Callings.CallThis |= (Delays.Hide | Delays.Show);
            Callings.ShowCall.CallingTime = CallTime.OnComplete;
            Callings.ShowCall.Call = RunBy.RunOneByOne;
            
            Callings.HideCall.CallingTime = CallTime.OnStart;
            Callings.HideCall.Call = RunBy.RunOneByOne;

        }
    }
#endif

    private void OnValidate()
    {
        if (ChildList.Count <= 0)
        {
            Callings = new();
            Debug.Log("Calling Reset");
        }
    }

}

[Serializable]
public class DefineEvents
{
    [SerializeField] internal EventTypes AddEvents = EventTypes.None;
   
    [ConditionalField(nameof(AddEvents), false, EventTypes.OnStart)]
    public UnityEvent m_OnStart = null;

    [ConditionalField(nameof(AddEvents), false, EventTypes.OnComplete)]
    public UnityEvent m_OnComplete = null;
}

[Serializable, Flags]
public enum EventTypes
{
    None = 0,
  
    OnStart = 1 << 0,
    OnComplete = 1 << 1
}

[Serializable]
public enum DelayFrom
{
    None,
    StaticValue,
    FromChild
}
   
[Serializable, Flags]
public enum Delays
{
    None = 0,
    Show = 1 << 0,
    Hide = 1 << 1
}

[Serializable]
public enum RunBy
{
    None, 
    RunAllAtOnce,
    RunOneByOne
}

[Serializable]
public class InitialDelays
{
    [SerializeField] internal bool InitialDelay = false;
    [ConditionalField(nameof(InitialDelay), false)]
    public DelayFrom DelayValue = DelayFrom.None;
    [ConditionalField(nameof(DelayValue), true, DelayFrom.None)]
    public Delays AddIn = Delays.None;
    [ConditionalField(nameof(DelayValue), false, DelayFrom.StaticValue)]
    public float StaticValue = 0.3f;
}

[Serializable]
public class Calls
{ 
    [SerializeField] internal Delays CallThis = Delays.None;
    [ConditionalField(nameof(CallThis), false, Delays.Show)]
    [SerializeField] internal OnCall ShowCall;
    [ConditionalField(nameof(CallThis), false, Delays.Hide)]
    [SerializeField] internal OnCall HideCall;
}

[Serializable]
public struct OnCall
{
    [SerializeField] internal CallTime CallingTime;
    [SerializeField] internal RunBy Call;
}

[Serializable]
public enum CallTime
{
    None, 
    OnStart,
    OnComplete
}
