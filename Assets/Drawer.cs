using UnityEngine;
using System.Collections;

public class Drawer : MonoBehaviour {
	public float distanceThreshold = 10.0;
	Vector2 startingPoint;
	Vector2 endingPoint;
	ArrayList<Transform> objects;

	// Use this for initialization
	void Start () {
		objects = new ArrayList<Transform> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			startingPoint = Input.mousePosition;
		}
		if (Input.GetMouseButtonUp (0)) {
			endingPoint = Input.mousePosition;
		}
		if (Input.GetMouseButton (0)) {
			if (Vector2.distance (Input.mousePosition, objects [-1].position) > distanceThreshold) {
			}
		}
	}
}
