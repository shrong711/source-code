using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPanelUI : MonoBehaviour
{
    [SerializeField]
    private Transform[] Grids;  //物品欄


    public Transform GetEmptyGrid()  //取得物品欄的物品
    {  
        for(int i = 0; i < Grids.Length; i++)
        {
            if(Grids[i].childCount == 0)  //如果子物件底下有物件 代表有物品
            {
                return Grids[i];
            }
            
        }
        return null;  //代表空間滿了
    }

}
