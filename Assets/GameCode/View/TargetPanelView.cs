using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TargetPanelView : CommonMonoBehaviour {

    EntityModel m_target = null;
    Text txtTargetName;

    [SerializeField]
    Transform debuffAnchor;

    [SerializeField]
    int DebuffIconOffset = 50;

    List<RectTransform> debuffIcons = new List<RectTransform>();

	// Use this for initialization
	void Awake() {
        Debug.Assert(debuffAnchor != null);

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

    void setNewTarget(EntityModel target )
    {
        removeListeners(m_target);
        m_target = target;
        if (m_target != null)
        {
            gameObject.SetActive(true);
            txtTargetName.text = target.getName();
            attachListeners(target);
        }
        else
        {
            gameObject.SetActive(false);
        }

        updateDebuffIcons();
    }

    void attachListeners(EntityModel target)
    {
        if (target == null) return;
        target.addListener(GameEntityDebuffApplied.EvtName, onDebuffApplied);
        target.addListener(GameEntityDebuffRemoved.EvtName, onDebuffRemoved);
    }
    void removeListeners(EntityModel target)
    {
        if (target == null) return;
        target.removeListener(GameEntityDebuffApplied.EvtName, onDebuffApplied);
        target.removeListener(GameEntityDebuffRemoved.EvtName, onDebuffRemoved);
    }
    void onDebuffApplied(EventObject e)
    {
        Debug.LogWarning("onDebuffApplied");
        updateDebuffIcons();
    }
    void onDebuffRemoved(EventObject e)
    {
        Debug.LogWarning("onDebuffRemoved");
        updateDebuffIcons();
    }

    void updateDebuffIcons()
    {
        //TODO: dont nuke this completely
        debuffAnchor.DetachChildren();
        debuffIcons.Clear();

        if (m_target == null) return;

        var debuffs = m_target.getDebuffs();
        var negatives = m_target.getNegativeEffects();

        Vector3 pos = debuffAnchor.transform.position;
        foreach(var pair in debuffs)
        {
            CastEffect debuff = pair.Value;
            Image icon = SkillButtonFactory.CreateDebuffIcon(debuff);

            RectTransform rt = icon.GetComponent<RectTransform>();
            rt.SetParent(debuffAnchor, false);
            rt.position = pos;
            pos.x += DebuffIconOffset;
            debuffIcons.Add(rt);
        }

        foreach (var debuff in negatives)
        {
            Image icon = SkillButtonFactory.CreateDebuffIcon(debuff);

            RectTransform rt = icon.GetComponent<RectTransform>();
            rt.SetParent(debuffAnchor, false);
            rt.position = pos;
            pos.x += DebuffIconOffset;
            debuffIcons.Add(rt);
        }


    }
}
