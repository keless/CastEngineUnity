using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;

public class PlayerEntityController : CommonMonoBehavior {

    EntityModel m_model;

    CastCommandModel m_testAbility;

	// Use this for initialization
	void Start () {

        m_model = new EntityModel("bobzilla");
        m_model.setProperty("hp_curr", m_model.hp_base / 2, null);

        EntityView ev = GetComponent<EntityView>();
        ev.setModel(m_model);

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
}
