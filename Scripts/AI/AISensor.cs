using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISensor : MonoBehaviour
{  //透過AIStateMachine的事件方法獲得進入感測器範圍內的威脅
    private AIStateMachine _parentStateMachine = null;
    public AIStateMachine parentStateMachine { set { _parentStateMachine = value; } }  

    void OnTriggerEnter(Collider col)  //進入
    {
        if(_parentStateMachine != null)
        {
            _parentStateMachine.OnTriggerEvent(AITriggerEventType.Enter, col);  
        }
    }

    void OnTriggerStay(Collider col)  //停留
    {
        if (_parentStateMachine != null)
        {
            _parentStateMachine.OnTriggerEvent(AITriggerEventType.Stay, col);
        }
    }

    void OnTriggerExit(Collider col)  //離開
    {
        if (_parentStateMachine != null)
        {
            _parentStateMachine.OnTriggerEvent(AITriggerEventType.Exit, col);
        }
    }

}
