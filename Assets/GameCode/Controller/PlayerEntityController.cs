using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;



public class PlayerEntityController : CommonMonoBehaviour, IHitpointValueProvider
{

    EntityModel m_model;

    CastCommandModel m_testAbility;

    public static PlayerEntityController AddPlayerEntityController( GameObject go, EntityModel model  ) {
        PlayerEntityController component = go.AddComponent<PlayerEntityController>();
        component.SetModel(model);
        return component;
    }

    // Use this for initialization
    void Start () {

        SetListener("btnSkill1", onBtnSkill1);
        SetListener(KeyEvent.EvtName, onKeyEvent);
	}

    new void OnDestroy()
    {
        //clean up!
        RemoveModel();
    }

    public void SetModel(EntityModel model)
    {
        RemoveModel();
        m_model = model;
        m_model.addListener(EntityStartCast.EvtName, onStartCast);
        m_model.addListener(EntityStartCast.EvtName, onStartChannel);
        m_model.addListener(EntityStartCooldown.EvtName, onStartCooldown);
    }

    public void RemoveModel()
    {
        if (m_model == null) return;
        m_model.removeListener(EntityStartCast.EvtName, onStartCast);
        m_model.removeListener(EntityStartCast.EvtName, onStartChannel);
        m_model.removeListener(EntityStartCooldown.EvtName, onStartCooldown);
        m_model = null;
    }

    void onStartCast(EventObject e)
    {
        EntityStartCast evt = e as EntityStartCast;
        EventBus.ui.dispatch(new AbilityStartCast(evt.abilityIdx, evt.castPeriod, evt.startTime));
    }

    void onStartChannel(EventObject e)
    {
        /* TODO: startChannel UI event
        EntityStartChannel evt = e as EntityStartChannel;

        EventBus.ui.dispatch(new AbilityStartChannel(evt.abilityIdx, evt.channelPeriod, evt.startTime));
        */
    }

    void onStartCooldown(EventObject e)
    {
        EntityStartCooldown evt = e as EntityStartCooldown;
        EventBus.ui.dispatch(new AbilityStartCooldown(evt.abilityIdx, evt.cooldownPeriod, evt.startTime));
    }

    void onKeyEvent(EventObject e)
    {
        KeyEvent evt = e as KeyEvent;
        if (!evt.isDown) return; // only listening to key down

        if (evt.keyCode == KeyCode.Tab)
        {
            //player target change
            doPlayerTargetChange();
        }

        if(evt.keyCode == KeyCode.Alpha1)
        {
            attemptAbility(0);
        }
        if (evt.keyCode == KeyCode.Alpha2)
        {
            attemptAbility(1);
        }
    }

    void onBtnSkill1(EventObject e)
    {
        Debug.Log("todo: use ability");

        attemptAbility(0);
    }

    void attemptAbility( int abilityIdx )
    {
        if (!m_model.canCast())
        {
            return; //cant perform an ability
        }

        CastCommandState ability = m_model.getAbilities()[abilityIdx];
        if(ability == null)
        {
            return;
        }

        CastTarget target = m_model.getTarget();
        if (!target.hasTargetsAtRangeFromEntity(ability.getRange(), m_model))
        {
            Debug.Log("ability " + (abilityIdx + 1) + " has no targets in range");
            return;
        }

        Debug.Log("start cast on ability " + abilityIdx);

        ability.startCast();
        m_model.dispatch(new TriggerAnimEvent("someAnimation"));

        CastCommandModel abilityModel = ability.getModel();
        EventBus.ui.dispatch(new AbilityStartCast(abilityIdx, abilityModel.castTime, CastCommandTime.Get()));
    }

    void doPlayerTargetChange()
    {
        List<ICastEntity> blackList = new List<ICastEntity>();
        blackList.Add(m_model);
        CastTarget target = m_model.getTarget();
        List<ICastEntity> currentTargets = target.getEntityList();
        int currentTargetCount = currentTargets.Count;
        blackList.AddRange(currentTargets);
        List<ICastEntity> otherTargets = CastWorldModel.Get().getPhysicsInterface().GetEntitiesInRadius(m_model.position, 500, blackList);

        int newTargetCount = 0;
        ICastEntity newTarget = null;
        if (otherTargets.Count == 0)
        {
            target.clearTargetEntities();
        }
        else if(otherTargets.Count == 1)
        {
            target.clearTargetEntities();
            newTarget = otherTargets[0];
            target.addTargetEntity(newTarget);
            newTargetCount = 1;
        }
        else
        {
            //choose one (closest?)
            //todo: heuristic
            target.clearTargetEntities();
            newTarget = otherTargets[0];
            target.addTargetEntity(newTarget);
            newTargetCount = 1;
        }

        if( currentTargetCount != newTargetCount )
        {
            EventBus.game.dispatch(new PlayerTargetSelected(newTarget));
        }
    }

    // IHitpointValueProvider
    public int getHPCurr()
    {
        return m_model.hp_curr;
    }
    public int getHPMax()
    {
        return m_model.hp_base;
    }
}
