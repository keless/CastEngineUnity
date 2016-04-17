using System;
using System.Collections.Generic;

/// <summary>
/// sent when player selects a new target
///  - used by TargetPanelView to show target info and hp bar
///  - used by TriggerHPBarOnTargeted to show hp bar
/// </summary>
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
///  sent by GameWorldModel when an entity dies
/// </summary>
public class EntityDied : EventObject
{
    public const string EvtName = "EntityDied";
    public ICastEntity target = null;

    public EntityDied(ICastEntity targ) : base (EvtName)
    {
        target = targ;
    }
}
public class PlayerInitialized : EventObject
{
    public const string EvtName = "PlayerInitialized";
    public EntityModel player { get; private set; }

    public PlayerInitialized(EntityModel playerModel) : base(EvtName)
    {
        player = playerModel;
    }
}