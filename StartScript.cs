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
	public GUIStyle loginBoxStyle;
	public GUIStyle myOKButtonStyle;
	public GUIStyle nameLable;
	public GUIStyle nameText;
	public GUIStyle feedbackStyle;
	public Texture2D myAvatar;
	public float analogClockSize = 256;
	public float analogClockCenterSize = 32;
	public OnGUIClockHands myGuiClock;

	private UserFeedbackFunctions myFeedbackFunctions;
	private Rect feedbackRect;
	private DrawClockFunctions myDrawClocks;
	private GameObject myPlayers;
	private PlayerScript myPlayerScript;
	private Player myPlayer;
	private bool elapsedIsSnapped;
	private string name;
	private const string defaultString = "EnterYourNameHere";

	void Start() {
		myDrawClocks = new DrawClockFunctionsImpl ();
		name = defaultString;
		feedbackRect = new Rect (20, 20, 450, 350);
		myFeedbackFunctions = new UserFeedbackFunctionsImpl ();
	}

	void Awake() {
		myPlayers = GameObject.Find("Player");
		myPlayerScript = (PlayerScript) myPlayers.GetComponent ("PlayerScript");
		this.elapsedIsSnapped = myPlayerScript.elapsedIsSnapped;
		//loadNextQuestion ();
	}

	void OnGUI () {

		// Make a background box
		GUI.Box(new Rect(170,40,1024,512), "", myBoxStyle);

		// set the feedback style
		setFeedbackStyle ();

		//position and label the current time clock
		myDrawClocks.positionClock (((Screen.width/2) - (analogClockSize/2)), ((Screen.height/2) - (analogClockSize/2)), analogClockSize, "", myTextStyle, myGuiClock, analogClockBackground, analogClockCenter, analogClockCenterSize);

		if (myPlayerScript.isInvalidFiles ()) {
			//the config files are caput and so need to tell user this
			string feedbackString = "There is a problem with the config files so please correct and then re-run";
			myFeedbackFunctions.giveFeedback (feedbackRect, feedbackString);
			return;
		}

		if (myPlayerScript.isLoggedIn()) {

			myPlayer = myPlayerScript.getCurrentPlayer();

			string feedbackString = "Hi ";
			feedbackString = feedbackString + myPlayer.getName() + ". You are currently on Level ";
			feedbackString = feedbackString + myPlayer.getcurrentPlayerLevel().ToString () + "." + myPlayer.getcurrentQuestionLevel().getLevel().ToString();
			feedbackString = feedbackString + "\n Now click Practise to practice elapsed times or Play to test what you have learned";
			myFeedbackFunctions.giveFeedback (feedbackRect, feedbackString);

			//group buttons together .. then all button coordinates are relative to the group coordinates
			GUI.BeginGroup (new Rect (20, 150, 150, 400));

			// invoke play scene If Play is pressed
			if (GUI.Button (new Rect (10, 20, 90, 90), "Play", myButtonStyle)) {
				Application.LoadLevel (2); 
			}
		
			// invoke practice scene If Practice is pressed
			if (GUI.Button (new Rect (10, 120, 90, 90), "Practice", myButtonStyle)) {
				Application.LoadLevel (1);
			}

			// Quit the application
			if (GUI.Button (new Rect (10, 220, 90, 90), "Quit", myQuitButton)) {
				Application.Quit ();
			}

			GUI.EndGroup ();

		} else {
			myFeedbackFunctions.giveFeedback (feedbackRect, "Please login below ");

			// get the user to login or register
			GUI.BeginGroup (new Rect (10, 150, 400, 200),loginBoxStyle);
			GUI.Label (new Rect (100, 10, 300, 40), "Please enter your name then press OK");
			GUI.Label (new Rect (15, 70, 80, 40), "Name", nameLable);

			name = GUI.TextField(new Rect(100, 70, 200, 40), name, 40, nameText);

			if (GUI.Button (new Rect (140,140,121,40), "OK", myOKButtonStyle)) {
				// the following call will check if existing player and if so
				// will return a pointer to the player. If not, will create a
				// new player and return a pointer to the new player
				if (!name.Equals(defaultString)) myPlayer = myPlayerScript.getPlayer(name);
			}
			GUI.EndGroup ();
		}

	}

	private void setFeedbackStyle() {
		GUIStyle feedBox = new GUIStyle(GUI.skin.box);
		feedBox.normal.background = myAvatar;
		myFeedbackFunctions.updateStyles (feedBox, feedbackStyle);
	}
	                              
}
