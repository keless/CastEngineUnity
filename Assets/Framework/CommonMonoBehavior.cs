using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommonMonoBehavior : MonoBehaviour {

    protected class ListenerCleanup
    {
        public string _eventBus;
        public string _eventName;
        public EventBus.Handler _eventHandler;
        public ListenerCleanup(string eventBus, string eventName, EventBus.Handler eventHandler)
        {
            _eventBus = eventBus; _eventName = eventName; _eventHandler = eventHandler;
        }
    }
    protected List<ListenerCleanup> _listenersToCleanUp = new List<ListenerCleanup>();

    protected void SetListener(string eventName, EventBus.Handler eventHandler, string eventBus = "ui")
    {
        _listenersToCleanUp.Add(new ListenerCleanup(eventBus, eventName, eventHandler));
        EventBus.Get(eventBus).addListener(eventName, eventHandler);
    }
	
	public void OnDestroy ()
    {
	    foreach( ListenerCleanup listener in _listenersToCleanUp)
        {
            EventBus.Get(listener._eventBus).removeListener(listener._eventName, listener._eventHandler);
        }
        _listenersToCleanUp.Clear();
	}
}
