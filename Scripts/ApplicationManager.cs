using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class GameState  //遊戲狀態 (電力 鍵盤鎖)
{
    public string Key = null;
    public string Value = null;
}

public class ApplicationManager : MonoBehaviour
{
    [SerializeField]
    private List<GameState> _startingGameStates = new List<GameState>();  //儲存 KEY 對應遊戲狀態 VALUE 對應開啟關閉

    private static ApplicationManager _Instance;
    private Dictionary<string, string> _gameStateDictionary = new Dictionary<string, string>();  //儲存遊戲清單的狀態


    public static ApplicationManager instance
    {
        get
        {
            if(_Instance == null)  //如果沒有
            {
                _Instance = (ApplicationManager)FindObjectOfType(typeof(ApplicationManager));  //搜尋場景上的類型
            }
            return _Instance;  //回傳
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);  //轉換場景時保存
        ResetGameStates();
        
    }

    void ResetGameStates()
    {
        _gameStateDictionary.Clear();  //清除字典裡的狀態

        for (int i = 0; i < _startingGameStates.Count; i++)  //搜尋所有遊戲狀態
        {
            GameState gs = _startingGameStates[i];  //取得KEY
            _gameStateDictionary[gs.Key] = gs.Value;  //把值存進字典
        }
    }

    public string GetGameState(string key)  //取得遊戲狀態
    {
        string result = null;
        _gameStateDictionary.TryGetValue(key, out result);  //嘗試取得
        return result;
    }

    public bool SetGameState(string key, string value)  //設置遊戲狀態
    {
        if(key == null || value == null)
        {
            return false;
        }
        _gameStateDictionary[key] = value;
        return true;
    }

    public void LoadMainMenu()
    {
        //PhotonNetwork.LoadLevel("Main Menu");
        SceneManager.LoadScene("Main Menu");
    }

    public void LoadGame()
    {
        ResetGameStates();
        SceneManager.LoadScene("GameStory");
    }

    public void LoadLobby()
    {
        ResetGameStates();
        SceneManager.LoadScene("Lobby");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
