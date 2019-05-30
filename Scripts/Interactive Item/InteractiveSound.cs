using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveSound : InteractiveItem
{
    [TextArea(3, 10)]  //顯示文字框
    [SerializeField]
    private string _infoText = null;  //要顯示的項目文字
    [TextArea(3, 10)]
    [SerializeField]
    private string _activatedText = null;  //要顯示的互動文字
    [SerializeField]
    private float _activatedTextDuration = 3.0f;  //顯示的時間
    [SerializeField]
    private AudioCollection _audioCollection = null;  //聲音集合
    [SerializeField]
    private int _bank = 0;  //使用的聲音清單

    private IEnumerator _coroutine = null;
    private float _hideActivatedTextTime = 0.0f;  //隱藏互動文字的時間

    public override string GetText()
    {
        if(_coroutine != null || Time.time < _hideActivatedTextTime)
        {
            return _activatedText;
        }
        else
        {
            return _infoText;
        }
    }

    public override void Activate(CharacterManager characterManager)
    {
        if(_coroutine == null)
        {
            _hideActivatedTextTime = Time.time + _activatedTextDuration;
            _coroutine = DoActivation();
            StartCoroutine(_coroutine);
        }
    }

    private IEnumerator DoActivation()
    {
        if(_audioCollection == null || AudioManager.instance == null)
        {
            yield break;
        }

        AudioClip clip = _audioCollection[_bank];  //獲得音樂集合
        if(clip == null)
        {
            yield break;
        }

        AudioManager.instance.PlayOneShotSound(_audioCollection.audioGroup, clip, transform.position,
                                               _audioCollection.volume, _audioCollection.spatialBlend, _audioCollection.priority);  //撥放音效
        yield return new WaitForSeconds(clip.length);  //等音樂播完

        _coroutine = null;  //清除協程 
    }
}
