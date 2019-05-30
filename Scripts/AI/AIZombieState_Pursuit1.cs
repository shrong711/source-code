using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIZombieState_Pursuit1 : AIZombieState  //追逐目標
{
    [SerializeField][Range(0, 10)]
    private float _speed = 1.0f;  //速度
    [SerializeField][Range(0.0f, 1.0f)]
    float _LookAtWeight = 0.7f;  //控制殭屍的頭看向玩家
    [SerializeField][Range(0.0f, 90.0f)]
    float _LookAtAngleThreshold = 15.0f;  //頭的角度 角度越大 越早轉動頭部
    [SerializeField]
    private float _slerpSpeed = 5.0f;  //平滑速度
    [SerializeField]
    private float _repathDistanceMultiplier = 0.035f;  //縮短與目標之間的距離
    [SerializeField]
    private float _repathVisualMinDuration = 0.05f;  //視覺威脅最小計算路徑秒數
    [SerializeField]
    private float _repathVisualMaxDuration = 5.0f;  //視覺威脅最大計算路徑秒數
    [SerializeField]
    private float _repathAudioMinDuration = 0.25f;  //聲音威脅最小計算路徑秒數
    [SerializeField]
    private float _repathAudioMaxDuration = 5.0f;  //聲音威脅最大計算路徑秒數
    [SerializeField]
    private float _maxDuration = 40.0f;  //追逐狀態最大的時間

    private float _timer = 0.0f;  //經過的時間
    private float _repathTimer = 0.0f;  //追逐狀態的時間
    private float _currentLookAtWeight = 0.0f;

    public override AIStateType GetStateType()  //回傳狀態
    {
        return AIStateType.Pursuit;
    }  

    public override void OnEnterState()  //進入追逐狀態
    {
        Debug.Log("Enter Pursuit State");
        base.OnEnterState();
        if(_zombieStateMachine == null)
        {
            return;
        }

        _zombieStateMachine.NavAgentControl(true, false);  //AI移動控制
        _zombieStateMachine.seeking = 0;  //不轉向
        _zombieStateMachine.feeding = false;  //不吃屍體
        _zombieStateMachine.attackType = 0;  //不攻擊

        _timer = 0.0f;  //經過的時間 默認為0
        _repathTimer = 0.0f;  //總時間

        _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.targetPosition);  //計算路徑
       // _zombieStateMachine.navAgent.Resume();  //繼續路徑
        _zombieStateMachine.navAgent.isStopped = false;

        _currentLookAtWeight = 0.0f;
}

    public override AIStateType OnUpdate()
    {
        _timer += Time.deltaTime;  //計時器
        _repathTimer += Time.deltaTime;

        if(_timer > _maxDuration)  //如果追逐時間超過40秒
        {
            return AIStateType.Patrol;  //回到巡邏狀態
        }

        if(_stateMachine.targetType == AITargetType.Visual_Player && _zombieStateMachine.inMeleeRange)  //如果在追玩家 並進入目標碰撞器範圍
        {
            return AIStateType.Attack;  //攻擊狀態
        }

        if (_zombieStateMachine.isTargetReached)  //如果到達目標碰撞器內
        {
            switch (_stateMachine.targetType)  //目標源頭類型
            {
                case AITargetType.Audio:  //如果是聲音
                case AITargetType.Visual_Light:  //如果是燈光
                    _stateMachine.ClearTarget();  //清除威脅
                    return AIStateType.Alerted;  //回到警戒狀態並搜尋光的來源

                case AITargetType.Visual_Food:  //如果是食物
                    return AIStateType.Feeding;  //吃東西
            }
        }
        //如果因為任何原因遺失路徑 進入警戒狀態 嘗試重新獲得目標 或是最後放棄恢復巡邏
        if(_zombieStateMachine.navAgent.isPathStale || (!_zombieStateMachine.navAgent.hasPath && !_zombieStateMachine.navAgent.pathPending) //如果路徑是舊的或是沒有路徑&&路徑計算中或是路徑不完整
           || _zombieStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            return AIStateType.Alerted;  //回到警戒狀態
        }

        if (_zombieStateMachine.navAgent.pathPending)
        {
            _zombieStateMachine.speed = 0;
        }
        else
        {
            _zombieStateMachine.speed = _speed;

            // 如果接近玩家並且玩家在視野內 繼續追逐玩家
            if (!_zombieStateMachine.useRootRotation && _zombieStateMachine.targetType == AITargetType.Visual_Player
               && _zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player && _zombieStateMachine.isTargetReached)
            {
                Vector3 targetPos = _zombieStateMachine.targetPosition;  //取得玩家位置
                targetPos.y = _zombieStateMachine.transform.position.y;  //計算四元數 像2D旋轉
                Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);  //面向玩家
                _zombieStateMachine.transform.rotation = newRot;  //跟著玩家旋轉 不輕易的被甩掉
            }
            else if (!_zombieStateMachine.useRootRotation && !_zombieStateMachine.isTargetReached)  // 如果不是玩家 還在前往目標的路上 保持旋轉
            {
                Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navAgent.desiredVelocity);  //面向目標
                _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);  //隨著時間平滑旋轉
            }
            else if (_zombieStateMachine.isTargetReached)  //如果到達不是玩家的目的地(聲音)
            {
                return AIStateType.Alerted;  //進入警戒狀態 試圖搜尋聲音來源
            }
        }
       
        if(_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)  //如果威脅是玩家
        {
            if(_zombieStateMachine.targetPosition != _zombieStateMachine.VisualThreat.position)  //如果玩家正在移動位置
            {
                if(Mathf.Clamp(_zombieStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer)  //如果距離玩家很近時頻繁的計算路徑
                {
                    _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.VisualThreat.position);  //計算新路徑
                    _repathTimer = 0.0f;  //重置時間
                }
            }
            _stateMachine.SetTarget(_zombieStateMachine.VisualThreat);  //確保這是當前目標
            return AIStateType.Pursuit;  //保持追逐狀態
        }

        if(_zombieStateMachine.targetType == AITargetType.Visual_Player)  //如果目標類型是最後一個看到的玩家
        {
            return AIStateType.Pursuit;  //保持追擊狀態
        }

        if(_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)  //如果威脅是光源
        {
            if(_zombieStateMachine.targetType == AITargetType.Audio || _zombieStateMachine.targetType == AITargetType.Visual_Food)  //然後有一個威脅較低的目標 
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);  //設置新目標
                return AIStateType.Alerted;  //回到警戒狀態
            }
            else if(_zombieStateMachine.targetType == AITargetType.Visual_Light)  //如果威脅是光源
            {
                int currentID = _zombieStateMachine.targetColliderID;  //獲得目標碰撞器的ID

                if(currentID == _zombieStateMachine.VisualThreat.collider.GetInstanceID())  //如果這是一樣的光源
                {
                    if(_zombieStateMachine.targetPosition != _zombieStateMachine.VisualThreat.position)  //如果光源位置有變
                    {
                        if(Mathf.Clamp(_zombieStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer) //接近目標時 計算路徑更頻繁
                        {
                            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.VisualThreat.position);  //重新計算路徑
                            _repathTimer = 0.0f;  //重置時間
                        }  
                    }
                    _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);  //再設一次目標

                    return AIStateType.Pursuit;
                }
                else  //如果是不同光源
                {
                    _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);  //設置新目標
                    return AIStateType.Alerted;  //回到警戒狀態
                }
            }
        }
        else if(_zombieStateMachine.AudioThreat.type == AITargetType.Audio)  //如果威脅是聲音
        {
            if(_zombieStateMachine.targetType == AITargetType.Visual_Food)  //如果殭屍走向食物  食物為最低威脅
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);  //設置聲音威脅
                return AIStateType.Alerted;  //回到警戒狀態 搜尋聲音來源
            }
            else if (_zombieStateMachine.targetType == AITargetType.Audio)  
            {
                int currentID = _zombieStateMachine.targetColliderID;  //獲得目標碰撞體id

                if(currentID == _zombieStateMachine.AudioThreat.collider.GetInstanceID())  //如果這是同樣的聲音
                {
                    if(_zombieStateMachine.targetPosition != _zombieStateMachine.AudioThreat.position)  //如果位置不一樣
                    {
                        if(Mathf.Clamp(_zombieStateMachine.AudioThreat.distance * _repathDistanceMultiplier, _repathAudioMinDuration, _repathAudioMaxDuration) < _repathTimer)  //接近目標時 計算路徑更頻繁
                        {
                            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.AudioThreat.position);  //重新計算路徑
                            _repathTimer = 0.0f;
                        }
                    }
                    _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);  //確保目標一樣
                    return AIStateType.Pursuit;  //繼續找目標
                }
                else  //如果不一樣的聲音
                {
                    _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);  //設置新目標
                    return AIStateType.Alerted;  //回到警戒狀態 搜尋目標
                }
            }
        }
        return AIStateType.Pursuit;  //默認為追逐狀態
    }

    public override void OnAnimatorIKUpdated()
    {
        if (_zombieStateMachine == null)
        {
            return;
        }

        if (Vector3.Angle(_zombieStateMachine.transform.forward, _zombieStateMachine.targetPosition - _zombieStateMachine.transform.position) < _LookAtAngleThreshold)  //如果面向玩家的角度小於控制頭的角度
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
