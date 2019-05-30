using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField]
    GameObject emptyInfo, content;
    [SerializeField]
    InputField nameField;
    [SerializeField]
    Text readyLabel;
    [SerializeField]
    Toggle readyToggle;

    static int i = 0;
    static bool isShow = false;

    public PhotonPlayer player { get; private set; }

    void Start()
    {
        i = 0;
        isShow = false;
    }

    void Update()
    {
        //若無玩家則顯示無玩家訊息 暫時停止腳本
        if(player == null)
        {
            emptyInfo.SetActive(true);
            content.SetActive(false);
            enabled = false;
            return;
        }
        if (!player.IsLocal)
        {
            nameField.text = player.NickName;
        }
        bool isReady = player.CustomProperties.ContainsKey("ready");
        readyLabel.text = (isReady) ? "就緒" : "等待";
    }

    public void Register(PhotonPlayer player)  //當有玩家進入 將玩家與此UI連結
    {
        this.player = player;
        if(player == null)
        {
            return;
        }

        emptyInfo.SetActive(false);
        content.SetActive(true);
        enabled = true;

        if (player.IsLocal)
        {
            if(player.NickName == "")
            {
                //player.NickName = "玩家" + player.ID.ToString();
                player.NickName = PlayerPrefs.GetString("acc", "");
            }
            nameField.text = player.NickName;
            nameField.interactable = true;
            readyToggle.gameObject.SetActive(true);
        }
        else
        {
            nameField.interactable = false;
            readyToggle.gameObject.SetActive(false);
        }
    }

    public void OnReadyChanged(bool isReady)
    {
        var hash = player.CustomProperties;
        if (isReady && PhotonNetwork.room.PlayerCount > 1)
        {
            hash.Add("ready", 1);
        }
        else
        {
            hash["ready"] = null;
            readyToggle.isOn = false;
            isShow = true;
            i++;                     
        }

        if (isShow == true && i > 1)
        {
            isShow = false;
            Alert.Show("錯誤", "至少需要兩位玩家");
        }
        isShow = false;
        player.SetCustomProperties(hash);  //使用此方法修改玩者自訂屬性，Photon會自動進行同步
    }

    public void OnNameEdited(string newName)
    {
        PhotonNetwork.playerName = newName;
    }
}
