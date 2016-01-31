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
    ServerBehavior net;

    void Start()
    {
        net = GameObject.FindGameObjectWithTag("GameController").GetComponent<ServerBehavior>();
        net.menu = this;
    }

    public void HostGameButtonPush()
    {
        if (hostGameUI.activeSelf)
        {
            hostGameUI.SetActive(false);

            net.StopHosting();
        }
        else
        {
            hostGameUI.SetActive(true);
            joinGameUI.SetActive(false);
            hostLocalUI.SetActive(false);
            creditsUI.SetActive(false);

            net.BeginHosting();
        }
    }
    public void JoinGameButtonPush()
    {
        if (joinGameUI.activeSelf)
        {
            joinGameUI.SetActive(false);

            net.StopListGames();
        }
        else
        {
            hostGameUI.SetActive(false);
            joinGameUI.SetActive(true);
            hostLocalUI.SetActive(false);
            creditsUI.SetActive(false);

            BeginListGames();
            net.BeginListGames();
        }
    }
    public void HostLocalButtonPush()
    {
        if (hostLocalUI.activeSelf)
        {
            hostLocalUI.SetActive(false);

            net.StopHostLocal();
        }
        else
        {
            hostGameUI.SetActive(false);
            joinGameUI.SetActive(false);
            hostLocalUI.SetActive(true);
            creditsUI.SetActive(false);

            net.BeginHostLocal();
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

    public GameObject joinButtonPrefab;
    List<GameObject> joinGameButtons = new List<GameObject>();
    int joinListY;

    void BeginListGames()
    {
        joinListY = 60;
    }

    public void OnMatchList(ListMatchResponse matchListResponse, NetworkMatch networkMatch, NetworkMatch.ResponseDelegate<JoinMatchResponse> OnMatchJoined)
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

    public void StopListGames()
    {
        foreach (GameObject but in joinGameButtons)
        {
            GameObject.Destroy(but);
        }
        joinGameButtons.Clear();
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
            myClient.Connect(fromAddress, net.LOCAL_PORT);
        });
        joinListY -= 50;
        joinGameButtons.Add(newButton);
    }

    public void DoSinglePlayer()
    {
        SceneManager.LoadScene("main");
    }
}
