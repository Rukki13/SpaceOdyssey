using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEntryPoint : MonoBehaviour
{
    public static GameEntryPoint I;

    [Header("Development")]
    [SerializeField, Tooltip("Game parameters used for testing in the Unity Editor.")]
    private MultiplayerGameParams developmentParams;

    private void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
#if UNITY_EDITOR
        //InitializeGameFromMobile(-1, JsonUtility.ToJson(developmentParams));
#endif
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            CallMobileFunction.SendJson(string.Concat('{', "\"event\" : \"back_pressed\"", '}'));
        }
    }

    public void InitializeGameFromMobile(string jsonParams)
    {
        StartCoroutine(InitializeGameFromMobileCoroutine(jsonParams));
    }

    private IEnumerator InitializeGameFromMobileCoroutine(string jsonParams)
    {
        MultiplayerGameParams gameParams = JsonUtility.FromJson<MultiplayerGameParams>(jsonParams);
        if (gameParams == null)
        {
            Debug.Log("Failed to parse JSON parameters.");
            yield break;
        }
        Debug.Log($"Received Game Params JSON: {jsonParams}");
        if(gameParams.gameId != string.Empty)
        {
            yield return SceneManager.LoadSceneAsync(gameParams.gameId);
            while (GameManager.I == null)
            {
                yield return null;
            }
        }
        Debug.Log($"Parsed Parameters: {gameParams}");
        GameManager.I.Initialize(gameParams);
    }

    public void StartAsPlayer1()
    {
        InitializeGameFromMobile(JsonUtility.ToJson(developmentParams));
    }

    public void StartAsPlayer2()
    {
        string player2Id = developmentParams.player2Id;
        developmentParams.player2Id = developmentParams.player1Id;
        developmentParams.player1Id = player2Id;

        InitializeGameFromMobile(JsonUtility.ToJson(developmentParams));
    }

    public void StartAgainstBot()
    {
        developmentParams.player2Id = string.Concat("a99", developmentParams.player2Id);
        InitializeGameFromMobile(JsonUtility.ToJson(developmentParams));
    }

    private void OnDestroy()
    {
        I = null;
    }
}
