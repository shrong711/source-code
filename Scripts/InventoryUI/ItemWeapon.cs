using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemWeapon : Item  //武器
{
    public int Damage { get; private set; }

    public ItemWeapon(int id, string name, string description, string icon, int damage)
                        :base(id, name, description, icon)
    {
        this.Damage = damage;
        base.ItemType = "ItemWeapon";
    }
}
