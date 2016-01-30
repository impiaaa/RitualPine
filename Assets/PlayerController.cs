using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
    List<char> spell;

	// Use this for initialization
	void Start () {
        spell = new List<char>();
    }
	
    void SubmitStroke(char stroke)
    {
        if (stroke != ' ')
        {
            spell.Add(stroke);
        }
    }

    public void SubmitSpell()
    {
        print(new string(spell.ToArray()));
        spell.Clear();
    }
}
