using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockdownTrigger : Photon.PunBehaviour
{
    [SerializeField]
    protected float _downloadTime = 10.0f;  //解鎖時間
    [SerializeField]
    protected Slider _downloadBar;  //解鎖條
    [SerializeField]
    protected Text _hintText;  //提示文字
    [SerializeField]
    protected MaterialController _materialController;  //材質球控制 
    [SerializeField]
    protected GameObject _lockedLight;  //電腦鎖定的燈
    [SerializeField]
    protected GameObject _unlockedLight;  //電腦解鎖的燈

    private ApplicationManager _applicationManager;  //應用程序管理 
    //private GameSceneManager _gameSceneManager = null;  //場景管理
    public bool _inTrigger = false;  //是不是在範圍內
    private float _downloadProgress = 0.0f;  //解鎖條進度
    private AudioSource _audioSource;  //音樂
    private bool _downloadComplete = false;  //是否解鎖完成

    void OnEnable()
    {
        ggggg();
        photonView.RPC("ggggg", PhotonTargets.All);
    }

    [PunRPC]
    void ggggg()
    {
        _applicationManager = ApplicationManager.instance;
        _audioSource = GetComponent<AudioSource>();
        _downloadProgress = 0.0f;

        if (_materialController != null)
        {
            _materialController.OnStart();
        }

        if (_applicationManager != null)
        {
            string lockedDown = _applicationManager.GetGameState("LOCKDOWN");  //是否有鎖定狀態

            if (string.IsNullOrEmpty(lockedDown) || lockedDown.Equals("TRUE"))  //檢查狀態是否為空值 ||是否為鎖定狀態
            {
                if (_materialController != null)
                {
                    _materialController.Activate(false);  //顯示原本的材質球
                }

                if (_unlockedLight)
                {
                    _unlockedLight.SetActive(false);  //關閉解鎖狀態的燈
                }

                if (_lockedLight)
                {
                    _lockedLight.SetActive(true);  //開啟鎖定狀態的燈
                }
                _downloadComplete = false;
            }
            else if (lockedDown.Equals("FALSE"))  //如果解鎖了
            {
                if (_materialController != null)
                {
                    _materialController.Activate(true);  //更換材質球
                }

                if (_unlockedLight)
                {
                    _unlockedLight.SetActive(true);  //開始解鎖燈
                }

                if (_lockedLight)
                {
                    _lockedLight.SetActive(false);  //關閉鎖定燈
                }
                _downloadComplete = true;  //解鎖完成
            }
        }
        ResetSoundAndUI();  //重置聲音跟UI
    }

    void Update()
    {
        if (_inTrigger)
        {
            if (Input.GetButton("Use"))
            {
               // aaaaa();
                photonView.RPC("aaaaa", PhotonTargets.All);
            }
            
        }
        else
        {
            _downloadProgress = 0.0f;
            ResetSoundAndUI();  //重置聲音跟UI 
        }
     
    }

    [PunRPC]
    void aaaaa()
    {
        if (_downloadComplete)
        {
            return;
        }
        if (_audioSource && !_audioSource.isPlaying)
        {
            _audioSource.Play();
        }
        _downloadProgress = Mathf.Clamp(_downloadProgress + Time.deltaTime, 0.0f, _downloadTime);  //計算下載進度

        if (_downloadProgress != _downloadTime)  //如果還沒下載玩
        {
            if (_downloadBar)
            {
                _downloadBar.gameObject.SetActive(true);  //開啟下載條
                _downloadBar.value = _downloadProgress / _downloadTime;  //計算下載條速度
            }
            return;
        }
        else
        {
            _downloadComplete = true;
            ResetSoundAndUI();  //重置聲音跟UI

            if (_hintText)
            {
                _hintText.text = "資料下載完成";
            }
            _applicationManager.SetGameState("LOCKDOWN", "FALSE");  //關閉鎖定狀態

            if (_materialController != null)
            {
                _materialController.Activate(true);
            }

            if (_unlockedLight)
            {
                _unlockedLight.SetActive(true);  //開始解鎖燈
            }

            if (_lockedLight)
            {
                _lockedLight.SetActive(false);  //關閉鎖定燈
            }
        }
       
    }

    void ResetSoundAndUI()  //重置聲音跟UI
    {
        if(_audioSource && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        if (_downloadBar)
        {
            _downloadBar.value = _downloadProgress;
            _downloadBar.gameObject.SetActive(false);
        }

        if (_hintText)
        {
            _hintText.text = "按住E鍵下載資料";
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(_inTrigger || _downloadComplete)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            _inTrigger = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (_downloadComplete)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            _inTrigger = false;
        }
    }
}
