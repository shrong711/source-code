using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatView : MonoBehaviour
{
    [SerializeField]
    ScrollRect scrollView;
    [SerializeField]
    InputField inputField;
    [SerializeField]
    Text template;

    void Start()
    {
        template.gameObject.SetActive(false);
        PhotonNetwork.OnEventCall += OnReciveMessage;  //註冊事件
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            inputField.ActivateInputField();
        }

        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrWhiteSpace(inputField.text) || Input.GetKeyDown(KeyCode.KeypadEnter) && !string.IsNullOrWhiteSpace(inputField.text))
        {
            Send();
            inputField.text = "";
        }
    }

    void Send()
    {
        byte evCode = 0; //事件的分組 可用0~200
        bool reliable = true;  //是否可靠傳輸
        RaiseEventOptions eventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All};  //事件的一些選項 例如傳輸的對象 是否快取等
        PhotonNetwork.RaiseEvent(evCode, inputField.text, reliable, eventOptions);
    }

    void OnReciveMessage(byte evCode, object content, int senderID)  //senderID 發送此事件的玩家編號
    {
        string nickName = PhotonPlayer.Find(senderID).NickName;
        string message = (string)content;
        template.text = $"<color=#79F>{ nickName}:</color>{message}";  //玩家:輸入的訊息
        Text row = Instantiate(template, scrollView.content);
        row.gameObject.SetActive(true);
        StartCoroutine(ScrollToBottom());
    }

    IEnumerator ScrollToBottom()  //收到訊息後滾動到最底部
    {
        yield return null;
        scrollView.verticalNormalizedPosition = 0f;
    }
}
