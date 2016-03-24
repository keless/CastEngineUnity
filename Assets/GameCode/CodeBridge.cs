using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CodeBridge : CommonMonoBehaviour {

    public Camera MainCamera;

    GameWorldModel m_gameWorld;
    EntityModel m_playerEntity;
    GameObject m_playerObject
    {
        get
        {
            return m_playerObject.gameObject;
        }
    }

	// Use this for initialization
	void Start () {

        m_gameWorld = new GameWorldModel(this);
        CreatePlayerEntity(new Vector3(0, 1, 0));
        CreateDummyEntity(new Vector3(10, 1, 0));
    }
	
	// Update is called once per frame
	void Update () {

	}

    new void OnDestroy()
    {
        Debug.Log("CodeBridge: OnDestroy()");

        base.OnDestroy();

        m_gameWorld.Destroy();
        m_gameWorld = null;
    }

    //public GameObject entityPrefab;
    void CreatePlayerEntity( Vector3 pos, Quaternion rot = new Quaternion() )
    {
        //ThirdPersonUserControl
        //ThirdPersonCharacter
        //EntityView
        //PlayerEntityController
        GameObject entityPre = (GameObject)Resources.Load("GameEntity", typeof(GameObject));
        GameObject go = (GameObject)Instantiate(entityPre, pos, rot);
        go.SetActive(false);
        go.tag = "Player";

        //PlayerEntityController peCtrl = go.AddComponent<PlayerEntityController>();
        EntityModel model = new EntityModel("player");
        PlayerEntityController.AddPlayerEntityController(go, model);

        
        //todo: em.initFromJson();
        m_gameWorld.AddGameObjectEntityPair(go, model);
        m_playerEntity = model;

        m_playerEntity.setProperty("hp_curr", m_playerEntity.hp_base / 2, null);

        go.SetActive(true);

        UnityCameraFollow ucf = GameObject.FindObjectOfType<UnityCameraFollow>() as UnityCameraFollow;
        ucf.target = model.gameObject.transform.FindChild("OTSTarget");
        //todo: set public members of added components so they work
    }

    //public GameObject dummyPrefab;
    void CreateDummyEntity( Vector3 pos, Quaternion rot = new Quaternion())
    {
        GameObject dummyPre = (GameObject)Resources.Load("DummyEntity", typeof(GameObject));
        GameObject go = (GameObject)Instantiate(dummyPre, pos, rot);

        EntityModel model = new EntityModel("dummy");
        m_gameWorld.AddGameObjectEntityPair(go, model);
    }

    public void DestroyGameObject ( GameObject go, float dt = 0.0f )
    {
        Destroy(go, dt);
    }
}
