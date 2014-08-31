using Vectrosity;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;

public class PracticeManager : MonoBehaviour {
	
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
	public GUIStyle timerStyle;
	public GUIStyle myButtonStyle;
	public GUIStyle myPlusMinusButtonStyle;
	public GUIStyle myOKButtonStyle;
	public GUIStyle HrMinLabel;
	public GUIStyle HrMinText;
	public GUIStyle myMarkerStyle;
	public GUIStyle myReturnButton;
	public GUIStyle myBoxStyle;
	
	private GUIStyle sliderBackgroundStyle;
	public GUIStyle thumbStyle;
	
	//private GameObject myClock;
	//private OnGUIClockHands myClockHandsScript;
	
	// the start, end hours & minutes should be provided as parameters
	// by either a calling scene or by getNextQuestion button
	private float startHours;
	private float startMinutes;
	private float endHours;
	private float endMinutes;
	private float workingHours;
	private float workingMinutes;
	private List<float> workingTime;
	private List<float> answer;
	private bool sliderGTElapsed=false;
	private bool endTimeInvalid = false;
	private Boolean elapsedIsSnapped;

	private float sliderValue;
	private float maxSliderValue;
	private float minSliderValue;
	private string measure;

	private GUIStyle myStyle; 
	private GUIStyle myBackgroundStyle;
	
	private CreateCurves curveScript;
	private ClockFunctions myClockFunctions;
	private DrawClockFunctions myDrawClocks;
	private SliderFunctions mySliderFunctions;
	private UserFeedbackFunctions myFeedbackFunctions;
	private Rect feedbackRect;

	private bool addStartState=true;
	private bool addEndState=false;
	private bool practiceState_startNotSet=false;
	private bool practiceState=false;

	private float curveCallCount=0.0f;
	private string answerStr="";

	void Start() {
		curveScript = Camera.main.GetComponent<CreateCurves>();
		myClockFunctions = new ClockFunctionsImpl ();
		mySliderFunctions = new SliderFunctionsImpl ();
		myDrawClocks = new DrawClockFunctionsImpl ();
		
		feedbackRect = new Rect (20, 20, 450, 350);
		myFeedbackFunctions = new UserFeedbackFunctionsImpl ();
		answer = new List<float> ();
	}

	void OnGUI ()
	{
		// Make a background box
		GUI.Box(new Rect((Screen.width-820)/2,20,820,410), "", myBoxStyle);
		
		setStyles ();
		GUIStyle feedBox = new GUIStyle(GUI.skin.box);
		feedBox.normal.background = myAvatar;
		myFeedbackFunctions.updateStyles (feedBox, myStyle);

		//add the "New" and "Quit" buttons
		addButtons (new Rect(20, 150, 150, 450));

		Rect startEndGroup = new Rect(Screen.width/2 - 148.0f, 30, 350, 450);
		if (addStartState) {
			/* Come into this state on entry or when new button pressed
			 * The user is presented with a widget to select the start time
			 * Once a start has been selected, the user is presented with
			 * a clock showing the selected start time and the state moves to the
			 * addendstate
			 */

			if (addStartStateWidgets(startEndGroup)) {
				addStartState=false;
				addEndState=true;
			}
		}
		if (addEndState) {
			/* Come into this state once startstate has completed
			 * The user is presented with both the start clock and a widget to select the end time
			 * Once a end has been selected, the user is presented with
			 * a second clock showing the selected end time and the state moves to the
			 * practiceState_startNotSet
			 */
			
			if (addEndStateWidgets(startEndGroup)) {
				addEndState=false;
				practiceState_startNotSet=true;
			}
		}
		if (practiceState_startNotSet) {
			/* Come into this state once endstate has completed
			 * The user is presented with the start clock, end clock, slider and is asked to
			 * double click to set the start point. They are not able to move the slider at this point.
			 * Once the start point has been selected, the state moves to the
			 * practiceState
			 */
			
			if (practiceState_startNotSetWidgets(startEndGroup)) {
				practiceState_startNotSet=false;
				practiceState=true;
			}
		}
		if (practiceState) {
			/* Come into this state once the start point has been selected
			 * The user is presented with the start clock, end clock, slider, slider clock and is asked to
			 * move the slider and to see what happens with the elasped time and curves
			 */
			
			practiceStateWidgets(startEndGroup);
		}

		//GUI.Label (new Rect (20, 500, 100, 40), "count=" + curveCallCount.ToString ());

	}

	void addButtons(Rect buttonRect) {
		//group buttons together .. then all button coordinates are relative to the group coordinates
		GUI.BeginGroup(buttonRect);
		//application quit button
		if (GUI.Button (new Rect (10,120,90,90), "Return", myReturnButton)) {
			Application.LoadLevel(0);
		}
		
		//New button
		if (GUI.Button (new Rect (10, 20, 90, 90), "New", myButtonStyle)) {
			curveScript.removeCurves();
			mySliderFunctions.initialiseStartMarker();
			addStartState=true;
			addEndState=false;
			practiceState_startNotSet=false;
			practiceState=false;
			startHours=0.0f;
			startMinutes=0.0f;
			endHours=0.0f;
			endMinutes=0.0f;
			workingHours=0.0f;
			workingMinutes=0.0f;
			answer.Clear();
			endTimeInvalid = false;
			curveCallCount=0.0f;
		} 
		GUI.EndGroup();
	}

	bool addStartStateWidgets(Rect startEndGroup) {

		bool timeConfirmed = false;
		bool moveToNextState = false;
		measure = "whole";

		myFeedbackFunctions.giveFeedback (feedbackRect, "Start by entering a start time and then press OK");

		//present the time selector widget 
		GUI.BeginGroup (new Rect (548, 210, 141, 175),timerStyle);

		workingTime = myClockFunctions.setTime (workingHours, workingMinutes, HrMinLabel, HrMinText, myPlusMinusButtonStyle);
		workingHours = workingTime [0];
		workingMinutes = workingTime [1];

		startHours = workingHours;
		startMinutes = workingMinutes;
		if (GUI.Button (new Rect (10,110,121,40), "OK", myOKButtonStyle)) {
			if ((startHours != 0.0f) || (startMinutes != 0.0f)) timeConfirmed=true;
		}
		GUI.EndGroup ();

		GUI.BeginGroup (startEndGroup);
		// show the time selected on the start clock
		float startEndClockSize = 128.0f;
		float centreSize = 12.0f;
		int temp = myStyle.fontSize;
		myStyle.fontSize = 12;
		myDrawClocks.positionClock (20, 20, startEndClockSize, startHours, startMinutes, 0.0f, "Start time", myStyle, analogGuiClock, analogClockBackground, analogClockCenter, centreSize);
		myStyle.fontSize = temp;
		GUI.EndGroup ();

		if (timeConfirmed) {
			moveToNextState=true;
			workingHours=0.0f;
			workingMinutes=0.0f;
		}

		return moveToNextState;
	}

	bool addEndStateWidgets(Rect startEndGroup) {

		bool moveToNextState = false;
		bool timeConfirmed = false;

		if (!endTimeInvalid) myFeedbackFunctions.giveFeedback (feedbackRect, "Great. Now enter a finish time and then press OK");
		else myFeedbackFunctions.giveFeedback (feedbackRect, "Oops - The end time is not later than the start time. Please try again.");

		//present the time selector widget 
		GUI.BeginGroup (new Rect (697, 210, 141, 175),timerStyle);
		//setTime ();
		workingTime = myClockFunctions.setTime (workingHours, workingMinutes, HrMinLabel, HrMinText, myPlusMinusButtonStyle);
		workingHours = workingTime [0];
		workingMinutes = workingTime [1];
		// end of new code
		endHours = workingHours;
		endMinutes = workingMinutes;
	
		if (GUI.Button (new Rect (10,110,121,40), "OK", myOKButtonStyle)) {
			if ((endHours != 0.0f) || (endMinutes != 0.0f)) {
				if ((endHours<startHours) || ((endHours==startHours) && (endMinutes <= startMinutes))) {
					endTimeInvalid=true;
				} else {
					measure = setMeasure(startMinutes, endMinutes);
					timeConfirmed=true;
					endTimeInvalid=false;
				}
			}
		}
		GUI.EndGroup ();

		//position and label start and end clocks
		GUI.BeginGroup(startEndGroup);
		float startEndClockSize = 128.0f;
		float centreSize = 12.0f;
		int temp = myStyle.fontSize;
		myStyle.fontSize = 12;
		// If a time has been selected present the end clock with
		// the hands pointing to selected time
		myDrawClocks.positionClock (20, 20, startEndClockSize, startHours, startMinutes, 0.0f, "Start time", myStyle, analogGuiClock, analogClockBackground, analogClockCenter, centreSize);
		myDrawClocks.positionClock (20 + startEndClockSize + 20, 20, startEndClockSize, endHours, endMinutes, 0.0f, "End time", myStyle, analogGuiClock, analogClockBackground, analogClockCenter, centreSize);
		myStyle.fontSize = temp;
		GUI.EndGroup();

		if (timeConfirmed) {
			setInitialSliderValues(measure);
			//derive and set answer
			List<float> endTime = new List<float>();
			endTime.Add (endHours);
			endTime.Add (endMinutes);

			answer = myClockFunctions.deriveElapsedTime(endTime, elapsedIsSnapped, sliderValue, maxSliderValue, measure, startHours, startMinutes);

			/*debug code DateTime sliderDate = new DateTime (DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, (int)endTime [0], (int)endTime [1],0);
			DateTime startDate = new DateTime (DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, (int)startHours, (int)startMinutes, 0);
			
			TimeSpan difference = sliderDate.Subtract (startDate);
			int hoursDiff = difference.Hours;
			int minsDiff = difference.Minutes;
			answerStr = "answer, hrs=" + answer [0].ToString () + " mins=" + answer [1].ToString () + "calculated diff="+hoursDiff.ToString()+":"+minsDiff.ToString()+"\n";
			answerStr = answerStr + "endHr=" + endHours.ToString () + " EndMn=" + endMinutes.ToString () + " startHrs="+startHours.ToString () + " StartMn=" + startMinutes.ToString ()+" measure="+measure;
			*/
			moveToNextState=true;
		}
		return moveToNextState;
	}
	
	bool practiceState_startNotSetWidgets(Rect startEndGroup) {
		bool moveToNextState = false;
		//position and label start and end clocks
		GUI.BeginGroup(startEndGroup);
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

		float tempSliderValue = 0.0f;
		tempSliderValue = mySliderFunctions.positionStartMarker ("startNotSet" , feedbackRect, sliderRect, minSliderValue, maxSliderValue, measure, startHours, startMinutes, endHours, endMinutes, myClockFunctions, myMarkerStyle, myFeedbackFunctions);

		if (tempSliderValue != 0.0f) {
			sliderValue = tempSliderValue;
			List<float> sliderTime = myClockFunctions.deriveSliderHoursMins(minSliderValue, maxSliderValue, sliderValue, measure);
			//position slider clock
			myDrawClocks.positionClock (Screen.width - analogClockSize - 580, 200, analogClockSize, sliderTime [0], sliderTime [1], 0.0f, "", myStyle, analogGuiClock, analogClockBackground, analogClockCenter, analogClockCenterSize);
			moveToNextState=true;
		}

		return moveToNextState;
	}
	
	void practiceStateWidgets(Rect startEndGroup) {
		if (!sliderGTElapsed) {
			myFeedbackFunctions.giveFeedback (feedbackRect, "Great. Now move the slider button up and down and see what happens to the clock and the elapsed time");
		} else {
			string feedbackStr = "Brilliant. ";
			feedbackStr = feedbackStr + myClockFunctions.deriveElapsedTimeString(answer);
			feedbackStr = feedbackStr + "\nNow try counting up the times on the arcs to check that this is right";
			myFeedbackFunctions.giveFeedback (feedbackRect, feedbackStr);
		}
		//position and label start and end clocks
		GUI.BeginGroup(startEndGroup);
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

		//position and label the timeline
		sliderValue = GUI.HorizontalSlider (sliderRect, sliderValue, minSliderValue, maxSliderValue, sliderBackgroundStyle, thumbStyle);
		mySliderFunctions.assignLabels(0.0f, startMinutes, 12.0f, endMinutes, sliderRect.x, sliderRect.y, sliderRect.width, measure);
		
		//position the start marker
		float tempSliderValue = 0.0f;
		float startMarker = mySliderFunctions.getStartMarkerValue ();
		tempSliderValue = mySliderFunctions.positionStartMarker (startMarker, sliderRect, minSliderValue, maxSliderValue, myMarkerStyle);
		if (tempSliderValue!=0.0f) sliderValue = tempSliderValue;

		// Calculate and display the elapsed time. Nothing will be displayed if elapsed time is < 0.
		List<float> sliderTime = myClockFunctions.deriveSliderHoursMins(minSliderValue, maxSliderValue, sliderValue, measure);
		//string elapsedString = myClockFunctions.deriveElapsedTimeString(sliderTime, elapsedIsSnapped, sliderValue, maxSliderValue, measure, startHours, startMinutes);

		//check if elasped time between slider and start is now greater or equal to the elapsed time
		// between endtime and starttime
		string elapsedString = "";
		
		List<float> elapsedTime = myClockFunctions.deriveElapsedTime(sliderTime, elapsedIsSnapped, sliderValue, maxSliderValue, measure, startHours, startMinutes);
		if ((elapsedTime [0] > answer [0]) || ((elapsedTime [0] == answer [0]) && (elapsedTime [1] >= answer [1]))) {
			//elapsedString = myClockFunctions.deriveElapsedTimeString (answer, sliderTime, elapsedIsSnapped, sliderValue, maxSliderValue, measure, startHours, startMinutes);
			elapsedString = myClockFunctions.deriveElapsedTimeString(answer);
			//position slider clock but with time set to end hours and minutes
			myDrawClocks.positionClock (Screen.width - analogClockSize - 580, 200, analogClockSize, endHours, endMinutes, 0.0f, "", myStyle, analogGuiClock, analogClockBackground, analogClockCenter, analogClockCenterSize);
			sliderGTElapsed=true;
		} else {
			elapsedString = myClockFunctions.deriveElapsedTimeString (sliderTime, elapsedIsSnapped, sliderValue, maxSliderValue, measure, startHours, startMinutes);
			//position slider clock but with time set to current position of slider
			myDrawClocks.positionClock (Screen.width - analogClockSize - 580, 200, analogClockSize, sliderTime [0], sliderTime [1], 0.0f, "", myStyle, analogGuiClock, analogClockBackground, analogClockCenter, analogClockCenterSize);
			sliderGTElapsed=false;
		}

		//string elapsedString = myClockFunctions.deriveElapsedTimeString(answer, sliderTime, elapsedIsSnapped, sliderValue, maxSliderValue, measure, startHours, startMinutes);
		if (!elapsedString.Equals ("")) {
			GUI.Box (new Rect (Screen.width/2 - 165.0f, 370, 350, 80), elapsedString, myStyle);
			curveScript.removeCurves();
			curveScript.addCurves(sliderTime, sliderRect.x + 4, sliderRect.width / 12.0f, 120, startHours, startMinutes, endHours, endMinutes);
			curveCallCount++;
			writeCurveLabels(sliderRect.y-24);
			//GUI.Label(new Rect(100,400,200,50), "curveNo="+curveScript.getNumberOfCurves().ToString() + " sliderHrs="+sliderTime[0].ToString() + myString);
		} else curveScript.removeCurves();
		

		//GUI.EndGroup ();
	}

	string setMeasure(float startMinutes, float endMinutes) {
		string measure = "whole";
		if ((endMinutes==15.0f) || (startMinutes==15.0f)) return "quarter";
		if ((endMinutes==45.0f) || (startMinutes==45.0f)) return "quarter";
		if ((endMinutes==30.0f) || (startMinutes==30.0f)) return "half";
		return measure;
	}

	void setInitialSliderValues(string measure) {
		minSliderValue=0.0f;
		elapsedIsSnapped = true;
		sliderValue=minSliderValue;
		if (measure.Equals("whole")) {
			maxSliderValue=12.0f;
		} else {
			if (measure.Equals("half")) {
				maxSliderValue=24.0f;
			} else {
				maxSliderValue=48.0f;
			}
		}
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
