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

    public static Image CreateDebuffIcon( CastEffect debuff )
    {
        Image iconPre = (Image)Resources.Load<Image>("imgTargetDebuffIcon");
        Image icon = GameObject.Instantiate(iconPre);

        //todo: data-drive this

        Sprite sprIcon = Resources.Load<Sprite>("SpellIcons/horror-acid-3");

        icon.sprite = sprIcon;

        return icon;
    }
}

