using UnityEngine;
using System.Collections;
using UnityEngine.UI;


[RequireComponent(typeof(Slider))]
public class UIHPBarView : CommonMonoBehaviour
{

    Slider sliderBar;

    IHitpointValueProvider hpValueProvider;

    void Start()
    {
        sliderBar = GetComponent<Slider>();

        gameObject.SetActive(false);

        SetListener(PlayerTargetSelected.EvtName, onTargetSelected, "game");
    }

    // Update is called once per frame
    void Update()
    {
        Transform healthBar = this.transform.GetChild(0);
        //todo: HP percentage
        float percent = hpValueProvider.getHPCurr() / (float)hpValueProvider.getHPMax();
        sliderBar.value = sliderBar.maxValue * percent;
    }

    void onTargetSelected(EventObject e)
    {
        PlayerTargetSelected evt = e as PlayerTargetSelected;

        EntityModel target = evt.target as EntityModel;
        hpValueProvider = target;

        if(hpValueProvider == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}
