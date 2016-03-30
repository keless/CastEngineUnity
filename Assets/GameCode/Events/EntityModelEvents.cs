using System;
using System.Collections.Generic;

/// <summary>
/// sent when entity performs an action that requires an animation
///  animationName is string data to suggest which animation should be triggered
/// </summary>
public class TriggerAnimEvent : EventObject
{
    public const string EvtName = "TriggerAnimEvent";
    public string animName { get; protected set; }
    public TriggerAnimEvent(string animationName) : base(EvtName)
    {
        animName = animationName;
    }
}

/// <summary>
/// sent when entity ability goes on idle
/// </summary>
public class EntityStartIdle : EventObject
{
    public const string EvtName = "EntityStartIdle";
    public int abilityIdx { get; protected set; }
    public double startTime { get; protected set; }
    public EntityStartIdle(int abilityIndex, double startTimeS) : base(EvtName)
    {
        abilityIdx = abilityIndex;
        startTime = startTimeS;
    }
}

/// <summary>
/// sent when entity starts casting an ability
/// </summary>
public class EntityStartCast : EventObject
{
    public const string EvtName = "EntityStartCast";
    public int abilityIdx { get; protected set; }
    public float castPeriod { get; protected set; }
    public double startTime { get; protected set; }
    public EntityStartCast(int abilityIndex, float period, double startTimeS ) : base(EvtName)
    {
        abilityIdx = abilityIndex;
        castPeriod = period;
        startTime = startTimeS;
    }
}

/// <summary>
/// sent when entity starts channeling an ability
/// </summary>
public class EntityStartChannel : EventObject
{
    public const string EvtName = "EntityStartChannel";
    public int abilityIdx { get; protected set; }
    public float channelPeriod { get; protected set; }
    public double startTime { get; protected set; }
    public EntityStartChannel(int abilityIndex, float period, double startTimeS) : base(EvtName)
    {
        abilityIdx = abilityIndex;
        channelPeriod = period;
        startTime = startTimeS;
    }
}

/// <summary>
/// sent when entity's ability goes on cooldown
/// </summary>
public class EntityStartCooldown : EventObject
{
    public const string EvtName = "EntityStartCooldown";
    public int abilityIdx { get; protected set; }
    public float cooldownPeriod { get; protected set; }
    public double startTime { get; protected set; }
    public EntityStartCooldown(int abilityIndex, float period, double startTimeS) : base(EvtName)
    {
        abilityIdx = abilityIndex;
        cooldownPeriod = period;
        startTime = startTimeS;
    }
}