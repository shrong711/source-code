using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAk47 : InteractiveItem
{
    [SerializeField]
    private GameObject ak47 = null;
    [SerializeField]
    private BoxCollider ak47collider = null;

    public Material _material;
    public MeshFilter mesh;
    public AudioSource audios;

    protected override void Start()
    {
        base.Start();
        _material.SetFloat("_OutlineWidth", 1.00f);
    }

    public override string GetText()
    {
        return "按E獲取步槍";
    }

    public override void Activate(CharacterManager characterManager)
    {
        if (ak47)
        {
            audios.Play();
            mesh.mesh = null; 
            ItemUIManager.Instance.StoreItem(0);
            //ak47.gameObject.SetActive(false);
            ak47collider.enabled = false;
        }    
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter");
        if(other.CompareTag("Player"))    
        _material.SetFloat("_OutlineWidth", 1.08f);
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _material.SetFloat("_OutlineWidth", 1.00f);
    }

   /* protected override void OnTriggerEnter()
    {
        
    }*/

}
