using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Match;
using UnityEngine.UI;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;

public class ServerBehavior : NetworkManager
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
    public MainMenuUI menu;

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

    NetworkMatch networkMatch;
    NetworkDiscovery networkDiscovery;

    void Awake()
    {
        networkMatch = gameObject.AddComponent<NetworkMatch>();
        networkDiscovery = gameObject.AddComponent<OverriddenNetworkDiscovery>();
        networkDiscovery.showGUI = false;
        //networkDiscovery.useNetworkManager = false;
        networkDiscovery.Initialize();
    }

    public Text hostLabel;
    ulong netID;

    public void BeginHosting()
    {
        CreateMatchRequest create = new CreateMatchRequest();
        create.name = SystemInfo.deviceName;
        create.size = 2;
        create.advertise = true;
        create.password = "";

        networkMatch.CreateMatch(create, OnMatchCreate);
        hostLabel.text = "Creating room";
    }

    void OnMatchCreate(CreateMatchResponse matchResponse)
    {
        if (matchResponse.success)
        {
            Debug.Log("Create match succeeded");
            hostLabel.text = "Hosting as \"" + SystemInfo.deviceName + "\" at " + matchResponse.address;
            netID = (ulong)matchResponse.networkId;
            Utility.SetAccessTokenForNetwork((NetworkID)netID, new NetworkAccessToken(matchResponse.accessTokenString));
            NetworkServer.RegisterHandler(MsgType.Ready, (NetworkMessage netMsg) =>
            {
                print("Ready");
            });
            NetworkServer.RegisterHandler(MsgType.Connect, (NetworkMessage netMsg) =>
            {
                print("Connect");
            });
            NetworkServer.RegisterHandler(MsgType.AddPlayer, (NetworkMessage netMsg) =>
            {
                print("AddPlayer");
            });
            NetworkServer.RegisterHandler(MsgType.Error, (NetworkMessage netMsg) =>
            {
                print("Error");
            });
            /*
                SceneManager.LoadScene("main");
                GameObject.Find("Global").GetComponent<Game>().Networked = true;
                NetworkClient myClient = new NetworkClient();
                //myClient.RegisterHandler(MsgType.Connect, OnConnected);
                myClient.Connect(new MatchInfo(matchResponse));
            });*/
            NetworkServer.Listen(new MatchInfo(matchResponse), 9000);
        }
        else
        {
            Debug.LogError("Create match failed");
            hostLabel.text = "Failed to open room";
        }
    }

    public void StopHosting()
    {
        networkMatch.DestroyMatch((NetworkID)netID, null);
        hostLabel.text = "";
    }

    public int LOCAL_PORT = 0x5254554c;

    public void BeginListGames()
    {
        networkMatch.ListMatches(0, 20, "", OnMatchList);
        networkDiscovery.StartAsClient();
    }

    public override void OnMatchList(ListMatchResponse matchListResponse)
    {
        if (matchListResponse.success && matchListResponse.matches != null)
        {
            menu.OnMatchList(matchListResponse, networkMatch, OnSBMatchJoined);
        }
    }

    public void OnSBMatchJoined(JoinMatchResponse matchJoin)
    {
        if (matchJoin.success)
        {
            Debug.Log("Join match succeeded");
            Utility.SetAccessTokenForNetwork(matchJoin.networkId, new NetworkAccessToken(matchJoin.accessTokenString));
            SceneManager.LoadScene("main");
            GameObject.Find("Global").GetComponent<Game>().Networked = true;
            NetworkClient myClient = new NetworkClient();
            //myClient.RegisterHandler(MsgType.Connect, OnConnected);
            myClient.Connect(new MatchInfo(matchJoin));
        }
        else
        {
            Debug.LogError("Join match failed");
        }
    }

    public void StopListGames()
    {
        menu.StopListGames();
        networkDiscovery.StopBroadcast();
    }

    public class OverriddenNetworkDiscovery : NetworkDiscovery
    {
        public override void OnReceivedBroadcast(string fromAddress, string data)
        {
            GetComponent<ServerBehavior>().menu.OnReceivedBroadcast(fromAddress, data);
        }
    }

    public Text hostLabelLocal;
    public void BeginHostLocal()
    {
        //networkDiscovery.broadcastData = SystemInfo.deviceName;
        networkDiscovery.StartAsServer();
        hostLabelLocal.text = "Broadcasting as " + networkDiscovery.broadcastData;
    }

    public void StopHostLocal()
    {
        networkDiscovery.StopBroadcast();
    }

    // called when a client connects 
    public override void OnServerConnect(NetworkConnection conn)
    {
        print("OnServerConnect");
    }

    // called when a client disconnects
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        print("OnServerDisconnect");
        NetworkServer.DestroyPlayersForConnection(conn);
    }

    // called when a client is ready
    public override void OnServerReady(NetworkConnection conn)
    {
        print("OnServerReady");
        NetworkServer.SetClientReady(conn);
    }

    // called when a network error occurs
    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        print("OnServerError");
    }
}
