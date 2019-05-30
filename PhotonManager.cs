using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PhotonManager : Photon.PunBehaviour  //Photon.PunBehaviour經過 Photon 修改功能的 MonoBehaviour，基本功能與原本無異，主要是新增了跟網路功能相關的成員。
{  //建立玩家物件
    [SerializeField]
    GameObject[] startPositions;

    float lastEsc;

    void Awake()
    {
        CreatPlayerObject();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))  //離開遊戲功能
        {
            if(Time.time - lastEsc < 1f)
            {
                PhotonNetwork.LeaveRoom();
                UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
            }
            else
            {
                lastEsc = Time.time;
            }
        }
    }

    void CreatPlayerObject()
    {
        int id = PhotonNetwork.player.ID;
        GameObject startPosition = startPositions[id - 1];
        PhotonNetwork.Instantiate("Prefabs/Player", startPosition.transform.position, startPosition.transform.rotation, 0);
        print("id"+id);
        print("房間人數:" + PhotonNetwork.room.PlayerCount);
    }

    [ContextMenu("Find Start Positions")]  //可以將一個方法登記到編輯器中組件右上角的齒輪選單當中，以方便在編輯模式下執行特定操作
    private void FindStartPosistions()
    {
        GameObject[] startPositions = GameObject.FindGameObjectsWithTag("StartPosition");
        Array.Sort(startPositions, (x, y) => x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex()));  //對陣列內部進行排序，排序演算法會按照元素內容自動選擇
        this.startPositions = startPositions;
    }















   /* public const string VersionMark = "The end of the worldv1.0";  //驗證版本

    [SerializeField]
    Button joinBtn, randomJoinBtn, creatBtn;  //建立 隨機加速 加入房間 按鈕

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(VersionMark);  //與 Photon 伺服器建立連線，引數可傳入遊戲版本，以將不同遊戲版本的玩家分流。
    }

    public override void OnConnectedToMaster()  //當連接到Photon時 Pun會自動調用所有繼承自MonoBehaviour的腳本中的OnConnectedToMaster()方法
    {
        print("與Master Server建立連線");
        PhotonNetwork.JoinLobby();  //在Master Server上 會加入默認大廳 列出當前正在使用的房間  //與遊戲大廳連接，引數留空將連接到預設的大廳。
    }

    public override void OnJoinedLobby()  //進入大廳後 才啟用房間相關操作
    {
        print("進入大廳");
        joinBtn.interactable = true;  //打開按鈕
        randomJoinBtn.interactable = true;
        creatBtn.interactable = true;
    }*/



}
