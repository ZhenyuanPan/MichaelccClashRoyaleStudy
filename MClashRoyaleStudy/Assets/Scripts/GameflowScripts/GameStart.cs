using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCUIFramework;
public class GameStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //游戏入口脚本, 启动UI界面
        UIRoot.SetInitResolutionParmas(new Vector2(1600,900));
        UIPage._ShowPage<LogoPage>(true);
    }

}
