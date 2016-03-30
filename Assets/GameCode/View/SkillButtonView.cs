using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class SkillButtonView : CommonMonoBehaviour {

    [SerializeField] int abilityIndex;

    enum SkillButtonState
    {
        IDLE,
        CASTING,
        CHANNELING,
        COOLDOWN
    }

    SkillButtonState m_state;
    double m_stateStart = 0;
    float m_statePeriod = 0f;

    Image imgCooldownFilter;

	// Use this for initialization
	void Awake () {
        imgCooldownFilter = transform.FindChild("imgCooldownFilter").GetComponent<Image>();
        Debug.Assert(imgCooldownFilter != null);

        gotoState(SkillButtonState.IDLE, 0f, CastCommandTime.Get());

        SetListener(AbilityStartCast.EvtName, onAbilityStartCast);
        SetListener(AbilityStartCooldown.EvtName, onAbilityStartCooldown);
        SetListener(AbilityStartIdle.EvtName, onAbilityStartIdle);
    }
	
	// Update is called once per frame
	void Update () {

        if (m_state == SkillButtonState.IDLE) return;

        double ct = CastCommandTime.Get();
        float dt = (float)(ct - m_stateStart);

        if (dt > m_statePeriod) dt = m_statePeriod;
        float pct = dt / m_statePeriod;

        switch(m_state)
        {
            case SkillButtonState.CASTING:
                imgCooldownFilter.fillAmount = (pct);
                break;
            case SkillButtonState.COOLDOWN:
                imgCooldownFilter.fillAmount = 1.0f - (pct);
                break;
        }
	}

    void gotoState(SkillButtonState state, float period, double start)
    {
        m_state = state;
        m_statePeriod = period;
        m_stateStart = start;
        switch(m_state)
        {
            case SkillButtonState.IDLE:
                imgCooldownFilter.fillAmount = 0;
                break;
            case SkillButtonState.CASTING:
                imgCooldownFilter.fillAmount = 0;
                break;
            case SkillButtonState.CHANNELING:
                imgCooldownFilter.fillAmount = 0;
                break;
            case SkillButtonState.COOLDOWN:
                imgCooldownFilter.fillAmount = 100;
                break;
            default:
                Debug.LogError("unhandled skillbutton state");
                break;
        }
    }

    void onAbilityStartCast(EventObject e)
    {
        AbilityStartCast evt = e as AbilityStartCast;
        if (evt.abilityIdx != abilityIndex)
        {
            return;
        }

        gotoState(SkillButtonState.CASTING, evt.castPeriod, evt.startTime);
    }

    void onAbilityStartCooldown(EventObject e)
    {
        AbilityStartCooldown evt = e as AbilityStartCooldown;
        if (evt.abilityIdx != abilityIndex)
        {
            return;
        }

        gotoState(SkillButtonState.COOLDOWN, evt.cooldownPeriod, evt.startTime);
    }

    void onAbilityStartIdle(EventObject e)
    {
        AbilityStartIdle evt = e as AbilityStartIdle;
        if (evt.abilityIdx != abilityIndex)
        {
            return;
        }

        gotoState(SkillButtonState.IDLE, 0f, evt.startTime);
    }
}
