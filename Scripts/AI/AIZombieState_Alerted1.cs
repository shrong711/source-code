using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Alerted1 : AIZombieState
{
    [SerializeField][Range(1.0f, 60.0f)]
    float _maxDuration = 10.0f;  //持續警戒狀態的最大時間
    [SerializeField]
    float _waypointAngleThreshold = 90.0f;  //航點轉換方向的角度
    [SerializeField]
    float _threatAngleThreshold = 10.0f;  //威脅角度(決定方向 右邊 左邊 或是轉更多)
    [SerializeField]
    float _directionChangeTime = 1.5f;  //方向轉換時間
    [SerializeField]
    float _slerpSpeed = 45.0f;  //旋轉速度


    float _timer = 0.0f;  //經過多少時間
    float _directionChangeTimer = 0.0f;  //方向轉換計時器
    float _screamChance = 0.0f;  //尖叫機率
    float _nextScream = 0.0f;  //下一次尖叫的時間
    float _screamFrequency = 120.0f;  //尖叫冷卻時間

    public override AIStateType GetStateType()
    {
        return AIStateType.Alerted;  //回傳警戒狀態
    }

    public override void OnEnterState()  //第一次進入狀態時
    {
        Debug.Log("Enter Alerted state");
        base.OnEnterState();
        if (_zombieStateMachine == null)
        {
            return;
        }

        _zombieStateMachine.NavAgentControl(true, false);  //AI移動控制
        _zombieStateMachine.speed = 0;  //閒置時速度為0
        _zombieStateMachine.seeking = 0;  //不轉向
        _zombieStateMachine.feeding = false;  //不吃屍體
        _zombieStateMachine.attackType = 0;  //不攻擊
        _timer = _maxDuration;
        _directionChangeTimer = 0.0f;
        _screamChance = _zombieStateMachine.screamChance - UnityEngine.Random.value;  //計算尖叫機會
    }

    public override AIStateType OnUpdate()
    {
        _timer -= Time.deltaTime;  //隨著時間減少
        _directionChangeTimer += Time.deltaTime;
        if(_timer <= 0.0f)  //如果持續時間到了
        {
            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));  //回到之前的航點
           // _zombieStateMachine.navAgent.Resume();  //繼續路徑
            _zombieStateMachine.navAgent.isStopped = false;
            _timer = _maxDuration;  //繼續警戒狀態
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)  //如果威脅是玩家
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);  //設置玩家為目標

            if(_screamChance > 0.0f && Time.time > _nextScream)  //尖叫
            {
                if (_zombieStateMachine.Scream())
                {
                    _screamChance = float.MinValue;
                    _nextScream = Time.time + _screamFrequency;
                    return AIStateType.Alerted;
                }
            }
            return AIStateType.Pursuit;  //追逐玩家
        }

        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio)  //如果威脅是聲音
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);  //把聲音設置為威脅
            _timer = _maxDuration;  //保持警戒狀態            
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)  //如果威脅是光源
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);  //把光源設置為威脅
            _timer = _maxDuration;  //保持警戒狀態   
        }

        if(_zombieStateMachine.AudioThreat.type == AITargetType.None && 
           _zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food &&
           _zombieStateMachine.targetType == AITargetType.None)  //如果沒有威脅 目標為食物
        {
            _zombieStateMachine.SetTarget(_stateMachine.VisualThreat);  //設置食物為目標
            return AIStateType.Pursuit;  //找食物
        }

        float angle;  //當前前進向量或是與目標之間的角度

        if((_zombieStateMachine.targetType == AITargetType.Audio || _zombieStateMachine.targetType == AITargetType.Visual_Light) && !_zombieStateMachine.isTargetReached)  //如果是聲音威脅 或是光源
        {
            angle = AIState.FindSignedAngle(_zombieStateMachine.transform.forward, _zombieStateMachine.targetPosition - _zombieStateMachine.transform.position);

            if(_zombieStateMachine.targetType == AITargetType.Audio && Mathf.Abs(angle) < _threatAngleThreshold)
            {
                return AIStateType.Pursuit;
            }

            if(_directionChangeTimer > _directionChangeTime)  //如果轉向方向計時器 > 轉換方向時間 
            {
                if (UnityEngine.Random.value < _zombieStateMachine.intelligence)  //如果隨機數小於殭屍智能
                {
                    _zombieStateMachine.seeking = (int)Mathf.Sign(angle);  //殭屍左右看
                }
                else
                {
                    _zombieStateMachine.seeking = (int)Mathf.Sign(UnityEngine.Random.Range(-1.0f, 1.0f));  //隨機左右轉
                }
                _directionChangeTimer = 0.0f;  //重置計時器
            }           
        }
        else if(_zombieStateMachine.targetType == AITargetType.Waypoint && !_zombieStateMachine.navAgent.pathPending)  //如果是航點
        {
            angle = AIState.FindSignedAngle(_zombieStateMachine.transform.forward, _zombieStateMachine.navAgent.steeringTarget - _zombieStateMachine.transform.position);  //找到旋轉的角度

            if (Mathf.Abs(angle) < _waypointAngleThreshold)
            {
                return AIStateType.Patrol;  //恢復巡邏狀態
            }

            if(_directionChangeTimer > _directionChangeTime)
            {
                _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
                _directionChangeTimer = 0.0f;              
            } 
        }
        else
        {
            if(_directionChangeTimer > _directionChangeTime)
            {
                _zombieStateMachine.seeking = (int)Mathf.Sign(UnityEngine.Random.Range(-1.0f, 1.0f));
                _directionChangeTimer = 0.0f;
            }
        }

        if (!_zombieStateMachine.useRootRotation)  //如果不使用根旋轉
        {
            _stateMachine.transform.Rotate(new Vector3(0.0f, _slerpSpeed * _zombieStateMachine.seeking * Time.deltaTime, 0.0f)); //旋轉Y軸
        }

        return AIStateType.Alerted;
    }
}
