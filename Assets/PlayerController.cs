using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class PlayerController : MonoBehaviour
{
	List<char> spell;
	List<Transform> strokeObjects;
	public Transform strokeSpritePrefab;
	public float spriteSeparation;
	public Sprite[] strokeSprites;
	public bool SpawnStrokeSprites;
	Game game;

	// Use this for initialization
	void Start()
	{
		spell = new List<char>();
		strokeObjects = new List<Transform>();
		game = FindObjectOfType<Game>();
	}

	void OnConnectedToServer() {
		NetworkClient client = NetworkManager.singleton.client;
		client.RegisterHandler(ServerBehavior.TimerUpdate, OnClientTimerUpdateMessage);
		client.RegisterHandler(ServerBehavior.PhaseChange, OnClientPhaseChangeMessage);
		client.RegisterHandler(ServerBehavior.MakeMove, OnClientMakeMoveMessage);
		client.RegisterHandler(ServerBehavior.NodeChange, OnClientNodeChangeMessage);
	}

	void SubmitStroke(char stroke)
	{
		if (stroke != ' ')
		{
			spell.Add(stroke);
			if (game != null && game.Phase == (int)Game.PhaseNames.START_MOVE_PHASE)
			{
				game.Self.CastGlyph(new string(new char[] { stroke }));
			}
			if (SpawnStrokeSprites)
			{
				Sprite sprite;
				switch (stroke)
				{
					case '^':
						sprite = strokeSprites[0];
						break;
					case 'v':
						sprite = strokeSprites[1];
						break;
					case '<':
						sprite = strokeSprites[2];
						break;
					case '>':
						sprite = strokeSprites[3];
						break;
					case '|':
						sprite = strokeSprites[4];
						break;
					case '-':
						sprite = strokeSprites[5];
						break;
					case 'O':
						sprite = strokeSprites[6];
						break;
					case 'Z':
						sprite = strokeSprites[7];
						break;
					default:
						return;
				}
				Vector2 pos = new Vector2(spell.Count * spriteSeparation + strokeSpritePrefab.position.x, strokeSpritePrefab.position.y);
				Transform newObj = (Transform)Object.Instantiate(strokeSpritePrefab, pos, Quaternion.identity);
				newObj.GetComponent<SpriteRenderer>().sprite = sprite;
				strokeObjects.Add(newObj);
			}
		}
	}

	public void SubmitSpell()
	{
		foreach (Transform obj in strokeObjects)
		{
			GameObject.Destroy(obj.gameObject);
		}
		strokeObjects.Clear();
		MessageBase message = new StringMessage(new string(spell.ToArray()));
		spell.Clear();
		NetworkClient client = NetworkManager.singleton.client;
		client.Send(1002, message);
	}

	void OnClientTimerUpdateMessage(NetworkMessage netMsg)
	{
		var timerMessage = netMsg.ReadMessage<FloatMessage>();
		game.OnTimerUpdate(timerMessage.value);
	}

	void OnClientPhaseChangeMessage(NetworkMessage netMsg)
	{
		var phaseMessage = netMsg.ReadMessage<IntegerMessage>();
		game.OnPhaseChange(phaseMessage.value);
	}

	void OnClientMakeMoveMessage(NetworkMessage netMsg)
	{
		var moveMessage = netMsg.ReadMessage<StringMessage>();
		game.OnEnemyMove(moveMessage.value);
	}

	void OnClientNodeChangeMessage(NetworkMessage netMsg)
	{
		var nodeChangeMessage = netMsg.ReadMessage<NodeMessage>();
		game.OnNodeChange(nodeChangeMessage);
	}
}

public class FloatMessage : MessageBase {
	public float value;
	public FloatMessage() {
		value = 0.0f;
	}
	public FloatMessage(float x) {
		value = x;
	}
}
