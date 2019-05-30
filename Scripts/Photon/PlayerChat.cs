using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class PlayerChat : NetworkBehaviour
{
    public class ChatReceiveEvent : UnityEvent<int, string> { }
    public static ChatReceiveEvent onReceiveMessage = new ChatReceiveEvent();

    private void Awake()
    {
        if (isLocalPlayer)
        {
            onReceiveMessage.RemoveAllListeners();
        }
    }

    [Command]  //命令Server執行
    public void CmdSay(string msg)
    {
        RpcReceiveMessage(connectionToClient.connectionId, msg);  //取得玩家連線編號
    }

    [ClientRpc]  //Server要求Client執行
    void RpcReceiveMessage(int connID, string msg)
    {
        onReceiveMessage?.Invoke(connID, msg);
    }
}
