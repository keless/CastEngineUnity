using UnityEngine;
using System.Collections;

public class EntityView : MonoBehaviour, IHitpointValueProvider
{

    EntityModel m_pModel;

	// Use this for initialization
	void Start () {
        test();
    }
	
    void test()
    {
        setModel(new EntityModel("dummy"));
    }

	// Update is called once per frame
	void Update () {
	
	}

    public void setModel( EntityModel pModel )
    {
        m_pModel = pModel;
    }

    // IHitpointValueProvider
    public int getHPCurr()
    {
        return m_pModel.hp_curr;
    }
    public int getHPMax()
    {
        return m_pModel.hp_base;
    }
}
