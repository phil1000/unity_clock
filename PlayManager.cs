using Vectrosity;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;

public class PlayManager : MonoBehaviour {
	
	// these variables are used to assign the images used for the 
	// face and bacground of the clock. They need to be specified as
	// public variables so they can be assigned within the Unity 
	// Workbench Inspector
	public OnGUIClockHands analogGuiClock;
	public float analogClockSize = 256;
	public float analogClockCenterSize = 32;
	public Texture analogClockBackground;
	public Texture analogClockCenter;
	public Texture2D boxBackground;
	public Texture2D myAvatar;
	public Texture2D background;
	public GUIStyle myButtonStyle;
	public GUIStyle myMarkerStyle;
	public GUIStyle myReturnButton;
	public GUIStyle myBoxStyle;
	
	private GUIStyle sliderBackgroundStyle;
	public GUIStyle thumbStyle;
	
	private GameObject myQuestions;
	private questionScript myScript;
	
	//private GameObject myClock;
	//private OnGUIClockHands myClockHandsScript;
	
	// the start, end hours & minutes should be provided as parameters
	// by either a calling scene or by getNextQuestion button
	private float startHours;
	private float startMinutes;
	private float endHours;
	private float endMinutes;
	private Boolean elapsedIsSnapped;
	/*
	 * The logic for the slider values should be as follows 
	 * ... 
	 * the min value should be 2 hours before the start hour and the 
	 * max value should be 2 hours after the end hour. 
	 * e.g. if start time is 4pm and end time is 8pm, min=2pm and
	 * max=10pm. Both are set to an hour mark i.e. zero minutes. 
	 * If the time increments are in 30 minute slots then we need
	 * double the number of slots between the start and end 
	 * hours i.e. if min = 2pm and max = 10pm then we need 18 notches
	 * .. if the increment is quarter hours then we would need 36 notches
	 * 
	 * This does mean that we cannot automatically use the slider value when moving the hands on the clock
	 * and that we have to translate the slider value into a time
	 * before sending the value into the draw function of OnGuiClock_mine
	 */
	private float sliderValue;
	private float maxSliderValue;
	private float minSliderValue;
	private string measure;
	private bool remainingQuestions=true;
	
	private GUIStyle myStyle; 
	private GUIStyle myBackgroundStyle;

	private CreateCurves curveScript;
	private ClockFunctions myClockFunctions;
	private DrawClockFunctions myDrawClocks;
	private SliderFunctions mySliderFunctions;
	private UserFeedbackFunctions myFeedbackFunctions;
	private Rect feedbackRect;
	
	void Start() {
		curveScript = Camera.main.GetComponent<CreateCurves>();
		myClockFunctions = new ClockFunctionsImpl ();
		mySliderFunctions = new SliderFunctionsImpl ();
		myDrawClocks = new DrawClockFunctionsImpl ();

		feedbackRect = new Rect (20, 20, 450, 300);
		myFeedbackFunctions = new UserFeedbackFunctionsImpl ();
	}
	
	void Awake() {
		myQuestions = GameObject.Find("Questions");
		myScript = (questionScript) myQuestions.GetComponent ("questionScript");
		this.elapsedIsSnapped = myScript.elapsedIsSnapped;
		loadNextQuestion ();
	}
	
	void OnGUI ()
	{
		// Make a background box
		GUI.Box(new Rect((Screen.width-820)/2,20,820,410), "", myBoxStyle);

		setStyles ();
		GUIStyle feedBox = new GUIStyle(GUI.skin.box);
		feedBox.normal.background = myAvatar;
		myFeedbackFunctions.updateStyles (feedBox, myStyle);

		//group buttons together .. then all button coordinates are relative to the group coordinates
		GUI.BeginGroup(new Rect(20, 150, 150, 450));
		//application quit button
		if (GUI.Button (new Rect (10,120,90,90), "Return", myReturnButton)) {
			Application.LoadLevel(0);
		}

		//next question button
		if (remainingQuestions) {
			if (GUI.Button (new Rect (10, 20, 90, 90), "Next \nQuestion", myButtonStyle)) {
				remainingQuestions = loadNextQuestion (); 
				mySliderFunctions.initialiseStartMarker();
				curveScript.removeCurves();
			} 
		}
		GUI.EndGroup();

		if (!remainingQuestions) {
			myFeedbackFunctions.giveFeedback (feedbackRect, "No More Questions");
		}

		//position and label start and end clocks
		GUI.BeginGroup(new Rect(Screen.width/2 - 148.0f, 30, 350, 450));
		float startEndClockSize = 128.0f;
		float centreSize = 12.0f;
		int temp = myStyle.fontSize;
		myStyle.fontSize = 12;
		myDrawClocks.positionClock (20, 20, startEndClockSize, startHours, startMinutes, 0.0f, "Start time", myStyle, analogGuiClock, analogClockBackground, analogClockCenter, centreSize);
		myDrawClocks.positionClock (20 + startEndClockSize + 20, 20, startEndClockSize, endHours, endMinutes, 0.0f, "End time", myStyle, analogGuiClock, analogClockBackground, analogClockCenter, centreSize);
		myStyle.fontSize = temp;
		GUI.EndGroup();

		//position and label all the slider widgets... clock and timeline
		//GUI.BeginGroup(new Rect(Screen.width/2 - 148.0f, 20, 350, 450));

		//load the timeline and slider clock
		Rect sliderRect = new Rect(190,480,1000,60);
		Event e = Event.current;
		if (e.isMouse && e.type == EventType.MouseDown && e.clickCount == 2) {
			mySliderFunctions.setStartMarkerValue(sliderValue);
		}

		//position and label the timeline
		sliderValue = GUI.HorizontalSlider (sliderRect, sliderValue, minSliderValue, maxSliderValue, sliderBackgroundStyle, thumbStyle);
		mySliderFunctions.assignLabels(0.0f, startMinutes, 12.0f, endMinutes, sliderRect.x, sliderRect.y, sliderRect.width, measure);
		
		//position the start marker
		if (remainingQuestions) {
			float tempSliderValue = mySliderFunctions.positionStartMarker ("blah",feedbackRect, sliderRect, minSliderValue, maxSliderValue, measure, startHours, startMinutes, endHours, endMinutes, myClockFunctions, myMarkerStyle, myFeedbackFunctions);
			if (tempSliderValue!=0.0f) sliderValue = tempSliderValue;
		}
		
		// Calculate and display the elapsed time. Nothing will be displayed if elapsed time is < 0.
		List<float> sliderTime = myClockFunctions.deriveSliderHoursMins(minSliderValue, maxSliderValue, sliderValue, measure);
		string elapsedString = myClockFunctions.deriveElapsedTimeString(sliderTime, elapsedIsSnapped, sliderValue, maxSliderValue, measure, startHours, startMinutes);
		if (!elapsedString.Equals ("")) {
			GUI.Box (new Rect (Screen.width/2 - 165.0f, 370, 350, 80), elapsedString, myStyle);
			curveScript.removeCurves();
			//curveScript.addFirstPoint (sliderTime, sliderRect.x + 4, sliderRect.width / 12.0f, 120, startHours, startMinutes);
			//curveScript.drawCurves (sliderTime, sliderRect.x + 4, sliderRect.width / 12.0f, 120, startHours, endHours, endMinutes);
			//curveScript.addLastPoint (sliderTime, sliderRect.x + 4, sliderRect.width / 12.0f, 120, startHours, endHours, endMinutes);
			curveScript.addCurves(sliderTime, sliderRect.x + 4, sliderRect.width / 12.0f, 120, startHours, startMinutes, endHours, endMinutes);
			writeCurveLabels(sliderRect.y-24);
			//GUI.Label(new Rect(100,400,200,50), "curveNo="+curveScript.getNumberOfCurves().ToString() + " sliderHrs="+sliderTime[0].ToString() + myString);
		} else curveScript.removeCurves();

		//position slider clock
		myDrawClocks.positionClock (Screen.width - analogClockSize - 580, 200, analogClockSize, sliderTime [0], sliderTime [1], 0.0f, "", myStyle, analogGuiClock, analogClockBackground, analogClockCenter, analogClockCenterSize);
		//GUI.EndGroup ();
	}

	void writeCurveLabels(float height) {
		GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.fontSize = 12;
		labelStyle.alignment = TextAnchor.MiddleCenter;
		List<CurvePointLabel> myLabels = curveScript.getCurvePointLables ();
		foreach (CurvePointLabel myLable in myLabels) {
			GUI.Label (new Rect(myLable.getLeft(), height, myLable.getWidth(), 30), myLable.getText(), labelStyle);
		}
	}

	bool loadNextQuestion() {
		Question myQuestion = myScript.getNextQuestion ();
		if (myQuestion != null) {
			this.startHours = myQuestion.getStartHours ();
			this.startMinutes = myQuestion.getsStartMinutes ();
			this.endHours = myQuestion.getEndHours ();
			this.endMinutes = myQuestion.getEndMinutes ();
			this.sliderValue = myQuestion.getSliderValue ();
			this.maxSliderValue = myQuestion.getMaxSliderValue ();
			this.minSliderValue = myQuestion.getMinSliderValue ();
			this.measure = myQuestion.getMeasure ();
			return true;
		} else {
			return false;
		}
	}	
	void setStyles() {
		sliderBackgroundStyle = new GUIStyle (GUI.skin.horizontalSlider);
		myStyle = new GUIStyle(GUI.skin.label); 

		myBackgroundStyle = new GUIStyle(GUI.skin.box);
		myStyle.fontSize = 18;
		myStyle.fontStyle = FontStyle.BoldAndItalic;
		myStyle.alignment = TextAnchor.MiddleCenter;
		myStyle.normal.textColor = Color.white;
		
		myBackgroundStyle.fontStyle = FontStyle.BoldAndItalic;
		myBackgroundStyle.alignment = TextAnchor.MiddleCenter;
		myBackgroundStyle.normal.textColor = Color.white;
		myBackgroundStyle.normal.background = background;
		myBackgroundStyle.fontSize = 14;
	}
}
