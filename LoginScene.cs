using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class User
{
    public static JSONObject current;  //目前登入的使用者
    public static string token;
}

public class LoginScene : MonoBehaviour
{
    public InputField loginAccount, loginPassword;  //帳號 密碼
    public InputField signAccount, signUpPassword, signUpPasswordConfirm, signUpEmail;  //註冊帳號 密碼 密碼二次確認 信箱
    public GameObject loginView, signView;  //登入畫面 註冊畫面
    public GameObject loginPanel;

    private void Start()
    {
        loginAccount.text = PlayerPrefs.GetString("acc", "");  //取得上次登入的帳號
        loginPassword.text = PlayerPrefs.GetString("pwd", "");  //密碼
    }

    public void OnLoginClick()  //登入
    {
        StartCoroutine(Login());
    }

    public void OnSignUpClick()  //註冊
    {
        StartCoroutine(SignUp());
    }

    public void OnSwitchToLogin()  //切換登入畫面
    {
        loginView.SetActive(true);
        signView.SetActive(false);
    }

    public void OnSwitchToSignUp()  //切換註冊畫面
    {
        loginView.SetActive(false);
        signView.SetActive(true);
    }

    public void OnSwitchMainScene()
    {
        loginPanel.SetActive(false);
    }

    IEnumerator Login()  //登入
    {
        //檢查是否留空未填
        if(string.IsNullOrEmpty(loginAccount.text) || string.IsNullOrEmpty(loginPassword.text))
        {
            Alert.Show("錯誤", "帳號與密碼不得空白");
            yield break;
        }

        using (var request = API.Login(loginAccount.text, loginPassword.text))
        {
            yield return request.SendWebRequest();  //發出請求

            if(request.responseCode != 200)
            {
                Alert.Show("登入失敗:", request.responseCode.ToString() + request.downloadHandler.text);
                yield break;
            }

            //將資料轉換為json
            var json = new JSONObject(request.downloadHandler.text);  //new JSONObject() 建立JSON物件，可支援字串、字典等來源格式。
            User.token = json["token"].str;  //儲存token
            User.current = json["user"];
            PlayerPrefs.SetString("acc", loginAccount.text);  //設置登入的帳號
            PlayerPrefs.SetString("pwd", loginPassword.text);  //密碼
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
            /*  if (json != null)
            {
                Alert.Show("登入成功", "歡迎! " + json["name"].str);
                print("歡迎! " + json["name"].str);
            }
            else
            {
                Alert.Show("登入失敗", "解析伺服器數據失敗");
            }*/
        }
    }

    IEnumerator SignUp()  //註冊
    {
        //檢查欄位空白
        if (string.IsNullOrEmpty(signAccount.text) || string.IsNullOrEmpty(signUpPassword.text) ||
            string.IsNullOrEmpty(signUpEmail.text))
        {
            Alert.Show("錯誤", "欄位不可空白");
            yield break;
        }

        //檢查密碼
        if(signUpPassword.text != signUpPasswordConfirm.text)
        {
            Alert.Show("錯誤", "兩次輸入密碼不一致");
            yield break;
        }

        using (var request = API.SignUp(signAccount.text, signUpPassword.text, signUpEmail.text))
        {
            yield return request.SendWebRequest();  //發出註冊請求

            if(request.responseCode != 200)
            {
                Alert.Show("錯誤", "註冊失敗!" + "錯誤代碼" + request.responseCode + "\n" + request.downloadHandler.text);
                yield break;
            }

            var json = new JSONObject(request.downloadHandler.text);
            if(json == null)
            {
                Alert.Show("錯誤", "json解析失敗");
                yield break;
            }

            Alert.Show("註冊成功",  "3秒後自動登入");
            loginAccount.text = signAccount.text;
            loginPassword.text = signUpPassword.text;
            Invoke("LoginInvoke", 3);
        }
    }

    public void LoginInvoke()
    {
        StartCoroutine(Login());
    }
}
