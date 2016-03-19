using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IEventBus
{
    void Destroy();
    void addListener(string eventName, EventBus.Handler callbackFunction);
    void removeListener(string eventName, EventBus.Handler callbackFunction);
    void dispatch(EventObject eventObj);
}

/// <summary>
///  EventBus provides an addListener/removeListener/dispatch interface
///  not guaranteed to be thread-safe
/// </summary>
public class EventBus : IEventBus
{
    string _name;
    public string name { get { return _name; } }

    public bool verbose { get; set; }

    public delegate void Handler(EventObject evtobj);

    class EventProxy
    {
        public event Handler _event;

        public void dispatch( EventObject eo )
        {
            if(_event != null )
            {
                _event(eo);
            }
        }
    }

    Dictionary<string, EventProxy> _eventListeners;

    public EventBus(string busName)
    {
        _name = busName;
        _eventListeners = new Dictionary<string, EventProxy>();

        verbose = true; //
    }

    public void Destroy()
    {
        _eventListeners.Clear();
    }

    public void addListener(string eventName, Handler callbackFunction)
    {
        if(!_eventListeners.ContainsKey(eventName))
        {
            _eventListeners.Add(eventName, new EventProxy());
        }

        _eventListeners[eventName]._event += callbackFunction;
    }

    public void removeListener(string eventName, Handler callbackFunction)
    {
        if (_eventListeners.ContainsKey(eventName))
        {
            _eventListeners[eventName]._event -= callbackFunction;
        }
    }

    public void dispatch(EventObject eventObj)
    {

        if(verbose)
        {
            Debug.Log("eb[" + _name + "] dispatch " + eventObj.name);
        }

        string eventName = eventObj.name;
        if (_eventListeners.ContainsKey(eventName))
        {
            _eventListeners[eventName].dispatch(eventObj);
        }
    }

    private static Dictionary<string, EventBus> _busses = new Dictionary<string, EventBus>();
    public static EventBus Get(string busName)
    {
        if(!_busses.ContainsKey(busName))
        {
            _busses.Add(busName, new EventBus(busName));
        }
        return _busses[busName];
    }
    public static EventBus ui
    {
        get { return EventBus.Get("ui"); }
    }
    public static EventBus game
    {
        get { return EventBus.Get("game"); }
    }
}

public class EventObject
{
    private string _name;
    public string name { get { return _name; } }

    public EventObject(string evtName)
    {
        _name = evtName;
    }
}
