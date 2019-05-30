using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class ItemUIManager : MonoBehaviour  //背包管理
{
    private static ItemUIManager _instance;

    public static ItemUIManager Instance { get { return _instance; } }  //讓其他腳本可以抓
    public GridPanelUI GridPanelUI;  //得到GridPanelUI 從裡面得到一個空的格子
    public TooltipUI TooltipUI;  //要得到TooltipUI
    public DragItemUI DragItemUI;  //取得DragItemUI
    public CharacterManager CharacterManager;

    public Dictionary<int, Item> _itemLists;  //裝有物品訊息的字典
    public List<Sprite> _weapons;  //武器圖片
    public List<Sprite> _missions;  //任務物品圖片
    public List<Sprite> _consumable;  //消耗品圖片

    private bool isShow = false;
    private bool isDrag = false;
    private bool isEmpty = false;
    public bool isClosed = false;

    void Awake()
    {
        _instance = this;
        Load();  //開始前讀取物品
        //註冊委派事件
        GridUI.OnEnter += GridUI_OnEnter;
        GridUI.OnExit += GridUI_OnExit;
        GridUI.OnLeftBeginDrag += GridUI_OnLeftBeginDrag;
        GridUI.OnLeftEndDrag += GridUI_OnLeftEndDrag;
        GridUI.OnDoubleClick += GridUI_OnDoubleClick;
    }

    void Update()
    {
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GameObject.Find("ItemUI").transform as RectTransform,
                                                               Input.mousePosition, null, out position);  //把螢幕座標轉換成相對座標
        if (isShow)
        {
            TooltipUI.Show();
            TooltipUI.SetLocalPosition(position);
        }
        else if (isDrag)
        {
            DragItemUI.Show();
            DragItemUI.SetLocalPosition(position);
        }
    }

    #region 事件回調  
    private void GridUI_OnEnter(Transform gridTransform)
    {  //獲得Grid的數據
        Item item = ItemModel.GetItem(gridTransform.name);
        if(item == null)
        {
            Debug.Log("空");
            return;
        }
        string text = GetTooltipText(item);  //取得物品描述
        TooltipUI.UpdateToolTip(text);  //顯示物品描述
        isShow = true;
    }

    private void GridUI_OnExit()
    {
        isShow = false;
        TooltipUI.Hide();
    }

    private void GridUI_OnLeftBeginDrag(Transform gridTransform)
    {
        if(gridTransform == null)
        {
            return;
        }

        if(gridTransform.childCount == 0)
        {
            isEmpty = true;
            return;
        }
        else
        {
            Item item = ItemModel.GetItem(gridTransform.name);

            if(item.Id == 0)
            {
                DragItemUI.UpdateItem(_weapons[0]);
            }
            else if (item.Id == 1)
            {
                DragItemUI.UpdateItem(_weapons[1]);
            }
            else if (item.Id == 2)
            {
                DragItemUI.UpdateItem(_consumable[0]);
            }
            else if (item.Id == 3)
            {
                DragItemUI.UpdateItem(_missions[0]);
            }
            Destroy(gridTransform.GetChild(0).gameObject);
            isDrag = true;
            isEmpty = false;
        }
    }

    private void GridUI_OnLeftEndDrag(Transform prevTransform,Transform enterTransform)
    {     
        isDrag = false;
        DragItemUI.Hide();

        if (isEmpty)
        {
            return;
        }

        if(enterTransform.tag == "Trash")  //丟東西
        {
            ItemModel.DeleteItem(prevTransform.name);
            Debug.LogWarning("物品已丟");
        }
        else if(enterTransform.tag == "Grid")  //拖曳到另一個格子
        {
            if(enterTransform.childCount == 0)  //直接丟進去
            {
                Item item = ItemModel.GetItem(prevTransform.name);
                ItemModel.DeleteItem(prevTransform.name);  //刪除原來的數據
                this.CreatNewItem(item, enterTransform);
            }
            else  //交換
            {
                Destroy(enterTransform.GetChild(0).gameObject);  //刪除原來的物品
                //獲取數據
                Item prevGirdItem = ItemModel.GetItem(prevTransform.name);  
                Item enterGirdItem = ItemModel.GetItem(enterTransform.name);
                //交換兩個的物品
                this.CreatNewItem(prevGirdItem, enterTransform);
                this.CreatNewItem(enterGirdItem, prevTransform);
            }
        }
        else  //拖到UI其他地方 讓他還原
        {
            Item item = ItemModel.GetItem(prevTransform.name);
            this.CreatNewItem(item, prevTransform);
        }
    }

    private void GridUI_OnDoubleClick(Transform gridTransform)
    {
        Item item = ItemModel.GetItem(gridTransform.name);
        if (item == null)
        {
            Debug.Log("return");
            return;
        }
        
        if(item.Id == 2)
        {
            Debug.Log("補包");
            CharacterManager.AddHealth(10);
            ItemModel.DeleteItem(gridTransform.name);
            Destroy(gridTransform.GetChild(0).gameObject);
        }
                 
    }
    #endregion

    private void CreatNewItem(Item item, Transform parent)
    {
        GameObject itemPrefab = Resources.Load<GameObject>("ItemPrefabs/Item");  //取得欲置物路徑
        if(item == null)
        {
            return;
        }
        if (item.Id == 0)
        {
            itemPrefab.GetComponent<ItemUI>().UpdateItem(_weapons[0]);  //取得圖片
        }
        else if (item.Id == 1)
        {
            itemPrefab.GetComponent<ItemUI>().UpdateItem(_weapons[1]);  //取得圖片
        }
        else if (item.Id == 2)
        {
            itemPrefab.GetComponent<ItemUI>().UpdateItem(_consumable[0]);  //取得圖片
        }
        else if (item.Id == 3)
        {
            itemPrefab.GetComponent<ItemUI>().UpdateItem(_missions[0]);  //取得圖片
        }

        GameObject itemGo = GameObject.Instantiate(itemPrefab);  //複製物品

        itemGo.transform.SetParent(parent);  //把物品放到空的Grid底下
        itemGo.transform.localPosition = Vector3.zero;
        itemGo.transform.localScale = Vector3.one;

        ItemModel.StoreItem(parent.name, item);  //儲存訊息
    }

    private void Load()
    {
        _itemLists = new Dictionary<int, Item>();

        ItemWeapon w1 = new ItemWeapon(0, "步槍", "龍武的槍!", "", 10);
        ItemWeapon w2 = new ItemWeapon(1, "手槍", "就是手槍", "", 5);

        ItemConSumable c1 = new ItemConSumable(2, "補包", "吃了活跳跳", "", 0);

        ItemMission m1 = new ItemMission(3, "通行證", "通行無阻!!", "");

        _itemLists.Add(w1.Id, w1);
        _itemLists.Add(w2.Id, w2);
        _itemLists.Add(c1.Id, c1);
        _itemLists.Add(m1.Id, m1);
    }

    public void StoreItem(int itemId)  //儲存物品到物品欄
    {
        if (!_itemLists.ContainsKey(itemId))  //嘗試尋找物品看是否為空
        {
            return;
        }

        Transform emptyGrid = GridPanelUI.GetEmptyGrid();  //取得物品欄的物品

        if(emptyGrid == null)
        {
            Debug.LogWarning("背包已滿");
            return;
        }
        Item temp = _itemLists[itemId];  //找到這個東西的訊息 
        /*GameObject itemPrefab = Resources.Load<GameObject>("ItemPrefabs/Item");  //取得欲置物路徑

        if(itemType == 0)
        {
            itemPrefab.GetComponent<ItemUI>().UpdateItem(_weapons[itemId]);  //取得圖片
        }
        else if(itemType == 1)
        {
            itemPrefab.GetComponent<ItemUI>().UpdateItem(_missions[itemId]);  //取得圖片
        }
        else
        {
            itemPrefab.GetComponent<ItemUI>().UpdateItem(_consumable[itemId]);  //取得圖片
        }
     
        GameObject itemGo = GameObject.Instantiate(itemPrefab);  //複製物品

        itemGo.transform.SetParent(emptyGrid);  //把物品放到空的Grid底下
        itemGo.transform.localPosition = Vector3.zero;
        itemGo.transform.localScale = Vector3.one;

        ItemModel.StoreItem(emptyGrid.name, temp);  //儲存訊息*/     
        this.CreatNewItem(temp, emptyGrid);
              
    }

    private string GetTooltipText(Item item)  //取得物品描述
    {
        if(item == null)
        {
            return "";
        }

        StringBuilder sb = new StringBuilder();  //表示可變動的字元字串。 這個類別無法被繼承。
        sb.AppendFormat("<color=red><size=40>{0}</size></color>\n\n", item.Name);//將處理複合格式字串所傳回的字串 (其中包含零或更多的格式項目) 附加至這個執行個體。 每一個格式項目會由對應之物件引數的字串表示所取代。

        switch (item.ItemType)
        {
            case "ItemWeapon":  //武器
                 ItemWeapon itemWeapon = item as ItemWeapon;
                 sb.AppendFormat("<size=35>攻擊:{0}</size>\n\n", itemWeapon.Damage);
                 break;
            case "ItemConSumable":  //消耗品
                 ItemConSumable itemConsumable = item as ItemConSumable;
                 sb.AppendFormat("<size=35>Health:{0}</size>\n\n", itemConsumable.BackHp);
                 break;
            case "ItemMission":  //任務物品
                 ItemMission itemMission = item as ItemMission;
                 sb.AppendFormat("<size=35>卡片</size>\n\n");
                 break;
            default:
                 break;
        }
        sb.AppendFormat("<color=yellow><size=35>描述 :" + "{0}</size></color>", item.Description);
        return sb.ToString();
    }
}
