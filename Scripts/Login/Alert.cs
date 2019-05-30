using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Alert : MonoBehaviour  //提示視窗
{
    [SerializeField]
    Text titleLbl, messageLbl;  //抬頭文字 訊息

    static Alert prefab;  //欲置物

    public string title
    {
        get
        {
            return titleLbl.text;
        }
        set
        {
            titleLbl.text = value;
        }
    }

    public string content
    {
        get
        {
            return messageLbl.text;
        }
        set
        {
            messageLbl.text = value;
        }
    }

    public static void Show(string title, string message)  //秀出錯誤訊息
    {
        if(prefab == null)
        {
            //從Resource資料夾中載入欲置物
            prefab = Resources.Load<Alert>("Alert");
        }

        var alert = Instantiate(prefab);
        alert.title = title;
        alert.content = message;
    }

    public void OnClose()
    {
        Destroy(gameObject);
    }


}
