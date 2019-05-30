using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaterialController
{
    [SerializeField]
    protected Material Material = null;  //材質球
    [SerializeField]
    protected Texture _diffuseTexture = null;  //漫反射圖片
    [SerializeField]
    protected Color _diffuseColor = Color.white;  //漫反射顏色
    [SerializeField]
    protected Texture _normalMap = null;  //法線圖片
    [SerializeField]
    protected float _normalStrength = 1.0f;  //強度

    [SerializeField]
    protected Texture _emissiveTexture = null;  //發光圖片
    [SerializeField]
    protected Color _emissionColor = Color.black;  //顏色
    [SerializeField]
    protected float _emissionScale = 1.0f;  //增加顏色大小 HDR

    protected MaterialController _backup = null;  //儲存資料
    protected bool _started = false;

    public Material material { get { return Material; } }

    public void OnStart()
    {
        if(Material == null || _started)
        {
            return;
        }
        _started = true;
        _backup = new MaterialController();  //儲存資料

        _backup._diffuseColor = Material.GetColor("_Color");  //取得顏色
        _backup._diffuseTexture = Material.GetTexture("_MainTex");  //從標準shader裡取得紋理
        _backup._emissionColor = Material.GetColor("_EmissionColor");  //取得發光顏色
        _backup._emissionScale = 1;
        _backup._emissiveTexture = Material.GetTexture("_EmissionMap");  //取得發光紋理
        _backup._normalMap = Material.GetTexture("_BumpMap");  //法線貼圖
        _backup._normalStrength = Material.GetFloat("_BumpScale");

        if (GameSceneManager.instance)
        {
            GameSceneManager.instance.RegisterMaterialController(Material.GetInstanceID(), this);  //註冊這個材質球ID到遊戲管理
        }
    }

    public void Activate(bool activate)
    {
        if(!_started || Material == null)
        {
            return;
        }

        if (activate)
        {
            Material.SetColor("_Color", _diffuseColor);
            Material.SetTexture("_MainTex", _diffuseTexture);
            Material.SetColor("_EmissionColor", _emissionColor * _emissionScale);
            Material.SetTexture("_EmissionMap", _emissiveTexture);
            Material.SetTexture("_BumpMap", _normalMap);
            Material.SetFloat("_BumpScale", _normalStrength);
        }
        else
        {
            Material.SetColor("_Color", _backup._diffuseColor);
            Material.SetTexture("_MainTex", _backup._diffuseTexture);
            Material.SetColor("_EmissionColor", _backup._emissionColor * _backup._emissionScale);
            Material.SetTexture("_EmissionMap", _backup._emissiveTexture);
            Material.SetTexture("_BumpMap", _backup._normalMap);
            Material.SetFloat("_BumpScale", _backup._normalStrength);
        }
    }

    public void OnReset()
    {
        if(_backup == null || Material == null)
        {
            return;
        }

        Material.SetColor("_Color", _backup._diffuseColor);
        Material.SetTexture("_MainTex", _backup._diffuseTexture);
        Material.SetColor("_EmissionColor", _backup._emissionColor * _backup._emissionScale);
        Material.SetTexture("_EmissionMap", _backup._emissiveTexture);
        Material.SetTexture("_BumpMap", _backup._normalMap);
        Material.SetFloat("_BumpScale", _backup._normalStrength);
    }

    public int GetInstanceID()
    {
        if(Material == null)
        {
            return -1;
        }

        return Material.GetInstanceID();
    }
}
