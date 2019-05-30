using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PathDisplayMode  
{
    None,  
    Connections,  //連接每個航點的線
    Paths  //航點路徑
}

public class AIWayPointNetwork : MonoBehaviour
{
    [HideInInspector]
    public PathDisplayMode DisplayMode = PathDisplayMode.Connections;  //默認顯示每個航點的線
    [HideInInspector]
    public int UIStart = 0;  //航線起點
    [HideInInspector]
    public int UIEnd = 0;  //航線終點

    public List<Transform> Waypoints = new List<Transform>();  //航點
}
