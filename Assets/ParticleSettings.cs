using UnityEngine;
using System.Collections;

public class ParticleSettings : MonoBehaviour {
	void Start () {
        foreach (ParticleSystem s in GameObject.FindObjectsOfType<ParticleSystem>())
        {
            var e = s.emission;
            print("e "+e);
            float m = PlayerPrefs.GetFloat("EmissionRateMultiple");
            print("m "+m);
            print("min "+e.rate.constantMin);
            print("max "+e.rate.constantMax);
            e.rate = new ParticleSystem.MinMaxCurve(m*e.rate.constantMax);
            print("min2 "+e.rate.constantMin);
            print("max2 "+e.rate.constantMax);
        }
    }
}
