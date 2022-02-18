using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MCUIFramework;

public partial class DeckPage : UIPage
{
    	public Image cardArea;
	public Transform startPos;
	public Transform endPos;

    protected override string UIPath => "DeckPage";
    protected override void OnAwake() 
    {
        		cardArea = uitransform.Find("CardArea").GetComponent<Image>();
		startPos = uitransform.Find("startPos").GetComponent<Transform>();
		endPos = uitransform.Find("endPos").GetComponent<Transform>();

        OnStart();
    }
    
}
