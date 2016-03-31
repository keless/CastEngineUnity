using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class EntityFactory : MonoBehaviour
{
    public static EntityModel CreatePlayer(GameWorldModel world, Vector3 pos, Quaternion rot = new Quaternion())
    {
        //ThirdPersonUserControl
        //ThirdPersonCharacter
        //EntityView
        //PlayerEntityController
        GameObject entityPre = (GameObject)Resources.Load("GameEntity", typeof(GameObject));
        GameObject go = (GameObject)Instantiate(entityPre, pos, rot);
        go.tag = "Player";

        //PlayerEntityController peCtrl = go.AddComponent<PlayerEntityController>();
        EntityModel model = new EntityModel("player");
        PlayerEntityController.AddPlayerEntityController(go, model);

        EntityAnimationController animController = go.GetComponent<EntityAnimationController>();
        animController.SetModel(model);

        go.GetComponentInChildren<BillboardHPBar>().hpValueProvider = model;

        //todo: em.initFromJson();
        world.AddGameObjectEntityPair(go, model);

        //create temp ability
        string strJson = @"{
                ""name"": ""Attack"",
		        ""castTime"": 0.45,
		        ""cooldownTime"": 2.85,
		        ""range"": 5,
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
        JToken abilityJson = JToken.Parse(strJson);
        CastCommandModel ability1 = new CastCommandModel(abilityJson);

        // add to model
        CastCommandState ability = new CastCommandState(ability1, model);
        model.testAddAbility(ability);


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
