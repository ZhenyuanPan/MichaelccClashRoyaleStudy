using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MCUIFramework;

public partial class GameOverPage : UIPage
{
    	public Button okBtn;
	public Text winnerText;

    protected override string UIPath => "GameOverPage";
    protected override void OnAwake() 
    {
        		okBtn = uitransform.Find("okBtn").GetComponent<Button>();
		winnerText = uitransform.Find("WinnerText").GetComponent<Text>();

        OnStart();
    }
    
}
