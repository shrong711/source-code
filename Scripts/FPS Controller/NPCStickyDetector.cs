using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStickyDetector : MonoBehaviour
{
    FPSController _controller = null;
	
	void Start ()
    {
        _controller = GetComponentInParent<FPSController>();  //取得父物件的腳本
	}

    void OnTriggerStay(Collider col)  //如果持續再碰撞器內
    {
        AIStateMachine machine = GameSceneManager.instance.GetAIStateMachine(col.GetInstanceID());  //取得殭屍碰撞器
        if(machine != null && _controller != null)
        {
            _controller.DoStickiness();  //減緩速度
            machine.VisualThreat.Set(AITargetType.Visual_Player, _controller.characterController, _controller.transform.position, Vector3.Distance(machine.transform.position, _controller.transform.position));  //設為目標
            machine.SetStateOverride(AIStateType.Attack);  //強迫進入攻擊狀態
        }
    }
}
