using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public enum AIStateType  //殭屍狀態
{
    None,  
    Idle,  //閒置
    Alerted,  //警戒
    Patrol,  //巡邏
    Attack,  //攻擊
    Feeding,  //飢餓
    Pursuit,  //追擊
    Dead  //死亡
}

public enum AITargetType  //目標類型
{
    None,
    Waypoint,  //航點
    Visual_Player,  //玩家
    Visual_Light,  //光線
    Visual_Food,  //食物(屍體)
    Audio  //聲音
}

public enum AITriggerEventType  //觸發事件
{
    Enter,  //進入
    Stay,  //停留
    Exit  //離開
}

public enum AIBoneAlignmentType  //判斷臀部骨頭的前進方向
{
    XAxis,  //X軸
    YAxis,  //Y軸
    ZAxis,  //Z軸
    XAxisInverted,  //X反轉
    YAxisInverted,  //Y反轉
    ZAxisInverted  //Z反轉
}

public struct AITarget  //儲存目標類型訊息
{
    private AITargetType _type;  //目標類型
    private Collider _collider;  //碰撞體(碰到什麼)
    private Vector3 _position;  //正確世界座標位置
    private float _distance;  //與玩家距離
    private float _time;  //目標上次ping的時間 (時間假設是5秒 當5秒過後殭屍會放棄當前所做的事)

    public AITargetType type { get { return _type; } }  //回傳目標類型
    public Collider collider { get { return _collider; } }  //回傳碰撞體
    public Vector3 position { get { return _position; } }  //回傳位置
    public float distance { get { return _distance; } set { _distance = value; } }  //回傳正確的距離
    public float time { get { return _time; } }  //回傳時間

    public void Set(AITargetType t, Collider c, Vector3 p, float d)  //設置值
    {
        _type = t;  //設置傳進來的類別
        _collider = c;  //設置傳進來的碰撞體
        _position = p;  //設置傳進來的位置
        _distance = d;  //設置傳進來的距離
        _time = Time.time;  //設置時間(unity)
    }

    public void Clear()  //重置所有值 
    {
        _type = AITargetType.None;  //重置為默認狀態
        _collider = null;  //重製碰撞體為空值
        _position = Vector3.zero;  //重置座標為0
        _time = 0.0f;  //重置時間為0
        _distance = Mathf.Infinity;  //重置距離為無限遠
    }
}

public abstract class AIStateMachine : Photon.PunBehaviour  //抽象類別 封裝方法
{
    public AITarget VisualThreat = new AITarget();  //儲存殭屍看到的威脅(玩家 光線)  避免狀態轉換遺失訊息
    public AITarget AudioThreat = new AITarget();  //儲存殭屍聽到的威脅(槍聲 )
    protected AITarget _target = new AITarget();  //儲存目標訊息(航點 玩家)
    protected int _rootPositionRefCount = 0;  //blend tree動畫位置 默認為0 不做任何動作
    protected int _rootRotationRefCount = 0;  //blend tree動畫旋轉 默認為0
    protected bool _isTargetReached = false;  //是否到達目標
    protected List<Rigidbody> _bodyParts = new List<Rigidbody>();  //儲存殭屍身體上的布娃娃骨頭鋼體
    protected int _aiBodyPartLayer = -1;  //布娃娃圖層

    protected Dictionary<string, bool> _animLayerActive = new Dictionary<string, bool>();  //管理動畫圖層活動狀態 

    [SerializeField]
    protected AIStateType _currentStateType = AIStateType.Idle;  //默認列舉狀態
    [SerializeField]
    protected Transform _rootBone = null;  //臀部骨頭(父骨頭)
    [SerializeField]
    protected AIBoneAlignmentType _rootBoneAlignment = AIBoneAlignmentType.ZAxis;  //判斷骨頭前進位置 默認為Z軸
    [SerializeField]
    protected SphereCollider _targetTrigger = null;  //目標球形碰撞體 默認為空值 (當殭屍進入它的目標類型半徑時 設置一個球形碰撞器)
    [SerializeField]
    protected SphereCollider _sensorTrigger = null;  //感測器球型碰撞體 默認為空值 (偵測殭屍範圍內的威脅)
    [SerializeField]
    protected AIWayPointNetwork _waypointNetwork = null;  //航點
    [SerializeField]
    protected bool _randomPatrol = false;  //隨機巡邏
    [SerializeField]
    protected int _currentWaypoint = -1;  //當前航點
    [SerializeField][Range(0, 15)]  //滑塊
    protected float _stoppingDistance = 1.0f;  //停止距離

    protected ILayeredAudioSource _layeredAudioSource = null;
    
    protected Animator _animator = null;  //動畫控制器
    protected NavMeshAgent _navAgent = null;  //AI
    protected Collider _collider = null;  //殭屍身上碰撞體
    protected Transform _transform = null;  //位置
    protected AIState _currentState = null;  //默認殭屍狀態
    protected Dictionary<AIStateType, AIState> _states = new Dictionary<AIStateType, AIState>();  //狀態字典

    public bool isTargetReached { get { return _isTargetReached; } }  //到達目標回傳
    public bool inMeleeRange { get; set; }  //碰撞範圍 碰撞後回傳狀態
    public Animator animator { get { return _animator; } }  //回傳動畫控制器
    public NavMeshAgent navAgent { get { return _navAgent; } }  //回傳AI

    public Vector3 sensorPosition  //感測器位置
    {
        get
        {
            if(_sensorTrigger == null)
            {
                return Vector3.zero;
            }

            Vector3 point = _sensorTrigger.transform.position;  //觸發感測器的位置
            point.x += _sensorTrigger.center.x * _sensorTrigger.transform.lossyScale.x;  //依據比例縮放
            point.y += _sensorTrigger.center.y * _sensorTrigger.transform.lossyScale.y;
            point.z += _sensorTrigger.center.z * _sensorTrigger.transform.lossyScale.z;
            return point;  //返回碰撞器的中心點
        }
    }

    public float sensorRadius  //感測器半徑
    {
        get
        {
            if(_sensorTrigger == null)
            {
                return 0.0f;
            }

            float radius = Mathf.Max(_sensorTrigger.radius * _sensorTrigger.transform.lossyScale.x, //最大值計算半徑
                                     _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.y);
            return Mathf.Max(radius, _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.z);
        }
    }

    public bool useRootPosition { get { return _rootPositionRefCount > 0; } }  //測試是否可以應用位置
    public bool useRootRotation { get { return _rootRotationRefCount > 0; } }  //測試是否可以應用旋轉

    public AITargetType targetType { get { return _target.type; } }  //取得目標類型
    public Vector3 targetPosition { get { return _target.position; } }  //目標位置
    public int targetColliderID  //獲得目標碰撞器
    {
        get
        {
            if (_target.collider)
            {
                return _target.collider.GetInstanceID();
            }
            else
            {
                return -1;
            }
        }
    }

    public void SetLayerActive(string layerName, bool active)
    {
        _animLayerActive[layerName] = active;
        if(active == false && _layeredAudioSource != null)
        {
            _layeredAudioSource.Stop(_animator.GetLayerIndex(layerName));
        }
    }

    public bool IsLayerActive(string layerName)
    {
        bool result;
        if(_animLayerActive.TryGetValue(layerName, out result))
        {
            return result;
        }
        return false;
    }

    public bool PlayAudio(AudioCollection clipPool, int bank, int layer, bool looping = true)
    {
        if(_layeredAudioSource == null)
        {
            return false;
        }
        return _layeredAudioSource.Play(clipPool, bank, layer, looping);
    }

    public void StopAudio(int layer)
    {
        if(_layeredAudioSource != null)
        {
            _layeredAudioSource.Stop(layer);
        }
    }

    public void MuteAudio(bool mute)
    {
        if(_layeredAudioSource != null)
        {
            _layeredAudioSource.Mute(mute);
        }
    }

    protected virtual void Awake()  
    {
        _transform = transform;  //取得位置
        _animator = GetComponent<Animator>();  //取得動畫控制器
        _navAgent = GetComponent<NavMeshAgent>();  //取得AI
        _collider = GetComponent<Collider>();  //取得碰撞器

        AudioSource audioSource = GetComponent<AudioSource>();

        _aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");  //取得圖層索引

        if (GameSceneManager.instance != null)  //如果遊戲場景管理器不為空值
        {  //註冊State Machine 存到場景資料庫
            if (_collider)  //如果有碰撞器
            {
                GameSceneManager.instance.RegisterAIStateMachine(_collider.GetInstanceID(), this);  //將這個碰撞器加入字典
            }

            if (_sensorTrigger)
            {
                GameSceneManager.instance.RegisterAIStateMachine(_sensorTrigger.GetInstanceID(), this);  //將這個碰撞器加入字典
            }
        }

        if(_rootBone != null)
        {
            Rigidbody[] bodies = _rootBone.GetComponentsInChildren<Rigidbody>();  //找到所有骨頭的鋼體

            foreach(Rigidbody bodyPart in bodies)  //檢查每一個骨頭剛體
            {
                if(bodyPart != null && bodyPart.gameObject.layer == _aiBodyPartLayer)  //圖層索引為_aiBodyPartLayer的骨頭
                {
                    _bodyParts.Add(bodyPart);  //添加到清單
                    GameSceneManager.instance.RegisterAIStateMachine(bodyPart.GetInstanceID(),this);  //儲存實例ID及所屬殭屍
                }
            }
        }

        if(_animator && audioSource && AudioManager.instance)
        {
            _layeredAudioSource = AudioManager.instance.RegisterLayeredAudioSource(audioSource, _animator.layerCount);
        }
       
    }

    protected virtual void Start()  
    {
        if(_sensorTrigger != null)
        {
            AISensor script = _sensorTrigger.GetComponent<AISensor>();  
            if(script != null)
            {
                script.parentStateMachine = this;  //把這個值設給_parentStateMachine 
            }
        }

        AIState[] states = GetComponents<AIState>();  //取得這個物件所有狀態

        foreach(AIState state in states)  //搜尋所有狀態
        {
            if(state != null && !_states.ContainsKey(state.GetStateType()))  //如果狀態不為空值 且字典裡面沒有符合的狀態
            {
                _states[state.GetStateType()] = state;  //把目前的狀態加入字典裡
                state.SetStateMachine(this);  //傳入狀態
            }
        }

        if (_states.ContainsKey(_currentStateType))  //如果設置的狀態包含在字典裡
        {
            _currentState = _states[_currentStateType];  //將字典裡的狀態設給目前狀態
            _currentState.OnEnterState();  //進入新狀態
        }
        else
        {
            _currentState = null;
        }

        if (_animator)
        {
            AIStateMachineLink[] scripts = _animator.GetBehaviours<AIStateMachineLink>();  //從AIStateMachineLink中獲得所有行為
            foreach (AIStateMachineLink script in scripts)  
            {
                script.stateMachine = this;  //把所有行為設給stateMachine
            }
        }
    }

    public void SetStateOverride(AIStateType state)  //強制進入狀態
    {
        if (state != _currentStateType && _states.ContainsKey(state))  //如果傳進來的狀態不是目前狀態 && 狀態已經在字典裡
        {
            if(_currentState != null)
            {
                _currentState.OnExitState();  //離開狀態
            }
            _currentState = _states[state];  //獲得新狀態
            _currentStateType = state;  //獲得新狀態類型
            _currentState.OnEnterState();  //進入新狀態
        }
    }

    public Vector3 GetWaypointPosition(bool increment)
    {
        if(_currentWaypoint == -1)  //第一次調用
        {
            if (_randomPatrol)  //如果隨機巡邏
            {
                _currentWaypoint = Random.Range(0, _waypointNetwork.Waypoints.Count);  //依照航點列表 隨機
            }
            else //如果不是隨機巡邏
            {
                _currentWaypoint = 0;  //第一個航點
            }
        }
        else if (increment)  //如果不是第一次調用
        {
            NextWaypoint();  //下一個航點
        }

        if(_waypointNetwork.Waypoints[_currentWaypoint] != null)  //從航點清單中找新的航點
        {
            Transform newWaypoint = _waypointNetwork.Waypoints[_currentWaypoint]; //航點位置
            SetTarget(AITargetType.Waypoint, null, newWaypoint.position, Vector3.Distance(newWaypoint.position, transform.position));  //設置新的航點  
            return newWaypoint.position;         
        }
        return Vector3.zero;        
    }

    private void NextWaypoint()  //選擇新航點 從路徑中隨機選擇航點 或是依照順序
    {
        if (_randomPatrol && _waypointNetwork.Waypoints.Count > 1)  //如果是隨機巡邏 && 有航點
        {
            int oldWaypoint = _currentWaypoint;  //當前航點
            while (_currentWaypoint == oldWaypoint)  //持續生成航點 直到不是正確的航點
            {
                _currentWaypoint = Random.Range(0, _waypointNetwork.Waypoints.Count);  //生成隨機航點
            }
        }
        else  //如果不是隨機巡邏
        {
            _currentWaypoint = _currentWaypoint == _waypointNetwork.Waypoints.Count - 1 ? 0 : _currentWaypoint + 1;  //增加當前的航點
        }       
    }

    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d)  //設置正確的目標 並配置碰撞器
    {
        _target.Set(t, c, p, d);  //呼叫設置方法 設置目標訊息
        if(_targetTrigger != null)  //如果目標不為空值 
        {    //重新定位球型碰撞器 設置正確的半徑 位置 並開啟碰撞器
            _targetTrigger.radius = _stoppingDistance;  //把停止距離設給球型碰撞器的半徑
            _targetTrigger.transform.position = _target.position;  //目標位置給球形碰撞器
            _targetTrigger.enabled = true;  //開啟球形碰撞器
        }
    }

    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d, float s)  //OverLoad
    {
        _target.Set(t, c, p, d);
        if (_targetTrigger != null)
        {
            _targetTrigger.radius = s;  //自訂的停止距離
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    public void SetTarget(AITarget t)  //設置正確的目標 並配置碰撞器
    {   //分配新的目標
        _target = t;  //把傳進來目標設給_target
        if (_targetTrigger != null)
        {  //定位球型碰撞器 設置正確的半徑 位置 並開啟碰撞器
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = t.position;  
            _targetTrigger.enabled = true;
        }
    }

    public void ClearTarget()  //清除正確的目標(當目標不在使用時清除)
    {
        _target.Clear();  //重置值
        if(_targetTrigger != null)  //如果有碰撞器
        {
            _targetTrigger.enabled = false;  //關閉碰撞器
        }
    }

    protected virtual void FixedUpdate() //(當新的Trigger在固定時間更新時 偵測範圍內的目標 並決定做什麼)
    {
        VisualThreat.Clear();  //清除威脅
        AudioThreat.Clear();  //清除聲音威脅
        if(_target.type != AITargetType.None)  //如果不為空狀態
        {
            _target.distance = Vector3.Distance(_transform.position, _target.position);   //重新計算到目標的正確距離
        }
        _isTargetReached = false;
    }

    protected virtual void Update()  
    {
        if(_currentState == null && !PhotonNetwork.isMasterClient)  //如果目前狀態為空值
        {
            return;  //返回
        }
       AIStateType newStateType = _currentState.OnUpdate();  //轉換狀態
        if(newStateType != _currentStateType)  //如果狀態不等於目前狀態類型
        {
            AIState newState = null;  //儲存新狀態
            if(_states.TryGetValue( newStateType, out newState))  //如果key存在於字典中 把新的值設給它
            {
                _currentState.OnExitState();  //離開目前狀態
                newState.OnEnterState();  //進入新狀態
                _currentState = newState;  //把新狀態給目前狀態
            }
            else if(_states.TryGetValue(AIStateType.Idle, out newState))  //如果找不到當前狀態 默認為閒置狀態
            {
                _currentState.OnExitState();  //離開目前狀態
                newState.OnEnterState();  //進入新狀態
                _currentState = newState;  //把新狀態給目前狀態
            }
            _currentStateType = newStateType;  //設置新狀態類型
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(_targetTrigger == null || other != _targetTrigger)  //如果我的目標為空值 或者 另一個不是目標
        {
            return;  //返回
        }

        _isTargetReached = true;

        if (_currentState)
        {
            _currentState.OnDestinationReached(true);  //新目標(讓殭屍知道它進入的航點或是玩家的位置...)
        }
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger)  //如果我的目標為空值 或者 另一個不是目標
        {
            return;  //返回
        }

        _isTargetReached = true;       
    }

    protected void OnTriggerExit(Collider other)
    {
        if(_targetTrigger == null || _targetTrigger != other)  //如果我的目標為空值 或者 另一個不是目標
        {
            return;
        }

        _isTargetReached = false;

        if(_currentState != null)  //如果狀態不為空值
        {
            _currentState.OnDestinationReached(false);  //目標不存在
        }    
    }

    public virtual void OnTriggerEvent(AITriggerEventType type, Collider other)  
    {
        if(_currentState != null)
        {
            _currentState.OnTriggerEvent(type, other);  //把消息傳給當前狀態
        }
    }

    protected virtual void OnAnimatorMove()  //在根運動已被評估但未應用於對象之後由Unity調用。 這允許我們通過代碼確定如何處理根運動信息
    {
        if(_currentState != null)
        {
            _currentState.OnAnimatorUpdated();
        }
    }

    protected virtual void OnAnimaotrIK(int layerIndex)  //在IK系統更新之前由Unity調用，讓我們有機會設置IK目標和權重。
    {
        if(_currentState != null)
        {
            _currentState.OnAnimatorIKUpdated();
        }
    }

    public void NavAgentControl(bool positionUpdate, bool rotationUpdate)  //配置NavMeshAgent以啟用/禁用位置/旋轉的自動更新到我們的轉換
    {
        if (_navAgent)
        {
            _navAgent.updatePosition = positionUpdate;
            _navAgent.updateRotation = rotationUpdate;
        }
    }

    public void AddRootMotionRequest(int rootPosition, int rootRotation)  //由狀態機行為調用以啟用/禁用根運動
    {  // 1 -1
        _rootPositionRefCount += rootPosition;
        _rootRotationRefCount += rootRotation;
    }

    public virtual void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager characterManager, int hitDirection = 0)  //取得發生撞擊的位置.力道.傷害.被擊中的鋼體.被哪個玩家擊中.被打到的方向
    {
        
    }

    protected virtual void OnDestroy()
    {
        if(_layeredAudioSource != null && AudioManager.instance)
        {
            AudioManager.instance.UnregisterLayeredAudioSource(_layeredAudioSource);
        }
    }
}
