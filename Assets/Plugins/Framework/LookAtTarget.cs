using UnityEngine;
using System.Collections;

public class LookAtTarget : MonoBehaviour {

    public string TargetTagName = "Player";
    public Transform target = null;        // Reference to the target's transform.

    public Vector3 offset = new Vector3(0, 1, 0);

    // Use this for initialization
    void Start () {
	    
	}
	
	void LateUpdate () {
        if (target == null && TargetTagName != null)
        {
            GameObject go = GameObject.FindGameObjectWithTag(TargetTagName);
            if (go)
            {
                target = go.transform;
            }
            else
            {
                Debug.Log("couldnt find target");
            }
        }

        if(target)
        {
            transform.LookAt(target.position + offset);
        }
    }
}
