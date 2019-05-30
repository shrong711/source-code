using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemMission : Item  //任務物品
{
    public ItemMission(int id, string name, string description, string icon)
                          :base(id, name, description, icon)
    {
        this.ItemType = "ItemMission";
    }
}
