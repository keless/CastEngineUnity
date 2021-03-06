﻿using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public enum CastEffectType
{
    DAMAGE_STAT,  //decrements stats that can be decremented (health, mana, etc)
    SUPPRESS_STAT, //temporarily decrements stats while in effect (str, agi, etc)
    HEAL_STAT,    //increments stats that can be incremented
    BUFF_STAT,        //temporarily increases stats while in effect
    SEND_EVENT, 	//causes an event to be sent to the game bus
}

/*
 
 effects supported:
   * 
 
 todo support:
   * effects that spawn more effects that jump to nearby targets
   * effects that target nearby targets
   * stacking buffs/debuffs (game code specific?)
   * 
 
 */

public class CastEffect
{
    CastEffectType m_type = CastEffectType.DAMAGE_STAT; //CastEffectType
    public double m_startTime = 0.0; //double
    double m_lifeTime = 0.0; //double
	float m_tickFreq = 0.0f; //float

    bool m_isChannelEffect = false; //bool
    bool m_isReturnEffect = false; //bool
    bool m_isAoeEffect = false; //bool

    string m_name = ""; //string
    string m_damageType = ""; //string
    string m_targetStat = ""; //string
    string m_stackFlag = ""; //string

    int m_numTicksCompleted = 0; //int
    float m_value = 0.0f; //float - how much damage, or how much suppression, or how much buff, etc

    ICastEntity m_pTarget = null; //ICastEntity
	ICastEntity m_pOrigin = null; //ICastEntity

    CastCommandModel m_pModel = null; //CastCommandModel
    CastCommandState m_pParent = null; //CastCommandState
    int m_modelEffectIndex = 0; //int - index into list of effects held by CastCommandModel

    public CastEffect()
    {

    }

    public void Destroy()
    {
        this.cancelTicks();
    }

    // in: CastEffect parent
    protected void _initReturnEffect(CastEffect parent)
    {
        this.m_isReturnEffect = true;

        var originState = parent.getParentState();
        var from = parent.getOriginEntity();
        var effectIdx = parent.m_modelEffectIndex;
        var isChannelEffect = parent.m_isChannelEffect;
        this.init(originState, effectIdx, from, isChannelEffect);
    }

    /// in: CastCommandState originState, int effectIdx, ICastEntity fromEntity, bool isChannelEffect
    public void init(CastCommandState originState, int effectIdx, ICastEntity fromEntity, bool isChannelEffect)
    {
        this.m_pParent = originState;
        this.m_pModel = originState.getModel();
        //TODO: this.m_pModel.retain();
        this.m_modelEffectIndex = effectIdx;
        this.m_isChannelEffect = isChannelEffect;

        var json = this.getDescriptor();


        if (json.hasOwnMember("name"))
            this.m_name = (string)json["name"];
        else
            this.m_name = this.m_pModel.getName();

        var type = (string)json["effectType"] ?? "damage";
        if (type == "damage")
        {
            this.m_type = CastEffectType.DAMAGE_STAT;
        }
        else if (type == "heal")
        {
            this.m_type = CastEffectType.HEAL_STAT;
        }
        else if (type == "buff")
        {
            this.m_type = CastEffectType.BUFF_STAT;
        }
        else if (type == "debuff")
        {
            this.m_type = CastEffectType.SUPPRESS_STAT;
        }
        else if (type == "event")
        {
            this.m_type = CastEffectType.SEND_EVENT;
        }

        this.m_pOrigin = fromEntity;

        this.m_isAoeEffect = (bool?)json["aoeRadius"] ?? false;
        this.m_damageType = (string)json["damageType"] ?? ""; //ex: fire
        this.m_targetStat = (string)json["targetStat"] ?? ""; //ex: hp_curr

        this.m_value = (float?)json["valueBase"] ?? 0.0f; //ex: 10 damage
                                                 //todo: handle caster stat modifiers

        this.m_lifeTime = (float?)json["lifeTime"] ?? 0.0f; //ex: 1.0 seconds

        this.m_tickFreq = (float?)json["tickFreq"] ?? this.m_tickFreq;
    }

    // in: ICastEntity to
    public void setTarget(ICastEntity target)
    {
        if (!CastWorldModel.Get().isValid(target)) return;
        this.m_pTarget = target;
    }


    public void startTicks()
    {
        if (this.m_type == CastEffectType.BUFF_STAT || this.m_type == CastEffectType.SUPPRESS_STAT || this.m_type == CastEffectType.SEND_EVENT || this.getLifeTime() == 0)
        {
            Debug.Log("xxx start ticks, do effect immediately");
            this.doEffect();
        }
        else {
            Debug.Log("xxx start ticks, schedule onTick");
            this.m_numTicksCompleted = 0;
            CastCommandScheduler.Get().scheduleSelector(this.onTick, this.m_tickFreq);
        }
    }


    public double numTicks()
    {
        return (this.m_lifeTime / this.m_tickFreq) + 1;
    }

    //string
    public string getName()
    {
        return this.m_name;
    }
    //CastEffectType
    public CastEffectType getType()
    {
        return this.m_type;
    }
    //double
    public double getStartTime()
    {
        return this.m_startTime;
    }

    //ICastEntity
    public ICastEntity getTarget()
    {
        return this.m_pTarget;
    }
    //ICastEntity
    public ICastEntity getOriginEntity()
    {
        return this.m_pOrigin;
    }

    //double
    public double getLifeTime()
    {
        return this.m_lifeTime;
    }

    //CastCommandState
    public CastCommandState getParentState()
    {
        return this.m_pParent;
    }

    // in: double currtime
    //double
    public double getElapsedTime(double currTime)
    {
        return currTime - this.m_lifeTime;
    }

    //bool
    public bool isPositiveEffect()
    {
        return this.m_type == CastEffectType.BUFF_STAT || this.m_type == CastEffectType.HEAL_STAT;
    }


    public void initReturnEffect( CastEffect parent )
    {
        this.m_isReturnEffect = true;

        var originState = parent.m_pParent;
        //ICastEntity* to = parent.m_pTarget;
        var from = parent.m_pOrigin;
        var effectIdx = parent.m_modelEffectIndex;
        var isChannelEffect = parent.m_isChannelEffect;
        this.init(originState, effectIdx, from, isChannelEffect);
    }

    //bool
    public bool hasReturnEffect()
    {
        var json = this.getDescriptor();
        return json.hasOwnMember("returnEffect");
    }

    //bool
    public bool isAoe()
    {
        return this.m_isAoeEffect;
    }

    //float
    public float getTravelSpeed()
    {
        return (float?)this.m_pModel.descriptor["travelSpeed"] ?? 0.0f;
    }

    // in: float dt
    public void onTick()
    {
        Debug.Log("onTick()");

        if (!CastWorldModel.Get().isValid(this.m_pTarget)) return;

        Debug.Log("onTick() targetvalid");

        var currTime = CastCommandTime.Get();
        var delta = currTime - this.m_startTime;
        if (delta > this.m_lifeTime) delta = this.m_lifeTime;

        if (this.m_type == CastEffectType.SUPPRESS_STAT || this.m_type == CastEffectType.BUFF_STAT)
        {
            //handle buff/debuff end
            if (delta >= this.m_lifeTime)
            {
                //CastCommandScheduler.Get().unscheduleSelector(this.onTick); //not a repeating timer, dont need to cancel if we were just called
                if (this.m_type == CastEffectType.BUFF_STAT)
                {
                    this.m_pTarget.endBuffProperty(this.m_targetStat, -1 * this.m_value, this);
                }
                else
                {
                    this.m_pTarget.endBuffProperty(this.m_targetStat, this.m_value, this);
                }

                this.m_pTarget.removeEffect(this);
            }
            else
            {
                CastCommandScheduler.Get().scheduleSelector(this.onTick, this.m_tickFreq);
            }

        }
        else {
            Debug.Log("type != stat");

            //handle dmg/heal over time ticks
            var numTicksPassed = delta / this.m_tickFreq;

            var ticksToDo = numTicksPassed - this.m_numTicksCompleted;
            Debug.Log("ticks to do " + ticksToDo + " passed - completed : " + numTicksPassed + " - " + m_numTicksCompleted);
            for (var i = 0; i < ticksToDo; i++)
            {
                this.doEffect();
                this.m_numTicksCompleted++;
            }

            if (delta >= this.m_lifeTime)
            {
                Debug.Log("effect ticks complete");

                this.m_pTarget.removeEffect(this);
                //this.cancelTicks(); //not a repeating timer, dont need to cancel if we were just called
            }
            else
            {
                //continue ticking
                CastCommandScheduler.Get().scheduleSelector(this.onTick, this.m_tickFreq);
            }
        }
    }


    public void cancelTicks()
    {
        Debug.Log("cancelTicks()");
        CastCommandScheduler.Get().unscheduleSelector(this.onTick);
    }


    public void doEffect()
    {
        Debug.Log("doEffect()_");

        var world = CastWorldModel.Get();

        if (!world.isValid(this.m_pTarget)) return;

        if (this.m_startTime == 0) this.m_startTime = CastCommandTime.Get();

        JToken json = this.getDescriptor();

        if (json.hasOwnMember("react"))
        {
            this.m_pTarget.handleEffectReaction(json["react"], this);
        }

        Debug.Log("doEffect()");

        switch (this.m_type)
        {
            case CastEffectType.DAMAGE_STAT:
                this.m_pTarget.incProperty(this.m_targetStat, -1 * this.m_value, this);
                break;
            case CastEffectType.HEAL_STAT:
                this.m_pTarget.incProperty(this.m_targetStat, this.m_value, this);
                break;
            case CastEffectType.SUPPRESS_STAT:
                this.m_pTarget.startBuffProperty(this.m_targetStat, -1 * this.m_value, this);
                CastCommandScheduler.Get().scheduleSelector(this.onTick, this.m_lifeTime);
                break;
            case CastEffectType.BUFF_STAT:
                this.m_pTarget.startBuffProperty(this.m_targetStat, this.m_value, this);
                CastCommandScheduler.Get().scheduleSelector(this.onTick, this.m_lifeTime);
                break;
            case CastEffectType.SEND_EVENT:
                this.m_pTarget.handleEffectEvent(this.m_name, this);
                break;
            default:
                Debug.Log("TODO: handle effect type " + this.m_type);
                break;
        }

        if (json.hasOwnMember("returnEffect"))
        {
            json = json["returnEffect"];

            //validate
            if (!world.isValid(this.m_pOrigin)) return;

            var bounce = new CastEffect();
            bounce.initReturnEffect(this);
            bounce.m_value = this.m_value;
            //swap direction
            var from = this.m_pTarget;
            var to = this.m_pOrigin;

            var ghostTarget = new CastTarget();
            ghostTarget.addTargetEntity(to);

            world.addEffectInTransit(from, bounce, ghostTarget, CastCommandTime.Get());

            ghostTarget = null; //TODO: release()
        }
    }

    // in: string name
    //json
    public JToken getDescriptor(string descriptorName = null )
    {

        if (this.m_pModel == null || this.m_modelEffectIndex < 0) return new JObject();

        JToken json;
        if (this.m_isChannelEffect)
        {
            json = this.m_pModel.getEffectOnChannel(this.m_modelEffectIndex);
        }
        else {
            json = this.m_pModel.getEffectOnCast(this.m_modelEffectIndex);
        }

        if (descriptorName != null)
        {
            return json[descriptorName] ?? new JObject();
        }

        if (this.m_isReturnEffect)
        {
            json = json["returnEffect"] ?? new JObject();
        }

        return json;
    }

    //CastEffect
    public CastEffect clone()
    {
        var effect = new CastEffect();

        effect.m_type = this.m_type;
        effect.m_startTime = this.m_startTime;
        effect.m_lifeTime = this.m_lifeTime;
        effect.m_tickFreq = this.m_tickFreq;
        effect.m_damageType = this.m_damageType;
        effect.m_targetStat = this.m_targetStat;
        effect.m_stackFlag = this.m_stackFlag;
        effect.m_numTicksCompleted = this.m_numTicksCompleted;
        effect.m_value = this.m_value;
        effect.m_pTarget = this.m_pTarget;
        effect.m_pOrigin = this.m_pOrigin;
        effect.m_pModel = this.m_pModel;
        effect.m_pParent = this.m_pParent;
        effect.m_modelEffectIndex = this.m_modelEffectIndex;
        effect.m_isChannelEffect = this.m_isChannelEffect;
        effect.m_isReturnEffect = this.m_isReturnEffect;
        effect.m_name = this.m_name;

        return effect;
    }
}
