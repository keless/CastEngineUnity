using System;
using System.Collections.Generic;

/// <summary>
/// Called on UI bus when an ability is cast (it's UI button should show casting status)
///     abilityIdx is the index into player's ability list to the ability
///     castPeriod is the time in seconds the casting will take
///     startTime is the time from CastCommandTime which the cooldown started (animation should pause when CastCommandTime pauses)
/// </summary>
public class AbilityStartCast : EventObject
{
    public const string EvtName = "AbilityStartCast";
    public int abilityIdx { get; protected set; }
    public float castPeriod { get; protected set; }
    public double startTime { get; protected set; }
    public AbilityStartCast(int abilityIndex, float castTimeS, double startTimeS) : base(EvtName)
    {
        abilityIdx = abilityIndex;
        castPeriod = castTimeS;
        startTime = startTimeS;
    }
}

/// <summary>
/// Called on UI bus when an ability is on cooldown (it's UI button should show cooldown status)
///     abilityIdx is the index into player's ability list to the ability
///     castPeriod is the time in seconds the casting will take
///     startTime is the time from CastCommandTime which the cooldown started (animation should pause when CastCommandTime pauses)
/// </summary>
public class AbilityStartCooldown : EventObject
{
    public const string EvtName = "AbilityStartCooldown";
    public int abilityIdx { get; protected set; }
    public float cooldownPeriod { get; protected set; }
    public double startTime { get; protected set; }
    public AbilityStartCooldown(int abilityIndex, float cooldownTimeS, double startTimeS) : base(EvtName)
    {
        abilityIdx = abilityIndex;
        cooldownPeriod = cooldownTimeS;
        startTime = startTimeS;
    }
}