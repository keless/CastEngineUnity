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
        buttonPre.name = "btnSkill" + abilityIdx;

        Text label = buttonPre.transform.FindChild("Text").GetComponent<Text>();
        label.text = strLabel;

        SkillButtonView sbView = buttonPre.GetComponent<SkillButtonView>();
        sbView.abilityIndex = abilityIdx;

        return buttonPre;
    }
}

