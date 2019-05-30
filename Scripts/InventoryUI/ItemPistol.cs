using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPistol : InteractiveItem
{
    [SerializeField]
    private GameObject pistol = null;
    [SerializeField]
    private BoxCollider pistolcollider = null;

    public Material _material;
    public MeshFilter mesh;
    public AudioSource audiosource;

    protected override void Start()
    {
        base.Start();
        _material.SetFloat("_OutlineWidth", 1.00f);
    }

    public override string GetText()
    {
        return "按E獲取手槍";
    }

    public override void Activate(CharacterManager characterManager)
    {
        if (pistol)
        {
            audiosource.Play();
            mesh.mesh = null;
            ItemUIManager.Instance.StoreItem(1);
            //pistol.gameObject.SetActive(false);
            pistolcollider.enabled = false;
        }    
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter");
        if (other.CompareTag("Player"))
            _material.SetFloat("_OutlineWidth", 1.11f);
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _material.SetFloat("_OutlineWidth", 1.00f);
    }
}
