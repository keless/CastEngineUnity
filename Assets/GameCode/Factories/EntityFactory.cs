using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class EntityFactory : MonoBehaviour
{
    static string tempAbility1 = @"{
                ""name"": ""Attack"",
		        ""castTime"": 0.45,
		        ""cooldownTime"": 2.85,
		        ""range"": 5,
                ""element"":""sword"",
		        ""effectsOnCast"": [
				        {
						        ""effectType"": ""damage"",
						        ""damageType"": ""piercing"",
						        ""targetStat"": ""hp_curr"",
						        ""valueBase"": 50,
						        ""valueStat"": ""str"",
						        ""valueMultiplier"": 2,
						        ""react"": ""shake""
                        }
		        ]
	        }";
    static string tempAbility2 = @"{
                ""name"": ""Shoot"",
		        ""castTime"": 0.15,
		        ""cooldownTime"": 0.75,
		        ""range"": 50,
                ""element"":""ice"",
		        ""effectsOnCast"": [
				        {
						        ""effectType"": ""damage"",
						        ""damageType"": ""piercing"",
						        ""targetStat"": ""hp_curr"",
						        ""valueBase"": 10,
						        ""valueStat"": ""int"",
						        ""valueMultiplier"": 2,
						        ""react"": ""shatter""
                        }
		        ]
	        }";

    static string tempAbility3 = @"{
                ""name"": ""Curse"",
		        ""castTime"": 0.15,
		        ""cooldownTime"": 1.75,
		        ""range"": 50,
                ""element"":""shadow"",
		        ""effectsOnCast"": [
				        {
						        ""effectType"": ""damage"",
                                ""tickFreq"":1,
                                ""lifeTime"":5,
						        ""targetStat"": ""hp_curr"",
						        ""valueBase"": 10,
						        ""valueStat"": ""int"",
						        ""valueMultiplier"": 2
                        }
		        ]
	        }";

    static void AddAbilityToModel(JToken abilityJson, EntityModel model)
    {
        //TODO: use abilityId as input, then look up a dictionary JSON cache of abilities
        CastCommandModel abilityModel = new CastCommandModel(abilityJson);

        // add instance to model
        CastCommandState ability = new CastCommandState(abilityModel, model);
        model.testAddAbility(ability);
    }

    public static EntityModel CreatePlayer(GameWorldModel world, Vector3 pos, Quaternion rot = new Quaternion())
    {
        //ThirdPersonUserControl
        //ThirdPersonCharacter
        //EntityView
        //PlayerEntityController
        GameObject entityPre = (GameObject)Resources.Load<GameObject>("GameEntity");
        GameObject go = (GameObject)Instantiate(entityPre, pos, rot);
        go.tag = "Player";

        //PlayerEntityController peCtrl = go.AddComponent<PlayerEntityController>();
        EntityModel model = new EntityModel("player");

        AddAbilityToModel(JToken.Parse(tempAbility1), model);
        AddAbilityToModel(JToken.Parse(tempAbility2), model);
        AddAbilityToModel(JToken.Parse(tempAbility3), model);
        
        PlayerEntityController.AddPlayerEntityController(go, model);

        EntityAnimationController animController = go.GetComponent<EntityAnimationController>();
        animController.SetModel(model);

        go.GetComponentInChildren<BillboardHPBar>().hpValueProvider = model;

        //todo: em.initFromJson();
        world.AddGameObjectEntityPair(go, model);

        go.SetActive(true);

        UnityCameraFollow ucf = GameObject.FindObjectOfType<UnityCameraFollow>() as UnityCameraFollow;
        ucf.target = model.gameObject.transform.FindChild("OTSTarget");
        //todo: set public members of added components so they work

        return model;
    }

    //public GameObject dummyPrefab;
    public static EntityModel CreateDummy(GameWorldModel world, Vector3 pos, Quaternion rot = new Quaternion())
    {
        GameObject dummyPre = (GameObject)Resources.Load("DummyEntity", typeof(GameObject));
        GameObject go = (GameObject)Instantiate(dummyPre, pos, rot);

        EntityModel model = new EntityModel("dummy");
        world.AddGameObjectEntityPair(go, model);

        go.transform.FindChild("hpBar").gameObject.SetActive(false);

        go.SetActive(true);

        return model;
    }
}
