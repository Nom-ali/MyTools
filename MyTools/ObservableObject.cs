
using System.Collections.Generic;
using UnityEngine;
using RNA.SaveManager;

public enum ObservableState
{
    Begin,
    Changed
}

public class ObservableObject<T>
{
    protected T _value = default;

    public bool ShowDebugLogs = false;
    public System.Action<ObservableState> OnStateChanged;
    public System.Action OnValueChanged;
    private bool RunOnce = false;

    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                if (ShowDebugLogs) Debug.Log($"New Value {value} is  not equal to the old value.");
                NotifyObservers(ObservableState.Begin);
                _value = value;
                NotifyOnValueChanged();
                NotifyObservers(ObservableState.Changed);
            }
            else if(!RunOnce)
            {
                if (ShowDebugLogs) Debug.Log($"New Value {value} is equal to the old value. \n Running Notify once.");
                NotifyOnValueChanged();
                NotifyObservers(ObservableState.Changed);
                RunOnce = true;
            }
        }
    }

    public void Initialize(string Key, UnityEngine.UI.Text coinsText = null, T defaultValue = default(T))
    {
        OnValueChanged += () => 
        {
            if (coinsText) coinsText.text = Value.ToString();
            if (ShowDebugLogs) Debug.Log("Updating texting value");
        };

        OnValueChanged += () =>
            SaveManager.Prefs.SetObject(Key, Value, ShowDebugLogs);

        Value = SaveManager.Prefs.GetObject(Key, defaultValue, ShowDebugLogs);
    }


    protected void NotifyObservers(ObservableState state)
    {
        if (OnStateChanged != null)
        {
            if (ShowDebugLogs) Debug.Log("On State changed, Checking current state: " + state);
            OnStateChanged.Invoke(state);
        }
    }

    protected void NotifyOnValueChanged()
    {
        if (OnValueChanged != null)
        {
            if (ShowDebugLogs) Debug.Log("On Value Changed");
            OnValueChanged.Invoke();
        }
    }
}