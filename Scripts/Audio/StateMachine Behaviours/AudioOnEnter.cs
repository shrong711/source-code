﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioOnEnter : StateMachineBehaviour
{
    [SerializeField]
    AudioCollection _audioCollection = null;
    [SerializeField]
    int _bank = 0;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)  //進入動畫第一針時
    {
        if(AudioManager.instance == null || _audioCollection == null)
        {
            return;
        }

        AudioManager.instance.PlayOneShotSound(_audioCollection.audioGroup, _audioCollection[_bank], animator.transform.position, 
                                               _audioCollection.volume, _audioCollection.spatialBlend, _audioCollection.priority);  //撥放聲音
    }
}
