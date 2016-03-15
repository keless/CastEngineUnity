using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CastTargetType
{
    ENTITIES,
    WORLD_POSITION,
    RELATIVE_POSITION
}

public class CastTarget
{
    CastTargetType m_type;
    Vector2 m_position;
    List<ICastEntity> m_entityList;

    public CastTarget( CastTargetType ctype = CastTargetType.ENTITIES )
    {
        this.m_type = ctype;
        this.m_position = new Vector2();
        this.m_entityList = new List<ICastEntity>();
    }

    public List<ICastEntity> getEntityList()
    {
        return this.m_entityList;
    }

    public CastTargetType getType()
    { return this.m_type; }


    public void clearTargetEntities()
    {
        this.m_entityList.Clear();
    }

    // in: ICastEntity target
    public void addTargetEntity(ICastEntity target )
    {
        if (target == null) return;

        this.m_type = CastTargetType.ENTITIES;
        this.m_entityList.Add(target);
    }

    public void setTargetPosition(Vector2 target )
    {
        this.m_position.Set(target.x, target.y);
    }

    //will purge pointers to entities that are invalid
    public void validateTargets()
    {
        CastWorldModel world = CastWorldModel.Get();
        for (var i = this.m_entityList.Count - 1; i >= 0; i--)
        {
            if (!world.isValid(this.m_entityList[i]))
            {
                this.m_entityList.RemoveAt(i);
            }
        }
    }

    public bool hasTargetsAtRangeFromEntity(float range, ICastEntity fromEntity )
    {
        float rangeSq = range * range;
        if (this.m_type == CastTargetType.ENTITIES)
        {
            bool foundTarget = false;
            CastWorldModel world = CastWorldModel.Get();
            for (var i = this.m_entityList.Count - 1; i >= 0; i--)
            {
                if (!world.isValid(this.m_entityList[i]))
                {
                    this.m_entityList.RemoveAt(i);
                }
                else {
                    if (!foundTarget)
                    {
                        ICastEntity to = this.m_entityList[i];
                        Vector2? distVec = world.getPhysicsInterface().GetVecBetween(fromEntity, to);

                        if (distVec.HasValue && distVec.Value.sqrMagnitude <= rangeSq)
                        {
                            foundTarget = true;
                            //note: dont break here, we still want to perform world->isValid on the rest of the elements
                        }
                    }
                }
            }

            return foundTarget;
        }
        else {
            return true; //TODO: physics targeting (aka skill shots)
        }
    }

    public bool hasValidTarget()
    {
        if (this.m_type == CastTargetType.ENTITIES)
        {
            this.validateTargets();
            return this.m_entityList.Count > 0;
        }
        else {
            return true; //TODO: physics targeting (aka skill shots)
        }
    }
}
