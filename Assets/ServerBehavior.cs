using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ServerBehavior : NetworkBehaviour {
    const short MyBeginMsg = 1002;

    public void Start()
    {
        NetworkServer.RegisterHandler(MyBeginMsg, OnServerReadyToBeginMessage);
        print("registered");
    }

    void OnServerReadyToBeginMessage(NetworkMessage netMsg)
    {
        var beginMessage = netMsg.ReadMessage<StringMessage>();
        Debug.Log("received OnServerReadyToBeginMessage " + beginMessage.value);
    }
}
