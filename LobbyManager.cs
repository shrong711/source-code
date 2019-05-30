using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class LobbyManager : Photon.PunBehaviour  //Photon.PunBehaviour經過 Photon 修改功能的 MonoBehaviour，基本功能與原本無異，主要是新增了跟網路功能相關的成員。
{
    public const string VersionMark = "The end of the worldv1.0";  //驗證版本

    [SerializeField]
    Button joinBtn, randomJoinBtn, creatBtn;  //建立 隨機加速 加入房間 按鈕
    [SerializeField]
    ScrollRect scrollView;  //大廳

    //Transform template;  //房間列表位置
    RoomInfo[] roomList;  //房間清單
    Button selectedButton;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(VersionMark);  //與 Photon 伺服器建立連線，引數可傳入遊戲版本，以將不同遊戲版本的玩家分流。
        scrollView.content.GetChild(0).gameObject.SetActive(false);  //關閉房間清單
    }

    void RefreshList()
    {
        GameObject template = scrollView.content.GetChild(0).gameObject;  //取得房間
        template.SetActive(true);
        Text[] labels = template.GetComponentsInChildren<Text>();  //取得房間下的Text

        //先清空原本的列表
        for(int i = 1; i < scrollView.content.childCount; i++)
        {
            Destroy(scrollView.content.GetChild(i).gameObject);
        }
     
        roomList = PhotonNetwork.GetRoomList();  //當有房間時取得房間列表
        foreach(RoomInfo room in PhotonNetwork.GetRoomList())
        {
            labels[0].text = room.Name;  //房名
            labels[1].text = $"{room.PlayerCount}/{room.MaxPlayers}";  //
            labels[2].text = (room.CustomProperties["password"].ToString() != "") ? "是" : "";
            Instantiate(template, scrollView.content);
        }

        template.SetActive(false);
    }

    public override void OnReceivedRoomListUpdate()  //當接收到房間清單的更新通知時，會被執行。
    {
        RefreshList();
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
    }

    public void CreateRoom(string roomName, RoomOptions options)  //建立房間
    {
        print("創建房間");
        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
    {
        print($"{codeAndMsg[0]}:{codeAndMsg[1]}");  //codeAndMsg:['錯誤代碼', '錯誤訊息']
    }

    public override void OnJoinedRoom()
    {
        print($"以加入房間 房間目前有{PhotonNetwork.room.PlayerCount} 人");
        print(PhotonNetwork.room.ToStringFull());
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameRoom"); //轉入房間場景
    }  

    public void OnRoomSelected(Button sender)  //選擇房間
    {
        sender.interactable = false;  
        if(selectedButton != null)
        {
            selectedButton.interactable = true;
        }

        selectedButton = sender;
    }

    public void OnJoinRoomClick()  //加入房間
    {
        if(selectedButton == null)
        {
            return;
        }

        int index = selectedButton.transform.GetSiblingIndex() - 1;  //找到這個物件的層級
        RoomInfo room = roomList[index];  //取得房間
        if((string)room.CustomProperties["password"] == "")  //如果沒有密碼
        {
            PhotonNetwork.JoinRoom(room.Name);  //直接加入
        }
        else
        {
            //詢問密碼
        }
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)  //加入房間錯誤提示
    {
        print($"{codeAndMsg[0]}:{codeAndMsg[1]}");
    }

    public void OnJoinRandomClick()  //隨機加入
    {
        ExitGames.Client.Photon.Hashtable options = new ExitGames.Client.Photon.Hashtable
        {
            { "password", ""}
        };
        PhotonNetwork.JoinRandomRoom(options, 0);
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)  //隨機加入錯誤提示
    {
        print($"{codeAndMsg[0]}:{codeAndMsg[1]}");
    }

    public void OnSearchValueChanged(string value)  //搜索功能
    {
        for(int i = 0; i < roomList.Length; i++)  //找到所有房間
        {
            bool active = roomList[i].Name.Contains(value);  //如果有這個房名
            scrollView.content.GetChild(i + 1).gameObject.SetActive(active);
        }
    }
}
