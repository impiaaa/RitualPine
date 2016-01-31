using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

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
        NetworkClient client = NetworkManager.singleton.client;
        MessageBase message = new StringMessage(new string(spell.ToArray()));
        client.Send(1002, message);
        print("sent message "+message+" to "+client);
        spell.Clear();
    }
}
