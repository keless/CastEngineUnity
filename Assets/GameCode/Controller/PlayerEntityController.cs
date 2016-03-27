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
        component.m_model = model;
        return component;
    }

    // Use this for initialization
    void Start () {

        SetListener("btnSkill1", onBtnSkill1);
        SetListener(KeyEvent.EvtName, onKeyEvent);
	}
	
	// Update is called once per frame
	void Update () {
	
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
