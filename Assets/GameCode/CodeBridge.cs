﻿using UnityEngine;
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
        m_playerEntity = EntityFactory.CreatePlayer(m_gameWorld, new Vector3(0, 1, 0));
        m_playerEntity.setProperty("hp_curr", m_playerEntity.hp_base / 2, null);

        EntityFactory.CreateDummy(m_gameWorld, new Vector3(0, 1, 3));
    }
	
	// Update is called once per frame
	void Update () {
        CastCommandTime.UpdateDelta(Time.deltaTime);
        CastCommandScheduler.Get().Update();
	}

    new void OnDestroy()
    {
        base.OnDestroy();

        m_gameWorld.Destroy();
        m_gameWorld = null;
    }

    public void DestroyGameObject ( GameObject go, float dt = 0.0f )
    {
        Destroy(go, dt);
    }
}
