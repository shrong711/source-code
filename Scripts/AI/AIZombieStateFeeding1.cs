using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieStateFeeding1 : AIZombieState
{
    [SerializeField]
    float _slerpSpeed = 5.0f;
    [SerializeField]
    Transform _bloodParticlesMount = null;  //血粒子系統位置
    [SerializeField][Range(0.01f, 1.0f)]
    float _bloodParticlesBurstTime = 0.1f;  //血粒子系統時間
    [SerializeField][Range(1, 100)]
    int _bloodParticlesBurstAmout = 10;  //粒子數量


    private int _eatingStateHash = Animator.StringToHash("Feeding State");  //吃屍體動畫
    private int _crawlEatingStateHash = Animator.StringToHash("Crawl Feeding State");  //趴著吃動畫
    private int _eatingLayerIndex = -1;  //動畫層
    private float _timer = 0.0f;  //計時器

    public override AIStateType GetStateType()  //取得狀態
    {
        return AIStateType.Feeding;
    }

    public override void OnEnterState()
    {
        Debug.Log("Enter Feeding State");
        base.OnEnterState();  //呼叫基底方法 
        if(_zombieStateMachine == null)
        {
            return;
        }

        if(_eatingLayerIndex == -1)  //取得動畫層
        {
            _eatingLayerIndex = _zombieStateMachine.animator.GetLayerIndex("Cinematic");  
        }

        _timer = 0.0f;  //重置時間

        _zombieStateMachine.feeding = true;  //飢餓狀態
        _zombieStateMachine.seeking = 0;  //不旋轉
        _zombieStateMachine.speed = 0;  //速度為0
        _zombieStateMachine.attackType = 0;  //不攻擊

        _zombieStateMachine.NavAgentControl(true, false);  //更新AI位置 不旋轉
    }

    public override void OnExitState()  //離開飢餓狀態
    {
        if(_zombieStateMachine != null)
        {
            _zombieStateMachine.feeding = false;  //不再飢餓
        }
    }

    public override AIStateType OnUpdate()
    {
        _timer += Time.deltaTime;  

        if(_zombieStateMachine.satisfaction > 0.9f)  //飢餓感大於0.9
        {
            _zombieStateMachine.GetWaypointPosition(false);  //走向下一個航點
            return AIStateType.Alerted;  //返回警戒狀態
        }

        if(_zombieStateMachine.VisualThreat.type != AITargetType.None && _zombieStateMachine.VisualThreat.type != AITargetType.Visual_Food)  //除了無狀態.食物
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);  //都設為目標
            return AIStateType.Alerted;  //回到警戒狀態 嘗試尋找目標
        }

        if(_zombieStateMachine.AudioThreat.type == AITargetType.Audio)  //如果是聲音
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);  //設置目標
            return AIStateType.Alerted;  //回到警戒狀態
        }

        int currentHash = _zombieStateMachine.animator.GetCurrentAnimatorStateInfo(_eatingLayerIndex).shortNameHash;
        if (currentHash == _eatingStateHash || currentHash == _crawlEatingStateHash)  //如果正在撥放吃屍體動畫
        {
            _zombieStateMachine.satisfaction = Mathf.Min(_zombieStateMachine.satisfaction + ((Time.deltaTime * _zombieStateMachine.replenishRate) / 100.0f), 1.0f);  //隨著時間補充飽足感
            if(GameSceneManager.instance && GameSceneManager.instance.bloodParticles && _bloodParticlesMount)
            {
                if(_timer > _bloodParticlesBurstTime)
                {
                    ParticleSystem system = GameSceneManager.instance.bloodParticles;  //抓到粒子效果
                    system.transform.position = _bloodParticlesMount.transform.position;  //把要噴血的位置給粒子系統
                    system.transform.rotation = _bloodParticlesMount.transform.rotation;  //旋轉
                    var settings = system.main;  //取得主要設置面板
                    settings.simulationSpace = ParticleSystemSimulationSpace.World;  //模擬世界空間
                    system.Emit(_bloodParticlesBurstAmout);
                    _timer = 0.0f;  //重置時間
                }
            }
        }

        if (!_zombieStateMachine.useRootRotation)  //沒有使用根旋轉
        {
            Vector3 targetPos = _zombieStateMachine.targetPosition;  //取得目標位置
            targetPos.y = _zombieStateMachine.transform.position.y;  //計算四元數 像2D旋轉
            Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);  //面向玩家
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);  //隨著時間平滑旋轉
        }

        Vector3 headToTarget = _zombieStateMachine.targetPosition - _zombieStateMachine.animator.GetBoneTransform(HumanBodyBones.Head).position;
        _zombieStateMachine.transform.position = Vector3.Lerp(_zombieStateMachine.transform.position, _zombieStateMachine.transform.position + headToTarget, Time.deltaTime);  //讓餵食動畫在離頭部近一點

        return AIStateType.Feeding;  //保持飢餓狀態
    }
}
