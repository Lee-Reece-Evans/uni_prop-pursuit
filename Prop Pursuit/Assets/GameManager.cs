using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Photon.MonoBehaviour
{
    public GameObject Hunter;
    public GameObject player;
    public Transform[] spawnPoints;
    private List<int> CurrentPlayerID = new List<int>();

    public static GameManager Instance { get; private set; }

    [SerializeField]
    private int TotalPlayers;
    public bool TimeUp = false;
    public bool GameOver = false;
    public bool IsHunter = false;

    public float SpawnTimerForHunter = 30f;
    public float RoundTime = 180f;
    public bool GameStarted;
    public int RandomHunterSelect;

    public bool acceptInputs = true;
    
    public GameObject HunterCanvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        if (!GameOver)
        {
            if (PhotonNetwork.isMasterClient && otherPlayer.ID == RandomHunterSelect)
            {
                // props win because hunter left the game
                photonView.RPC("SetGameOver", PhotonTargets.All, 2);
            }
            else // if already died then don't decrement
            {
                PlayerDied(otherPlayer.ID);
            }         
        }
    }

    void Start()
    {
        photonView.RPC("IncrementTotalPlayers", PhotonTargets.All, PhotonNetwork.player.ID);
    }

    [PunRPC]
    void AllPlayersSpawn()
    {
        CurrentPlayerID.Sort(); // make all players lists the same order
        if (RandomHunterSelect == PhotonNetwork.player.ID)
        {
            PlayerDataManager.Instance.IncreaseGamesPlayed(false);
            PlayerDataManager.Instance.IncreaseGamesLost(false); // start off with a loss if player leaves before game over
            PlayerDataManager.Instance.RequestSave();
            PhotonNetwork.Instantiate(Hunter.name, spawnPoints[5].position, Quaternion.identity, 0); // this is for the hunter spawn
            IsHunter = true;
        }
        else
        {
            PlayerDataManager.Instance.IncreaseGamesPlayed(true);
            PlayerDataManager.Instance.IncreaseGamesLost(true);
            PlayerDataManager.Instance.RequestSave();
            PhotonNetwork.Instantiate(player.name, spawnPoints[CurrentPlayerID.IndexOf(PhotonNetwork.player.ID)].position, Quaternion.identity, 0);
        }
    }

    void Update()
    {
        if (GameStarted && !GameOver)
        {
            if (PhotonNetwork.isMasterClient)
            {
                CheckGameState();
            }
            StartTimers();
        }
    }

    void CheckGameState()
    {
        // if hunter left game = props win, gameover
        if (TotalPlayers == 1 || TimeUp)
        {
            // Hunter wins
            // if total players = 1 and is the hunter player. Hunter wins. if TimeUp && TotalPlayers == 1 && HunterID

            if (TotalPlayers == 1 && CurrentPlayerID.Contains(RandomHunterSelect))
            {
                GameOver = true;
                photonView.RPC("SetGameOver", PhotonTargets.All, 1); // if client hosts spawns players
            }

            if (TimeUp && TotalPlayers > 1)     //  ||      if hunter disconnects. Escapers win automatically 
            {
              GameOver = true;
              photonView.RPC("SetGameOver", PhotonTargets.All, 2);
            }
        }
    }

    void StartTimers()
    {
        if (SpawnTimerForHunter > 0)
        {
            SpawnTimerForHunter -= Time.deltaTime;
            Ingame_UI_Manager.instance.SetHunterTimerText(SpawnTimerForHunter);
            if (PhotonNetwork.player.ID == RandomHunterSelect)
            {
                HunterCanvas.SetActive(true);
            }
        }

        if (SpawnTimerForHunter <= 0 && RoundTime > 0)
        {
            RoundTime -= Time.deltaTime;
            Ingame_UI_Manager.instance.SetSurvivalTimerText(RoundTime);

            if (PhotonNetwork.player.ID == RandomHunterSelect)
            {
                HunterCanvas.SetActive(false);
            }


            if (RoundTime <= 0)
            {
                RoundTime = 0;
                Ingame_UI_Manager.instance.SetSurvivalTimerText(0f);
                TimeUp = true;
            }
        }
    }

    [PunRPC]
    void HunterSelection(int RandomHunter)
    {
        RandomHunterSelect = RandomHunter;
    }

    [PunRPC]
    void IncrementTotalPlayers(int playerID)
    {
        TotalPlayers++;
        CurrentPlayerID.Add(playerID);

        if (PhotonNetwork.isMasterClient && TotalPlayers == PhotonNetwork.playerList.Length)
        {
            //pick random to be hunter
            Random.InitState((int)System.DateTime.Now.Ticks);
            RandomHunterSelect = CurrentPlayerID[Random.Range(0, CurrentPlayerID.Count)];

            photonView.RPC("HunterSelection", PhotonTargets.All, RandomHunterSelect);
            photonView.RPC("AllPlayersSpawn", PhotonTargets.All);
            photonView.RPC("AllPlayersGameStarted", PhotonTargets.All);
        }
        Ingame_UI_Manager.instance.SetPlayersAliveText(TotalPlayers);
    }

    public void PlayerDied(int ID)
    {
        photonView.RPC("DecrementTotalPlayers", PhotonTargets.All, ID);
    }

    [PunRPC]
    public void DecrementTotalPlayers(int ID)
    {
        if (PhotonNetwork.isMasterClient && ID == RandomHunterSelect && !GameOver) // hunter has not already left check !gameover
        {
            // props win because hunter died
            photonView.RPC("SetGameOver", PhotonTargets.All, 2);
        }
        if (CurrentPlayerID.Contains(ID))
        {
            TotalPlayers--;
            CurrentPlayerID.RemoveAll(x => x == ID);
            Ingame_UI_Manager.instance.SetPlayersAliveText(TotalPlayers);
            Ingame_Tab_Manager.instance.PlayerDied(ID);
        }
    }

    [PunRPC]
    void AllPlayersGameStarted(PhotonMessageInfo info)
    {
        // keep all timers in sync by removing lag from the RPC call.
        float LagTime = (float)(PhotonNetwork.time - info.timestamp);
        SpawnTimerForHunter -= LagTime;
        GameStarted = true;
    }

    [PunRPC]
    void SetGameOver(int GameOverCondition)
    {
        GameOver = true;
        if (GameOverCondition == 1)
        {
            if (PhotonNetwork.player.ID != RandomHunterSelect)
            {
                Ingame_UI_Manager.instance.SetGameOverText("Escapers Lose");
            }
            else
            {
                PlayerDataManager.Instance.IncreaseGamesWon(false);
                PlayerDataManager.Instance.DecreaseGamesLost(false);
                PlayerDataManager.Instance.RequestSave();
                Ingame_UI_Manager.instance.SetGameOverText("Hunter Wins");
            }
        }

        if (GameOverCondition == 2)
        {
            if (PhotonNetwork.player.ID != RandomHunterSelect)
            {
                PlayerDataManager.Instance.IncreaseGamesWon(true);
                PlayerDataManager.Instance.DecreaseGamesLost(true);
                PlayerDataManager.Instance.RequestSave();
                Ingame_UI_Manager.instance.SetGameOverText("Escapers Win");
            }
            else
            {
                Ingame_UI_Manager.instance.SetGameOverText("Hunter Lose");
            }
        }
    }
}






