using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ServerBehavior : MonoBehaviour
{
	public const short TimerUpdate = 0x544D;
	public const short PhaseChange = 0x4653;
	public const short MakeMove = 0x4D56;
	public const short NodeChange = 0x4E44;

	public float[] PhaseDurations = {4.0f, 4.0f, 4.0f, 4.0f};

	int Phase;

	float lastPhaseStartTime;
	float lastTick;
	HashSet<int> clientsThatSentMoves;
    bool gameActive;

    void Start()
    {
        NetworkServer.RegisterHandler(MakeMove, OnServerMakeMoveMessage);
        clientsThatSentMoves = new HashSet<int>();
    }

    public void StartGame()
	{
		lastPhaseStartTime = Time.time;
		lastTick = Time.time;
        gameActive = true;
        clientsThatSentMoves.Clear();
    }

    public void StopGame()
    {
        gameActive = false;
    }

	public void Update()
	{
        if (gameActive)
        {
            if (Time.time - lastTick > 1)
            {
                NetworkServer.SendToAll(TimerUpdate, new FloatMessage(Time.time - lastPhaseStartTime));
                lastTick = Time.time;
            }
            if (Time.time - lastPhaseStartTime >= PhaseDurations[Phase])
            {
                clientsThatSentMoves.Clear();
                Phase = (Phase + 1) % PhaseDurations.Length;
                NetworkServer.SendToAll(PhaseChange, new IntegerMessage(Phase));
                lastPhaseStartTime = Time.time;
                clientsThatSentMoves.Clear();
            }
        }
	}

	void OnServerMakeMoveMessage(NetworkMessage netMsg)
	{
		if (gameActive && !clientsThatSentMoves.Contains(netMsg.conn.connectionId)) {
			var moveMessage = netMsg.ReadMessage<StringMessage>();
			foreach (var conn in NetworkServer.connections)
			{
				if (conn != netMsg.conn)
				{
					conn.Send(MakeMove, moveMessage);
				}
			}
			clientsThatSentMoves.Add(netMsg.conn.connectionId);
		}
	}
}
