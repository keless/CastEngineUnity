using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public interface IHitpointValueProvider
{
    int getHPCurr();
    int getHPMax();
}

public class EntityModel : ICastEntity, IEventBus, IHitpointValueProvider
{
    EventBus eventBus = new EventBus("EntityModel");
    string m_name = "";

    bool m_isDead;

    Dictionary<string, int> m_stats;

    //todo List<object> m_items;
    List<CastCommandState> m_abilities;
    CastTarget m_abilityTargets;

    List<CastEffect> m_negativeEffects;
    List<CastEffect> m_positiveEffects;
    Dictionary<string, CastEffect> m_buffs;
    Dictionary<string, CastEffect> m_debuffs;

    public EntityModel(string name)
    {
        Debug.Log("creating EntityModel(" + name + ")");

        eventBus.verbose = false;
        m_isDead = false;

        m_name = name;
        m_abilities = new List<CastCommandState>();
        m_abilityTargets = new CastTarget();

        m_negativeEffects = new List<CastEffect>();
        m_positiveEffects = new List<CastEffect>();
        m_buffs = new Dictionary<string, CastEffect>();
        m_debuffs = new Dictionary<string, CastEffect>();

        m_stats = new Dictionary<string, int>();
        m_stats.Add("hp_base", 100);
        m_stats.Add("hp_curr", 100);
        m_stats.Add("mana_base", 100);
        m_stats.Add("mana_curr", 100);
        m_stats.Add("agi_base", 10);
        m_stats.Add("agi_curr", 10);
        m_stats.Add("int_base", 10);
        m_stats.Add("int_curr", 10);
        m_stats.Add("str_base", 10);
        m_stats.Add("str_curr", 10);
        m_stats.Add("xp_next", 0);
        m_stats.Add("xp_curr", 0);
        m_stats.Add("xp_level", 1);
        m_stats.Add("xp_worth", 10);

    }

    public GameObject gameObject { get; set; }
    public Vector3 position { get { return gameObject.transform.position; } }

    bool _destroyed = false;
    ~EntityModel()
    {
        if (!_destroyed) {
            Debug.LogError("EntityModel destructor called before Destroy(), someone forgot to clean up");
            this.Destroy();
        }
    }
    public void Destroy()
    {
        Debug.Log("destroying EntityModel(" + m_name + ")");
        _destroyed = true;
        eventBus.Destroy();
    }

    public string getName()
    {
        return m_name;
    }

    public int hp_curr
    {
        get
        {
            return m_stats["hp_curr"];
        }
        private set
        {
            m_stats["hp_curr"] = value;
        }
    }
    public int hp_base
    {
        get
        {
            return m_stats["hp_base"];
        }
        private set
        {
            m_stats["hp_base"] = value;
        }
    }

    public JToken toJson()
    {
        return JToken.FromObject(this);
    }

    public bool initFromJson(JToken json)
    {
        m_name = json.Get<string>("name");

        if (json.hasOwnMember("stats"))
        {
            JObject stats = json["stats"] as JObject;
            foreach (JProperty stat in stats.Properties())
            {
                m_stats[stat.Name] = (int)stat.Value;
            }
        }

        if (json.hasOwnMember("inventory"))
        {
            //todo
        }

        return true;
    }

    public Dictionary<string, CastEffect> getBuffs()
    {
        return m_buffs;
    }

    public Dictionary<string, CastEffect> getDebuffs()
    {
        return m_debuffs;
    }

    public List<CastEffect> getNegativeEffects()
    {
        return m_negativeEffects;
    }

    public List<CastEffect> getPositiveEffects()
    {
        return m_positiveEffects;
    }

    public List<CastCommandState> getAbilities()
    {
        return m_abilities;
    }

    public bool canCast()
    {
        //no abilities are 'casting' or 'channeling'
        foreach (CastCommandState a in m_abilities)
        {
            if (a.isCasting() || a.isChanneling()) return false;
        }

        return true;
    }

    void _checkForDeath()
    {
        if(!m_isDead && hp_curr <= 0)
        {
            Debug.LogWarning("entity died");
            hp_curr = 0;
            m_isDead = true;

            dispatch(new GameEntityDied());

            CastWorldModel.Get().handleEntityDeath(this);
        }
    }

    // ICastEntity methods
    public void setProperty(string propName, float value, CastEffect effect)
    {
        if (!m_stats.ContainsKey(propName)) return;
        m_stats[propName] = (int)value;

        if( propName == "hp_base")
        {
            if(m_stats["hp_curr"] > m_stats["hp_base"])
            {
                m_stats["hp_curr"] = m_stats["hp_base"]; //cap
            }
        }

        dispatch(new GameEntityPropertyChangeEvt("setProperty", propName, value));

        if( propName == "hp_base" || propName == "hp_curr")
        {
            _checkForDeath();
        }
    }
    public void incProperty(string propName, float value, CastEffect effect)
    {
        if (!m_stats.ContainsKey(propName)) return;

        if( propName == "hp_curr" && value < 0 )
        {
            if (m_isDead) return;
        }

        m_stats[propName] += (int)value;

        if ( propName == "hp_base" )
        {
            if (hp_curr > hp_base)
            {
                hp_curr = hp_base; //clamp hp_curr to max

                _checkForDeath();
            }
        }
        else if (propName == "hp_curr")
        {
            //bounds check hp_curr
            if (hp_curr < 0) hp_curr = 0;
            if (hp_curr > hp_base) hp_curr = hp_base;

            _checkForDeath();
        }
        else if (propName == "xp_curr" && m_stats["xp_next"] != 0)
        {
            if (m_stats["xp_curr"] >= m_stats["xp_next"])
            {

                //todo: handle entity level up
                Debug.Log("todo: handle entity level up");
            }
        }
        dispatch(new GameEntityPropertyChangeEvt("incProperty", propName, value));
    }
    public void startBuffProperty(string propName, float value, CastEffect effect)
    {
        if (!m_stats.ContainsKey(propName)) return;
        m_stats[propName] = m_stats[propName] + (int)value;

        if(propName == "hp_base")
        {
            if( value < 0 )
            {
                if (hp_base < 0) hp_base = 0;
                if (hp_curr > hp_base) hp_curr = hp_base;
            }
        }
    }
    public void endBuffProperty(string propName, float value, CastEffect effect)
    {
        if (!m_stats.ContainsKey(propName)) return;
        m_stats[propName] = m_stats[propName] + (int)value;

        if( propName == "hp_base")
        {
            if( value < 0 )
            {
                //losing max health
                if (hp_base < 0) hp_base = 0;
                if (hp_curr > hp_base) hp_curr = hp_base;

                _checkForDeath();
            }
        }
    }

    // in: string propName
    //float
    public float getProperty(string propName)
    {
        if (!m_stats.ContainsKey(propName)) return 0; //todo
        return m_stats[propName];
    }

    //CastTarget
    public CastTarget getTarget()
    {
        return m_abilityTargets;
    }

    public void testAddAbility( CastCommandState ability )
    {
        m_abilities.Add(ability);
    }

    // in: json reaction, CastEffect source
    public void handleEffectReaction(JToken reaction, CastEffect source)
    {
        dispatch(new GameEntityReactEvt(reaction, source));
    }

    public void handleEffectEvent(string effectEventName, CastEffect source)
    {
        EventBus.game.dispatch(new GameEntityEffectEvt(effectEventName, source.getTarget(), this));
    }

    //effect is ARRIVING at this entity
    public void applyEffect(CastEffect effect)
    {
        if( effect.getLifeTime() == 0 )
        {
            Debug.Log("apply effect - instant");
            effect.startTicks();
        }else
        {
            switch(effect.getType())
            {
                case CastEffectType.BUFF_STAT:
                    m_buffs.Add(effect.getName(), effect);
                    break;
                case CastEffectType.SUPPRESS_STAT:
                    m_debuffs.Add(effect.getName(), effect);
                    this.dispatch(new GameEntityDebuffApplied(effect));
                    break;
                case CastEffectType.DAMAGE_STAT:
                    Debug.Log("apply effect - dot");
                    m_negativeEffects.Add(effect);
                    this.dispatch(new GameEntityDebuffApplied(effect));
                    break;
                case CastEffectType.HEAL_STAT:
                    Debug.Log("apply effect - hot");
                    m_positiveEffects.Add(effect);
                    break;
                default:
                    Debug.LogWarning("unexpected cast effect type");
                    break;
            }

            Debug.Log("apply effect - start effect");
            effect.startTicks();
        }
    }
    public void removeEffect(CastEffect effect)
    {
        switch(effect.getType())
        {
            case CastEffectType.BUFF_STAT:
                m_buffs.Remove(effect.getName());
                break;
            case CastEffectType.SUPPRESS_STAT:
                m_debuffs.Remove(effect.getName());
                this.dispatch(new GameEntityDebuffRemoved(effect));
                break;
            case CastEffectType.DAMAGE_STAT:
                m_negativeEffects.Remove(effect);
                this.dispatch(new GameEntityDebuffRemoved(effect));
                break;
            case CastEffectType.HEAL_STAT:
                m_positiveEffects.Remove(effect);
                break;
            default:
                Debug.LogWarning("unexpected cast effect type");
                break;
        }
    }

    public void onAbilityCastStart(CastCommandState ability)
    {
        int index = m_abilities.IndexOf(ability);
        this.dispatch(new EntityStartCast(index, ability.getCastPeriod(), CastCommandTime.Get()));
    }
    public void onAbilityChannelStart(CastCommandState ability)
    {
        int index = m_abilities.IndexOf(ability);
        this.dispatch(new EntityStartChannel(index, ability.getChannelPeriod(), CastCommandTime.Get()));
    }
    public void onAbilityCooldownStart(CastCommandState ability)
    {
        int index = m_abilities.IndexOf(ability);
        this.dispatch(new EntityStartCooldown(index, ability.getCooldownPeriod(), CastCommandTime.Get()));
    }
    public void onAbilityIdleStart(CastCommandState ability)
    {
        int index = m_abilities.IndexOf(ability);
        this.dispatch(new EntityStartIdle(index, CastCommandTime.Get()));
    }

    //IHitpointProvider
    public int getHPCurr()
    {
        return this.hp_curr;
    }

    public int getHPMax()
    {
        return this.hp_base;
    }

    //IEntityBus
    public void addListener(string eventName, EventBusDelegate callbackFunction)
    {
        eventBus.addListener(eventName, callbackFunction);
    }
    public void removeListener(string eventName, EventBusDelegate callbackFunction)
    {
        eventBus.removeListener(eventName, callbackFunction);
    }
    public void dispatch(EventObject eventObj)
    {
        eventBus.dispatch(eventObj);
    }
}
