using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

public class VertexSelction : MonoBehaviour {

	public GameObject indexFinger, rightHand, fireCone, fragment01, fragment02;
	public float radius;

	private Fragment frag1, frag2;
	private Transform tr;
	private Vector3 point, rayDirection;

	private bool allowSelection = true;

	void Start () {
		frag1 = fragment01.GetComponent<Fragment> ();
		frag2 = fragment02.GetComponent<Fragment> ();
	}

	void Update () {
		tr = indexFinger.transform;

		// Get location of the tip of the index finger and the direction vector it is pointing in.
		point = indexFinger.GetComponent<Transform> ().position;
		rayDirection = tr.TransformDirection (rightHand.gameObject.GetComponent<FingerDirectionDetector> ().PointingDirection);
		rayDirection.Normalize ();
		
		float angle = 0.0f;

		// Check if the right hand is pointing
		if (rightHand.gameObject.GetComponent<FingerDirectionDetector> ().isActiveAndEnabled) {
			// Fire multiple rays each frame. Although this is inefficent it allows quick selection of many triangles 
			// (speeds up selection proccess in scanned data).
			for (int z = 0; z < 20; z++) {

				// Allow any angle in 2pi radians
				angle = Random.Range (0f, 2f * Mathf.PI);

				// Set the ray direction based on the geometry of the fireCone to create a cone of rays
				Vector3 fire = -fireCone.transform.forward +
				               (fireCone.transform.right * Mathf.Sin (angle)) * radius + (fireCone.transform.up * Mathf.Cos (angle)) * radius;

				// Fire a raycast and return all hits on contact with fragment collder.
				RaycastHit[] hits = Physics.RaycastAll (point, fire, 1f);
				foreach (RaycastHit h in hits) {
					// Check if a ray hit a fragment collider and if vertex selection is active
					if (h.collider.gameObject.name == fragment01.name && allowSelection) 
						frag1.SelectVertcies (h, tr);
					else if(h.collider.gameObject.name == fragment02.name && allowSelection)
						frag2.SelectVertcies (h, tr);
				}
			}
		}
	}

	public void AllowSelection(){
		allowSelection = !allowSelection;
	}
}
