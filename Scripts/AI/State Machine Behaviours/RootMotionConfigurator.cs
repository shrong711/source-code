using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionConfigurator : AIStateMachineLink  //定義根動畫行為
{
    [SerializeField]
    private int _rootPosition = 0;  //動畫位置值
    [SerializeField]
    private int _rootRotation = 0;  //動畫旋轉值

    private bool _rootMotionProcessed = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)  //在第一偵之前 使用分配給此狀態的動畫
    {
        if (_stateMachine)
        {
           // Debug.Log(_stateMachine.GetType().ToString());
            _stateMachine.AddRootMotionRequest(_rootPosition, _rootRotation);  //請求為此動畫狀態啟用/禁用根動畫
            _rootMotionProcessed = true;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)  //在離開動畫之前播放最後一偵
    {
        if (_stateMachine && _rootMotionProcessed)
        {
            _stateMachine.AddRootMotionRequest(-_rootPosition, -_rootRotation);  //告知AI狀態機 放棄根運動請求
            _rootMotionProcessed = false;
        }
    }
}
