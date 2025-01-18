using System;
using System.Collections.Generic;
using UI.Event;
using UnityEngine;

public class EventAggregator : UnitySingleton<EventAggregator>
{
    private Dictionary<Type, List<Action<IEvent>>> _eventActionMap = new();

    public void RaiseEvent<T>(T payload) where T : IEvent
    {
        if (!_eventActionMap.ContainsKey(typeof(T)))
        {
            return;
        }

        foreach (var action in _eventActionMap[typeof(T)])
        {
            action.Invoke(payload);
        }
    }

    public void AddEventListener<T>(Action<T> action) where T : IEvent
    {
        if (!_eventActionMap.ContainsKey(typeof(T)))
        {
            _eventActionMap.Add(typeof(T), new List<Action<IEvent>>());
        }
        _eventActionMap[typeof(T)].Add(e => action((T)e));
    }
    
    public void RemoveEventListener<T>(Action<T> action) where T : IEvent
    {
        if (_eventActionMap.ContainsKey(typeof(T)))
        {
            _eventActionMap[typeof(T)].RemoveAll(a => a != null &&
                                                      a.Target == action.Target &&
                                                      a.Method == action.Method);
        }
    }

    public void RemoveAllEventListeners<T>() where T : IEvent
    {
        if (_eventActionMap.ContainsKey(typeof(T)))
        {
            _eventActionMap[typeof(T)].Clear();
        }
    }

    public void RemoveAll()
    {
        _eventActionMap.Clear();
    }
}
