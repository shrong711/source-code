using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComChannelName
{
    ComChannel1,
    ComChannel2,
    ComChannel3,
    ComChannel4
}

public class AIStateMachineLink : StateMachineBehaviour  //Animator狀態機行為  自定義動畫行為
{
    protected AIStateMachine _stateMachine;  //參考腳本
    public AIStateMachine stateMachine { set { _stateMachine = value; } }
}
