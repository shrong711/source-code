using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionObjective : Photon.PunBehaviour
{
     public CharacterManager ch;

    void Start()
    {
        ch = FindObjectOfType<CharacterManager>();
    }
    void OnTriggerEnter(Collider col)
    {
        if (GameSceneManager.instance)
        {
            PlayerInfo playerInfo = GameSceneManager.instance.GetPlayerInfo(col.GetInstanceID());
            if(playerInfo != null)
            {
                // photonView.RPC("Complete", PhotonTargets.All, playerInfo);
                //playerInfo.characterManager.DoLevelComplete();
                photonView.RPC("complate", PhotonTargets.All);
            }
        }
    }

    [PunRPC]
    void complate()
    {
        if (ch == null)
            return;

        ch.DoLevelComplete();
    }

}
