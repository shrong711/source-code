using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Idle1 : AIZombieState
{
    [SerializeField]
    Vector2 _idleTimeRange = new Vector2(10.0f, 60.0f);  //空閒時間範圍

    float _idleTime = 0.0f;  //閒置時間
    float _timer = 0.0f;


    public override AIStateType GetStateType()  //回傳狀態
    {
       // Debug.Log("StateType被父類別調用");
        return AIStateType.Idle;
    }

    public override void OnEnterState()  //第一次進入狀態時
    {
        Debug.Log("Enter idle state");
        base.OnEnterState();
        if(_zombieStateMachine == null)
        {
            return;
        }

        _idleTime = Random.Range(_idleTimeRange.x, _idleTimeRange.y);  //隨機閒置時間
        _timer = 0.0f;  //經過的時間

        _zombieStateMachine.NavAgentControl(true, false);  //AI移動控制
        _zombieStateMachine.speed = 0;  //閒置時速度為0
        _zombieStateMachine.seeking = 0;  //不轉向
        _zombieStateMachine.feeding = false;  //不吃屍體
        _zombieStateMachine.attackType = 0;  //不攻擊
        _zombieStateMachine.ClearTarget();  //清除目標
    }

    public override AIStateType OnUpdate()  //偵測每一針的狀態
    {
        if(_zombieStateMachine == null)  //沒有狀態
        {
            return AIStateType.Idle;  //預設閒置狀態
        }

        if(_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)  //如果威脅是玩家
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);  //把玩家設置為威脅
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

        if(_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food)  //如果是食物
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);  //設置食物為目標
            return AIStateType.Pursuit;  //找食物
        }

        _timer += Time.deltaTime;  //經過的時間

        if(_timer > _idleTime)  //如果時間大於閒置時間
        {
            //Debug.Log("go to patrol");
            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));  //下一個航點
           // _zombieStateMachine.navAgent.Resume();  //繼續
            _zombieStateMachine.navAgent.isStopped = false;
            return AIStateType.Alerted;  //轉為警戒狀態
        }

        return AIStateType.Idle;  //都沒有回傳閒置狀態
    }
}
