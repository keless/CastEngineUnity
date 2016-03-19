using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class PlayerEntityController : CommonMonoBehavior, IHitpointValueProvider
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

        //create temp ability
        string strJson = @"{
                ""name"": ""Attack"",
		        ""castTime"": 1.15,
		        ""cooldownTime"": 1.85,
		        ""range"": 5,
		        ""effectsOnCast"": [
				        {
						        ""effectType"": ""damage"",
						        ""damageType"": ""piercing"",
						        ""targetStat"": ""hp_curr"",
						        ""valueBase"": 2,
						        ""valueStat"": ""str"",
						        ""valueMultiplier"": 2,
						        ""react"": ""shake""
                        }
		        ]
	        }";
        JToken abilityJson = JToken.Parse(strJson);
        m_testAbility = new CastCommandModel(abilityJson);

        // add to model
        CastCommandState ability = new CastCommandState(m_testAbility, m_model);
        m_model.testAddAbility(ability);

        SetListener("btnSkill1", onBtnSkill1);
	}
	
	// Update is called once per frame
	void Update () {
	
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

        CastTarget target = m_model.getTarget();
        if (!target.hasTargetsAtRangeFromEntity(ability.getRange(), m_model))
        {
            Debug.Log("ability " + (abilityIdx + 1) + " has no targets in range");
            return;
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
