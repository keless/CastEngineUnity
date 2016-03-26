using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BillboardHPBar))]
public class TriggerHPBarOnTargeted : CommonMonoBehaviour {

    BillboardHPBar hpBar;
    GameObject entityObject;

	// Use this for initialization
	void Awake() {
        hpBar = GetComponent<BillboardHPBar>();
        entityObject = transform.parent.gameObject;
        Debug.Assert(entityObject);

        SetListener(PlayerTargetSelected.EvtName, onPlayerTargetSelected, "game");
    }

    void onPlayerTargetSelected(EventObject e)
    {
        PlayerTargetSelected evt = e as PlayerTargetSelected;
        EntityModel entity = evt.target as EntityModel;

        if(entity.gameObject == entityObject)
        {
            //we were chosen, yay!
            hpBar.hpValueProvider = entity;
            hpBar.gameObject.SetActive(true);
        }
        else
        {
            //im going to my dark place now
            hpBar.gameObject.SetActive(false);
        }
    }
}
