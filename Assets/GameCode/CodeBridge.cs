using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CodeBridge : CommonMonoBehavior {

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

class GameWorldModel : ICastPhysics
{
    CastWorldModel m_world;
    Dictionary<GameObject, EntityModel> m_objectEntityMap;
    CodeBridge m_codeBridge;

    public GameWorldModel(CodeBridge cb)
    {
        m_codeBridge = cb;
        m_objectEntityMap = new Dictionary<GameObject, EntityModel>();
        m_world = CastWorldModel.Get();
        m_world.setPhysicsInterface(this);
    }

    bool _destroyed = false;
    ~GameWorldModel()
    {
        if(!_destroyed)
        {
            Destroy();
        }
    }

    public void Destroy()
    {
        _destroyed = true;
        foreach (KeyValuePair<GameObject, EntityModel> gePair in m_objectEntityMap)
        {
            RemoveGameObjectEntityPair(gePair.Key, true);
        }
    }

    public void AddGameObjectEntityPair( GameObject go, EntityModel model )
    {
        model.gameObject = go;
        m_objectEntityMap.Add(go, model);
        m_world.AddEntity(model);
    }
    public void RemoveGameObjectEntityPair( GameObject go, bool destroy = true )
    {
        EntityModel model = m_objectEntityMap[go];
        model.gameObject = null;
        
        if(destroy)
        {
            m_codeBridge.DestroyGameObject(go);

            m_world.RemoveEntity(model);
            model.Destroy();
        }
    }

    // ICastPhysics
    public Vector3? GetVecBetween(ICastEntity fromEntity, ICastEntity toEntity)
    {
        EntityModel f = fromEntity as EntityModel;
        EntityModel t = toEntity as EntityModel;
        if( f == null || t == null)
        {
            return null;
        }

        return f.gameObject.transform.position - t.gameObject.transform.position;
    }

    // in: ICastEntity entity
    // out: null or Vec2D pos
    public Vector3? GetEntityPosition(ICastEntity entity)
    {
        GameObject go = (entity as EntityModel).gameObject;
        return go.transform.position;
    }

    // in: Vec2D p, float r, array[ICastEntity] ignoreEntities
    // out: array<ICastEntity> entities
    public List<ICastEntity> GetEntitiesInRadius(Vector3 p, float r, List<ICastEntity> ignoreEntities = null)
    {
        List<ICastEntity> found = new List<ICastEntity>();
        Collider[] hitColliders = Physics.OverlapSphere(p, r);
        foreach (Collider col in hitColliders)
        {
            GameObject go = col.gameObject;
            if (!m_objectEntityMap.ContainsKey(go))
            {
                Debug.LogWarning("todo: sphere search, need to filter on EntityModel objects only");
                continue;
            }
            EntityModel model = m_objectEntityMap[go];
            found.Add(model);
        }
        return found;
    }
}