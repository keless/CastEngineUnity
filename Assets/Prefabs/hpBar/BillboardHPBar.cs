using UnityEngine;
using System.Collections;


public class BillboardHPBar : CommonMonoBehaviour {

    public GameObject healthProvider;
    public IHitpointValueProvider hpValueProvider;
    public float initialWidth; 

    // Use this for initialization
    void Start () {
        initialWidth = this.transform.localScale.x;
	}
	
    void OnEnable()
    {
        if(hpValueProvider == null)
        {
            hpValueProvider = healthProvider.GetComponent<IHitpointValueProvider>();
        }
    }

	// Update is called once per frame
	void Update () {
        Camera cam = Camera.main;
        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);

        Transform healthBar = this.transform.GetChild(0);

        float percent = hpValueProvider.getHPCurr() / (float)hpValueProvider.getHPMax();

        Vector3 scale = healthBar.localScale;
        scale.x = initialWidth * percent;
        healthBar.localScale = scale;
    }
}
