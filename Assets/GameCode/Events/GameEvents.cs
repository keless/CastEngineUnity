using System;
using System.Collections.Generic;


public class PlayerTargetSelected : EventObject
{
    public const string EvtName = "PlayerTargetSelected";
    public ICastEntity target = null;

    public PlayerTargetSelected(ICastEntity targ) : base(EvtName)
    {
        target = targ;
    }
}

/// <summary>
/// sent on EntityModel's bus when entity performs an action that requires an animation
///  animationName is string data to suggest which animation should be triggered
/// </summary>
public class TriggerAnimEvent : EventObject
{
    public const string EvtName = "TriggerAnimEvent";
    public string animName { get; protected set; }
    public TriggerAnimEvent( string animationName ) : base(EvtName)
    {
        animName = animationName;
    }
}