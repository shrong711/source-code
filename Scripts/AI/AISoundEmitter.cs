using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISoundEmitter : MonoBehaviour
{
    [SerializeField]
    private float _decayRate = 1.0f;  //半徑遞減時間
    
    private SphereCollider _collider = null;  //碰撞器
    private float _srcRadius = 0.0f;  //原本的半徑
    private float _tgtRadius = 0.0f;  //目標半徑
    private float _interpolator = 0.0f;  //轉換半徑的插植
    private float _interpolatorSpeed = 0.0f;  //插植速度

    void Awake()
    {
        _collider = GetComponent<SphereCollider>();  //取得碰撞器
        if (!_collider)  //是否為有效的碰撞器
        {
            return;
        }

        _srcRadius = _tgtRadius = _collider.radius;  //設置半徑
        _interpolator = 0.0f;

        if(_decayRate > 0.02f)
        {
            _interpolatorSpeed = 1.0f / _decayRate;  //計算速度
        }
        else
        {
            _interpolatorSpeed = 0.0f;
        }
    }

    void FixedUpdate()
    {
        if (!_collider)
        {
            return;
        }

        _interpolator = Mathf.Clamp01(_interpolator + Time.deltaTime * _interpolatorSpeed);  //計算差植
        _collider.radius = Mathf.Lerp(_srcRadius, _tgtRadius, _interpolator);  //緩慢的切換半徑

        if(_collider.radius < Mathf.Epsilon)  //測量半徑大小
        {
            _collider.enabled = false;  //如果半徑幾乎為0 關閉碰撞器
        }
        else
        {
            _collider.enabled = true;  //否則開啟
        }
    }

    public void SetRadius(float newRadius, bool instantResize = false)  //設置新半徑
    {
        if (!_collider || newRadius == _tgtRadius)
        {
            return;
        }

        _srcRadius = (instantResize || newRadius > _collider.radius) ? newRadius : _collider.radius;  //如果瞬間調整大小為true || 新半徑大於舊半徑 設為新半徑 否則設為原本的半徑
        _tgtRadius = newRadius;  //新半徑
        _interpolator = 0.0f;  //重置差值
    }  
}
