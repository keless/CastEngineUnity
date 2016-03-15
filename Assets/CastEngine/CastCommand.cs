using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/*
CastCommandState
  carries all the stateful information of an INSTANCE of a cast command
states:
 *evt begin cast
  casting 
 *evt cast complete
 if( channeled ) 
 *evt channel start
  *evt cast/channel tick
 *evt channel end
 endif( channeled )
 *evt begin travel
  traveling
 *evt hit (can hit target, or possibly be blocked earlier in path)
  lingering (on world, on target(s))
*/

public class CastCommandState
{
    public const int IDLE = 0;
    public const int CASTING = 1;
    public const int CHANNELING = 2;
    public const int COOLDOWN = 3;

    int m_state;
    double m_timeStart;
    int m_channelTicks;
    CastCommandModel m_pModel;
    ICastEntity m_iOwner;
    string m_costStat;
    float m_costVal;

    public CastCommandState(CastCommandModel commandModel, ICastEntity entityOwner)
    {
        this.m_state = CastCommandState.IDLE; //CCSstate
        this.m_timeStart = 0;                 //double
        this.m_channelTicks = 0;              //int
        this.m_pModel = commandModel;         //CastCommandModel
        this.m_iOwner = entityOwner;          //ICastEntity

        this.m_costStat = commandModel.descriptor.Value<string>("costStat") ?? "";
        this.m_costVal = commandModel.descriptor.Value<float?>("costVal") ?? 0.0f;
    }

    public CastCommandModel getModel()
    {
        return m_pModel;
    }

    protected void _onCastComplete()
    {
        if (this.m_state != CastCommandState.CASTING) return;

        var world = CastWorldModel.Get();
        if (!world.isValid(this.m_iOwner)) return;

        //check for cost (but dont apply it yet)
        if (this.m_costVal != 0)
        {
            var res = this.m_iOwner.getProperty(this.m_costStat);

            //checking cost>0 so that if a tricky user wants cost to be 'negative' to 'add' value
            //  we can do that even if it is below resource (ex: cost = increased heat)
            if (this.m_costVal > 0 && this.m_costVal > res)
            {
                //not enough of resource to cast spell, so abort
                //todo: send evt aborted cast because of no resource
                this._onCooldownStart();
                return;
            }
        }

        var currTime = CastCommandTime.Get();
        this.m_timeStart = currTime;

        //spawn effects
        var target = this.m_iOwner.getTarget();
        var hasTargetInRange = target.hasTargetsAtRangeFromEntity(this.m_pModel.getRange(), this.m_iOwner);
        if (!hasTargetInRange)
        {
            this._onCooldownStart();
            return;
        }

        var foundTarget = false; //ensure we actually reach at least one target
        for (var i = 0; i < this.m_pModel.getNumEffectsOnCast(); i++)
        {
            //TODO: check for range

            var effect = new CastEffect();
            effect.init(this, i, this.m_iOwner, false);

            //TODO: send all effects as one array so only one "packet" has to travel

            world.addEffectInTransit(this.m_iOwner, effect, this.m_iOwner.getTarget(), currTime);
            foundTarget = true;
        }

        if (foundTarget && this.m_costVal != 0)
        {
            //apply cost
            this.m_iOwner.incProperty(this.m_costStat, -1 * this.m_costVal, null);
        }

        if (this.m_pModel.channelTime > 0)
        {
            //begin channeling
            this._onChannelStart();
        }
        else {
            this._onCooldownStart();
        }
    }


    protected void _onChannelStart()
    {
        this.m_state = CastCommandState.CHANNELING;
        this._scheduleCallback(this.m_pModel.channelFreq);
    }

    protected void _onChannelComplete()
    {
        if (this.m_state != CastCommandState.CHANNELING) return;
        if (!CastWorldModel.Get().isValid(this.m_iOwner)) return;

        this._onCooldownStart();
    }

    protected void _spawnChannelEffects()
    {
        var target = this.m_iOwner.getTarget();
        target.validateTargets();

        for (var i = 0; i < this.m_pModel.getNumEffectsOnChannel(); i++)
        {
            var effect = new CastEffect();
            effect.init(this, i, this.m_iOwner, true);

            //TODO: send all effects as one array so only one 'packet' has to travel

            var world = CastWorldModel.Get();
            world.addEffectInTransit(this.m_iOwner, effect, this.m_iOwner.getTarget(), CastCommandTime.Get());
        }
    }


    protected void _onCooldownStart()
    {
        this.m_state = CastCommandState.COOLDOWN;
        this._scheduleCallback(this.m_pModel.cooldownTime);
    }


    protected void _onCooldownComplete()
    {
        if (this.m_state != CastCommandState.COOLDOWN) return;

        var currTime = CastCommandTime.Get();
        this.m_timeStart = currTime;

        this.m_state = CastCommandState.IDLE;

        //TODO: send cooldown complete signal
    }


    protected void _scheduleCallback(double dt)
    {
        CastCommandScheduler.Get().scheduleSelector(this.onSchedulerTick, dt);
    }

    
    protected void _unscheduleCallback()
    {
        CastCommandScheduler.Get().unscheduleSelector(this.onSchedulerTick);
    }
    

    //string
    public string getName()
    {
        return (this.m_pModel != null) ? this.m_pModel.getName() : null;
    }

    //bool
    public bool canAfford()
    {
        if (this.m_costVal == 0) return true;

        var val = this.m_iOwner.getProperty(this.m_costStat);
        return val >= this.m_costVal;
    }

    //bool
    public bool isCasting()
    {
        return this.m_state == CastCommandState.CASTING;
    }

    //float
    // 0 means 'not casting', 1.0 means 'cast complete'
    public float getCastPct()
    {
        if (this.m_state == CastCommandState.CASTING)
        {
            var currTime = CastCommandTime.Get();
            float delta = (float)(currTime - this.m_timeStart);
            if (delta > this.m_pModel.castTime) delta = this.m_pModel.castTime; //TODO: support dynamic cast time modification
            if (delta < 0) delta = 0;
            return delta / this.m_pModel.castTime;
        }
        else {
            return 0.0f;
        }
    }

    //bool
    public bool isChanneling()
    {
        return this.m_state == CastCommandState.CHANNELING;
    }

    //float
    // 0 means 'not channeling' 1.0 means 'channel complete'
    public float getChannelPct()
    {
        if (this.m_state == CastCommandState.CHANNELING)
        {
            var currTime = CastCommandTime.Get();
            float delta = (float)(currTime - this.m_timeStart);
            if (delta > this.m_pModel.channelTime) delta = this.m_pModel.channelTime; //TODO: support dynamic cast time modification
            if (delta < 0) delta = 0;
            return delta / this.m_pModel.channelTime;
        }
        else {
            return 0.0f;
        }
    }

    //bool
    public bool isOnCooldown()
    {
        return this.m_state == CastCommandState.COOLDOWN;
    }

    //float
    //0 means 'on cooldown', 1.0 means 'off cooldown'
    public float getCooldownPct()
    {
        if (this.m_state == CastCommandState.COOLDOWN)
        {
            var currTime = CastCommandTime.Get();
            float delta = (float)(currTime - this.m_timeStart);
            if (delta > this.m_pModel.cooldownTime) delta = this.m_pModel.cooldownTime; //TODO: support dynamic cooldown modification
            if (delta < 0) delta = 0;
            return delta / this.m_pModel.cooldownTime;
        }
        else {
            return 0.0f;
        }
    }

    //bool
    public bool isIdle()
    {
        return this.m_state == CastCommandState.IDLE;
    }

    //float 
    public float getRange()
    {
        return (this.m_pModel != null) ? this.m_pModel.getRange() : 0;
    }

    // in: _nullable string dataName
    //json
    public JToken getDescriptor(string dataName)
    {
        if (dataName == null || dataName == "")
        {
            return this.m_pModel.descriptor;
        }

        return this.m_pModel.descriptor[dataName] ?? new JObject();
    }

    //bool
    public bool startCast()
    {
        if (!this.isIdle()) return false; //cant start casting

        this.m_state = CastCommandState.CASTING;
        this.m_timeStart = CastCommandTime.Get();
        this.m_channelTicks = 0;

        if (this.m_pModel.castTime == 0)
        {
            //handle instant cast
            this.onSchedulerTick();
        }
        else {
            //TODO: should we set castTime as delay instead of interval?
            this._scheduleCallback(this.m_pModel.castTime);
        }

        return true;
    }

    public void onSchedulerTick()
    {
        var currTime = CastCommandTime.Get();
        var delta = currTime - this.m_timeStart;
        if (this.m_state == CastCommandState.CASTING)
        {
            if (delta >= this.m_pModel.castTime)
            { //TODO: handle cast speed increase
              //casting complete
                this._onCastComplete();
            }
            else {
                Debug.LogError("shouldnt happen, cast time");
            }
        }
        else if (this.m_state == CastCommandState.CHANNELING)
        {
            //TODO: handle channeling ticks
            if (delta > this.m_pModel.channelTime) delta = this.m_pModel.channelTime;

            var numTicksPassed = delta / this.m_pModel.channelFreq;
            var ticksToDo = numTicksPassed - this.m_channelTicks;
            for (var i = 0; i < ticksToDo; i++)
            {
                // do tick
                this._spawnChannelEffects();
                this.m_channelTicks++;
            }

            if (delta >= this.m_pModel.channelTime)
            {
                //cancel callback
                this._onChannelComplete();
            }
            else {
                this._scheduleCallback(this.m_pModel.channelFreq);
            }
        }
        else if (this.m_state == CastCommandState.COOLDOWN)
        {
            if (delta >= this.m_pModel.cooldownTime)
            { //TODO: ahndle cooldown redux
              //cancel callback
                this._onCooldownComplete();
            }
            else {
                Debug.LogError("shouldnt happen, cooldown time");
            }
        }
    }


}

/*
 CastCommandModel
   carries shared resource information used to create CastCommandState objects from
     ie: one CastCommandModel represents "fireball", but three mages would each have their own CCS that points to the same CCM
*/

public class CastCommandModel
{
    public string name;
    public float castTime;
    public float channelTime;
    public float channelFreq;
    public float travelSpeed;
    public float range;
    public bool effectWhileTraveling;
    public bool stopOnHit;
    public float cooldownTime;
    public float effectSize;
    public float effectShape;
    public JObject descriptor; //json

    public CastCommandModel(JToken jsonCastData)
    {

        JObject descriptor = jsonCastData as JObject;
        Debug.Assert(descriptor != null);
        this.name = descriptor.Value<string>("name") ?? "effectName";

        //base values, unmodified by buff/debufs (which happens at time of cast)
        this.castTime = descriptor.Value<float?>("castTime") ?? 0.0f; //float - zero if instant
        this.channelTime = descriptor.Value<float?>("channelTime") ?? 0.0f; //float - zero if not channeled
        this.channelFreq = descriptor.Value<float?>("channelFreq") ?? 1.0f; //float - tick freq of channeling
        this.travelSpeed = descriptor.Value<float?>("travelSpeed") ?? 0.0f; //float - zero if instant
        this.range = descriptor.Value<float?>("range") ?? 0.0f; //float - distance it can travel
        this.effectWhileTraveling = descriptor.Value<bool?>("effectWhileTravel") ?? false; //bool - true if can cause effect while travelling (might be immediately consumed: fireball; or continue to effect while travelling: lava wall)
        this.stopOnHit = descriptor.Value<bool?>("stopOnHit") ?? false; //bool

        this.cooldownTime = descriptor.Value<float?>("cooldownTime") ?? 0.0f; //float - time in MS after castEnd before castBegin can start again
        this.effectSize = descriptor.Value<float?>("effectSize") ?? 0.0f; //float - zero if no physics involved
        this.effectShape = 0; //int - singleTarget, line, cone, circle, etc

        this.descriptor = descriptor; //json
    }

    //bool
    public bool isChanneled()
    {
        return this.channelTime > 0;
    }

    //bool 
    public bool isInstant()
    {
        return this.castTime <= 0;
    }

    //int - per tick
    public int getNumEffectsOnChannel()
    {
        return this.descriptor.Value<JArray>("effectsOnChannel").Count;
    }

    // in: int idx
    //json
    public JToken getEffectOnChannel(int idx)
    {
        return this.descriptor["effectsOnChannel"][idx];
    }

    //int
    public int getNumEffectsOnCast()
    {
        return this.descriptor.Value<JArray>("effectsOnCast").Count;
    }

    // in: int idx
    //json
    public JToken getEffectOnCast(int idx)
    {
        return this.descriptor["effectsOnCast"][idx];
    }

    //float
    public float getRange()
    {
        return this.range;
    }

    //string
    public string getName()
    {
        return this.name;
    }

}

/*
 CastCommandScheduler
 used by CastCommandStates to schedule callbacks and timer ticks
*/

public class CastCommandScheduler
{
    public delegate void ScheduleTask();

    static CastCommandScheduler instance = new CastCommandScheduler();
    public static CastCommandScheduler Get()
    {
        return CastCommandScheduler.instance;
    }

    Dictionary<double, List<ScheduleTask>> m_schedules = new Dictionary<double, List<ScheduleTask>>();
    double lastUpdate = 0;

    public CastCommandScheduler()
    {

    }

    public void scheduleSelector(ScheduleTask callback, double dt)
    {
        var ct = CastCommandTime.Get();
        var time = ct + dt;
        if (this.m_schedules.ContainsKey(time))
        {
            this.m_schedules[time].Add(callback);
        }
        else {
            var newList = new List<ScheduleTask>();
            newList.Add(callback);
            this.m_schedules.Add(time, newList);
        }
    }

    public void unscheduleSelector(ScheduleTask callback)
    {
        //todo?
        Debug.Log("todo: unschedule selector");
    }

    public void Update()
    {
        var ct = CastCommandTime.Get();
        if (ct == this.lastUpdate) return; //dont replay updates if time hasnt progressed
        this.lastUpdate = ct;

        var removeList = new List<double>();

		foreach (double time in this.m_schedules.Keys)
        {
            if (time <= ct)
            {
                removeList.Add(time);
                foreach (ScheduleTask callback in this.m_schedules[time])
                {
                    callback();
                }
            }
        }

        for (var i = 0; i < removeList.Count; i++)
        {
            this.m_schedules.Remove(removeList[i]);
        }
    }
}