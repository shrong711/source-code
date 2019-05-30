using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestruct : MonoBehaviour
{
    [SerializeField]
    private float _time = 10.0f;  //摧毀時間

    void Awake()
    {
        Invoke("DestroyNow", _time);  //10秒後摧毀
    }

    void DestroyNow()
    {
        DestroyObject(gameObject);
    }

}
