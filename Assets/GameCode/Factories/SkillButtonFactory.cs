using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.UI;


class SkillButtonFactory
{
    public static GameObject CreateSkillButton(int abilityIdx, string strLabel)
    {
        GameObject buttonPre = (GameObject)Resources.Load<GameObject>("btnSkill");

        GameObject btn = GameObject.Instantiate(buttonPre);
        btn.name = "btnSkill" + abilityIdx;

        Text label = btn.transform.FindChild("Text").GetComponent<Text>();
        label.text = strLabel;

        SkillButtonView sbView = btn.GetComponent<SkillButtonView>();
        sbView.abilityIndex = abilityIdx;

        return btn;
    }
}

