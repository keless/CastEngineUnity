using UnityEngine;
using System.Collections;

public interface IHitpointValueProvider
{
    int getHPCurr();
    int getHPMax();
}

public class BillboardHPBar : MonoBehaviour {

    public GameObject healthProvider;
    public IHitpointValueProvider hpValueProvider;
    public float initialWidth; 

    // Use this for initialization
    void Start () {
        //Debug.Assert(healthProvider != null);
        hpValueProvider = healthProvider.GetComponent<IHitpointValueProvider>();
        initialWidth = this.transform.localScale.x;
	}
	
	// Update is called once per frame
	void Update () {
        Camera cam = Camera.main;
        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);

        Transform healthBar = this.transform.GetChild(0);
        //todo: HP percentage
        float percent = hpValueProvider.getHPCurr() / (float)hpValueProvider.getHPMax();
        percent = 0.5f; //test
        //todo: apply to bar graphics
        Vector3 scale = healthBar.localScale;
        scale.x = initialWidth * percent;
        healthBar.localScale = scale;
    }
}
