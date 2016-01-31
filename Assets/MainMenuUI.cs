using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking.Match;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using System.Collections.Generic;

public class MainMenuUI : MonoBehaviour {
    public GameObject mainMenuUI;
    public GameObject hostGameUI;
    public GameObject joinGameUI;
    public GameObject hostLocalUI;
    public GameObject creditsUI;
    NetworkMatch networkMatch;
    NetworkDiscovery networkDiscovery;

    void Awake()
    {
        networkMatch = gameObject.AddComponent<NetworkMatch>();
        networkDiscovery = gameObject.AddComponent<OverriddenNetworkDiscovery>();
        networkDiscovery.showGUI = false;
        networkDiscovery.useNetworkManager = false;
        networkDiscovery.Initialize();
    }
    public void HostGameButtonPush()
    {
        if (hostGameUI.activeSelf)
        {
            hostGameUI.SetActive(false);

            StopHosting();
        }
        else
        {
            hostGameUI.SetActive(true);
            joinGameUI.SetActive(false);
            hostLocalUI.SetActive(false);
            creditsUI.SetActive(false);

            BeginHosting();
        }
    }
    public void JoinGameButtonPush()
    {
        if (joinGameUI.activeSelf)
        {
            joinGameUI.SetActive(false);

            StopListGames();
        }
        else
        {
            hostGameUI.SetActive(false);
            joinGameUI.SetActive(true);
            hostLocalUI.SetActive(false);
            creditsUI.SetActive(false);

            BeginListGames();
        }
    }
    public void HostLocalButtonPush()
    {
        if (hostLocalUI.activeSelf)
        {
            hostLocalUI.SetActive(false);

            StopHostLocal();
        }
        else
        {
            hostGameUI.SetActive(false);
            joinGameUI.SetActive(false);
            hostLocalUI.SetActive(true);
            creditsUI.SetActive(false);

            BeginHostLocal();
        }
    }
    public void CreditsButtonPush()
    {
        if (creditsUI.activeSelf)
        {
            creditsUI.SetActive(false);
        }
        else
        {
            hostGameUI.SetActive(false);
            joinGameUI.SetActive(false);
            hostLocalUI.SetActive(false);
            creditsUI.SetActive(true);
        }
    }

    public Text hostLabel;
    ulong netID;

    void BeginHosting()
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
            NetworkServer.Listen(new MatchInfo(matchResponse), 9000);
            NetworkServer.RegisterHandler(MsgType.Connect, (NetworkMessage netMsg) => {
                print("OnConnected");
                SceneManager.LoadScene("main");
                GameObject.Find("Global").GetComponent<Game>().Networked = true;
                NetworkClient myClient = new NetworkClient();
                //myClient.RegisterHandler(MsgType.Connect, OnConnected);
                myClient.Connect(new MatchInfo(matchResponse));
            });
        }
        else
        {
            Debug.LogError("Create match failed");
            hostLabel.text = "Failed to open room";
        }
    }

    void StopHosting()
    {
        networkMatch.DestroyMatch((NetworkID)netID, null);
        hostLabel.text = "";
    }

    public GameObject joinButtonPrefab;
    List<GameObject> joinGameButtons = new List<GameObject>();
    int joinListY;
    int LOCAL_PORT = 0x5254554c;

    void BeginListGames()
    {
        networkMatch.ListMatches(0, 20, "", OnMatchList);
        networkDiscovery.StartAsClient();
        joinListY = 60;
    }

    public void OnMatchList(ListMatchResponse matchListResponse)
    {
        if (matchListResponse.success && matchListResponse.matches != null)
        {
            foreach (MatchDesc md in matchListResponse.matches)
            {
                GameObject newButton = GameObject.Instantiate(joinButtonPrefab);
                newButton.transform.parent = joinGameUI.transform;
                newButton.transform.localPosition = new Vector2(newButton.transform.position.x, joinListY);
                newButton.GetComponentInChildren<Text>().text = md.name;
                newButton.GetComponent<Button>().onClick.AddListener(() => {
                    networkMatch.JoinMatch(md.networkId, "", OnMatchJoined);
                });
                joinListY -= 20;
                joinGameButtons.Add(newButton);
            }
        }
    }

    public void OnMatchJoined(JoinMatchResponse matchJoin)
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

    void StopListGames()
    {
        foreach (GameObject but in joinGameButtons)
        {
            GameObject.Destroy(but);
        }
        joinGameButtons.Clear();
        networkDiscovery.StopBroadcast();
    }

    public class OverriddenNetworkDiscovery : NetworkDiscovery
    {
        public override void OnReceivedBroadcast(string fromAddress, string data)
        {
            GetComponent<MainMenuUI>().OnReceivedBroadcast(fromAddress, data);
        }
    }

    public void OnReceivedBroadcast(string fromAddress, string data)
    {
        foreach (GameObject but in joinGameButtons)
        {
            if (but.name == fromAddress)
            {
                return;
            }
        }
        GameObject newButton = GameObject.Instantiate(joinButtonPrefab);
        newButton.name = fromAddress;
        newButton.transform.parent = joinGameUI.transform;
        newButton.transform.localPosition = new Vector2(newButton.transform.position.x, joinListY);
        newButton.GetComponentInChildren<Text>().text = data;
        newButton.GetComponent<Button>().onClick.AddListener(() => {
            SceneManager.LoadScene("main");
            GameObject.Find("Global").GetComponent<Game>().Networked = true;
            NetworkClient myClient = new NetworkClient();
            myClient.Connect(fromAddress, LOCAL_PORT);
        });
        joinListY -= 50;
        joinGameButtons.Add(newButton);
    }

    public Text hostLabelLocal;
    void BeginHostLocal()
    {
        networkDiscovery.broadcastData = SystemInfo.deviceName;
        networkDiscovery.StartAsServer();
        hostLabelLocal.text = "Broadcasting as " + networkDiscovery.broadcastData;
    }

    void StopHostLocal()
    {
        networkDiscovery.StopBroadcast();
    }
}
