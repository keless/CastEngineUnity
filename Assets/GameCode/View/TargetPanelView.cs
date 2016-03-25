using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TargetPanelView : CommonMonoBehaviour {

    ICastEntity m_target = null;
    Text txtTargetName;

	// Use this for initialization
	void Start () {

        txtTargetName = gameObject.GetComponentInChildren<Text>(true);

        SetListener(PlayerTargetSelected.EvtName, onTargetSelected, "game");

        gameObject.SetActive(false);
	}

    void onTargetSelected(EventObject e)
    {
        PlayerTargetSelected evt = e as PlayerTargetSelected;
        
        if( evt.target == m_target )
        {
            Debug.LogWarning("target panel onTargetSelected() - that target was already selected, ignoring");
            return;
        }

        setNewTarget(evt.target);
    }

    void setNewTarget( ICastEntity target )
    {
        m_target = target;
        if (m_target != null)
        {
            gameObject.SetActive(true);

            EntityModel model = target as EntityModel;
            txtTargetName.text = model.getName();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
