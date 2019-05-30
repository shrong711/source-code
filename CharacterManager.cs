using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : Photon.PunBehaviour
{
    [SerializeField]
    private CapsuleCollider _meleeTrigger;  //攻擊碰撞器
    [SerializeField]
    private CameraBloodEffect _cameraBloodEffect;  //相機血效果腳本
    [SerializeField]
    private Camera _camera;  //相機
    [SerializeField]
    private float _health = 100.0f;  //生命
    [SerializeField]
    private AISoundEmitter _soundEmitter;  //玩家聲音腳本
    [SerializeField]
    private float _walkRadius = 0.0f;  //走路聲音碰撞器半徑
    [SerializeField]
    private float _runRadius = 7.0f;  //跑步聲音碰撞器半徑
    [SerializeField]
    private float _landingRadius = 12.0f;  //剛落在地面上聲音碰撞器半徑
    [SerializeField]
    private float _bloodRadiusScale = 6.0f;  //血腥味範圍
    [SerializeField]
    private PlayerHUD _playerHUD;
    [SerializeField]
    private AudioCollection _damageSounds;  //受傷聲音
    [SerializeField]
    private AudioCollection _painSounds;  //疼痛聲音
    [SerializeField]
    private AudioCollection _tauntSounds;  //玩家口哨聲
    [SerializeField]
    private float _nextPainSoundTime = 0.0f;  //下次受到傷害的間隔
    [SerializeField]
    private float _painSoundOffset = 0.35f;  //被殭屍碰到後瞬間播放受傷聲音
    [SerializeField]
    private float _tauntRadius = 10.0f;  //口哨半徑

    private Collider _collider;  //碰撞器
    private FPSController _fpsController;  //玩家腳本
    private CharacterController _characterController;  //角色控制器
    private GameSceneManager _gameSceneManager;  //場景管理

    private int _aiBodyPartLayer = -1;  //殭屍布娃娃圖層
    private int _aiEntity = -1;
    private int _interactiveMask = 0;
    private float _nextAttackTime = 0;  //下一次攻擊時間
    private float _nextTauntTime = 0;  //下一次口哨聲音
    public bool _isItemOpen = false;
	
    public float health { get { return _health; } }
    public float stamina { get { return _fpsController != null ? _fpsController.stamina : 0.0f; } }
    public FPSController fpsController { get { return _fpsController; } }

   

 

    void Start()
    {
        _playerHUD = GameObject.FindObjectOfType<PlayerHUD>();
        _collider = GetComponent<Collider>();  
        _fpsController = GetComponent<FPSController>();
        _characterController = GetComponent<CharacterController>();
        _gameSceneManager = GameSceneManager.instance;
        _aiEntity = LayerMask.NameToLayer("Wall");
        _aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");
        _interactiveMask = 1 << LayerMask.NameToLayer("Interactive");  //計算圖層整數索引
        _isItemOpen = false;
        _playerHUD._gridPannel.SetActive(false);
        _playerHUD._backGround.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (!PhotonNetwork.inRoom)
        {
            Debug.Log("關閉聊天");
            _playerHUD.ChatView(GameMode.Single);
        }
        else
        {
            Debug.Log("開啟聊天");
            _playerHUD.ChatView(GameMode.MultiPlayer);
        }

        if(_gameSceneManager != null)
        {  //把玩家訊息給場景管理
            PlayerInfo info = new PlayerInfo();  
            info.camera = _camera;  
            info.characterManager = this;
            info.collider = _collider;
            info.meleeTrigger = _meleeTrigger;

            _gameSceneManager.RegisterPlayerInfo(_collider.GetInstanceID(), info);  //註冊取得的玩家訊息到字典
        }    

        if (_playerHUD)
        {
            _playerHUD.Fade(2.0f, ScreenFadeType.FadeIn);
        }

        if (!photonView.isMine)
        {
            _camera.gameObject.SetActive(false);
            _fpsController.enabled = false;
            enabled = false;
        }
    }

    public void TakeDamage(float amount, bool doDamage, bool doPain)  //受到傷害
    {
        _health = Mathf.Max(_health - (amount * Time.deltaTime), 0.0f);

        if (_fpsController)
        {
            _fpsController.dragMultiplier = 0.0f;  //受到傷害時速度趨近於0 實現被殭屍抓住的感覺
        } 

        if(_cameraBloodEffect != null)
        {
            _cameraBloodEffect.minBloodAmount = (1.0f - (_health / 100.0f)) * 0.5f;  //最小血量效果
            _cameraBloodEffect.bloodAmount = Mathf.Min(_cameraBloodEffect.minBloodAmount + 0.3f, 1.0f);  //血量效果
        }

        if (AudioManager.instance)
        {
            if(doDamage && _damageSounds != null)
            {
                AudioManager.instance.PlayOneShotSound(_damageSounds.audioGroup, _damageSounds.audioClip, 
                                                       transform.position, _damageSounds.volume, _damageSounds.spatialBlend, _damageSounds.priority);
            }
            if(doPain && _damageSounds != null && _nextPainSoundTime < Time.time)
            {
                AudioClip painClip = _painSounds.audioClip;
                if (painClip)
                {
                    _nextPainSoundTime = Time.time + painClip.length;
                    StartCoroutine(AudioManager.instance.PlayOneShotSoundDelayed(_painSounds.audioGroup, painClip, transform.position, _painSounds.volume, 
                                                                                 _painSounds.spatialBlend, _painSoundOffset, _painSounds.priority));
                }
            }
        }

        if(_health <= 0.0f)
        {
            photonView.RPC("DoDeath", PhotonTargets.All);
        }
    }

    public GameObject[] bulletHole; // 彈孔

    
    
    public void DoDamage(int hitDirection = 0)  //攻擊
    {
        if(_camera == null)
        {
            return;
        }
        if(_gameSceneManager == null)
        {
            return;
        }

        Ray ray;  //射線
        RaycastHit hit;  //儲存射線訊息
        bool isSomethingHit = false;  //是否打到東西
        ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));  //射線螢幕寬高一半 Z0
        isSomethingHit = Physics.Raycast(ray, out hit, 100.0f, 1 << _aiBodyPartLayer);  //射線射到的東西
        if (isSomethingHit)  //如果有射到東西
        {
            AIStateMachine stateMachine = _gameSceneManager.GetAIStateMachine(hit.rigidbody.GetInstanceID());  //取得字典裡的碰撞器          
            if (stateMachine)  //如果有碰撞器
            {
                if (stateMachine == null)
                    return;

                Debug.Log("玩家:" + photonView.viewID +"1111111111111111111111111111111");
                stateMachine.TakeDamage(hit.point, ray.direction * 1.0f, 25, hit.rigidbody, this, 0);
                //photonView.RPC("aaa", PhotonTargets.All, stateMachine);             
                _nextAttackTime = Time.time + 0.3f;            
            }           
        }

        if (Physics.Raycast(ray, out hit, 100.0f, 1 << _aiEntity))
        {
            Debug.Log("BulletHole");
            Instantiate(bulletHole[Random.Range(0, bulletHole.Length)],
            hit.point,
            Quaternion.FromToRotation(Vector3.forward, hit.normal));
        }
    }

   [PunRPC]
    void RpcTakeDamage()
    {
        DoDamage();     
    }   
     
    void Update()
    {
        Ray ray;
        RaycastHit hit;
        RaycastHit[] hits;

        ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));  //螢幕中間的射線
        float rayLengh = Mathf.Lerp(1.0f, 1.8f, Mathf.Abs(Vector3.Dot(_camera.transform.forward, Vector3.up)));  //計算射線長度  (DOT在90度為0 0度時為1 計算0~1的值 用絕對值計算)
        hits = Physics.RaycastAll(ray, rayLengh, _interactiveMask);  //獲得所有被射線射到的東西

        if (hits.Length > 0)  //如果有射到東西
        {
            int highestPriority = int.MinValue;  //最高優先
            InteractiveItem priorityObject = null;  //儲存項目

            for (int i = 0; i < hits.Length; i++)  //搜尋每個打到的東西
            {
                hit = hits[i];
                InteractiveItem interactiveObject = _gameSceneManager.GetInteractiveItem(hit.collider.GetInstanceID());  //嘗試從遊戲管理獲得互動項目
                if (interactiveObject != null && interactiveObject.priority > highestPriority)  //如果有 && 如果比儲存的優先層級更高
                {
                    priorityObject = interactiveObject;  //儲存項目
                    highestPriority = priorityObject.priority;  //設為最高優先
                }
            }

            if (priorityObject != null)
            {
                if (_playerHUD)
                {
                    _playerHUD.SetInteractionText(priorityObject.GetText());  //顯示項目文字
                }
                if (Input.GetButtonDown("Use") && photonView.isMine)
                {
                    priorityObject.Activate(this);  //開啟互動
                }
            }
        }
        else
        {
            if (_playerHUD)
            {
                _playerHUD.SetInteractionText(null);  //如果射線沒有東西 就消除文字 並關閉
            }
        }

        if (Input.GetMouseButtonDown(0) && Time.time > _nextAttackTime && !_isItemOpen)  //如果按下滑鼠左鍵
        {
            photonView.RPC("RpcTakeDamage", PhotonTargets.All);
            //photonView.RPC("DoDamage", PhotonTargets.All, this);
            // DoDamage();
            Debug.Log("玩家:" + photonView.viewID + "開槍");
        }

        if (_fpsController || _soundEmitter != null)
        {
            float newRadius = Mathf.Max(_walkRadius, (100.0f - _health) / _bloodRadiusScale);  //預設為走路半徑  || 當前血量低於一百 取最大值

            switch (_fpsController.movementStatus)
            {
                case PlayerMoveStatus.Landing:
                    newRadius = Mathf.Max(newRadius, _landingRadius);
                    break;
                case PlayerMoveStatus.Running:
                    newRadius = Mathf.Max(newRadius, _runRadius);
                    break;
            }
            _soundEmitter.SetRadius(newRadius);  //設置新半徑
            _fpsController.dragMultiplierLimit = Mathf.Max(_health / 100.0f, 0.25f);  //血越低 移動速度越慢 但不低於0.25
        }

        if (Input.GetKeyDown(KeyCode.T) && !_isItemOpen)
        {
            DoTaunt();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (!_isItemOpen)
            {
                _playerHUD._gridPannel.SetActive(true);
                _playerHUD._backGround.SetActive(true);
                _isItemOpen = true;
                //Time.timeScale = 0;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                _playerHUD._gridPannel.SetActive(false);
                _playerHUD._backGround.SetActive(false);
                _isItemOpen = false;
                // Time.timeScale = 1;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }


        if (_playerHUD)
        {
            _playerHUD.Invalidate(this);
        }     
    }
   
    void DoTaunt()  //吹口哨
    {
        if(_tauntSounds == null || Time.time < _nextTauntTime)
        {
            return;
        }
        AudioClip taunt = _tauntSounds[0];
        AudioManager.instance.PlayOneShotSound(_tauntSounds.audioGroup, taunt, transform.position, 
                                               _tauntSounds.volume, _tauntSounds.spatialBlend, _tauntSounds.priority);  //撥放聲音
        if(_soundEmitter != null)
        {
            _soundEmitter.SetRadius(_tauntRadius);  //設置半徑
        }
        _nextTauntTime = Time.time + taunt.length;  //下一次使用時間
    }

    public void DoLevelComplete()
    {
        if (_fpsController)
        {
            _fpsController.freezeMovement = true;
        }
        if (_playerHUD)
        {
            _playerHUD.Fade(4.0f, ScreenFadeType.FadeOut);
            _playerHUD.ShowMissionText("任務完成");
            _playerHUD.Invalidate(this);
        }

        Invoke("GameOver", 4.0f);
    }

    public void AddHealth(float addhealth)
    {
        _health = Mathf.Min((_health + addhealth) , 100.0f);
    }

    [PunRPC]
    public void DoDeath()
    {
        Debug.Log("玩家:" + PhotonNetwork.player.ID + "死亡");
        if (_fpsController)
        {
            _fpsController.freezeMovement = true;
        }

        if (_playerHUD)
        {
            _playerHUD.Fade(3.0f, ScreenFadeType.FadeOut);
            _playerHUD.ShowMissionText("任務失敗");
            _playerHUD.Invalidate(this);
        }

        Invoke("GameOver", 3.0f);
    }

    void GameOver()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

       if (ApplicationManager.instance)
        {
            ApplicationManager.instance.LoadMainMenu();
        }
    }
}
