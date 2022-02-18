using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MCUIFramework;

public partial class LogoPage : UIPage
{
    	public Slider progressSlider;

    protected override string UIPath => "LogoPage";
    protected override void OnAwake() 
    {
        progressSlider = uitransform.Find("ProgressSlider").GetComponent<Slider>();

        OnStart();
    }
}
