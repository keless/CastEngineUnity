using System;
using System.Collections.Generic;
using UnityEngine;

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
        if (!_destroyed)
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

    public void AddGameObjectEntityPair(GameObject go, EntityModel model)
    {
        model.gameObject = go;
        m_objectEntityMap.Add(go, model);
        m_world.AddEntity(model);
    }
    public void RemoveGameObjectEntityPair(GameObject go, bool destroy = true)
    {
        EntityModel model = m_objectEntityMap[go];
        model.gameObject = null;

        if (destroy)
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
        if (f == null || t == null)
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
                Debug.LogWarning("todo: sphere search, need to filter on EntityModel objects only?");
                continue;
            }
            EntityModel model = m_objectEntityMap[go];

            if( ignoreEntities != null && ignoreEntities.Contains(model))
            {
                continue;
            }

            found.Add(model);
        }
        return found;
    }
}