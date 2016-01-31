using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ServerBehavior : NetworkBehaviour {
    const short PhaseChange = 0x4653;
    const short MakeMove = 0x4D56;

    private class MoveMessage : MessageBase
    {
        public Card card;
    }

    public void Start()
    {
        NetworkServer.RegisterHandler(MakeMove, OnServerMakeMoveMessage);
    }

    void OnServerMakeMoveMessage(NetworkMessage netMsg)
    {
        var moveMessage = netMsg.ReadMessage<MoveMessage>();
        foreach (var conn in NetworkServer.connections)
        {
            if (conn != netMsg.conn)
            {
                conn.Send(MakeMove, moveMessage);
            }
        }
    }
}
