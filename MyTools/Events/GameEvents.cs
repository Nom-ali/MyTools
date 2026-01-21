using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class GameEvents
{
    private static readonly Dictionary<string, Delegate> eventTable = new();

    // ------------------------------
    // Subscribe
    // ------------------------------
    public static void Subscribe(string key, Action listener)
    {
        if (eventTable.TryGetValue(key, out var del))
        {
            if (del is Action)
                eventTable[key] = Delegate.Combine(del, listener);
            else
                throw new InvalidOperationException($"Event key '{key}' already registered with a different type.");
        }
        else
        {
            eventTable[key] = listener;
        }
    }

    public static void Subscribe<T>(string key, Action<T> listener)
    {
        if (eventTable.TryGetValue(key, out var del))
        {
            if (del is Action<T>)
                eventTable[key] = Delegate.Combine(del, listener);
            else
                throw new InvalidOperationException($"Event key '{key}' already registered with a different type.");
        }
        else
        {
            eventTable[key] = listener;
        }
    }

    // ------------------------------
    // Unsubscribe
    // ------------------------------
    public static void Unsubscribe(string key, Action listener)
    {
        if (eventTable.TryGetValue(key, out var del) && del is Action)
        {
            var currentDel = Delegate.Remove(del, listener);
            if (currentDel == null) eventTable.Remove(key);
            else eventTable[key] = currentDel;
        }
    }

    public static void Unsubscribe<T>(string key, Action<T> listener)
    {
        if (eventTable.TryGetValue(key, out var del) && del is Action<T>)
        {
            var currentDel = Delegate.Remove(del, listener);
            if (currentDel == null) eventTable.Remove(key);
            else eventTable[key] = currentDel;
        }
    }

    // ------------------------------
    // Publish
    // ------------------------------
    public static void Publish(EventKeys key)
    {
        if (eventTable.TryGetValue(key.ToString(), out var del) && del is Action callback)
        {
            callback.Invoke();
        }
        else
        {
            UnityEngine.Debug.Log("<color=red>No Event Found Againt key:</color> " + key);
        }
    }

    public static void Publish(string key)
    {
        if (eventTable.TryGetValue(key, out var del) && del is Action callback)
        {
            callback.Invoke();
        }
        else
        {
            UnityEngine.Debug.Log("<color=red>No Event Found Againt key:</color> " + key);
        }
    }

    public static void Publish<T>(string key, T eventData)
    {
        if (eventTable.TryGetValue(key, out var del) && del is Action<T> callback)
        {
            callback.Invoke(eventData);
        }
        else
        {
            UnityEngine.Debug.Log("<color=red>No Event Found Againt key:</color> " + key);
        }
    }
}
