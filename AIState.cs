using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour  //抽象類別 AI系統使用的抽象方法
{
    public virtual void SetStateMachine(AIStateMachine stateMachine)  //獲得所有狀態時 將狀態加入字典
    {
        _stateMachine = stateMachine;
    }
    //默認處理程序
    public virtual void OnEnterState()  //進入狀態
    {

    }

    public virtual void OnExitState()  //離開狀態
    {

    }

    public virtual void OnAnimatorUpdated()  //動畫更新
    {
        if (_stateMachine.useRootPosition)  
        {  //設定速度給動畫
            _stateMachine.navAgent.velocity = _stateMachine.animator.deltaPosition / Time.deltaTime; 
        }

        if (_stateMachine.useRootRotation)
        {  //設定旋轉值
            _stateMachine.transform.rotation = _stateMachine.animator.rootRotation;
        }
    }

    public virtual void OnAnimatorIKUpdated()  //控制身體部位
    {

    }

    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other)  //自訂Trigger事件
    {

    }

    public virtual void OnDestinationReached(bool isReached)  //是否有新目標
    {

    }
    public abstract AIStateType GetStateType();  //抽象方法 取得狀態類型
    public abstract AIStateType OnUpdate();  //轉換狀態方法

    protected AIStateMachine _stateMachine;

    public static void ConvertSphereColliderToWorldSpace(SphereCollider col, out Vector3 pos, out float radius)  //將球型碰撞器位置和半徑轉為世界空間
    {
        pos = Vector3.zero;  //預設值
        radius = 0.0f;  //預設值

        if (col == null)  //如果沒有碰撞體
        {
            return;  //返回
        }
        //計算球型碰撞器中心的世界位置
        pos = col.transform.position;
        pos.x += col.center.x * col.transform.lossyScale.x;
        pos.y += col.center.y * col.transform.lossyScale.y;
        pos.z += col.center.z * col.transform.lossyScale.z;
        //計算球型碰撞器的世界空間半徑
        radius = Mathf.Max(col.radius * col.transform.lossyScale.x, 
                           col.radius * col.transform.lossyScale.y);
        radius = Mathf.Max(radius, col.radius * col.transform.lossyScale.z);
    }

    public static float FindSignedAngle(Vector3 fromVector, Vector3 toVector)  //找到動畫轉向角度
    {
        if(fromVector == toVector)  //確保兩個向量都一樣
        {
            return 0.0f;  //角度為0
        }

        float angle = Vector3.Angle(fromVector, toVector);  //儲存兩個向量的角度
        Vector3 cross = Vector3.Cross(fromVector, toVector);  //交叉向量 獲得垂直向量
        angle *= Mathf.Sign(cross.y);  //正弦直 (如果是-1就左轉 1右轉)
        return angle;  //回傳角度
    }
}
