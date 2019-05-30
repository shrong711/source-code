using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    [SerializeField]
    private Text outLineText = null;
    [SerializeField]
    private Text ContenText = null;

    public void UpdateToolTip(string text)  //更新文字內容
    {
        outLineText.text = text;  //控制邊框
        ContenText.text = text;  //控制顯示內容
    }

    public void Show()  //顯示文字
    {
        gameObject.SetActive(true);
    }

    public void Hide()  //隱藏文字
    {
        gameObject.SetActive(false);
    }

    public void SetLocalPosition(Vector2 position)
    {
        transform.localPosition = position;
    }
}
