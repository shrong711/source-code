using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner : Photon.PunBehaviour
{
    
    public GameObject _prefab;
    [SerializeField]
    List<Transform> _spawnPoints = new List<Transform>();

    void Start()
    {
        if(_spawnPoints.Count == 0 || _prefab == null)
        {
            return;
        }
        if (PhotonNetwork.isMasterClient)
        {          
            Invoke("Time", 2);
        }       
    }

    void Time()
    {
        Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
        Vector3 s = spawnPoint.position;
        if (spawnPoint == null)
        {
            Debug.Log(PhotonNetwork.player.ID + "Null");
        }
        photonView.RPC("spawn", PhotonTargets.All, s);
    }

    [PunRPC]
    void spawn(Vector3 spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.Log(PhotonNetwork.player.ID + "Null");
            return;
        }
        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.Instantiate("Prefabs/AccessCode Card 1", spawnPoint, Quaternion.identity, 0);
        }      
        //Instantiate(_prefab, spawnPoint, Quaternion.identity);
    }
}
