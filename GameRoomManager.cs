using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoomManager : Photon.PunBehaviour
{
    [SerializeField]
    PlayerInfoUI[] playerRows = new PlayerInfoUI[4];  //顯示房間玩家訊息

    void Start()
    {
        PhotonNetwork.automaticallySyncScene = true;
        RefreshList();
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)  //如果有玩家加入
    {
        RefreshList();
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)  //如果玩家離開
    {
        RefreshList();
    }
    //[0] = Player [1] = Hashtable
    public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {  //遊戲開始最少2人 & 由MasterClient統一進行轉場
        if(!PhotonNetwork.isMasterClient || PhotonNetwork.room.PlayerCount < 2)
        {
            Debug.Log(PhotonNetwork.room.PlayerCount);
            return;
        }
        //檢查每位玩家是否準備好
        foreach(var player in PhotonNetwork.playerList)
        {
            if (player.CustomProperties.ContainsKey("ready"))
            {
                continue;
            }
            return;
        }
        Debug.Log(PhotonNetwork.automaticallySyncScene);
        Debug.Log(PhotonNetwork.room.PlayerCount);
        PhotonNetwork.room.IsVisible = false;
        PhotonNetwork.room.IsOpen = false;
        PhotonNetwork.LoadLevel("GameStory");
    }

    void RefreshList()  //按照順序排列
    {
        var playerList = PhotonNetwork.playerList;
        System.Array.Sort(playerList, (a, b) => a.ID.CompareTo(b.ID));
        
        for(int i = 0; i < playerRows.Length; i++)
        {
            if(i < playerList.Length)
            {
                playerRows[i].Register(playerList[i]);
            }
            else
            {
                playerRows[i].Register(null);
            }
        }       
    }

    public void OnQuitClick()  //離開房間
    {
        PhotonNetwork.LeaveRoom();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
    }
}
