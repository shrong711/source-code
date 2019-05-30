using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIBoneControlType  //AI骨骼控制
{
    Animated,  //動畫
    Ragdoll,  //布娃娃
    RagdollToAnim  //復活模式
}

public enum AIScreamPosition  //尖叫
{
    Entity,  //殭屍
    Player  //玩家
}

public class BodyPartSnapshot  //用於儲存從布娃娃轉換時每個身體部位的位置訊息
{
    public Transform transform;  //位置
    public Vector3 position;  //儲存位置
    public Quaternion rotation;  //儲存旋轉位置
    public Quaternion localRotation;
}

public class AIZombieStateMachine : AIStateMachine   //殭屍狀態控制器
{
    [SerializeField][Range(10.0f, 360.0f)]
    float _fov = 50.0f;  //視野
    [SerializeField][Range(0.0f, 1.0f)]
    float _sight = 0.5f;  //可視的距離
    [SerializeField][Range(0.0f, 1.0f)]
    float _hearing = 1.0f;  //聽到的範圍
    [SerializeField][Range(0.0f, 1.0f)]
    float _aggression = 0.5f;  //攻擊
    [SerializeField][Range(0, 100)]
    int _health = 100;  //血量
    [SerializeField][Range(0, 100)]
    int _lowerBodyDamage = 0;  //下半身傷害
    [SerializeField][Range(0, 100)]
    int _upperBodyDamage = 0;  //上半身傷害
    [SerializeField][Range(0, 100)]
    int _upperBodyThreshold = 30;  //受到多少傷害時 上半身受損 
    [SerializeField][Range(0, 100)]
    int _limpThreshold = 30;  //受到多少傷害時 跛腳  
    [SerializeField][Range(0, 100)]
    int _crawlThreshold = 90;  //受到多少傷害時 爬行 
    [SerializeField][Range(0.0f, 1.0f)]
    float _intelligence = 0.5f;  //確認聲音的來源
    [SerializeField][Range(0.0f, 1.0f)]
    float _satisfaction = 1.0f;  //飢餓感
    [SerializeField][Range(0.0f, 1.0f)]
    float _screamChance = 1.0f;
    [SerializeField][Range(0.0f, 50.0f)]
    float _screamRadius = 20.0f;  //尖叫半徑
    [SerializeField]
    AIScreamPosition _screamPosition = AIScreamPosition.Entity;  //預設在殭屍身上
    [SerializeField]
    AISoundEmitter _screamPrefab = null;  //聲音預置物
    [SerializeField]
    AudioCollection _ragdollCollection = null;  //布娃娃聲音
    [SerializeField]
    float _replenishRate = 0.5f;  //增加殭屍的飽足感
    [SerializeField]
    float _depletionRate = 0.1f;  //降低殭屍的飽足感
    [SerializeField]
    float _reanimationBlendTime = 1.5f;  //從混和動畫轉到動畫控制的時間
    [SerializeField]
    float _reanimationWaitTime = 3.0f;  //重新製作殭屍動畫前等待3秒 
    [SerializeField]
    LayerMask _geometryLayers = 0;  //幾何圖層

    private int _seeking = 0;  //動畫控制器的參數 控制轉向
    private bool _feeding = false;  //動畫控制器的參數 控制吃屍體
    private bool _crawling = false; //動畫控制器參數 控制是否爬行
    private int _attackType = 0;  //動畫控制器參數 控制攻擊
    private float _speed = 0.0f;  //速度
    private float _isScreaming = 0.0f;  //動畫控制器參數 控制尖叫
    private float _nextRagdollSoundTime = 0.0f;  //下一次撥放布娃娃疼痛聲音

    private AIBoneControlType _boneControlType = AIBoneControlType.Animated;  //預設由動畫控制
    private List<BodyPartSnapshot> _bodyPartSnapShots = new List<BodyPartSnapshot>();  //儲存訊息
    private float _ragdollEndTime = float.MinValue;  //布娃娃結束時間
    private Vector3 _ragdollHipPosition;
    private Vector3 _ragdollFeetPosition;
    private Vector3 _ragdollHeadPosition;
    private IEnumerator _reanimationCoroutine = null;
    private float _mecanimTransitionTime = 0.1f;

    private int _speedHash = Animator.StringToHash("Speed");  //紀錄Speed狀態
    private int _seekingHash = Animator.StringToHash("Seeking");  //紀錄Seeking狀態
    private int _feedingHash = Animator.StringToHash("Feeding");  //紀錄Feeding狀態
    private int _attackHash = Animator.StringToHash("Attack");  //紀錄Attack狀態
    private int _crawlingHash = Animator.StringToHash("Crawling");  //紀錄爬行狀態
    private int _screamingHash = Animator.StringToHash("Screaming");  //紀錄尖叫
    private int _screamHash = Animator.StringToHash("Scream");  //尖叫Trigger
    private int _hitTriggerHash = Animator.StringToHash("Hit");  //紀錄被打到狀態
    private int _hitTypeHash = Animator.StringToHash("HitType");  //紀錄被打到的類型
    private int _lowerBodyDamageHash = Animator.StringToHash("Lower Body Damage");  //紀錄下半身傷害
    private int _upperBodyDamageHash = Animator.StringToHash("Upper Body Damage");  //紀錄上半身傷害
    private int _reanimateFromBackHash = Animator.StringToHash("Reanimate From Back");  //紀錄復活
    private int _reanimateFromFrontHash = Animator.StringToHash("Reanimate From Front");
    private int _stateHash = Animator.StringToHash("State");
    private int _upperBodyLayer = -1;
    private int _lowerBodyLayer = -1;


    public float replenishRate { get { return _replenishRate; } }
    public float fov { get { return _fov; } }
    public float hearing { get { return _hearing; } }
    public float sight { get { return _sight; } }
    public bool crawling { get { return _crawling; } }
    public float intelligence { get { return _intelligence; } }
    public float satisfaction { get { return _satisfaction; } set { _satisfaction = value; } }
    public float aggression { get { return _aggression; } set { _aggression = value; } }
    public int health { get { return _health; } set { _health = value; } }
    public int attackType { get { return _attackType; } set { _attackType = value; } }
    public bool feeding { get { return _feeding; } set { _feeding = value; } }
    public int seeking { get { return _seeking; } set { _seeking = value; } }
    public float speed { get { return _speed; } set { _speed = value; } }
    public bool isCrawling { get { return (_lowerBodyDamage >= _crawlThreshold); } }
    public bool isScreaming { get { return _isScreaming > 0.1f; } }

    public bool Scream()
    {
        if (isScreaming)
        {
            return true;
        }
        if(_animator == null || IsLayerActive("Cinematic") || _screamPrefab == null)
        {
            return false;
        }

        _animator.SetTrigger(_screamHash);
        Vector3 spawnPos = _screamPosition == AIScreamPosition.Entity ? transform.position : VisualThreat.position;  //如果是殭屍把位置設在殭屍身上 否則設在玩家身上
        AISoundEmitter screamEmitter = Instantiate(_screamPrefab, spawnPos, Quaternion.identity) as AISoundEmitter;  //複製預置物 回傳AISoundEmitter腳本

        if(screamEmitter != null)
        {
            screamEmitter.SetRadius(_screamRadius);
        }
        return true;
    }

    public float screamChance
    {
        get { return _screamChance; }
    }

    protected override void Start()
    {
        base.Start();

        if(_animator != null)
        {
            _lowerBodyLayer = _animator.GetLayerIndex("Lower Body");  //取得圖層索引
            _upperBodyLayer = _animator.GetLayerIndex("Upper Body");  //取得圖層索引
        }

        if(_rootBone != null)
        {
            Transform[] transforms = _rootBone.GetComponentsInChildren<Transform>();  //取得所有子骨頭位置
            foreach(Transform trans in transforms)  //搜尋每一個位置
            {
                BodyPartSnapshot snapShot = new BodyPartSnapshot();
                snapShot.transform = trans;  //存進類別
                _bodyPartSnapShots.Add(snapShot);  //添加到清單
            }
        }
        UpdateAnimatorDamage();
    }

    protected override void Update()
    {      
        if (_animator != null && PhotonNetwork.isMasterClient)
        {  //播放動畫
            base.Update();
            _animator.SetFloat(_speedHash, _speed);
            _animator.SetBool(_feedingHash, _feeding);
            _animator.SetInteger(_seekingHash, _seeking);
            _animator.SetInteger(_attackHash, _attackType);
            _animator.SetInteger(_stateHash, (int)_currentStateType);
            _isScreaming = IsLayerActive("Cinematic") ? 0.0f : _animator.GetFloat(_screamingHash);
            _satisfaction = Mathf.Max(0, _satisfaction - ((_depletionRate * Time.deltaTime) / 100.0f) * Mathf.Pow(_speed, 3.0f));  //隨著時間降低飽足感
        }              
       
    }

    protected void UpdateAnimatorDamage()
    {
        if(_animator != null)
        {
            if(_lowerBodyLayer != -1)
            {
                _animator.SetLayerWeight(_lowerBodyLayer, (_lowerBodyDamage > _limpThreshold && _lowerBodyDamage < _crawlThreshold) ? 1.0f : 0.0f);  //如果下半身損傷大於閥值 && 小於爬行閥值 下半身圖層權重設為1 否則為0
            }

            if(_upperBodyLayer != -1)
            {
                _animator.SetLayerWeight(_upperBodyLayer, (_upperBodyDamage > _upperBodyThreshold && _lowerBodyDamage < _crawlThreshold) ? 1.0f : 0.0f);  //如果上半身損傷大於上半身閥值 && 下半身損傷小於爬行閥值 上半身圖層權重設為1 否則為0
            }
            _animator.SetBool(_crawlingHash, isCrawling);
            _animator.SetInteger(_lowerBodyDamageHash, _lowerBodyDamage);
            _animator.SetInteger(_upperBodyDamageHash, _upperBodyDamage);

            if(_lowerBodyDamage > _limpThreshold && _lowerBodyDamage < _crawlThreshold)
            {
                SetLayerActive("Lower Body", true);
            }
            else
            {
                SetLayerActive("Lower Body", false);
            }
            if(_upperBodyDamage > _upperBodyThreshold && _lowerBodyDamage < _crawlThreshold)
            {
                SetLayerActive("Upper Body", true);
            }
            else
            {
                SetLayerActive("Upper Body", false);
            }
        }
    }

    
    public override void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager characterManager, int hitDirection = 0)  //取得發生撞擊的位置.力道.傷害.被擊中的鋼體.被哪個玩家擊中.被打到的方向
    {
        if (GameSceneManager.instance != null && GameSceneManager.instance.bloodParticles != null)  //檢查
        {
            ParticleSystem sys = GameSceneManager.instance.bloodParticles;  //粒子系統
            sys.transform.position = position;  //粒子系統位置
            var settings = sys.main;  //取得主要設置面板
            settings.simulationSpace = ParticleSystemSimulationSpace.World;
            sys.Emit(60);
        }

        float hitStrength = force.magnitude;  //力道大小
        float prevHealth = _health;

        if (_boneControlType == AIBoneControlType.Ragdoll)
        {
            if(bodyPart != null)
            {
                if(Time.time > _nextRagdollSoundTime && _ragdollCollection != null && _health > 0)
                {
                    AudioClip clip = _ragdollCollection[1];

                    if (clip)
                    {
                        _nextRagdollSoundTime = Time.time + clip.length;
                        AudioManager.instance.PlayOneShotSound(_ragdollCollection.audioGroup, clip, position, 
                                                               _ragdollCollection.volume, _ragdollCollection.spatialBlend, _ragdollCollection.priority);
                    }
                }

                if(hitStrength > 1.0f)  //如果力道大於1
                {
                    bodyPart.AddForce(force, ForceMode.Impulse);  //施加力道到身體
                }

                if (bodyPart.CompareTag("Head"))  //如果打到頭
                {
                    _health = Mathf.Max(_health - damage, 0);  //扣血 但不低於0
                }
                else if(bodyPart.CompareTag("Upper Body"))
                {
                    _upperBodyDamage += damage;
                }
                else if(bodyPart.CompareTag("Lower Body"))
                {
                    _lowerBodyDamage += damage;
                }

                UpdateAnimatorDamage();

                if(_health > 0)
                {
                    if (_reanimationCoroutine != null)
                    {
                        StopCoroutine(_reanimationCoroutine);
                    }
                    _reanimationCoroutine = Reanimate();
                    photonView.RPC("rean", PhotonTargets.All);                  
                   // StartCoroutine(_reanimationCoroutine);
                }         
            }
            return;
        }

        Vector3 attackerLocPos = transform.InverseTransformPoint(characterManager.transform.position);  // 取得玩家相對於殭屍的位置  (local space)
     //   Vector3 hitLocPos = transform.InverseTransformPoint(position);  //取得命中的位置 (local space)

        bool shouldRagdoll = (hitStrength > 1.0f);  //檢查力道       

        if (bodyPart != null)
        {          
            if (bodyPart.CompareTag("Head"))  //如果打到頭
            {
                _health = Mathf.Max(_health - damage, 0);  //扣血 但不低於0
                if(health == 0)
                {
                    shouldRagdoll = true;
                }
            }
            else if (bodyPart.CompareTag("Upper Body"))
            {
                _upperBodyDamage += damage;
                UpdateAnimatorDamage();
            }
            else if (bodyPart.CompareTag("Lower Body"))
            {
                _lowerBodyDamage += damage;
                UpdateAnimatorDamage();
                shouldRagdoll = true;
            }
        }

        if(_boneControlType != AIBoneControlType.Animated || isCrawling || IsLayerActive("Cinematic") || attackerLocPos.z < 0 )
        {
            shouldRagdoll = true;
        }

        if (!shouldRagdoll)
        {
            float angle = 0.0f;  
            if(hitDirection == 0)
            {
                Vector3 vecToHit = (position - transform.position).normalized;  //從命中點到中心點的向量 標準化
                angle = AIState.FindSignedAngle(vecToHit, transform.forward);  //從中心點到命中點的向量, Z軸向量  找到sign角度
            }

            int hitType = 0;  //被打到的部位
            if (bodyPart.gameObject.CompareTag("Head"))  //如果是頭
            {  //檢查角度 看撥放哪種動畫
                if(angle < -10 || hitDirection == -1)  //角度小於-10 左邊方向
                {
                    hitType = 1;  //左邊頭部被打到 撥放動畫
                }
                else if(angle > 10 || hitDirection == 1)  //角度大於10 右邊方向
                {
                    hitType = 3;  //右邊頭部
                }
                else
                {
                    hitType = 2;  //正面被擊中
                }
            }
            else if(bodyPart.gameObject.CompareTag("Upper Body"))  //如果被打到的是上半身
            {
                if(angle < -20 || hitDirection == -1)  //角度小於-20 左邊方向
                {
                    hitType = 4;  //撥放左邊被打動畫
                }
                else if(angle > 20 || hitDirection == 1)  //角度大於20 右邊方向
                {
                    hitType = 6;  //撥放右邊被打動畫
                }
                else
                {
                    hitType = 5;  //否則撥放中間動畫
                }
            }

            if (_animator)
            {
                _animator.SetInteger(_hitTypeHash, hitType);  //設置應該撥放的動畫
                _animator.SetTrigger(_hitTriggerHash);  //撥放
            }
            return;  //返回
        }
        else  //布娃娃狀態
        {
            if (_currentState)  //如果有狀態
            {
                _currentState.OnExitState();  //離開狀態
                _currentState = null;  //設為空值
                _currentStateType = AIStateType.None;  //類型設為NONE
            }

            if (_navAgent)
            {
                _navAgent.enabled = false;  //關掉AI
            }

            if (_animator)
            {
                _animator.enabled = false;  //關掉動畫控制器
            }

            if (_collider)
            {
                _collider.enabled = false;  //關掉碰撞器
            }

            if(_layeredAudioSource != null)
            {
                _layeredAudioSource.Mute(true);  //靜音
            }

            if(Time.time > _nextRagdollSoundTime && _ragdollCollection != null && prevHealth > 0)
            {
                AudioClip clip = _ragdollCollection[0];
                if (clip)
                {
                    _nextRagdollSoundTime = Time.time + clip.length;
                    AudioManager.instance.PlayOneShotSound(_ragdollCollection.audioGroup, clip, position,
                                                               _ragdollCollection.volume, _ragdollCollection.spatialBlend, _ragdollCollection.priority);
                }
            }

            inMeleeRange = false;  //關掉攻擊範圍

            foreach(Rigidbody body in _bodyParts)
            {
                if (body)
                {
                    body.isKinematic = false;  //關掉運動學 由物理系統控制
                }
            }
            if(hitStrength > 1.0f)
            {
                bodyPart.AddForce(force, ForceMode.Impulse);  //給與身體力道
            }

            _boneControlType = AIBoneControlType.Ragdoll;  //設置狀態
            
            if(_health > 0)  //如果血大於0
            {
                if (_reanimationCoroutine != null)  //確保不會重複發生
                {
                    StopCoroutine(_reanimationCoroutine);  //關閉協成
                }
                _reanimationCoroutine = Reanimate();
                photonView.RPC("rean", PhotonTargets.All);//啟用協成
            }    
        }
    }

    [PunRPC]
    void rean( )
    {
        if(_reanimationCoroutine == null)
        {
            return;
        }
        StartCoroutine(_reanimationCoroutine);
    }

    protected IEnumerator Reanimate()  //復活協成
    {
        if(_boneControlType != AIBoneControlType.Ragdoll || _animator == null)  //是否為有效地引用
        {
            yield break;
        }
        yield return new WaitForSeconds(_reanimationWaitTime);  //等待

        _ragdollEndTime = Time.time;  //紀錄時間

        foreach(Rigidbody body in _bodyParts)  //把所有鋼體
        {
            body.isKinematic = true;  //設為運動學
        }

        _boneControlType = AIBoneControlType.RagdollToAnim;  //切換為復活模式

        foreach(BodyPartSnapshot snapShot in _bodyPartSnapShots)
        {  //在動畫控制前 取得骨頭的最後位置 旋轉位置
            snapShot.position = snapShot.transform.position;
            snapShot.rotation = snapShot.transform.rotation;
            snapShot.localRotation = snapShot.transform.localRotation;
        }

        _ragdollHeadPosition = _animator.GetBoneTransform(HumanBodyBones.Head).position;  //紀錄布娃娃頭的位置
        _ragdollFeetPosition = (_animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + _animator.GetBoneTransform(HumanBodyBones.RightFoot).position) * 0.5f;  //紀錄腳的位置
        _ragdollHipPosition = _rootBone.position;

        _animator.enabled = true;  //啟動動畫控制器
        
        if(_rootBone != null)
        {
            float forwardTest;

            switch (_rootBoneAlignment)
            {
                case AIBoneAlignmentType.ZAxis:  //如果骨頭前進方向為Z軸
                    forwardTest = _rootBone.forward.y;  //取得Y值  判斷人物倒向哪邊
                    break;
                case AIBoneAlignmentType.ZAxisInverted:  //當Z在反方向時
                    forwardTest = -_rootBone.forward.y;  //
                    break;
                case AIBoneAlignmentType.YAxis:  //如果骨頭前進方向為Y軸
                    forwardTest = _rootBone.up.y;  //
                    break;
                case AIBoneAlignmentType.YAxisInverted:  //當Y在反方向時
                    forwardTest = -_rootBone.up.y;  //Y為-1
                    break;
                case AIBoneAlignmentType.XAxis:  //如果骨頭前進方向為X軸
                    forwardTest = _rootBone.right.y;  //
                    break;
                case AIBoneAlignmentType.XAxisInverted:  //當X在反方向時
                    forwardTest = -_rootBone.right.y;  //
                    break;
                default:
                    forwardTest = _rootBone.forward.y;
                    break;
            }
            //設置Trigger 撥放相應的動畫
            if(forwardTest >= 0)  //依照取得的Y值判斷人物往前倒 還是往後倒
            {
                _animator.SetTrigger(_reanimateFromBackHash);
            }
            else
            {
                _animator.SetTrigger(_reanimateFromFrontHash);
            }
        }
    }

    protected virtual void LateUpdate()
    {
        if(_boneControlType == AIBoneControlType.RagdollToAnim && PhotonNetwork.isMasterClient)
        {
            if(Time.time <= _ragdollEndTime + _mecanimTransitionTime)  //檢查是否在等待時間內
            {  //定位 並對齊方向
                Vector3 animatedToRagdoll = _ragdollHipPosition - _rootBone.position;  //紀錄位置
                Vector3 newRootPosition = transform.position + animatedToRagdoll;  //新位置
                RaycastHit[] hits = Physics.RaycastAll(newRootPosition + (Vector3.up * 0.25f), Vector3.down, float.MaxValue, _geometryLayers);
                newRootPosition.y = float.MinValue;
                foreach(RaycastHit hit in hits)
                {
                    if (!hit.transform.IsChildOf(transform))
                    {
                        newRootPosition.y = Mathf.Max(hit.point.y, newRootPosition.y);  //找到最高的Y位置
                    }
                }

                NavMeshHit navMeshHit;
                Vector3 baseOffset = Vector3.zero;  //基準偏移量
                if (_navAgent)
                {
                    baseOffset.y = _navAgent.baseOffset;  //設置原本的偏移量
                }

                if( NavMesh.SamplePosition(newRootPosition, out navMeshHit, 2.0f, NavMesh.AllAreas))  //查詢最近的點
                {
                    transform.position = navMeshHit.position + baseOffset;  //設置新位置
                }
                else
                {
                    transform.position = newRootPosition + baseOffset;
                }

                Vector3 ragdollDiretion = _ragdollHeadPosition - _ragdollFeetPosition;
                ragdollDiretion.y = 0.0f;

                Vector3 meanFeePosition = 0.5f * (_animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + _animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
                Vector3 animatedDirection = _animator.GetBoneTransform(HumanBodyBones.Head).position - meanFeePosition;
                animatedDirection.y = 0.0f;  // 只能繞著Y軸旋轉  身體要保持直的 所以設為0

                transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized, ragdollDiretion.normalized);  //旋轉
            }

            float blendAmount = Mathf.Clamp01((Time.time - _ragdollEndTime) / _reanimationBlendTime);  //計算插植

            foreach(BodyPartSnapshot snapshot in _bodyPartSnapShots)  //在布娃娃骨頭和動畫骨頭位置之間進行插值計算混合骨頭位置
            {
                if(snapshot.transform == _rootBone)
                {
                    snapshot.transform.position = Vector3.Lerp(snapshot.position, snapshot.transform.position, blendAmount);
                    snapshot.transform.rotation = Quaternion.Slerp(snapshot.rotation, snapshot.transform.rotation, blendAmount);
                }

                snapshot.transform.localRotation = Quaternion.Slerp(snapshot.localRotation, snapshot.transform.localRotation, blendAmount);             
            }

            if(blendAmount == 1.0f)  //離開復活模式
            {
                _boneControlType = AIBoneControlType.Animated;
                if (_navAgent)
                {
                    _navAgent.enabled = true;
                }
                if (_collider)
                {
                    _collider.enabled = true;
                }

                AIState newstate = null;
                if(_states.TryGetValue(AIStateType.Alerted, out newstate))
                {
                    if(_currentState != null)
                    {
                        _currentState.OnExitState();
                    }
                    newstate.OnEnterState();
                    _currentState = newstate;
                    _currentStateType = AIStateType.Alerted;
                }
            }
        }
    }
}
