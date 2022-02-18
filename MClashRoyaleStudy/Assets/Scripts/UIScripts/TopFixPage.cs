using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MCUIFramework;

public partial class TopFixPage : UIPage
{
    	public Button coinButton;
	public Button gemButton;
	public Slider expSlider;

    protected override string UIPath => "TopFixPage";
    protected override void OnAwake() 
    {
        		coinButton = uitransform.Find("CoinButton").GetComponent<Button>();
		gemButton = uitransform.Find("GemButton").GetComponent<Button>();
		expSlider = uitransform.Find("ExpSlider").GetComponent<Slider>();

        OnStart();
    }
    
}
