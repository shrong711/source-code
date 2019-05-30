using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveKeypad : InteractiveItem
{
    [SerializeField]
    protected Transform _elevator;
    [SerializeField]
    protected AudioCollection _collection;
    [SerializeField]
    protected int _bank = 0;
    [SerializeField]
    protected float _activationDelay = 0.0f;
    [SerializeField]
    protected BoxCollider _boxcollider;
    public IEnumerator cou;
    public CharacterManager ccc;
    public BodyController[] cc = new BodyController[2];
    public Material _material;
    bool _isActivated = false;

    protected override void Start()
    {
        base.Start();
        _material.SetFloat("_OutlineWidth", 1.00f);
        ccc = FindObjectOfType<CharacterManager>();
        cc = FindObjectsOfType<BodyController>();
        print(cc.Length);
       

        
       
    }

    void Update()
    {
        if(cc.Length < 2)
        {
            cc = FindObjectsOfType<BodyController>();
            print(cc.Length);
        }
    }

    public override string GetText()  //取得文字
    {
        ApplicationManager appDatabase = ApplicationManager.instance;
        if (!appDatabase)
        {
            return string.Empty;
        }

        string powerState = appDatabase.GetGameState("POWER");  //取得電力狀態
        string lockdownState = appDatabase.GetGameState("LOCKDOWN");  //取得上鎖狀態
        string accessCodeState = appDatabase.GetGameState("ACCESSCODE");  //取得需要密碼狀態

        if (string.IsNullOrEmpty(powerState) || !powerState.Equals("TRUE"))  //如果是空值 或是 TRUE 代表沒有電力
        {
            return "沒有電力";
        }
        else if (string.IsNullOrEmpty(lockdownState) || !lockdownState.Equals("FALSE"))   //如果是空值 或是 TRUE 代表按鍵上鎖
        {
            return "取得研究資料";
        }
        else if(string.IsNullOrEmpty(accessCodeState) || !accessCodeState.Equals("TRUE"))  //如果是空值 或是 FALSE 代表需要密碼解鎖電梯
        {
            return "需要通行證";
        }
        return "按E使用電梯";
    }

    public override void Activate(CharacterManager characterManager)
    {
        if (_isActivated)
        {
            return;
        }

        ApplicationManager appDatabase = ApplicationManager.instance;
        if (!appDatabase)
        {
            return;
        }

        string powerState = appDatabase.GetGameState("POWER");
        string lockdownState = appDatabase.GetGameState("LOCKDOWN");
        string accessCodeState = appDatabase.GetGameState("ACCESSCODE");


        if (string.IsNullOrEmpty(powerState) || !powerState.Equals("TRUE"))
        {
            return;
        }
        if (string.IsNullOrEmpty(lockdownState) || !lockdownState.Equals("FALSE"))
        {
            return;
        }
        if (string.IsNullOrEmpty(accessCodeState) || !accessCodeState.Equals("TRUE"))
        {
            return;
        }
        cou = DoDelayedActivation(ccc);
        photonView.RPC("ccccc", PhotonTargets.All);
        //StartCoroutine(DoDelayedActivation(characterManager));  //如果有電力 按鍵以解鎖 開啟協程
        _isActivated = true;
    }

    [PunRPC]
    void ccccc()
    {
        cou = DoDelayedActivation(ccc);
        if (cou == null)
        {
            Debug.Log("玩家:" + PhotonNetwork.player.ID + "空直");
            return;
        }
          

        StartCoroutine(cou);
    }

    
    protected IEnumerator DoDelayedActivation(CharacterManager characterManager)
    {
        if (!_elevator)
        {
            yield break;
        }

        if(_collection != null)
        {
            AudioClip clip = _collection[_bank];
            if (clip)
            {
                if (AudioManager.instance)
                {
                    AudioManager.instance.PlayOneShotSound(_collection.audioGroup, clip, _elevator.position,
                                                           _collection.volume, _collection.spatialBlend, _collection.priority);  //撥放聲音
                }
            }
        }
        yield return new WaitForSeconds(_activationDelay);  //等待按鍵聲音播完

        int temp = _elevator.GetComponent<PhotonView>().viewID;
        
        photonView.RPC("aaa", PhotonTargets.All, temp);
      
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter");
        if (other.CompareTag("Player"))
            _material.SetFloat("_OutlineWidth", 1.08f);
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _material.SetFloat("_OutlineWidth", 1.00f);
    }

    [PunRPC]
    void aaa(int id)
    {
        if (ccc != null)
        {
            Transform t = PhotonView.Find(id).transform;
            foreach(BodyController i in cc)
            {
                i.transform.parent = t;  //把電梯設為玩家父物件 
            }
           
            Animator animator = t.GetComponent<Animator>();  //取得動畫控制器
            if (animator)
            {
                animator.SetTrigger("Activate");  //撥放電梯動畫
            }

            if (ccc.fpsController)
            {
                ccc.fpsController.freezeMovement = true;  //凍結玩家移動
            }
        }
    }
}
