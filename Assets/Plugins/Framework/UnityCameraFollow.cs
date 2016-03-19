using UnityEngine;
using System.Collections;

/// <summary>
/// Place this script on a Camera
///     it will try to find an object with tag "Player" and cause the camera to follow it
/// </summary>
public class UnityCameraFollow : MonoBehaviour 
{
	public float xMargin = 1f;		// Distance in the x axis the player can move before the camera follows.
	public float zMargin = 1f;		// Distance in the z axis the player can move before the camera follows.
	public float xSmooth = 8f;		// How smoothly the camera catches up with it's target movement in the x axis.
	public float zSmooth = 8f;		// How smoothly the camera catches up with it's target movement in the z axis.

    public bool useMinMaxBounds = true;
	public Vector2 maxXAndZ;		// The maximum x and z coordinates the camera can have.
	public Vector2 minXAndZ;		// The minimum x and z coordinates the camera can have.

    public string TargetTagName = "Player";
	private Transform target = null;		// Reference to the player's transform.


	void Awake ()
	{
		// Setting up the reference.
	}


	bool CheckXMargin()
	{
		// Returns true if the distance between the camera and the player in the x axis is greater than the x margin.
		return Mathf.Abs(transform.position.x - target.position.x) > xMargin;
	}


	bool CheckYMargin()
	{
		// Returns true if the distance between the camera and the player in the y axis is greater than the y margin.
		return Mathf.Abs(transform.position.z - target.position.z) > zMargin;
	}


	void FixedUpdate ()
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

        if(TargetTagName != null)
        {
            TrackPlayer();
        }
	}
	
	
	void TrackPlayer ()
	{
		// By default the target x and y coordinates of the camera are it's current x and y coordinates.
		float targetX = transform.position.x;
		float targetZ = transform.position.z;

		// If the player has moved beyond the x margin...
		if(CheckXMargin())
			// ... the target x coordinate should be a Lerp between the camera's current x position and the player's current x position.
			targetX = Mathf.Lerp(transform.position.x, target.position.x, Time.fixedDeltaTime);

		// If the player has moved beyond the y margin...
		if(CheckYMargin())
            // ... the target y coordinate should be a Lerp between the camera's current y position and the player's current y position.
            targetZ = Mathf.Lerp(transform.position.z, target.position.z, Time.fixedDeltaTime);

        if(useMinMaxBounds)
        {
            // The target x and y coordinates should not be larger than the maximum or smaller than the minimum.
            targetX = Mathf.Clamp(targetX, minXAndZ.x, maxXAndZ.x);
            targetZ = Mathf.Clamp(targetZ, minXAndZ.y, maxXAndZ.y);
        }

		// Set the camera's position to the target position with the same z component.
		transform.position = new Vector3(targetX, transform.position.y, targetZ);
	}
}
