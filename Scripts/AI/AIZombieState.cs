using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIZombieState : AIState  //威脅的事件處理
{
    protected int _playerLayerMask = -1;
    protected int _bodyPartLayer = -1;
    protected int _visualLayerMask = -1;
    protected AIZombieStateMachine _zombieStateMachine = null;

    void Awake()
    {
        _playerLayerMask = LayerMask.GetMask("Player", "AI Body Part") + 1;  //玩家圖層
        _visualLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Visual Aggravator") + 1;
        _bodyPartLayer = LayerMask.NameToLayer("AI Body Part");  //圖層索引 
    }

    public override void SetStateMachine(AIStateMachine stateMachine)  //檢查是否是正確的狀態
    {
        if(stateMachine.GetType() == typeof(AIZombieStateMachine))
        {
            base.SetStateMachine(stateMachine);  //傳入參數
            _zombieStateMachine = (AIZombieStateMachine)stateMachine;
        }
    }

    public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)  //當威脅進入/停留/退出 殭屍的感測器時觸發事件 為任何威脅增加碰撞器 並檢查威脅成員的層級(玩家最優先) 
    {
        if(_zombieStateMachine == null)  //如果沒有返回
        {
            return;
        }

        if(eventType != AITriggerEventType.Exit)  //如果是 進入/停留事件
        {
            AITargetType curType = _zombieStateMachine.VisualThreat.type;  //當前的威脅

            if (other.CompareTag("Player"))  //如果威脅為玩家
            {
                float distance = Vector3.Distance(_zombieStateMachine.sensorPosition, other.transform.position);  //感測器與碰撞器距離
                if(curType != AITargetType.Visual_Player || curType == AITargetType.Visual_Player && distance < _zombieStateMachine.VisualThreat.distance)  //如果當前的威脅不是玩家 或是玩家比之前儲存的距離更近
                {
                    //碰撞器是否在殭屍視角內
                    RaycastHit hitInfo;
                    if(ColliderIsVisible(other, out hitInfo, _playerLayerMask))  //如果有找到玩家
                    {  //設置玩家為新威脅
                        _zombieStateMachine.VisualThreat.Set(AITargetType.Visual_Player, other, other.transform.position, distance);
                    }
                }
            }
            else if (other.CompareTag("Flash Light") && curType != AITargetType.Visual_Player)  //手電筒威脅
            {
                BoxCollider flashLightTrigger = (BoxCollider)other;  //手電筒碰撞器
                float distanceToThreat = Vector3.Distance(_zombieStateMachine.sensorPosition, flashLightTrigger.transform.position);  //感測器與手電筒的距離
                float zSize = flashLightTrigger.size.z * flashLightTrigger.transform.lossyScale.z;  //碰撞器z大小
                float aggrFactor = distanceToThreat / zSize;  //計算距離
                if(aggrFactor <= _zombieStateMachine.sight && aggrFactor <= _zombieStateMachine.intelligence)  //如果再視野內 AND 再聲音來源內
                {
                    _zombieStateMachine.VisualThreat.Set(AITargetType.Visual_Light, other, other.transform.position, distanceToThreat);  //設置聲音威脅
                }
            }
            else if(other.CompareTag("AI Sound Emitter"))  //聲音威脅
            {
                SphereCollider soundTrigger = (SphereCollider)other;  //球形碰狀器
                if(soundTrigger == null)
                {
                    return;
                }
                Vector3 agentSensorPosition = _zombieStateMachine.sensorPosition;  //AI感測器位置
                Vector3 soundPos;  //聲音位置
                float soundRadius;  //聲音半徑
                AIState.ConvertSphereColliderToWorldSpace(soundTrigger, out soundPos, out soundRadius);
                float distanceToThreat = (soundPos - agentSensorPosition).magnitude;  //殭屍到聲音的距離
                float distanceFactor = (distanceToThreat / soundRadius);  //計算距離 使其在中心位置處於聲音半徑為0時為1.0f
                distanceFactor += distanceFactor * (1.0f - _zombieStateMachine.hearing);  //可以聽到聲音的距離
                if(distanceFactor > 1.0f)  //如果超過可以聽到聲音的範圍(1.0f)
                {
                    return;  //返回
                }

                if(distanceToThreat < _zombieStateMachine.AudioThreat.distance)  //如果可以聽到聲音 而且比之前儲存的威脅距離更近
                {
                    _zombieStateMachine.AudioThreat.Set(AITargetType.Audio, other, soundPos, distanceToThreat);  //設置聲音威脅
                }

            }
            else if (other.CompareTag("AI Food") && curType != AITargetType.Visual_Player && curType != AITargetType.Visual_Light && 
                     _zombieStateMachine.satisfaction <= 0.9f && _zombieStateMachine.AudioThreat.type == AITargetType.None)  //最低的威脅
            {
                float distanceToThreat = Vector3.Distance(other.transform.position, _zombieStateMachine.sensorPosition);  //計算距離

                if(distanceToThreat < _zombieStateMachine.VisualThreat.distance)  //如果距離比之前儲存的威脅距離更近
                {
                    RaycastHit hitInfo;
                    if(ColliderIsVisible(other, out hitInfo, _visualLayerMask))  //如果再視野範圍內
                    {
                        _zombieStateMachine.VisualThreat.Set(AITargetType.Visual_Food, other, other.transform.position, distanceToThreat);  //設置食物給殭屍吃
                    }
                }
            }
        }
    }

    protected virtual bool ColliderIsVisible(Collider other, out RaycastHit hitInfo, int layerMask = -1)  //測試殭屍的視線
    {
        hitInfo = new RaycastHit();  //確保有視線回傳

        if(_zombieStateMachine == null)  //確保有抓到狀態機
        {
            return false;
        }
        
        Vector3 head = _stateMachine.sensorPosition;  //重新定位
        Vector3 direction = other.transform.position - head;  //頭到目標的距離
        float angle = Vector3.Angle(direction, transform.forward);  //視野角度
        if(angle > _zombieStateMachine.fov * 0.5f)  //如果殭屍的視野角度大於視野的一半 代表超出視野
        {
            return false;  
        }
        //測試射線 從殭屍視野感測器原點沿著碰撞器方向看 獲得所有被射線射到的東西
        RaycastHit[] hits = Physics.RaycastAll(head, direction.normalized, _zombieStateMachine.sensorRadius * _zombieStateMachine.sight, layerMask);  //獲得所有被射線打到的東西(以頭為感測器中心,方向為direction,感測器半徑 * 視野作為最大距離, 圖層)
        //找到最近的碰撞器而不是AI自己的身體 如果他不是目標 則目標被擋住
        float closestColliderDistance = float.MaxValue;  //儲存最近或最小的距離
        Collider closestCollider = null;  //儲存最近的對象

        for(int i = 0; i < hits.Length; i++)  //檢查每個命中的東西
        {
            RaycastHit hit = hits[i]; 
            if(hit.distance < closestColliderDistance)  //如果射線射到的東西距離比之前儲存的更近
            {
                if(hit.transform.gameObject.layer == _bodyPartLayer)  //如果是身體部位的圖層
                {
                    if(_stateMachine != GameSceneManager.instance.GetAIStateMachine(hit.rigidbody.GetInstanceID()))  //並假設他不是我們身體的一部份
                    {
                        closestColliderDistance = hit.distance;  //儲存距離
                        closestCollider = hit.collider;  //儲存碰撞器
                        hitInfo = hit;  //射線的訊息
                    }
                }
                else  //如果不是身體部位圖層 儲存射線命中的最新訊息
                {
                    closestColliderDistance = hit.distance;  //儲存距離
                    closestCollider = hit.collider;  //儲存碰撞器
                    hitInfo = hit;  //射線的訊息
                }
            }
        }
        if(closestCollider && closestCollider.gameObject == other.gameObject)  //如果最接近的碰撞體是目標 代表殭屍的視野有看到
        {
            return true;  //回傳true
        }
        return false;  //如果被其他東西擋住視線 代表沒看到 回傳false
    }
}
