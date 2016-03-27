#define EVENTBUS_USE_INTERNAL_LIST

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void EventBusDelegate(EventObject evtobj);
public interface IEventBus
{
    void Destroy();
    void addListener(string eventName, EventBusDelegate callbackFunction);
    void removeListener(string eventName, EventBusDelegate callbackFunction);
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
    public bool debugVerbose = true;



#if EVENTBUS_USE_INTERNAL_LIST
    
    Dictionary<string, List<EventBusDelegate>> m_listeners = new Dictionary<string, List<EventBusDelegate>>();

    public EventBus(string busName)
    {
        _name = busName;
        verbose = true; //
    }

    public void Destroy()
    {
        m_listeners.Clear();
    }

    public void addListener(string eventName, EventBusDelegate callback)
    {
        List<EventBusDelegate> evtListeners = null;
        if (m_listeners.TryGetValue(eventName, out evtListeners))
        {
            evtListeners.Remove(callback); //make sure we dont add duplicate
            evtListeners.Add(callback);
        }
        else {
            evtListeners = new List<EventBusDelegate>();
            evtListeners.Add(callback);

            m_listeners.Add(eventName, evtListeners);
        }
    }
    public void removeListener(string eventName, EventBusDelegate callback)
    {
        List<EventBusDelegate> evtListeners = null;
        if (m_listeners.TryGetValue(eventName, out evtListeners))
        {
            for (int i = 0; i < evtListeners.Count; i++)
            {
                evtListeners.Remove(callback);
            }
        }
    }
    public void dispatch(EventObject eventObj)
    {
        //FIXME: might need to COPY the list<dispatchers> here so that an 
        //	event listener that results in adding/removing listeners does 
        //	not invalidate this for loop
        string eventName = eventObj.name;
        List<EventBusDelegate> evtListeners = null;
        if (m_listeners.TryGetValue(eventName, out evtListeners))
        {
            for (int i = 0; i < evtListeners.Count; i++)
            {
                evtListeners[i](eventObj);
            }
        }
    }
#else
    public delegate void Handler(EventObject evtobj);
    class EventProxy
    {
        public event Handler _event;

        public void dispatch(EventObject eo)
        {
            if (_event != null)
            {
                _event(eo);
            }
        }

        public int numListeners()
        {
            if (_event != null)
            {
                return _event.GetInvocationList().Length;
            }
            return 0;
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
        if (!_eventListeners.ContainsKey(eventName))
        {
            _eventListeners.Add(eventName, new EventProxy());
        }

        if (debugVerbose)
        {
            Debug.Log("add listener " + eventName + " " + callbackFunction);
        }

        _eventListeners[eventName]._event += new Handler(callbackFunction);
    }

    public void removeListener(string eventName, Handler callbackFunction)
    {
        if (_eventListeners.ContainsKey(eventName))
        {
            if (debugVerbose)
            {
                Debug.Log("remove listener " + eventName + " " + callbackFunction);
            }

            _eventListeners[eventName]._event -= callbackFunction;
        }
    }

    public void dispatch(EventObject eventObj)
    {

        if (verbose)
        {
            Debug.Log("eb[" + _name + "] dispatch " + eventObj.name);
        }

        string eventName = eventObj.name;
        if (_eventListeners.ContainsKey(eventName))
        {
            if (debugVerbose)
            {
                Debug.Log(" to " + _eventListeners[eventName].numListeners() + " listeners");
            }

            _eventListeners[eventName].dispatch(eventObj);
        }
    }
#endif

    private static Dictionary<string, EventBus> _busses = new Dictionary<string, EventBus>();
    public static EventBus Get(string busName)
    {
        if (!_busses.ContainsKey(busName))
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