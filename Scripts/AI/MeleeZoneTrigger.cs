using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeZoneTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        AIStateMachine machine = GameSceneManager.instance.GetAIStateMachine(col.GetInstanceID());  //取得碰撞器ID
        if (machine)
        {
            machine.inMeleeRange = true;  
        }
    }

    void OnTriggerExit(Collider col)
    {
        AIStateMachine machine = GameSceneManager.instance.GetAIStateMachine(col.GetInstanceID());  //取得離開的碰撞器ID
        if (machine)
        {
            machine.inMeleeRange = false;
        }
    }
}
