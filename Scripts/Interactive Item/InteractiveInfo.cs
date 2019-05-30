using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveInfo : InteractiveItem
{
    [SerializeField]
    private string _infoText;  //顯示想要的文字

    public override string GetText()  //顯示文字 
    {
        return _infoText;
    }
}
