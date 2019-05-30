using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class GridUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public static Action<Transform> OnEnter;
    public static Action OnExit;

    public void OnPointerEnter(PointerEventData eventData)  //滑鼠移到物品
    {
        if(eventData.pointerEnter.tag == "Grid")
        {
            if(OnEnter != null)
            {
                OnEnter(transform);  //傳進位置
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)  //離開
    {
        if(eventData.pointerEnter.tag == "Grid")
        {
            if(OnExit != null)
            {
                OnExit();
            }
        }
    }
    //當左鍵開始拖曳與當左鍵放開時的方法
    public static Action<Transform> OnLeftBeginDrag;
    //第一個Transform:原來的格子 第二個Transform:進入的格子
    public static Action<Transform, Transform> OnLeftEndDrag;

    public void OnBeginDrag(PointerEventData eventData)  //剛開始拖曳
    {
        if(eventData.button == PointerEventData.InputButton.Left)  //當滑鼠左鍵被按下
        {
            if(OnLeftBeginDrag != null)  //當委派方法不為null時(也就是有註冊時 呼叫委派方法)
            {
                OnLeftBeginDrag(transform);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)  //持續拖曳中
    {

    }

    public void OnEndDrag(PointerEventData eventData)  //結束拖曳
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            if(OnLeftEndDrag != null)
            {
                if(eventData.pointerEnter == null)
                {
                    OnLeftEndDrag(transform, null);
                }
                else
                {
                    OnLeftEndDrag(transform, eventData.pointerEnter.transform);
                }                                            
            }         
        }
    }

    public static Action<Transform> OnDoubleClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.clickCount == 2 && eventData.pointerEnter.tag == "Grid")
        {
            Debug.Log("Double Click");
            OnDoubleClick(transform);
        }
    }
}
