using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.ThirdPerson;

public class EntityAnimationController : MonoBehaviour {

    public EntityModel m_pModel = null;

    void OnDestroy()
    {
        //clean up!
        RemoveModel();
    }

    public void SetModel( EntityModel model )
    {
        RemoveModel();
        m_pModel = model;
        m_pModel.addListener(TriggerAnimEvent.EvtName, onTriggerAnim);
    }

    public void RemoveModel()
    {
        if (m_pModel == null) return;
        m_pModel.removeListener(TriggerAnimEvent.EvtName, onTriggerAnim);
        m_pModel = null;
    }

    void onTriggerAnim(EventObject e)
    {
        Debug.LogWarning("todo: trigger animation");
    }
}
