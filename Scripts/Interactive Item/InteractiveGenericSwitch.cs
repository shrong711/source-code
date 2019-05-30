using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimatorParameterType  
{
    Trigger,
    Bool,
    Int,
    Float,
    String
}

[System.Serializable]
public class AnimatorParameter
{
    public AnimatorParameterType Type = AnimatorParameterType.Bool;
    public string Name;
    public string Value;
}

[System.Serializable]
public class AnimatorConfigurator
{
    [SerializeField]
    public Animator Animator;
    [SerializeField]
    public List<AnimatorParameter> AnimatorParams = new List<AnimatorParameter>();
}

public class InteractiveGenericSwitch : InteractiveItem
{
    [Header("Game State Management")]
    [SerializeField]
    protected List<GameState> _requiredStates = new List<GameState>();  //請求狀態
    [SerializeField]
    protected List<GameState> _activateStates = new List<GameState>();  //活動狀態
    [SerializeField]
    protected List<GameState> _deactivateStates = new List<GameState>();  //停止狀態

    [Header("Message")]
    [TextArea(3, 10)]
    [SerializeField]
    protected string _stateNoSetText = "";  //沒有設置狀態時 所顯示的文字
    [TextArea(3, 10)]
    [SerializeField]
    protected string _stateSetText = "";  //請求狀態文字
    [TextArea(3, 10)]
    [SerializeField]
    protected string _ObjectActiveText = "";  //活動狀態文字

    [Header("Activation Parameters")]
    [SerializeField]
    protected float _activationDelay = 1.0f;  //啟用的延遲時間
    [SerializeField]
    protected float _deactivationDelay = 1.0f;  //停用的延遲時間
    [SerializeField]
    protected AudioCollection _activationSounds;
    [SerializeField]
    protected AudioSource _audioSource;
    [SerializeField]
    protected SliderDoor1 _sliderDoor;

    [Header("Operating Mode")]
    [SerializeField]
    protected bool _startActivated = false;  //是否活動
    [SerializeField]
    protected bool _canToggle = false;  //是否重複互動

    [Header("Configurable Entities")]
    [SerializeField]
    protected List<AnimatorConfigurator> _animations = new List<AnimatorConfigurator>();
    [SerializeField]
    protected List<MaterialController> _materialControllers = new List<MaterialController>();
    [SerializeField]
    protected List<GameObject> _objectActivators = new List<GameObject>();
    [SerializeField]
    protected List<GameObject> _objectDeactivators = new List<GameObject>();

    protected IEnumerator _coroutine;
    protected bool _activated = false;
    protected bool _firstUse = false;

    protected override void Start()
    {
        base.Start();  //先呼叫基底方法 註冊

        for (int i = 0; i < _materialControllers.Count; i++)
        {
            if(_materialControllers[i] != null)
            {
                _materialControllers[i].OnStart();  //註冊材質球 並備份
            }
        }

        for(int i = 0; i < _objectActivators.Count; i++)  //找到所有打開開關時所要活動的對象
        {
            if(_objectActivators[i] != null)
            {
                _objectActivators[i].SetActive(false);  //關閉
            }
        }

        for(int i = 0; i < _objectDeactivators.Count; i++)
        {
            if(_objectDeactivators[i] != null)
            {
                _objectDeactivators[i].SetActive(true);
            }
        }

        if (_startActivated)
        {
            Activate(null);
            _firstUse = false;
        }
    }

    public override string GetText()
    {
        if (!enabled)  //如果沒有資料庫 或是 切換開關沒有開啟 
        {
            return string.Empty;//回傳空字串
        }

        if (_activated)  //如果已經啟動
        {
            return _ObjectActiveText;  //回傳啟動的文字
        }

        bool requiredStates = AreRequiredStatesSet();  //檢查設置的所有狀態 是否可以活動
        if (!requiredStates)
        {
            return _stateNoSetText;
        }
        else
        {
            return _stateSetText;
        }
    }

    protected bool AreRequiredStatesSet()  //
    {
        ApplicationManager appManager = ApplicationManager.instance;
        if (appManager == null)
        {
            return false;
        }

        for(int i = 0; i < _requiredStates.Count; i++)
        {
            GameState state = _requiredStates[i];
            string result = appManager.GetGameState(state.Key);  //字典裡是否有正確狀態
            if(string.IsNullOrEmpty(result) || !result.Equals(state.Value))
            {
                return false;
            }
        }
        return true;
    }

   // [PunRPC]
    protected void SetActivationStates()
    {
        ApplicationManager appManager = ApplicationManager.instance;
        if(appManager == null)
        {
            return;
        }

        if (_activated)
        {
            foreach(GameState state in _activateStates)
            {
                appManager.SetGameState(state.Key, state.Value);  //設置遊戲狀態
            }
        }
        else
        {
            foreach(GameState state in _deactivateStates)
            {
                appManager.SetGameState(state.Key, state.Value);  //設置遊戲狀態
            }
        }
    }

    [PunRPC]
    protected void actice()
    {
        ApplicationManager appManager = ApplicationManager.instance;
        if (appManager == null)
        {
            return;
        }
        //如果與一開始狀態不一樣 且不是在切換模式 就不能再打開或關閉開關
        if (_firstUse && !_canToggle)  //如果不能切換開關
         {
             return;  
         }
         if (!_activated)
         {
             bool requiredStates = AreRequiredStatesSet();
             if (!requiredStates)
             {
                 return;
             }
         }
          //開關以切換
         _activated = !_activated;
         _firstUse = true;

        if (_activationSounds != null && _activated)
        {
            AudioClip clipToPlay = _activationSounds[0];
            if (clipToPlay == null)
            {
                return;
            }

            if (_audioSource != null)
            {
                _audioSource.clip = clipToPlay;
                _audioSource.volume = _activationSounds.volume;
                _audioSource.spatialBlend = _activationSounds.spatialBlend;
                _audioSource.priority = _activationSounds.priority;
                _audioSource.outputAudioMixerGroup = AudioManager.instance.GetAudioGroupFromTrackName(_activationSounds.audioGroup);
                _audioSource.Play();
            }
        }

        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
        _coroutine = DoDelayedActivation();
        StartCoroutine(_coroutine);
    }

    public override void Activate(CharacterManager characterManager)
    {

        //如果與一開始狀態不一樣 且不是在切換模式 就不能再打開或關閉開關
        /* if(_firstUse && !_canToggle)  //如果不能切換開關
         {
             return;  
         }
         if (!_activated)
         {
             bool requiredStates = AreRequiredStatesSet();
             if (!requiredStates)
             {
                 return;
             }
         }
          //開關以切換
         _activated = !_activated;
         _firstUse = true;*/
        actice();
        photonView.RPC("actice", PhotonTargets.Others);
       
    }

    protected virtual IEnumerator DoDelayedActivation()
    {
        foreach (AnimatorConfigurator configurator in _animations)  //搜尋每個動畫配置器
        {
            if(configurator != null)  //如果有
            {
                foreach(AnimatorParameter param in configurator.AnimatorParams)  //找到每個動畫配置器裡面的動畫參數
                {
                    switch (param.Type)  //參數型態
                    {
                        case AnimatorParameterType.Bool:
                            bool boolean = bool.Parse(param.Value);  //把參數轉成布林值
                            configurator.Animator.SetBool(param.Name, _activated ? boolean : !boolean);  //設置
                            break;
                    }
                }
            }
        }
        yield return new WaitForSeconds(_activated ? _activationDelay : _deactivationDelay);
        // photonView.RPC("SetActivationStates", PhotonTargets.All);  //設置活動 / 非活動狀態
        SetActivationStates();

        if(_activationSounds != null && !_activated)
        {
            AudioClip clipToPlay = _activationSounds[1];

            if(_audioSource != null && clipToPlay)
            {
                _audioSource.clip = clipToPlay;
                _audioSource.volume = _activationSounds.volume;
                _audioSource.spatialBlend = _activationSounds.spatialBlend;
                _audioSource.priority = _activationSounds.priority;
                _audioSource.outputAudioMixerGroup = AudioManager.instance.GetAudioGroupFromTrackName(_activationSounds.audioGroup);
                _audioSource.Play();
            }
        }

        if(_objectActivators.Count > 0)  //如果有活動物件
        {
            for(int i = 0; i < _objectActivators.Count; i++)  //找到每個物件
            {
                if (_objectActivators[i])
                {
                    _objectActivators[i].SetActive(_activated);  //開啟
                }
            }

            if (_objectActivators[0].activeInHierarchy)
            {
                _sliderDoor.ZombiesEvent();
            }
        }

        if(_objectDeactivators.Count > 0)  //如果有停止活動的物件
        {
            for (int i = 0; i < _objectDeactivators.Count; i++)  //找到每個物件
            {
                if (_objectDeactivators[i])
                {
                    _objectDeactivators[i].SetActive(!_activated);  //關閉
                }
            }
        }

        for(int i = 0; i < _materialControllers.Count; i++)
        {
            if(_materialControllers[i] != null)
            {
                _materialControllers[i].Activate(_activated);
            }
        }
    } 
}
