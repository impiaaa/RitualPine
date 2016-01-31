using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Drawer : MonoBehaviour {
	public float distanceThreshold = 0.1f;
    public float circleThreshold = 2.0f;
    public Transform glowPrefab;

	Vector2 startingPoint;
	Vector2 endingPoint;
	List<Transform> objects;
	List<float> directions;

	// Use this for initialization
	void Start () {
		objects = new List<Transform> ();
		directions = new List<float>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown (0)) {
			startingPoint = point;
		}
		if (Input.GetMouseButton (0)) {
			float distance = 10000;
			if (objects.Count != 0) {
				distance = Vector2.Distance(point, objects[objects.Count - 1].position);
			}
			if (objects.Count == 0 || distance > distanceThreshold) {
				Transform newObj = (Transform)Instantiate(glowPrefab, point, Quaternion.identity);
				if (objects.Count >= 2) {
					Transform last = objects[objects.Count - 1];
					float angle = Mathf.Rad2Deg * Mathf.Atan2(point.y-last.position.y, point.x-last.position.x);
//					newObj.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
					directions.Add(angle);
				}
				objects.Add(newObj);
			}
		}
		if (Input.GetMouseButtonUp (0)) {
			endingPoint = point;
			char symbol = Recognize();
            SendMessageUpwards("SubmitStroke", symbol);
			foreach (Transform t in objects) {
				GameObject.Destroy(t.gameObject);
			}
			objects.Clear();
			directions.Clear();
		}
	}

	char Recognize() {
		// Split into 3 sections, minimizing std. dev. across them
		int minSplitFirst = -1, minSplitSecond = -1;
		float minStdDevSum = -1;
		float bestMeanFirst = 0, bestMeanSecond = 0, bestMeanThird = 0;
		float bestStddevFirst = 0, bestStddevSecond = 0, bestStddevThird = 0;
		for (int i = 1; i < directions.Count-1; i++) {
			for (int j = i+1; j < directions.Count; j++) {
				float meanFirst, stddevFirst, meanSecond, stddevSecond, meanThird, stddevThird;
				MeanAndStdDev(directions.GetRange(0, i), out meanFirst, out stddevFirst);
				MeanAndStdDev(directions.GetRange(i, j-i), out meanSecond, out stddevSecond);
				MeanAndStdDev(directions.GetRange(j, directions.Count-j), out meanThird, out stddevThird);
				float sumStddev = stddevFirst+stddevSecond+stddevThird;
				if (minStdDevSum == -1 || sumStddev < minStdDevSum) {
					minSplitFirst = i;
					minSplitSecond = j;
					minStdDevSum = sumStddev;
					bestMeanFirst = meanFirst;
					bestMeanSecond = meanSecond;
					bestMeanThird = meanThird;
					bestStddevFirst = stddevFirst;
					bestStddevSecond = stddevSecond;
					bestStddevThird = stddevThird;
				}
			}
		}
		if (minStdDevSum == -1) {
			return ' ';
		}
		int useBins = 3;
		if (minSplitSecond == directions.Count-1) {
			useBins--;
			minSplitSecond = directions.Count;
		}
		if (minSplitFirst == 1) {
			useBins--;
			minSplitFirst = minSplitSecond;
			minSplitSecond = directions.Count;
		}
		if (minSplitFirst == minSplitSecond-1) {
			useBins--;
			minSplitSecond = directions.Count;
		}
        if (useBins >= 1) MeanAndStdDev(directions.GetRange(0, minSplitFirst), out bestMeanFirst, out bestStddevFirst);
        if (useBins >= 2) MeanAndStdDev(directions.GetRange(minSplitFirst, minSplitSecond - minSplitFirst), out bestMeanSecond, out bestStddevSecond);
        if (useBins >= 3) MeanAndStdDev(directions.GetRange(minSplitSecond, directions.Count - minSplitSecond), out bestMeanThird, out bestStddevThird);
        //if (useBins >= 1) print("0.."+(minSplitFirst-1)+" ("+bestMeanFirst+" "+bestStddevFirst+")");
		//if (useBins >= 2) print(minSplitFirst+".."+(minSplitSecond-1)+" ("+bestMeanSecond+" "+bestStddevSecond+")");
		//if (useBins >= 3) print(minSplitSecond+".."+(directions.Count-1)+" ("+bestMeanThird+" "+bestStddevThird+")");

        if (useBins == 2 && Mathf.Abs(bestMeanFirst - 135) < 45 && Mathf.Abs(bestMeanSecond - 45) < 45)
        {
            return '<';
        }
        else if (useBins == 2 && Mathf.Abs(bestMeanFirst - -135) < 45 && Mathf.Abs(bestMeanSecond - -45) < 45)
        {
            return '<';
        }
        else if (useBins == 2 && Mathf.Abs(bestMeanFirst - 45) < 45 && Mathf.Abs(bestMeanSecond - 135) < 45)
        {
            return '>';
        }
        else if (useBins == 2 && Mathf.Abs(bestMeanFirst - -45) < 45 && Mathf.Abs(bestMeanSecond - -135) < 45)
        {
            return '>';
        }
        else if (useBins == 2 && Mathf.Abs(bestMeanFirst - -45) < 45 && Mathf.Abs(bestMeanSecond - 45) < 45)
        {
            return 'v';
        }
        else if (useBins == 2 && Mathf.Abs(bestMeanFirst - -135) < 45 && Mathf.Abs(bestMeanSecond - 135) < 45)
        {
            return 'v';
        }
        else if (useBins == 2 && Mathf.Abs(bestMeanFirst - 45) < 45 && Mathf.Abs(bestMeanSecond - -45) < 45)
        {
            return '^';
        }
        else if (useBins == 2 && Mathf.Abs(bestMeanFirst - 135) < 45 && Mathf.Abs(bestMeanSecond - -135) < 45)
        {
            return '^';
        }
        else if (Vector2.Distance(endingPoint, startingPoint) < circleThreshold && (useBins > 1 || bestStddevFirst > 50) && directions.Count > 4)
        {
            return 'O';
        }
        else if (useBins == 3 && Mathf.Abs(bestMeanFirst - 135) < 45 && Mathf.Abs(bestMeanThird - 45) < 45)
        {
            return '<';
        }
        else if (useBins == 3 && Mathf.Abs(bestMeanFirst - -135) < 45 && Mathf.Abs(bestMeanThird - -45) < 45)
        {
            return '<';
        }
        else if (useBins == 3 && Mathf.Abs(bestMeanFirst - 45) < 45 && Mathf.Abs(bestMeanThird - 135) < 45)
        {
            return '>';
        }
        else if (useBins == 3 && Mathf.Abs(bestMeanFirst - -45) < 45 && Mathf.Abs(bestMeanThird - -135) < 45)
        {
            return '>';
        }
        else if (useBins == 3 && Mathf.Abs(bestMeanFirst - -45) < 45 && Mathf.Abs(bestMeanThird - 45) < 45)
        {
            return 'v';
        }
        else if (useBins == 3 && Mathf.Abs(bestMeanFirst - -135) < 45 && Mathf.Abs(bestMeanThird - 135) < 45)
        {
            return 'v';
        }
        else if (useBins == 3 && Mathf.Abs(bestMeanFirst - 45) < 45 && Mathf.Abs(bestMeanThird - -45) < 45)
        {
            return '^';
        }
        else if (useBins == 3 && Mathf.Abs(bestMeanFirst - 135) < 45 && Mathf.Abs(bestMeanThird - -135) < 45)
        {
            return '^';
        }
        else if (useBins == 3 && Mathf.Abs(bestMeanFirst - bestMeanThird) < 45)
        {
            return 'Z';
        }
        else if (bestMeanFirst > 135 || bestMeanFirst < -135) {
			return '-';
		}
		else if (bestMeanFirst < 45 && bestMeanFirst > -45) {
			return '-';
		}
		else if (bestMeanFirst >= 45 && bestMeanFirst <= 135) {
			return '|';
		}
		else if (bestMeanFirst <= -45 && bestMeanFirst >= -135) {
			return '|';
		}
        return ' ';
	}

	// -- Utility functions --
	void MeanAndStdDev(IEnumerable<float> arr, out float mean, out float stddev) {
		mean = arr.Average();
		float sum = 0;
		foreach (float x in arr) {
			sum += Mathf.Pow(x - mean, 2);
		}
		stddev = Mathf.Sqrt(sum/arr.Count());
	}
}
