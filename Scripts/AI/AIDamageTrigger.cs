using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDamageTrigger : Photon.PunBehaviour
{
    [SerializeField]
    string _parameter = "";  //動畫控制器參數名子
    [SerializeField]
    int _bloodParticlesBurstAmount = 10;  //血粒子數量
    [SerializeField]
    float _damageAmount = 0.1f;  //攻擊力
    [SerializeField]
    bool _doDamageSound = true;  //是否播放傷害聲音
    [SerializeField]
    bool _doPainSound = true;  //是否播放疼痛聲音

    AIStateMachine _stateMachine = null;
    Animator _animator = null;
    int _parameterHash = -1;
    GameSceneManager _gameSceneManager = null;
    private bool _firstContact = false;  //第一次接觸


    void Start()
    {
        _stateMachine = transform.root.GetComponentInChildren<AIStateMachine>();  //抓到子物件的腳本
        if (_stateMachine != null)
        {
            _animator = _stateMachine.animator;  //取得動畫控制器
        }

        _parameterHash = Animator.StringToHash(_parameter);  //取得控制器參數

        _gameSceneManager = GameSceneManager.instance;
    }

    

    void OnTriggerEnter(Collider col)
    {


        if (!_animator)
        {
            return;
        }   

        if (col.gameObject.CompareTag("Player") && _animator.GetFloat(_parameterHash) > 0.9f)
        {
            _firstContact = true;
        }
    }

    void OnTriggerStay(Collider col)  //當碰撞器停留在玩家身上的碰撞器時 
    {
    

        if (!_animator)
        {
            return;
        } 

        if (col.gameObject.CompareTag("Player") && _animator.GetFloat(_parameterHash) > 0.9f)  //當我攻擊玩家 && 動畫的曲線大於0.9時
        {
            if(GameSceneManager.instance && GameSceneManager.instance.bloodParticles)  //檢查場景
            {
                ParticleSystem system = GameSceneManager.instance.bloodParticles;  //取得粒子系統
                system.transform.position = transform.position;  //粒子系統施放位置
                system.transform.rotation = Camera.main.transform.rotation;
                var settings = system.main;  //取得主要設置面板
                settings.simulationSpace = ParticleSystemSimulationSpace.World;
                system.Emit(_bloodParticlesBurstAmount);
            }
            
            if(_gameSceneManager != null)
            {
                PlayerInfo info = _gameSceneManager.GetPlayerInfo(col.GetInstanceID());  //告訴場景管理 被這個殭屍的碰撞器碰到
                if(info != null && info.characterManager != null)
                {
                    info.characterManager.TakeDamage(_damageAmount, _doDamageSound && _firstContact, _doPainSound);  //傳入傷害
                }
            }
            _firstContact = false;
        }
    }
	
}
