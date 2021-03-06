﻿using UnityEngine;
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

        SetListener("btnSkill0", onBtnSkill);
        SetListener("btnSkill1", onBtnSkill);
        SetListener("btnSkill2", onBtnSkill);
        SetListener(KeyEvent.EvtName, onKeyEvent);
        SetListener(EntityDied.EvtName, onEntityDeath, "game");
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
        m_model.addListener(EntityStartChannel.EvtName, onStartChannel);
        m_model.addListener(EntityStartCooldown.EvtName, onStartCooldown);
        m_model.addListener(EntityStartIdle.EvtName, onStartIdle);

        Debug.Log("set model - player initialized");
        EventBus.ui.dispatch(new PlayerInitialized(m_model));
    }

    public void RemoveModel()
    {
        if (m_model == null) return;
        m_model.removeListener(EntityStartCast.EvtName, onStartCast);
        m_model.removeListener(EntityStartChannel.EvtName, onStartChannel);
        m_model.removeListener(EntityStartCooldown.EvtName, onStartCooldown);
        m_model.removeListener(EntityStartIdle.EvtName, onStartIdle);
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

    void onStartIdle(EventObject e)
    {
        EntityStartIdle evt = e as EntityStartIdle;
        EventBus.ui.dispatch(new AbilityStartIdle(evt.abilityIdx, evt.startTime));
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

        if(evt.keyCode >= KeyCode.Alpha1 && evt.keyCode <= KeyCode.Alpha9 )
        {
            attemptAbility(evt.keyCode - KeyCode.Alpha1);
        }
    }

    void onBtnSkill(EventObject e)
    {
        int abilityIdx = int.Parse( e.name.Substring("btnSkill".Length) );
        attemptAbility(abilityIdx);
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
            EventBus.game.dispatch(new PlayerTargetSelected(newTarget as EntityModel));
        }
    }

    void onEntityDeath(EventObject e)
    {
        EntityDied evt = e as EntityDied;
        ICastEntity deadEntity = evt.target;

        //de-select target if it died
        CastTarget target = m_model.getTarget();
        
        if(target.getEntityList().Contains(deadEntity))
        {
            target.removeTargetEntity(deadEntity);

            List<ICastEntity> currentTargets = target.getEntityList();
            ICastEntity newTarget = (currentTargets.Count == 0) ? null : currentTargets[0];
            EventBus.game.dispatch(new PlayerTargetSelected(newTarget as EntityModel));
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
