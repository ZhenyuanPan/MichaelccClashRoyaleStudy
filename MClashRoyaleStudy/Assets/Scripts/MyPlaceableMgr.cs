using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlaceableMgr : MonoBehaviour
{
    public static MyPlaceableMgr instance;
    private void Awake()
    {
        instance = this;
    }
    public List<MyPlaceableView> friendlyPlaceable = new List<MyPlaceableView>();
    public List<MyPlaceableView> enemyPlaceable = new List<MyPlaceableView>();

}
