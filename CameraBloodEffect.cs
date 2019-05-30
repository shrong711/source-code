using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]  //編譯模式執行腳本
public class CameraBloodEffect : MonoBehaviour
{
    [SerializeField]
    private Texture2D _bloodTexture = null;  //血液貼圖
    [SerializeField]
    private Texture2D _bloodNormalMap = null;  //法線貼圖
    [SerializeField]
    private float _bloodAmount = 0.0f;  //血量效果(隨著玩家生命變化)
    [SerializeField]
    private float _minBloodAmount = 0.0f;  //最小血量效果
    [SerializeField]
    private float _distortion = 1.0f;  //放大或縮小法線貼圖值
    [SerializeField]
    private bool _autoFade = true;  //關閉或開啟腳本
    [SerializeField]
    private float _fadeSpeed = 0.05f;  //血下降速度

    [SerializeField]
    private Shader _shader = null;  //血液相機效果

    private Material _material = null;  //材質球

    public float bloodAmount { get { return _bloodAmount; } set { _bloodAmount = value; } }
    public float minBloodAmount { get { return _minBloodAmount; } set { _minBloodAmount = value; } }
    public float fadeSpeed { get { return _fadeSpeed; } set { _fadeSpeed = value; } }
    public bool autoFade { get { return _autoFade; } set { _autoFade = value; } }

    void Update()
    {
        _bloodAmount -= _fadeSpeed * Time.deltaTime;  //血量效果隨著時間消失
        _bloodAmount = Mathf.Max(_bloodAmount, _minBloodAmount);  //確保血量效果不會掉到0以下
    }
    

    void OnRenderImage(RenderTexture src, RenderTexture dest)  //畫面特效
    {
        if(_shader == null)  //如果沒有shader 
        {
            return;  //返回
        }

        if(_material == null)  //如果沒有材質球
        {
            _material = new Material(_shader);  //設置一個材質球並附有shader
        }

        if(_material == null)  //如果沒有材質球
        {
            return;  //返回
        }

        if(_bloodTexture != null)
        {
            _material.SetTexture("_BloodTex", _bloodTexture);  //設置shader裡的血貼圖
        }

        if(_bloodNormalMap != null)
        {
            _material.SetTexture("_BloodBump", _bloodNormalMap);  //設置shader裡的法線貼圖
        }

        _material.SetFloat("_Distortion", _distortion);  //設置shader裡的法線值
        _material.SetFloat("_BloodAmount", _bloodAmount);  //設置shader裡的血量

        Graphics.Blit(src, dest, _material);  //執行處理過的圖像效果
    }
	
}
