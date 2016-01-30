using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Drawer : MonoBehaviour {
	public float distanceThreshold = 0.1f;
    public Transform glowPrefab;
	Vector2 startingPoint;
	Vector2 endingPoint;
	List<Transform> objects;

	// Use this for initialization
	void Start () {
		objects = new List<Transform> ();
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown (0)) {
			startingPoint = point;
		}
		if (Input.GetMouseButtonUp (0)) {
			endingPoint = point;
		}
		if (Input.GetMouseButton (0)) {
            float distance = 10000;
            if (objects.Count != 0)
            {
                distance = Vector2.Distance(point, objects[objects.Count - 1].position);
                Debug.Log(distance);
            }
            if (objects.Count == 0 || distance > distanceThreshold) {
                objects.Add((Transform)Instantiate(glowPrefab, point, Quaternion.identity));
			}
		}
	}
}
