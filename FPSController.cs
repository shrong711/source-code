using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlayerMoveStatus  //玩家移動狀態
{
    NotMoving,  //沒有移動
    Crouching,  //蹲下
    Walking,  //走路
    Running,  //跑步
    NotGrounded,  //不再地上
    Landing  //回到地上
}

public enum CurveControlledBobCallbackType  //是否分別由 xHead yHead 事件處理曲線
{
    Horizontal,  //水平事件
    Vertical  //垂直事件
}

public delegate void CurveControlledBobCallback();  //處理列舉中的事件

[System.Serializable]
public class CurveControlledBobEvent
{
    public float Time = 0.0f;  //儲存時間
    public CurveControlledBobCallback Function = null;  //委派引用
    public CurveControlledBobCallbackType Type = CurveControlledBobCallbackType.Vertical;  //只使用垂直事件
}

[System.Serializable]
public class CurveControlledBob  //頭部搖擺
{
    [SerializeField]
    AnimationCurve _bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                  new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                  new Keyframe(2f, 0f));  //正弦波動畫曲線
    [SerializeField]
    float _horizontalMultiplier = 0.01f;  //增加水平值 控制水平頭部運動
    [SerializeField]
    float _verticalMultiplier = 0.02f;  //增加垂直值 控制垂直頭部運動
    [SerializeField]
    float _verticaltoHorizontalSpeedRatio = 2.0f;  //垂直與水平速度比 (遊戲速度的兩倍執行頭部搖擺)
    [SerializeField]
    float _baseInterval = 1.0f;  //控制速度往上或往下

    private float _prevXPlayHead;  //水平速度之前的位置
    private float _prevYPlayHead;  //垂直速度之前的位置
    private float _xPlayHead;  //目前水平速度
    private float _yPlayHead;  //目前垂直速度
    private float _curveEndTime;  //最後一針的時間
    private List<CurveControlledBobEvent> _events = new List<CurveControlledBobEvent>();  

    public void Initialize()
    {  //紀錄曲線時間長度
        _curveEndTime = _bobcurve[_bobcurve.length - 1].time;  //取得最後一針的時間
        _xPlayHead = 0.0f;  //重置
        _yPlayHead = 0.0f;  //重置
        _prevXPlayHead = 0.0f;
        _prevYPlayHead = 0.0f;
    }

    public void RegisterEventCallback(float time, CurveControlledBobCallback function, CurveControlledBobCallbackType type)  //註冊正確時間的事件
    {
        CurveControlledBobEvent ccbeEvent = new CurveControlledBobEvent();
        ccbeEvent.Time = time;  //儲存時間
        ccbeEvent.Function = function;  //事件
        ccbeEvent.Type = type;  //垂直運動
        _events.Add(ccbeEvent);  //加入清單
        _events.Sort(
                      delegate (CurveControlledBobEvent t1, CurveControlledBobEvent t2)
                    {
                        return (t1.Time.CompareTo(t2.Time));
                    }
                    );  //使用匿名方法 排序時間
    }

    public Vector3 GetVectorOffset(float speed)  //取得相機偏移量
    {
        _xPlayHead += (speed * Time.deltaTime) / _baseInterval;  //取得水平速度
        _yPlayHead += ((speed * Time.deltaTime) / _baseInterval) * _verticaltoHorizontalSpeedRatio;  //垂直速度 水平的兩倍

        if(_xPlayHead > _curveEndTime)
        {
            _xPlayHead -= _curveEndTime;  //回到剛開始的速度
        }

        if(_yPlayHead > _curveEndTime)
        {
            _yPlayHead -= _curveEndTime;  //回到剛開始的速度
        }

        for (int i = 0; i < _events.Count; i++)
        {
            CurveControlledBobEvent ev = _events[i];
            if (ev != null)
            {
                if (ev.Type == CurveControlledBobCallbackType.Vertical)
                {
                    if ((_prevYPlayHead < ev.Time && _yPlayHead >= ev.Time) ||
                        (_prevYPlayHead > _yPlayHead && (ev.Time > _prevYPlayHead || ev.Time <= _yPlayHead)))
                    {
                        ev.Function();
                    }
                }
                else
                {
                    if ((_prevXPlayHead < ev.Time && _xPlayHead >= ev.Time) ||
                        (_prevXPlayHead > _xPlayHead && (ev.Time > _prevXPlayHead || ev.Time <= _xPlayHead)))
                    {
                        ev.Function();
                    }
                }
            }
        }

        float xPos = _bobcurve.Evaluate(_xPlayHead) * _horizontalMultiplier;  //評估水平曲線在每一針的值
        float yPos = _bobcurve.Evaluate(_yPlayHead) * _verticalMultiplier;  //評估垂直曲線在每一針的值

        _prevXPlayHead = _xPlayHead;
        _prevYPlayHead = _yPlayHead;

        return new Vector3(xPos, yPos, 0f);  //回傳正確的擺動到相機
    }
}

[RequireComponent(typeof(CharacterController))]  //添加腳色控制器
public class FPSController : Photon.PunBehaviour
{
    [SerializeField]
    private AudioCollection _footSteps = null;  //腳步聲集合  
    [SerializeField]
    private float _crouchAttenuation = 0.2f;  //蹲下的聲音
    [SerializeField]
    private float _walkSpeed = 2.0f;  //走路速度
    [SerializeField]
    private float _runSpeed = 4.5f;  //跑步速度
    [SerializeField]
    private float _jumpSpeed = 7.5f;  //跳躍
    [SerializeField]
    private float _crouchSpeed = 1.0f;  //蹲下移動速度
    [SerializeField]
    private float _staminaDepletion = 5.0f;  //每秒失去的耐力值
    [SerializeField]
    private float _staminaRecovery = 10;  //增加耐力值
    [SerializeField]
    private float _stickToGroundForce = 5.0f;  //下壓力(往地面方向)
    [SerializeField]
    private float _gravityMultiplier = 2.5f;  //重力倍增(標準重力2.5倍)
    [SerializeField]
    private float _runStepLenthen = 0.75f;  //減緩跑步速度
    [SerializeField]
    private CurveControlledBob _headBob = new CurveControlledBob();  //實體化
    
    public GameObject _flashLight;  //手電筒
    [SerializeField]
    private bool _flashlightOnAtStart = true;  //一開始是否啟用手電筒

    [SerializeField]
    private UnityStandardAssets.Characters.FirstPerson.MouseLook _mouseLook;  //使用Unity標準資產 使用滑鼠控制相機

    private Camera _camera = null;  //相機
    private bool _jumpButtonPressed = false;  //是否按下跳躍按鈕
    private Vector2 _inputVector = Vector2.zero;  //輸入的X Y軸直(1 ~ -1)
    private Vector3 _moveDirection = Vector3.zero;  //儲存移動向量
    private bool _previouslyGrounded = false;  //是否在空中
    private bool _isWalking = true;  //是否走路
    private bool _isJumping = false;   //是否跳躍
    private bool _isCrouching = false;  //是否蹲下
    private float _fallingTimer = 0.0f;  //在空中的時間
    private Vector3 _localSpaceCameraPos = Vector3.zero;  //相機位置
    private float _controllerHeight = 0.0f;  //
    private float _stamina = 100;  //耐力
    private bool _freezeMovement = false;  //凍結移動
    private CharacterController _characterController = null;  //角色控制器
    private PlayerMoveStatus _movementStatus = PlayerMoveStatus.NotMoving;  //默認為不移動狀態

    public PlayerMoveStatus movementStatus { get { return _movementStatus; } }  //回傳移動狀態
    public float walkSpeed { get { return _walkSpeed; } }  //回傳走路速度
    public float runSpeed { get { return _runSpeed; } }  //回傳跑步速度

    float _dragMultiplier = 1.0f;  //碰到殭屍後速度
    float _dragMultiplierLimit = 1.0f;  //
    [SerializeField][Range(0.0f, 1.0f)]
    float _npcStickiness = 0.5f;

    private Animator animator;
    private Vector3 velocity = Vector3.zero;

    public float dragMultiplierLimit
    {
        get { return _dragMultiplierLimit; }
        set { _dragMultiplierLimit =Mathf.Clamp01(value); }
    }

    public float dragMultiplier
    {
        get { return _dragMultiplier; }
        set { _dragMultiplier = Mathf.Min(value, _dragMultiplierLimit); }
    }

    public CharacterController characterController
    {
        get { return _characterController; }
    }

    public bool freezeMovement
    {
        get { return _freezeMovement; }
        set { _freezeMovement = value; }
    }

    public float stamina
    {
        get { return _stamina; }
    }

    protected void Start()
    {
        _characterController = GetComponent<CharacterController>();  //取得角色控制器
        _controllerHeight = _characterController.height;  //取得人物高度
        _camera = Camera.main;  //獲得相機
        _localSpaceCameraPos = _camera.transform.localPosition;  //取得本地位置
        _movementStatus = PlayerMoveStatus.NotMoving;  //初始化為不移動狀態
        _fallingTimer = 0.0f;  //重置計時器
        _mouseLook.Init(transform, _camera.transform);  //取得角色控制器位置 相機位置
        _headBob.Initialize();  //初始化物件
        _headBob.RegisterEventCallback(1.5f, PlayFootStepSound, CurveControlledBobCallbackType.Vertical);

        animator = GetComponent<Animator>();

        if (_flashLight)
        {
            _flashLight.SetActive(_flashlightOnAtStart);
        }

        if (!photonView.isMine)
        {
            _characterController.enabled = false;
            enabled = false;
        }
    }

    protected void Update()
    {
        if (_characterController.isGrounded)  //如果在地上
        {
            _fallingTimer = 0.0f;  //計時器為0
        }
        else  //否則在空中
        {
            _fallingTimer += Time.deltaTime;  //開始計時
        }

        if(Time.timeScale > Mathf.Epsilon)  //如果時間不為0  
        {
            _mouseLook.LookRotation(transform, _camera.transform);  //依據滑鼠移動 旋轉玩家與相機
        }

        if (Input.GetButtonDown("Flashlight"))
        {
            if (_flashLight)
            {
                Debug.Log("玩家:" + transform.root.GetComponent<PhotonView>().viewID);

            }
            photonView.RPC("Flash", PhotonTargets.All);
            
        }

        if (!_jumpButtonPressed && !_isCrouching)
        {
            _jumpButtonPressed = Input.GetButtonDown("Jump");
        }

        if (Input.GetButtonDown("Crouch"))  //按下蹲下鍵
        {
            _isCrouching = !_isCrouching;
            _characterController.height = _isCrouching == true ? _controllerHeight / 2.0f : _controllerHeight;  //玩家高度為一半
        }

        if(!_previouslyGrounded && _characterController.isGrounded)  //如果剛落在地上
        {
            if(_fallingTimer > 0.5f)
            {
                //播放剛落地的聲音
            }
            _moveDirection.y = 0.0f;  //在地上Y為0
            _isJumping = false;  //不再跳躍
            _movementStatus = PlayerMoveStatus.Landing;  //切換為剛落地狀態
        }
        else if (!_characterController.isGrounded)  //如果不再地上
        {
            _movementStatus = PlayerMoveStatus.NotGrounded;  //切換為不再地上狀態
        }
        else if(_characterController.velocity.sqrMagnitude < 0.01f)  //檢查速度
        {
            _movementStatus = PlayerMoveStatus.NotMoving;  //沒有移動
        }
        else if (_isCrouching)  //如果蹲下
        {
            _movementStatus = PlayerMoveStatus.Crouching;  //切換蹲下狀態
        }
        else if (_isWalking)  //如果再走路
        {
            _movementStatus = PlayerMoveStatus.Walking;  //切換走路狀態
        }
        else
        {
            _movementStatus = PlayerMoveStatus.Running;  //跑步狀態
        }

        _previouslyGrounded = _characterController.isGrounded;  //

        if(_movementStatus == PlayerMoveStatus.Running)
        {
            _stamina = Mathf.Max(_stamina - _staminaDepletion * Time.deltaTime, 0.0f);  //計算耐力值 但不為0
        }
        else
        {
            _stamina = Mathf.Min(_stamina + _staminaRecovery * Time.deltaTime, 100.0f);  //恢復耐力值 但不超過100
        }

        _dragMultiplier = Mathf.Min(_dragMultiplier + Time.deltaTime, _dragMultiplierLimit);

        MoveControl();
        JumpControl();
        Reloading();
    }

    [PunRPC]
    void Flash()
    {
        _flashLight.SetActive(!_flashLight.activeSelf);
    }

    protected void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");  //輸入的X值
        float vertical = Input.GetAxis("Vertical");  //輸入的Y值
       // bool waswalking = _isWalking;  //是否再走路
        _isWalking = !Input.GetKey(KeyCode.LeftShift);  //如果沒有按住LeftShift isWalking = true
        float speed = _isCrouching ? _crouchSpeed : _isWalking ? _walkSpeed : Mathf.Lerp(_walkSpeed, _runSpeed, _stamina / 100.0f);  //設置速度 如果是走路狀態 設走路速度給它 如果不是給跑步速度(根據耐力值變化)
        _inputVector = new Vector2(horizontal, vertical);  //儲存輸入的XY向量

        if(_inputVector.sqrMagnitude > 1)  //如果輸入的向量平方大於1
        {
            _inputVector.Normalize();  //讓向量回到1
        }

        Vector3 desiredMove = transform.forward * _inputVector.y + transform.right * _inputVector.x;  //移動的方向

        RaycastHit hitInfo;  //儲存射線訊息
        if(Physics.SphereCast(transform.position, _characterController.radius, Vector3.down, out hitInfo, _characterController.height / 2f, 1))  //如果在斜坡上移動
        {  //獲得表面法線 
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;  //把移動向量投射到平面上
        }

        _moveDirection.x = !_freezeMovement ? desiredMove.x * speed * _dragMultiplier : 0.0f;  //設置正確的移動速度 
        _moveDirection.z = !_freezeMovement ? desiredMove.z * speed * _dragMultiplier : 0.0f;

        if (_characterController.isGrounded)  //如果玩家在地上
        {
            _moveDirection.y = -_stickToGroundForce;  //設置向下的力 讓玩家保持在地面上

            if (_jumpButtonPressed)  //如果按下跳躍鍵
            {
               // _moveDirection.y = _jumpSpeed;  //跳躍
                _jumpButtonPressed = false;  
                _isJumping = true;
                //撥放跳躍聲音
            }
        }
        else
        {
            _moveDirection += Physics.gravity * _gravityMultiplier * Time.fixedDeltaTime;  //給予重力 回到地上
        }

        _characterController.Move(_moveDirection * Time.fixedDeltaTime);  //玩家移動

        Vector3 speedXZ = new Vector3(_characterController.velocity.x, 0.0f, _characterController.velocity.z);

        if(speedXZ.magnitude > 0.01f)  //如果在移動
        {
            _camera.transform.localPosition = _localSpaceCameraPos + _headBob.GetVectorOffset(speedXZ.magnitude * (_isCrouching || _isWalking ? 1.0f : _runStepLenthen));  //設置相機位置
        }
        else
        {
            _camera.transform.localPosition = _localSpaceCameraPos;  //如果沒移動 回到原來的位置
        }
    }

    void PlayFootStepSound()
    {
       if(AudioManager.instance != null && _footSteps != null)
        {
            AudioClip soundToPlay;  //要撥放的聲音
            if (_isCrouching)  //如果是蹲下狀態
            {
                soundToPlay = _footSteps[1];  //蹲下聲音
            }
            else
            {
                soundToPlay = _footSteps[0];  //隨機撥放
            }
            AudioManager.instance.PlayOneShotSound("Player", soundToPlay, transform.position,
                                                   _isCrouching ? _footSteps.volume * _crouchAttenuation : _footSteps.volume, 
                                                   _footSteps.spatialBlend, _footSteps.priority);  //撥放音校
        }
    }

    public void DoStickiness()
    {
        _dragMultiplier = 1.0f - _npcStickiness;  //依據設定值降低速度
    }

   /* void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)  //當Photon對一個組件進行同步時會呼叫
    {
        if (stream.isWriting)
        {
            stream.SendNext(_flashLight);  //往同步串流中寫入一筆資料寫入的順序會影響讀取的順序

        }
        else  //ReciveNext()  從同步串流中讀取一筆資料 資料會按照寫入的順序被讀出 因為沒有型別安全 需要轉型才能使用
        {
            _flashLight = (GameObject)stream.ReceiveNext();
        }
    }*/

    // 跳躍動畫
    void JumpControl()
    {
        if (Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger("Jump");
        }
    }

    // 移動動畫
    void MoveControl()
    {
        velocity = transform.InverseTransformDirection(_characterController.velocity);
        animator.SetFloat("Forward", velocity.z);
        animator.SetFloat("Right", velocity.x);
    }

    void Reloading()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            animator.SetTrigger("Reload");
        }
    }
}
