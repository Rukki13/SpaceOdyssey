using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviourPun
{
    public static GameManager I;
    public static UnityAction<GameState> onStateChanged;
    public static UnityAction onGameStart;
    public static UnityAction<bool> onGameEnd;
    public static bool isAgainstBot;

    public static bool ignorePlayerPrefs;

    public static int localScore;
    public static int enemyScore;

    [Header("UI")]
    [SerializeField] private GameObject lobbyCanvas;

    [Header("SFX")]
    [SerializeField] private AudioClip victorySFX;
    [SerializeField] private AudioClip defeatSFX;

    private MultiplayerGameParams gameParams;
    private Coroutine waitBeforeStartingBotGame = null; //The Coroutine used to wait before the bot "joins"

    public enum GameState
    {
        WaitingForInitialization,
        AlreadyPlayedBefore,
        ConnectingToServer,
        JoiningRoom,
        WaitingForOpponent,
        WaitingToStartGame,
        Playing,
        ConcludedResults, //Either won or lost
        NetworkDisconnection
    }
    public static GameState state { get; private set; } = GameState.WaitingForInitialization;

    private void Awake()
    {
        ignorePlayerPrefs = false;
        enemyScore = 0;
        localScore = 0;
        onStateChanged = null;
        I = this;
        state = GameState.WaitingForInitialization;
        lobbyCanvas.SetActive(true);
        onGameEnd = null;
        onGameStart = null;
    }

    private void OnEnable()
    {
        PhotonManager.onConnected.AddListener(Photon_OnConnected);
        PhotonManager.onDisconnected.AddListener(Photon_OnDisconnected);
        PhotonManager.onRoomChanged.AddListener(Photon_OnRoomChanged);
        PhotonManager.onPlayerListChanged.AddListener(Photon_OnPlayerListChanged);
    }

    private void OnDisable()
    {
        PhotonManager.onConnected.RemoveListener(Photon_OnConnected);
        PhotonManager.onDisconnected.RemoveListener(Photon_OnDisconnected);
        PhotonManager.onRoomChanged.RemoveListener(Photon_OnRoomChanged);
        PhotonManager.onPlayerListChanged.RemoveListener(Photon_OnPlayerListChanged);
        onStateChanged = null;
        onGameStart = null;
        isAgainstBot = false;
    }

    public void Initialize(MultiplayerGameParams gameParams)
    {
        if (state != GameState.WaitingForInitialization)
        {
            Debug.Log("Already initialized the game. You must restart to change the player");
            return;
        }

#if !UNITY_EDITOR
            if (!ignorePlayerPrefs && PlayerPrefs.GetInt(gameParams.matchId, -1) == 1)
            {
                SetState(GameState.AlreadyPlayedBefore);
                return;
            }
#endif

        Application.runInBackground = true;

        this.gameParams = gameParams;

        if (gameParams.player2Id.StartsWith("b99") || gameParams.player2Id.StartsWith("a99"))
        {
            isAgainstBot = true;
        }
        else
        {
            isAgainstBot = false;
        }

        PhotonManager.I.ConnectPhoton(gameParams.player1Id);
        SetState(GameState.ConnectingToServer);
    }

    private void Photon_OnConnected()
    {
        if (state == GameState.ConnectingToServer) //First time I connect
        {
            PhotonManager.I.JoinRoom(gameParams.matchId);
            SetState(GameState.JoiningRoom);
        }
    }

    private void SetState(GameState newState)
    {
        if (newState == GameState.Playing && state != GameState.Playing)
        {
            onGameStart?.Invoke();
        }

        state = newState;

        if (waitBeforeStartingBotGame != null &&
            state == GameState.NetworkDisconnection)//Check if the bot is about to join the game
        {
            StopCoroutine(waitBeforeStartingBotGame);
            waitBeforeStartingBotGame = null;
        }

        onStateChanged?.Invoke(state);
    }

    private void Photon_OnDisconnected(DisconnectCause cause)
    {
        if (state == GameState.ConcludedResults || state == GameState.NetworkDisconnection)
            return;
        if (cause == DisconnectCause.DisconnectByClientLogic)
        {
            SetState(GameState.NetworkDisconnection);
            return;
        }

        if (state == GameState.JoiningRoom ||
            state == GameState.ConnectingToServer ||
            state == GameState.WaitingForOpponent ||
            state == GameState.WaitingToStartGame) //Reconnect
        {
            PhotonManager.I.ConnectPhoton(gameParams.player1Id);
            SetState(GameState.ConnectingToServer);
            return;
        }

        if (state == GameState.Playing)
        {
            OnGameEnded(false, true);
            return;
        }
    }

    private void Photon_OnRoomChanged(bool isInRoom, string error)
    {
        if (isInRoom)
        {
            SetState(GameState.WaitingForOpponent);
            //wait for opponent
            if (isAgainstBot)
            {
                waitBeforeStartingBotGame = StartCoroutine(WaitBeforeStartingBotGame());
            }
        }
        else
        {
            //nothing
        }
    }

    private IEnumerator WaitBeforeStartingBotGame()
    {
        yield return new WaitForSeconds(Random.Range(1f, 4f));
        waitBeforeStartingBotGame = null;
        SetState(GameState.WaitingToStartGame);
    }

    public void Win(bool won)
    {
        if (isAgainstBot)
        {
            OnGameEnded(won, false);
        }
        else
        {
            photonView.RPC("SetWinner", RpcTarget.All, won ? PhotonManager.localPlayer.ActorNumber : PhotonManager.enemyPlayer.ActorNumber);
        }
    }

    [PunRPC]
    private void SetWinner(int actorNumber)
    {
        if (PhotonManager.localPlayer.ActorNumber == actorNumber)
        {
            OnGameEnded(true, false);
        }
        else
        {
            OnGameEnded(false, false);
        }
    }

    private void OnGameEnded(bool won, bool enemyAborted)
    {
        if (state == GameState.ConcludedResults)
            return;

        Debug.Log($"Game ended (won={won}, enemy aborted={enemyAborted})");
        SetState(GameState.ConcludedResults);
        lobbyCanvas.SetActive(true);
        onGameEnd?.Invoke(won);
        if (won)
        {
            //Send API call
            SoundManager.I.PlaySFX(victorySFX);
            ConcludeResults.I.SendMatchResults(enemyAborted, gameParams.player1Id, gameParams.player2Id, localScore, enemyScore, gameParams.returnURL, gameParams.token);
        }
        else
        {
            SoundManager.I.PlaySFX(defeatSFX);
            PhotonManager.I.Disconnect();
            string winnerId = won ? gameParams.player1Id : gameParams.player2Id;
            string loserId = won ? gameParams.player2Id : gameParams.player1Id;
            if (isAgainstBot)
                ConcludeResults.I.SendMatchResults(false, winnerId, loserId, localScore, enemyScore, gameParams.returnURL, gameParams.token);
        }
    }

    private void Photon_OnPlayerListChanged()
    {
        if (PhotonManager.enemyPlayer != null)
        {
            //Ready to start game
            SetState(GameState.WaitingToStartGame);
        }
        else //Enemy left
        {
            if (state == GameState.WaitingToStartGame) //Wait for opponent to reconnect
                SetState(GameState.WaitingForOpponent);
            if (state == GameState.Playing) //Win
                OnGameEnded(true, true);
        }
    }

    public void StartGame()
    {
        if (state != GameState.WaitingToStartGame)
        {
            Debug.Log($"Can't start the game at this state: {state}");
            return;
        }
        Debug.Log("Game started!");
        PlayerPrefs.SetInt(gameParams.matchId, 1);
        SetState(GameState.Playing);
    }

    private void OnDestroy()
    {
        I = null;
    }
}