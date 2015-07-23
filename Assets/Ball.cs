using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
	public int maxTouch = 2;
	[Range(0,31)]
	public int layerMask = 0;
	public float distance = 0.2f;
	public float dampingRatio = 1;
	public float frequency = 1.8f;
	public float linearDrag = 1.0f;
	public float angularDrag = 5.0f;
	public bool centerOfMass = false;
	private SpringJoint2D[] springJoints;
	
	void Start ()
	{
		springJoints = new SpringJoint2D[maxTouch+1];
		
		for (int i = 0; i < maxTouch; i++) {
			GameObject go = new GameObject ("Dragger" + (i + 1));
			go.transform.parent = this.transform;
			
			Rigidbody2D body = go.AddComponent <Rigidbody2D>() as Rigidbody2D;
			springJoints [i] = go.AddComponent <SpringJoint2D>() as SpringJoint2D;
			body.isKinematic = true;
		}
	}

	void onTouch ()
	{
		if (transform.position.y >= Screen.height / 2) {
			Vector3 newGravity = new Vector3 (0, Mathf.Abs (Physics.gravity.y), 0);
			Physics2D.gravity = newGravity;
		} else {
			Vector3 newGravity = new Vector3 (0, -Mathf.Abs (Physics.gravity.y), 0);
			Physics2D.gravity = newGravity;
		}
	}
	
	void Update ()
	{
		foreach (Touch touch in Input.touches) {
			int Id = touch.fingerId;
			
			if (Id < maxTouch && touch.phase == TouchPhase.Began) {
				Camera mainCamera = FindCamera ();
				Ray ray = mainCamera.ScreenPointToRay (touch.position);
				RaycastHit2D hit = Physics2D.GetRayIntersection (ray, Mathf.Infinity, 1 << layerMask);
				
				if (hit.rigidbody != null && hit.rigidbody.isKinematic == false) {
					springJoints [Id].transform.position = hit.point;
					springJoints [Id].connectedBody = hit.rigidbody;
					onTouch();
					if (centerOfMass)
						springJoints [Id].connectedAnchor = hit.rigidbody.centerOfMass;
					else
						springJoints [Id].connectedAnchor = hit.transform.InverseTransformPoint (hit.point);
					
					float length = (hit.transform.position - mainCamera.transform.position).magnitude;
					StartCoroutine (DragObject (Id, length));
				}
			}
		}
	}
	
	IEnumerator DragObject (int Id, float length)
	{
		float oldDrag = springJoints [Id].connectedBody.drag;
		float oldAngularDrag = springJoints [Id].connectedBody.angularDrag;
		springJoints [Id].distance = distance;
		springJoints [Id].dampingRatio = dampingRatio;
		springJoints [Id].frequency = frequency;
		springJoints [Id].connectedBody.drag = linearDrag;
		springJoints [Id].connectedBody.angularDrag = angularDrag;
		Camera mainCamera = FindCamera ();

			while (true) {
			bool touchExists = false;
			foreach (Touch touch in Input.touches) {
				if (touch.fingerId == Id) {
					touchExists = true;
					Ray ray = mainCamera.ScreenPointToRay (touch.position);
					springJoints [Id].transform.position = ray.GetPoint (length);
				}
			}
			if (touchExists)
				yield return null;
			else
				break;
		}
		
		if (springJoints [Id].connectedBody) {
			springJoints [Id].connectedBody.drag = oldDrag;
			springJoints [Id].connectedBody.angularDrag = oldAngularDrag;
			springJoints [Id].connectedBody = null;
		}  
	}
	
	Camera FindCamera ()
	{
		if (GetComponent<Camera>())
			return GetComponent<Camera>();
		else
			return Camera.main;
	}
}
