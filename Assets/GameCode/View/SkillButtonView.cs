using UnityEngine;
using System.Collections;

public class SkillButtonView : CommonMonoBehaviour {

    [SerializeField] int abilityIndex;

	// Use this for initialization
	void Awake () {
        SetListener(AbilityStartCast.EvtName, onAbilityStartCast);
        SetListener(AbilityStartCooldown.EvtName, onAbilityStartCooldown);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void onAbilityStartCast(EventObject e)
    {
        AbilityStartCast evt = e as AbilityStartCast;
        if (evt.abilityIdx != abilityIndex)
        {
            return;
        }

        Debug.Log("todo: start cast animation on skill button " + abilityIndex);
    }

    void onAbilityStartCooldown(EventObject e)
    {
        AbilityStartCooldown evt = e as AbilityStartCooldown;
        if (evt.abilityIdx != abilityIndex)
        {
            return;
        }

        Debug.Log("todo: start cooldown animation on skill button " + abilityIndex);
    }
}
