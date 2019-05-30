using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFirstAid : InteractiveItem
{
    [SerializeField]
    private GameObject FirstAid = null;
    [SerializeField]
    private BoxCollider firstAidcollider = null;


    public Material _material;
    public MeshFilter mesh = null;
    public AudioSource audiosource = null;

    protected override void Start()
    {
        base.Start();
        _material.SetFloat("_OutlineWidth", 1.00f);
    }

    public override string GetText()
    {
        return "按E獲得補包";
    }

    public override void Activate(CharacterManager characterManager)
    {
        audiosource.Play();
        ItemUIManager.Instance.StoreItem(2);
        photonView.RPC("AID", PhotonTargets.All);
       /* if (FirstAid)
        {
            audiosource.Play();
            mesh.mesh = null;
            ItemUIManager.Instance.StoreItem(2);
            firstAidcollider.enabled = false;
        }*/
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter");
        if (other.CompareTag("Player"))
            _material.SetFloat("_OutlineWidth", 1.03f);
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _material.SetFloat("_OutlineWidth", 1.00f);
    }

    [PunRPC]
    void AID()
    {      
        mesh.mesh = null;       
        firstAidcollider.enabled = false;
    }
}
