using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIZombieState_Patrol1 : AIZombieState
{
    
    [SerializeField]
    float _turnOnSpotThreshold = 80.0f;  //轉向角度
    [SerializeField]
    float _slerpSpeed = 5.0f;  //轉向速度
    [SerializeField][Range(0.0f, 3.0f)]
    float _speed = 1.0f;  //速度

    public override AIStateType GetStateType()   //回傳狀態
    {
        return AIStateType.Patrol;
    }

    public override void OnEnterState()  //第一次進入狀態時
    {
        Debug.Log("Enter Patrol state");
        base.OnEnterState();
        if (_zombieStateMachine == null)
        {           
            return;
        }

        _zombieStateMachine.NavAgentControl(true, false);  //AI移動控制
        _zombieStateMachine.seeking = 0;  //不轉向
        _zombieStateMachine.feeding = false;  //不吃屍體
        _zombieStateMachine.attackType = 0;  //不攻擊

        _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));  //設置路徑

       // _zombieStateMachine.navAgent.Resume();  //確保AI有執行
        _zombieStateMachine.navAgent.isStopped = false;
    }

    public override AIStateType OnUpdate()  //偵測每一針的狀態
    {
        if(_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)  //如果威脅是玩家
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);  //設置玩家為目標
            return AIStateType.Pursuit;  //追逐玩家
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)  //如果威脅是光源
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);  //把光源設置為威脅
            return AIStateType.Alerted;  //進入警戒狀態
        }

        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio)  //如果威脅是聲音
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);  //把聲音設置為威脅
            return AIStateType.Alerted;  //進入警戒狀態
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food)  //如果是食物
        {
            if((1.0f - _zombieStateMachine.satisfaction) > (_zombieStateMachine.VisualThreat.distance / _zombieStateMachine.sensorRadius))  //如果飢餓大於食物來源的距離
            {
                _stateMachine.SetTarget(_stateMachine.VisualThreat);  //把食物設為新目標
                return AIStateType.Pursuit;  //找食物
            }            
        }

        if (_zombieStateMachine.navAgent.pathPending)  //如果路徑還在計算
        {
            _zombieStateMachine.speed = 0;  //速度為0
            return AIStateType.Patrol;  //回到巡邏狀態
        }
        else
        {
            _zombieStateMachine.speed = _speed;
        }

        float angle = Vector3.Angle(_zombieStateMachine.transform.forward, (_zombieStateMachine.navAgent.steeringTarget - _zombieStateMachine.transform.position)); //計算轉向目標的角度 (前進方向,(當前的路徑 - 目前位置))

        if (angle > _turnOnSpotThreshold)  //如果角度太大
        {
            return AIStateType.Alerted;  //退出巡邏狀態 切換為警戒狀態
        }

        if (!_zombieStateMachine.useRootRotation)  //不使用動畫旋轉 手動控制旋轉
        {
            Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navAgent.desiredVelocity);  //旋轉
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);  //隨著時間平滑移動
        }
        //如果因為任何原因遺失路徑
        if(_zombieStateMachine.navAgent.isPathStale || !_zombieStateMachine.navAgent.hasPath || 
            _zombieStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete)  //如果是舊路徑 || 沒有路徑 ||當前的路徑狀態(完整)
        {
            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(true));  //下一個新航點
        }

        return AIStateType.Patrol;  //默認巡邏狀態
    }

   

    public override void OnDestinationReached(bool isReached)  //當殭屍到達目標時
    {
        if (_zombieStateMachine == null || !isReached)
        {
            return;
        }
        
        if(_zombieStateMachine.targetType == AITargetType.Waypoint)  //選擇航點列表中的下一個航點
        {
            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(true));
        }
    }

   /* public override void OnAnimatorIKUpdated()
    {
        if (_zombieStateMachine == null)
        {
            return;
        }
        Debug.Log("IK");
        _zombieStateMachine.animator.SetLookAtPosition(_zombieStateMachine.targetPosition + Vector3.up);
        _zombieStateMachine.animator.SetLookAtWeight(0.55f);
    }*/
}
