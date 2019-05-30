using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPassCard : InteractiveGenericSwitch
{
    [SerializeField]
    private GameObject passCard;
    [SerializeField]
    private BoxCollider passCardCollider;

    public Material _material;
    public MeshFilter mesh;

    protected override void Start()
    {
        base.Start();
        _material.SetFloat("_OutlineWidth", 1.00f);
    }

    public override string GetText()
    {
        base.GetText();
        return "案E撿取卡片";
    }

    public override void Activate(CharacterManager characterManager)
    {
        base.Activate(characterManager);
        Debug.Log("玩家:" + PhotonNetwork.player.ID + "剪取卡片");
            ItemUIManager.Instance.StoreItem(3);
            //ak47.gameObject.SetActive(false);
            photonView.RPC("Getup", PhotonTargets.All);
        
    }

    [PunRPC]
    void Getup()
    {
        mesh.mesh = null;
        passCardCollider.enabled = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter");
        if (other.CompareTag("Player"))
            _material.SetFloat("_OutlineWidth", 1.1f);
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _material.SetFloat("_OutlineWidth", 1.00f);
    }
}
