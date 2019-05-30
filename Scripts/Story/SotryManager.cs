using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FadeStory
{
    Fadein,
    FadeOut
}

public class SotryManager : MonoBehaviour
{
    public Texture[] _storyTexture = null;  //故事圖片
    public RawImage _storyImage = null;  //故事Image
    public Image _storyPanel = null;  //遮罩
    public Text _storyDescript = null;  //故事敘述
    public Text _storyDescript1 = null;  //故事敘述
    public Text _storyDescript2 = null;

    public static int i = 0;

    private float _currentImageColor = 1.0f;  //正確的圖片透明度
    private float _currentPanelColor = 1.0f;
    private float _currentTextColor = 1.0f;
   // private float _timer = 0;
   // private float _time = 4.0f;
    private IEnumerator _coroutine = null;
    private IEnumerator _panelCoroutine = null;
    private IEnumerator _descript = null;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        _storyDescript1.gameObject.SetActive(false);
        _storyDescript2.gameObject.SetActive(false);


        if (_storyPanel)  //如果有圖片
        {
            Color color = _storyPanel.color;  //以區域變數紀錄目前的圖片顏色
            color.a = _currentPanelColor;  //紀錄正確的透明度
            _storyPanel.color = color;  //設給圖片
        }

        if (_storyDescript)
        {
            Color color = _storyDescript.color;  //以區域變數紀錄目前的圖片顏色
            color.a = _currentTextColor;  //紀錄正確的透明度
            _storyDescript.color = color;  //設給圖片
        }

        if (_storyImage)  //如果有圖片
        {
            Color color = _storyImage.color;  //以區域變數紀錄目前的圖片顏色
            color.a = _currentImageColor;  //紀錄正確的透明度
            _storyImage.color = color;  //設給圖片
        }

        FadePanel(1.5f, FadeStory.Fadein);
        Invoke("DelayShow", 3);   
    }

    public void DelayShow()  
    {
        Fade(1.5f, FadeStory.FadeOut);
    }

    public IEnumerator FadeDescript(Text des, float seconds)
    {   
        float timer = 0;  //計時器
        float target = 0;
        float srcFade = _currentTextColor;  //儲存目前的透明度
        Color oldColor = _storyDescript.color;  //以區域變數取得顏色
        if(i == 2)
        {
            srcFade = 1.0f;
        }
        if(i == 3)
        {
            srcFade = 1.0f;
        }
        if (seconds < 0.1f)  //避免傳入的秒數為0 發生錯誤
        {
            seconds = 0.1f;
        }

        while (timer < seconds)
        {
            timer += Time.deltaTime;  //開始計時
            _currentTextColor = Mathf.Lerp(srcFade, target, timer / seconds);  //計算透明度
            oldColor.a = _currentTextColor;  //設置透明度給區域變數
            des.color = oldColor;  //取得正確的透明度
            yield return null;
        }
        oldColor.a = _currentTextColor = target;  //確保跳出循環後 透明度為0
        _storyDescript.color = oldColor;
        
        if (i == 1)
        {          
            Fade(5, FadeStory.FadeOut);
        }
        if(i == 2)
        {
            Fade(5, FadeStory.FadeOut);
        }

        if(i == 3)
        {
            yield return new WaitForSeconds(1);
            PhotonNetwork.LoadLevel("Game");
        }
    }
	
	public void Fade(float seconds, FadeStory direction)
    {
        if(_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }

        float targetFade = 0.0f;

        switch (direction)
        {
            case FadeStory.Fadein:
                targetFade = 1.0f;
                break;
            case FadeStory.FadeOut:
                targetFade = 0.0f;
                break;
        }
        _coroutine = FadeStoryInternal(seconds, targetFade);
        StartCoroutine(_coroutine);
    }

    public void FadePanel(float seconds, FadeStory direction)
    {
        if (_panelCoroutine != null)
        {
            StopCoroutine(_panelCoroutine);
        }

        float targetFade = 0.0f;

        switch (direction)
        {
            case FadeStory.Fadein:
                targetFade = 0.0f;
                break;
            case FadeStory.FadeOut:
                targetFade = 1.0f;
                break;
        }
        _panelCoroutine = FadePanelInternal(seconds, targetFade);
        StartCoroutine(_panelCoroutine);
    }

    private IEnumerator FadeStoryInternal(float seconds, float targetFade)
    {
       /* if (!_storyImage)
        {
            yield break;
        }*/

        float timer = 0;  //計時器
        float srcFade = _currentImageColor;  //儲存目前的透明度
        if (i == 1)
            srcFade = 1.0f;
        if (i == 2)
            srcFade = 1.0f;
        Color oldColor = _storyImage.color;  //以區域變數取得顏色
        if (seconds < 0.1f)  //避免傳入的秒數為0 發生錯誤
        {
            seconds = 0.1f;
        }

        while (timer < seconds)
        {
            timer += Time.deltaTime;  //開始計時
            _currentImageColor = Mathf.Lerp(srcFade, targetFade, timer / seconds);  //計算透明度
            oldColor.a = _currentImageColor;  //設置透明度給區域變數
            _storyImage.color = oldColor;  //取得正確的透明度
            yield return null;  //跳出循環 等待下一針 直到時間超過
        }
        oldColor.a = _currentImageColor = targetFade;  //確保跳出循環後 透明度為0
        _storyImage.color = oldColor;
        i += 1;

        if(i == 1)
        {
            _storyImage.texture = _storyTexture[0];
            _descript = FadeDescript(_storyDescript, 3);
            StartCoroutine(_descript);
            yield break;
        }
            
        if (i == 2)
        {
            _storyImage.texture = _storyTexture[1];
            _storyDescript1.gameObject.SetActive(true);
            _descript = FadeDescript(_storyDescript1, 5);
            StartCoroutine(_descript);
            yield return new WaitForSeconds(2);
            _storyDescript2.gameObject.SetActive(true);
            _descript = FadeDescript(_storyDescript2, 4);
            StartCoroutine(_descript);
            yield break;
        } 
        
        if(i == 3)
        {          
            _storyDescript1.text = "政府派出特勤";
            _descript = FadeDescript(_storyDescript1, 5);
            StartCoroutine(_descript);
            yield return new WaitForSeconds(2);
            _storyDescript2.text = "前往研究所取回研究資料";
            _descript = FadeDescript(_storyDescript2, 4);
            StartCoroutine(_descript);
            yield break;
        }       
    }

    private IEnumerator FadePanelInternal(float seconds, float targetFade)
    {
        if (!_storyPanel)
        {
            yield break;
        }

        float timer = 0;  //計時器
        float srcFade = _currentPanelColor;  //儲存目前的透明度
        Color oldColor = _storyPanel.color;  //以區域變數取得顏色
        if (seconds < 0.1f)  //避免傳入的秒數為0 發生錯誤
        {
            seconds = 0.1f;
        }

        while (timer < seconds)
        {
            timer += Time.deltaTime;  //開始計時
            _currentPanelColor = Mathf.Lerp(srcFade, targetFade, timer / seconds);  //計算透明度
            oldColor.a = _currentPanelColor;  //設置透明度給區域變數
            _storyPanel.color = oldColor;  //取得正確的透明度
            yield return null;  //跳出循環 等待下一針 直到時間超過
        }
        oldColor.a = _currentPanelColor = targetFade;  //確保跳出循環後 透明度為0
        _storyPanel.color = oldColor;
    }
}
