using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item  //物品封裝方法
{
    public int Id { get;  set; }
    public string Name { get;  set; }
    public string Description { get;  set; }
    public string Icon { get;  set; }
    public string ItemType { get;  set; }

    public Item(int id, string name, string description, string icon)
    {
        this.Id = id;
        this.Name = name;
        this.Description = description;
        this.Icon = icon;
    }
}
