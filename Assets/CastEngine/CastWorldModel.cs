using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CastCommandTime
{
    static double s_time = 0.0;
    public static double Get()
    {
        return CastCommandTime.s_time;
    }

    // in: double dt in SECONDS
    public static double UpdateDelta(double dt )
    {
        CastCommandTime.s_time += dt;
        return CastCommandTime.s_time;
    }

    // in: double t
    public static double Set(double t )
    {
        CastCommandTime.s_time = t;
        return CastCommandTime.s_time;
    }
}


public interface ICastPhysics
{
    // in: ICastEntity fromEntity, ICastEntity toEntity
    // out: null or Vec2D distVec
    Vector3? GetVecBetween(ICastEntity fromEntity, ICastEntity toEntity);
    //{ return null; }

    // in: ICastEntity entity
    // out: null or Vec2D pos
    Vector3? GetEntityPosition(ICastEntity entity);
    //{ return null; }

    // in: Vec2D p, float r, array[ICastEntity] ignoreEntities
    // out: array<ICastEntity> entities
    List<ICastEntity> GetEntitiesInRadius(Vector3 p, float r, List<ICastEntity> ignoreEntities = null);
    //{ return new List<ICastEntity>(); }
}


public class CastEffectPath
{
    public double startTime = 0;
    public double speed = 0;
    public ICastEntity from = null;
    public ICastEntity to = null;
    public Vector3 toPosition = new Vector3();
    public float radius = 0.0f;
    public List<CastEffect> effects = new List<CastEffect>();
}

public interface ICastWorldDelegate
{
    void onEntityDeath(ICastEntity entity);
}

/*
 CastWorldModel
 
  simulates a 'world' that casted abilities happen inside of -- 
    basically just handles effects that are "in transit" to their targets
 
 //TODO: hold all ICastEntities, then implement CastEntityHandles instead of ICastEntity*
			and resolve CastEntityHandles into ICastEntities inside of CastWorldModel
	// this will allow us to gracefully abort effects in transit to/from dead entities
	// alternately1-- hold entities in a graveyard until all references to them expire
	// alternately2-- expose "retain/release" in interface
 
 */
public class CastWorldModel
{
    static CastWorldModel instance = new CastWorldModel();
    public static CastWorldModel Get()
    {
        return CastWorldModel.instance;
    }
	public static void Reset() {
        instance.Destroy();
		instance = new CastWorldModel ();
	}

    Dictionary<ICastEntity, ICastEntity> m_allEntities = new Dictionary<ICastEntity, ICastEntity>();
    List<CastEffectPath> m_effectsInTransit = new List<CastEffectPath>();
    ICastPhysics m_pPhysics = null;
    ICastWorldDelegate m_delegate = null;

    CastWorldModel()
    {

    }

    void Destroy()
    {
        m_delegate = null;
    }

    public void SetDelegate(ICastWorldDelegate castWorldDelegate)
    {
        m_delegate = castWorldDelegate;
    }

    public void AddEntity(ICastEntity entity )
    {
        this.m_allEntities[entity] = entity;
    }

    public bool RemoveEntity(ICastEntity entity )
    {
        if( m_allEntities.ContainsKey(entity))
        {
            m_allEntities.Remove(entity);
            return true;
        }
        return false;
    }

    public void handleEntityDeath(ICastEntity entity)
    {
        if( RemoveEntity(entity) )
        {
            if (m_delegate != null) m_delegate.onEntityDeath(entity);
        }
    }


    public int CountEntities()
    {
        return m_allEntities.Count;
    }

    // in: ICastPhysics physics
    public void SetPhysicsInterface(ICastPhysics physics )
    {
        this.m_pPhysics = physics;
    }

    //ICastPhysics
    public ICastPhysics getPhysicsInterface()
    {
        return this.m_pPhysics;
    }

    // in: ICastEntity fromEntity, CastEffect effect, CastTarget targetList, double startTime
    public void addEffectInTransit(ICastEntity fromEntity, CastEffect effect, CastTarget targetList, double startTime )
    {
        var speed = effect.getTravelSpeed();
        if (speed == 0.0)
        {
            this.addEffectInstant(fromEntity, effect, targetList, startTime);
            return;
        }

        if (targetList.getType() == CastTargetType.ENTITIES)
        {
            var path = new CastEffectPath();
            path.from = fromEntity;
            path.radius = 0.0f;
            path.speed = speed;
            path.startTime = startTime;

            var targets = targetList.getEntityList();
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];

                if (!CastWorldModel.Get().isValid(fromEntity)) continue;
                if (!CastWorldModel.Get().isValid(target)) continue;
                Debug.Log("add effect in transit");

                path.to = target;
                path.effects.Add(effect);
                //effect.retain(); //TODO
            }

            this.m_effectsInTransit.Add(path);
        }
        else {
            //TODO: world position
            //TODO: physics
        }
    }

    // in: ICastEntity fromEntity, CastEffect effect, CastTarget targetList, double startTime
    public void addEffectInstant(ICastEntity fromEntity, CastEffect effect, CastTarget targetList, double startTime )
    {

        if (targetList.getType() == CastTargetType.ENTITIES)
        {
            var path = new CastEffectPath();
            path.from = fromEntity;
            path.radius = 0.0f;
            path.speed = 0.0;
            path.startTime = startTime;

            var targets = targetList.getEntityList();
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];

                if (!CastWorldModel.Get().isValid(fromEntity)) continue;
                if (!CastWorldModel.Get().isValid(target)) continue;

                path.to = target;
                path.effects.Add(effect);
                //effect.retain(); //TODO
            }

            this.applyEffectToTarget(path);
            //effect.release(); //we dont hold onto the path, so dont hold onto the effect
        }
        else {
            //TODO: world position
        }
    }

    // in: CastEffectPath path
    public void applyEffectToTarget(CastEffectPath path )
    {
        Debug.Log("xxx applyEffectToTarget");

        var currTime = CastCommandTime.Get();
        for (var i = 0; i < path.effects.Count; i++)
        {
            var effect = path.effects[i];

            List<ICastEntity> targets = new List<ICastEntity>();
            if (path.to != null)
            {
                if (CastWorldModel.Get().isValid(path.to))
                {
                    targets.Add(path.to);

                    if (effect.isAoe())
                    {
                        var radius = (float)effect.getDescriptor("aoeRadius");
                        Vector3? p = this.m_pPhysics.GetEntityPosition(path.to);
                        if (p.HasValue)
                        {
							Debug.Log("possible bug here -- add targets dont overwrite it?"); //TODO possible bug here -- add targets dont overwrite it?
                            targets = this.m_pPhysics.GetEntitiesInRadius(p.Value, radius);
                        }
                    }
                }
            }
            else {
                //if targeted position, check physics to determine targets
                targets = this.m_pPhysics.GetEntitiesInRadius(path.toPosition, path.radius);
            }

            Dictionary<ICastEntity, ICastEntity> uniques = new Dictionary<ICastEntity, ICastEntity>(); //map<ICastEntity, ICastEntity>
            for (var t = 0; t < targets.Count; t++)
            {
                var target = targets[t];
                if (uniques.ContainsKey(target)) continue; //already applied
                uniques[target] = target;

                var eff = effect;
                if (t > 0) eff = effect.clone(); //targets might modify the effect, so give each additional target it's own copy

                eff.setTarget(target);
                eff.m_startTime = currTime; //start the clock on the effect's life time

                Debug.Log("xxx applyEffectToTarget --  target.applyEffect"); 
                target.applyEffect(eff);
            }
        }
    }


    // in: double dt in SECONDS
    public void updateStep(double dt )
    {
        List<int> resolvedPaths = new List<int>();

        CastCommandTime.UpdateDelta(dt);

        double currTime = CastCommandTime.Get();
        for (var i = 0; i < this.m_effectsInTransit.Count; i++)
        {
            //TODO: if blockable, check physics collision
            CastEffectPath path = this.m_effectsInTransit[i];

            var distToTargetFromOrigin = 1.0; //TODO: add physics checks
            var timeToTargetFromOrigin = distToTargetFromOrigin / path.speed;

            //TODO: if path is physics and can hit before stopping, check physics

            if (currTime - path.startTime >= timeToTargetFromOrigin)
            {
                this.applyEffectToTarget(path);

                //effect path reached target
                resolvedPaths.Add(i);
            }

            //TODO: probably some bug here VVVV 

            //clean up resolved paths
            for (var j = resolvedPaths.Count - 1; j >= 0;j--)
            {
                for (var e = this.m_effectsInTransit[i].effects.Count - 1; e >= 0; e--)
                {
                    //this.m_effectsInTransit[i].effects[e].release(); //TODO
                    this.m_effectsInTransit[i].effects[e] = null;
                }
                this.m_effectsInTransit[i].effects.Clear();

                this.m_effectsInTransit.RemoveAt(i);
            }
        }

        CastCommandScheduler.Get().Update();
    }

    public bool isValid(ICastEntity entity )
    {
        if (m_allEntities.ContainsKey(entity)) return true;
        return false;
    }
}
