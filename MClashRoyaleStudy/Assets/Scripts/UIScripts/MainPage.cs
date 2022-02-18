using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MCUIFramework;

public partial class MainPage : UIPage
{
    	public Button battlebtn;

    protected override string UIPath => "MainPage";
    protected override void OnAwake() 
    {
        		battlebtn = uitransform.Find("Battlebtn").GetComponent<Button>();

        OnStart();
    }
    
}
