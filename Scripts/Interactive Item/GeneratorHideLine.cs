using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorHideLine : InteractiveItem
{
    [SerializeField]
    protected Material _material = null;
    [SerializeField]
    protected BoxCollider _boxcollider = null;

    protected override void Start()
    {
        base.Start();
        _material.SetFloat("_OutlineWidth", 1.00f);
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter");
        if (other.CompareTag("Player"))
            _material.SetFloat("_OutlineWidth", 1.02f);
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _material.SetFloat("_OutlineWidth", 1.00f);
    }
}
