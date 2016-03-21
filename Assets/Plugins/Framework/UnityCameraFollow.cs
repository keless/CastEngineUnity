using UnityEngine;
using System.Collections;

/// <summary>
/// Place this script on a Camera
///     it will try to find an object with tag "Player" and cause the camera to follow it
/// </summary>
public class UnityCameraFollow : MonoBehaviour 
{
    public string TargetTagName = "Player";
	public Transform target = null;		// Reference to the player's transform.

    public float damping = 1;

    void Awake ()
	{
		// Setting up the reference.
	}

	void LateUpdate ()
	{
        if(target == null && TargetTagName != null)
        {
            GameObject go = GameObject.FindGameObjectWithTag(TargetTagName);
            if(go)
            {
                target = go.transform;
            }
            else
            {
                Debug.Log("couldnt find target");
            }
        }

        if(target != null)
        {
            TrackPlayer();
        }
	}

	void TrackPlayer ()
	{
        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * damping);
    }
}
