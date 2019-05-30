using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class API  //網路呼叫方法
{
    const string host = "https://cu103-project-shrong.c9users.io/";  //網址

    public static UnityWebRequest Login(string account, string password)  //登入
    {
        string url = host + "login";  //連到User.PHP的login function

        WWWForm form = new WWWForm();  //POST
        form.AddField("account", account);  //輸入名稱
        form.AddField("password", password);  //密碼

        return UnityWebRequest.Post(url, form);  //發出請求
    }

    public static UnityWebRequest SignUp(string account, string password, string email)  //註冊帳號
    {
        string url = host + "users";  //連到User.PHP的creat function

        WWWForm form = new WWWForm();  //POST
        form.AddField("account", account);  //輸入名稱
        form.AddField("password", password);  //密碼
        form.AddField("email", email);  //信箱

        return UnityWebRequest.Post(url, form);  //發出註冊請求
    }
}
