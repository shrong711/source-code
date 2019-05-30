using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Image _image = null;


    public void UpdateItem(Sprite image)
    {
        _image.sprite = image;
    }
	
}
