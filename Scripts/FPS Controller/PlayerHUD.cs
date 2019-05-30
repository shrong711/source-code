using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ScreenFadeType  //淡入淡出狀態
{
    FadeIn,  
    FadeOut
}

public enum GameMode
{
    Single,
    MultiPlayer
}

public class PlayerHUD : MonoBehaviour
{
    [SerializeField]
    private GameObject _crosshair = null;  //十字準心
    [SerializeField]
    private Text _healthText = null;  //生命
    [SerializeField]
    private Text _staminaText = null;  //耐力
    [SerializeField]
    private Text _interactionText = null;  //互動
    [SerializeField]
    private Image _screenFade = null;  //淡入淡出圖像
    [SerializeField]
    private Text _missionText = null;  //任務
    [SerializeField]
    private float _missionTextDisplayTime = 3.0f;  //任務文字消失時間
    [SerializeField]
    private float _dissolveamount = 0;
    [SerializeField]
    private RawImage _rawImage = null;

    public Shader _shader = null;
    public Material _material = null;
    public GameObject _gridPannel = null; //背包
    public GameObject _backGround = null;  //背包底板
    public GameObject _chatView = null;  //聊天

    private ShootInfo _shootInfo;
    // UI
    public Text bulletAmount;
    public Text totalBullet;

    float _currentFadeLevel = 1.0f;  //透明度
    IEnumerator _coroutine = null;
    IEnumerator _missionCoroutine = null;

    public void Start()
    {
        _shootInfo = FindObjectOfType<ShootInfo>();
        bulletAmount.text = _shootInfo.currentAmmo.ToString();
        totalBullet.text = "/ " + _shootInfo.totalAmmo.ToString();
        if (_screenFade)
        {
            Color color = _screenFade.color;  //以區域變數取得顏色
            color.a = _currentFadeLevel;  //剛開始不透明
            _screenFade.color = color;  //取得設定好的顏色
        }      
         if (_missionText)  //如果為有效的引用
         {
             Invoke("HideMissionText", _missionTextDisplayTime);  //在3秒後隱藏任務文字
         }

        if (_rawImage)
        {
            _rawImage.material = _material;
            _rawImage.gameObject.SetActive(false);
        }
      
        if (_material)
        {
            _material.shader = _shader;
            _material.SetFloat("_Dissolveamount", _dissolveamount);
        }
        Invoke("Shader", 3);
    }

    void Update()
    {
        bulletAmount.text = _shootInfo.currentAmmo.ToString();
    }

    public void Invalidate(CharacterManager charManager)  //顯示生命 耐力文字
    {
        if(charManager == null)
        {
            return;
        }
        if (_healthText)
        {
            _healthText.text = "生命 : " + ((int)charManager.health).ToString();
            _staminaText.text = "耐力 : " + ((int)charManager.stamina).ToString();
        }
    }

    public void SetInteractionText(string text)  //互動文字 武器 門
    {
        if (_interactionText)
        {
            if(text == null)
            {
                _interactionText.text = null;
                _interactionText.gameObject.SetActive(false);
            }
            else
            {
                _interactionText.text = text;
                _interactionText.gameObject.SetActive(true);
            }
        }       
    }

    public void Fade(float seconds, ScreenFadeType direction)  //淡入淡出
    {
        if(_coroutine != null)  //如果有協程在運行 停止 避免衝突
        {
            StopCoroutine(_coroutine);
        }
        float targetFade = 0.0f;  //目標透明度 (透明)

        switch (direction)  //選擇淡入淡出
        {
            case ScreenFadeType.FadeIn:  //淡入
                targetFade = 0.0f;  //透明度為0
                break;
            case ScreenFadeType.FadeOut:  //淡出
                targetFade = 1.0f;  //透明度為1
                break;
        }
        _coroutine = FadeInternal(seconds, targetFade);  //設置協程
        StartCoroutine(_coroutine);  //開始淡入淡出功能
    }

    public void FadeMissionText(float seconds,ScreenFadeType direction)
    {
        if (_missionCoroutine != null)  //如果有協程在運行 停止 避免衝突
        {
            StopCoroutine(_missionCoroutine);
        }
        float targetFade = 1.0f;

        switch (direction)
        {
            case ScreenFadeType.FadeIn:
                targetFade = 1.0f;
                break;
            case ScreenFadeType.FadeOut:
                targetFade = 0.0f;
                break;
        }
        _missionCoroutine = HideMissionImage(seconds, targetFade);
        StartCoroutine(_missionCoroutine);
    }

    public void ChatView(GameMode game)
    {
        switch (game)
        {
            case GameMode.Single:
                _chatView.SetActive(false);
                break;
            case GameMode.MultiPlayer:
                _chatView.SetActive(true);
                break;
        }
    }

    private IEnumerator FadeInternal(float seconds, float targetFade)  //淡入淡出協程
    {
        if (!_screenFade)  //檢查是否有圖像
        {
            yield break; 
        }
        float timer = 0;  //計時器
        float srcFade = _currentFadeLevel;  //儲存目前的透明度
        Color oldColor = _screenFade.color;  //以區域變數取得顏色
        if(seconds < 0.1f)  //避免傳入的秒數為0 發生錯誤
        {
            seconds = 0.1f;
        }

        while(timer < seconds)
        {
            timer += Time.deltaTime;  //開始計時
            _currentFadeLevel = Mathf.Lerp(srcFade, targetFade, timer / seconds);  //計算透明度
            oldColor.a = _currentFadeLevel;  //設置透明度給區域變數
            _screenFade.color = oldColor;  //取得正確的透明度
            yield return null;  //跳出循環 等待下一針 直到時間超過
        }
        oldColor.a = _currentFadeLevel = targetFade;  //確保跳出循環後 透明度為0
        _screenFade.color = oldColor;  
    }

    private IEnumerator HideMissionImage(float seconds, float target)
    {
        if (!_rawImage)  //檢查是否有圖像
        {
            yield break;
        }

        float timer = 0;  //計時器
        float srcFade = _dissolveamount;  //儲存目前的透明度
        if (seconds < 0.1f)  //避免傳入的秒數為0 發生錯誤
        {
            seconds = 0.1f;
        }

        while (timer < seconds)
        {
            timer += Time.deltaTime;  //開始計時
            _dissolveamount = Mathf.Lerp(srcFade, target, timer / seconds);  //計算透明度
            _material.SetFloat("_Dissolveamount", _dissolveamount);
            yield return null;  //跳出循環 等待下一針 直到時間超過
        }
         _dissolveamount = target;  //確保跳出循環後 透明度為0
    }

    public void ShowMissionText(string text)  //顯示任務文字
    {
        if (_missionText)
        {
            _missionText.text = text;
            _missionText.gameObject.SetActive(true);
        }
    }

    public void HideMissionText()  //隱藏任務文字
    {
        if (_missionText)
        {
            _missionText.gameObject.SetActive(false);
        }
    }

    public void Shader()
    {
        _rawImage.gameObject.SetActive(true);
        FadeMissionText(5, ScreenFadeType.FadeIn);
    }
}
