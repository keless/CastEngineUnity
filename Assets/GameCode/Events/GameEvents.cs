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

