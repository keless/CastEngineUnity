using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TargetPanelViewController : CommonMonoBehaviour {

    ICastEntity m_target = null;

    Image bg;

	// Use this for initialization
	void Start () {
        bg = GetComponent<Image>();
        bg.enabled = false;

        SetListener(PlayerTargetSelected.EvtName, onTargetSelected, "game");
	}
	
	// Update is called once per frame
	void Update () {
	
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
            bg.enabled = true;
            //todo: add target icon
        }
        else
        {
            bg.enabled = false;
            //todo: remove target icon
        }
    }
}
