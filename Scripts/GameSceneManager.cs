using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo  //玩家訊息
{
    public Collider collider = null;  //碰撞器
    public CharacterManager characterManager = null;  //角色控制器
    public Camera camera = null;  //相機
    public CapsuleCollider meleeTrigger = null;  //攻擊碰撞器
}

public class GameSceneManager : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _bloodParticles = null;  //血粒子效果


    private static GameSceneManager _instance = null;  //靜態遊戲場景管理器 
    public static GameSceneManager instance  //讓任何腳本都可以引用
    {
        get
        {
            if(_instance == null)
            {
                _instance = (GameSceneManager)FindObjectOfType(typeof(GameSceneManager));  //找到遊戲場景管理器
            }
            return _instance;  //回傳
        }
    }

    private Dictionary<int, AIStateMachine> _stateMachines = new Dictionary<int, AIStateMachine>(); //儲存場景上的碰撞器或是任何有關AIStateMachine狀態的東西
    private Dictionary<int, PlayerInfo> _playerInfos = new Dictionary<int, PlayerInfo>();  //儲存玩家訊息
    private Dictionary<int, InteractiveItem> _interactiveItems = new Dictionary<int, InteractiveItem>();  //儲存互動項目
    private Dictionary<int, MaterialController> _materialControllers = new Dictionary<int, MaterialController>();  //儲存材質球

    public ParticleSystem bloodParticles { get { return _bloodParticles; } }

    public void RegisterAIStateMachine(int key, AIStateMachine stateMachine)  //註冊碰撞器
    {
        if (!_stateMachines.ContainsKey(key))  //如果字典裡沒有碰撞器
        {
            _stateMachines[key] = stateMachine;  //新增碰撞器到字典裡
        }
    }

    public AIStateMachine GetAIStateMachine(int key)  //取得狀態
    {
        AIStateMachine machine = null;  //儲存狀態的臨時變數
        if(_stateMachines.TryGetValue(key, out machine))  //如果字典裡有狀態 
        {
            return machine;  //回傳狀態
        }
        return null;  //回傳空值
    }

    public void RegisterPlayerInfo(int key, PlayerInfo playerInfo)  //註冊玩家訊息
    {
        if (!_playerInfos.ContainsKey(key))  //如果沒有符合字典裡的訊息
        {
            _playerInfos[key] = playerInfo;  //添加到字典
        }
    }

    public PlayerInfo GetPlayerInfo(int key)  //取得玩家訊息
    {
        PlayerInfo info = null;
        if(_playerInfos.TryGetValue(key, out info))
        {
            return info;
        }

        return null;
    }

    public void RegisterInteractiveItem(int key, InteractiveItem script)  //註冊互動項目
    {
        if (!_interactiveItems.ContainsKey(key))
        {
            _interactiveItems[key] = script;
        }
    }

    public InteractiveItem GetInteractiveItem(int key)  //取得項目
    {
        InteractiveItem item = null;
        _interactiveItems.TryGetValue(key, out item);
        return item;
    }

    public void RegisterMaterialController(int key, MaterialController controller)
    {
        if (!_materialControllers.ContainsKey(key))
        {
            _materialControllers[key] = controller;
        }
    }

    protected void OnDestroy()
    {
        foreach(KeyValuePair<int, MaterialController> controller in _materialControllers)  //從材質字典中找到每個鍵值對
        {
            controller.Value.OnReset();  //重置值到原本輸入的狀態
        }
    }
}
