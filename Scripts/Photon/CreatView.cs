using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatView : MonoBehaviour
{
    [SerializeField]
    InputField nameField, passwordField, infoField;  //房間 密碼 描敘
    [SerializeField]
    Dropdown playerCountSelector;  //選擇人數
    [SerializeField]
    Toggle publicToggle;  //是否公開
    [SerializeField]
    LobbyManager lobbyManager;
    

    void Start()
    {

    }

    public void OnCreateClick()
    {
        string roomName = nameField.text;  //取得房間名稱
        if (string.IsNullOrWhiteSpace(roomName))  //如果為空值
        {
            return;  //返回
        }

        int maxPlayers = playerCountSelector.value * 2 + 2;  //最大玩家人數
        bool isVisible = publicToggle.isOn;  //是否公開

        //建立房間基本設定
        RoomOptions options = new RoomOptions  //包含創建房間時所需的公共房間屬性
        {
            MaxPlayers = (byte)maxPlayers,  //取得可隨時在房間的最大玩家數量
            IsVisible = isVisible  //定義這個房間是否在大廳列出
        };

        //建立自訂房間設定
        var customOption = new ExitGames.Client.Photon.Hashtable  //這是Hashtable類的替代品 使用Dictionary<object, object>作為基礎
        {
            { "info", infoField.text},
            { "password", passwordField.text}
        };
        options.CustomRoomProperties = customOption;  //設置房間自定義屬性
        options.CustomRoomPropertiesForLobby = new string[] { "password" };  //定義大廳中列出的自定義房間屬性

        lobbyManager.CreateRoom(roomName, options);
    }
    

}
