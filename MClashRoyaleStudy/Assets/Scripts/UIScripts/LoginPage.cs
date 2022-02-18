using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MCUIFramework;

public partial class LoginPage : UIPage
{
    	public InputField accInputField;
	public InputField pwdInputField;
	public Button loginButton;

    protected override string UIPath => "LoginPage";
    protected override void OnAwake() 
    {
        		accInputField = uitransform.Find("AccInputField").GetComponent<InputField>();
		pwdInputField = uitransform.Find("PwdInputField").GetComponent<InputField>();
		loginButton = uitransform.Find("LoginButton").GetComponent<Button>();

        OnStart();
    }
    
}
