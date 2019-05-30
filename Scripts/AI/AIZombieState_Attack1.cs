using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Attack1 : AIZombieState
{
    [SerializeField][Range(0, 10)]
    float _speed = 0.0f;  //速度
    [SerializeField]
    float _stoppingDistance = 1.0f;  //停止距離
    [SerializeField][Range(0.0f, 1.0f)]
    float _LookAtWeight = 0.7f;  //控制殭屍的頭看向玩家
    [SerializeField][Range(0.0f, 90.0f)]
    float _LookAtAngleThreshold = 15.0f;  //頭的角度 角度越大 越早控制頭部
    [SerializeField]
    float _slerpSpeed = 5.0f;  //平滑速度

    private float _currentLookAtWeight = 0.0f;

    public override AIStateType GetStateType()  //取得狀態
    {
        return AIStateType.Attack;
    }

    public override void OnEnterState()  //進入狀態
    {
        Debug.Log("Enter Attack State");
        base.OnEnterState();  
        if(_zombieStateMachine == null)  //是否為有效地引用
        {
            return;
        }

        _zombieStateMachine.NavAgentControl(true, false);  //控制殭屍移動 但不旋轉
        _zombieStateMachine.seeking = 0;  
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = UnityEngine.Random.Range(1, 100);  //隨機攻擊
        _zombieStateMachine.speed = _speed;
        _currentLookAtWeight = 0.0f;
    }

    public override void OnExitState()
    {
        _zombieStateMachine.attackType = 0;  //不攻擊
    }

    public override AIStateType OnUpdate()
    {
        Vector3 targetPos;
        Quaternion newRot;

        if (Vector3.Distance(_zombieStateMachine.transform.position, _zombieStateMachine.targetPosition) < _stoppingDistance)
        {
            _zombieStateMachine.speed = 0;
        }
        else
        {
            _zombieStateMachine.speed = _speed;
        }

        if(_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)  //如果是玩家
        {
            _zombieStateMachine.SetTarget(_stateMachine.VisualThreat);  //設置目標

            if (!_zombieStateMachine.inMeleeRange)  //如果不在攻擊範圍
            {
                return AIStateType.Pursuit;  //追逐玩家
            }

            if (!_zombieStateMachine.useRootRotation)  //如果不使用根旋轉
            {
                targetPos = _zombieStateMachine.targetPosition;  //玩家位置
                targetPos.y = _zombieStateMachine.transform.position.y;  //2D旋轉
                newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);  //面向玩家
                _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);  //平滑旋轉
            }

            _zombieStateMachine.attackType = UnityEngine.Random.Range(1, 100);  //隨機攻擊

            return AIStateType.Attack;  //保持攻擊狀態
        }

        if (!_zombieStateMachine.useRootRotation)
        {
            targetPos = _zombieStateMachine.targetPosition;  //玩家位置
            targetPos.y = _zombieStateMachine.transform.position.y;  //2D旋轉
            newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);  //面向玩家
            _zombieStateMachine.transform.rotation = newRot;  //面對玩家最後的位置
        }

        return AIStateType.Alerted;  //失去目標時回到警戒狀態 嘗試尋找玩家
    }

    public override void OnAnimatorIKUpdated()
    {
        if(_zombieStateMachine == null)
        {
            return;
        }

        if(Vector3.Angle(_zombieStateMachine.transform.forward, _zombieStateMachine.targetPosition - _zombieStateMachine.transform.position) < _LookAtAngleThreshold)  //如果面向玩家的角度小於控制頭的角度
        {
            _zombieStateMachine.animator.SetLookAtPosition(_zombieStateMachine.targetPosition + Vector3.up);  //看向玩家
            _currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, _LookAtWeight, Time.deltaTime);  //平移頭部位置
            _zombieStateMachine.animator.SetLookAtWeight(_currentLookAtWeight);  //轉動頭部
        }
        else  //離開攻擊時
        {
            _currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, 0.0f, Time.deltaTime);
            _zombieStateMachine.animator.SetLookAtWeight(_currentLookAtWeight);  //緩慢的面對玩家
        }
        
    }
}
