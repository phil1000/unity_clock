using UnityEngine;
using System.Collections;
using System;
using AssemblyCSharp;

public class StartScript : MonoBehaviour {

	// thse variables are public so that textures can be configures
	// within the Unity Inspector window.
	public Texture2D boxBackground;
	public Texture analogClockBackground;
	public Texture analogClockCenter;
	public GUIStyle myTextStyle;
	public GUIStyle myButtonStyle;
	public GUIStyle myQuitButton;
	public GUIStyle myBoxStyle;
	public float analogClockSize = 256;
	public float analogClockCenterSize = 32;
	public OnGUIClockHands myGuiClock;

	private DrawClockFunctions myDrawClocks;

	void Start() {
		myDrawClocks = new DrawClockFunctionsImpl ();
	}

	void OnGUI () {

		// Make a background box
		GUI.Box(new Rect(170,40,1024,512), "", myBoxStyle);
		
		//position and label the current time clock
		myDrawClocks.positionClock (((Screen.width/2) - (analogClockSize/2)), ((Screen.height/2) - (analogClockSize/2)), analogClockSize, "", myTextStyle, myGuiClock, analogClockBackground, analogClockCenter, analogClockCenterSize);

		//group buttons together .. then all button coordinates are relative to the group coordinates
		GUI.BeginGroup(new Rect(20, 20, 150, 400));

		// invoke play scene If Play is pressed
		if(GUI.Button(new Rect(10,20,90,90), "Play", myButtonStyle)) {
			Application.LoadLevel(2); 
		}
		
		// invoke practice scene If Practice is pressed
		if(GUI.Button(new Rect(10,120,90,90), "Practice", myButtonStyle)) {
			Application.LoadLevel(1);
		}

		// Quit the application
		if (GUI.Button (new Rect (10,220,90,90), "Quit", myQuitButton)) {
			Application.Quit();
		}

		GUI.EndGroup();

	}
}
