using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemConSumable : Item  //補品
{
    public int BackHp { get; private set; }

    public ItemConSumable(int id, string name, string description, string icon, int backHp):base(id, name, description, icon)
    {
        this.BackHp = backHp;
        base.ItemType = "ItemConsumable";
    }
}
