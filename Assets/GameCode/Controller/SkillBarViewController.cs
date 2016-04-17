using UnityEngine;
using System.Collections;

public class SkillBarViewController : CommonMonoBehaviour {


    public int spacing = 200;

	// Use this for initialization
	void Start () {
        Debug.Log("SkillBarViewController - start");
        SetListener(PlayerInitialized.EvtName, onPlayerInitialized);
	}

    void onPlayerInitialized(EventObject e)
    {
        Debug.Log("SkillBarViewController - player initialized");

        PlayerInitialized evt = e as PlayerInitialized;
        initFromPlayerEntity(evt.player);
    }

    void initFromPlayerEntity( EntityModel player )
    {
        var abilities = player.getAbilities();

        Transform anchor = transform.FindChild("btnStartAnchor");
        Vector3 pos = anchor.position;

        //TODO: clear all children (other skill buttons) except for "btnStartAnchor"

        int numAbilities = abilities.Count;
        for( var i=0; i< numAbilities; i++)
        {
            Debug.Log("create skill button for " + abilities[i].getName());

            GameObject skillBtn = SkillButtonFactory.CreateSkillButton(i, abilities[i].getName());
            skillBtn.transform.position = pos;
            pos.x += spacing;

            //xxx
            (skillBtn.transform as RectTransform).SetParent(transform, false);
            //skillBtn.transform.parent = transform;
        }
    }
}
