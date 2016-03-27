using UnityEngine;
using System.Collections.Generic;

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
