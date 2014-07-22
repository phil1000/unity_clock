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
	public GUIStyle myButtonStyle;
	public GUIStyle myMarkerStyle;
	public GUIStyle myEndMarkerStyle;
	public GUIStyle myReturnButton;
	
	private GUIStyle sliderBackgroundStyle;
	public GUIStyle thumbStyle;
	
	private GameObject myQuestions;
	private questionScript myScript;
	
	private GameObject myClock;
	private OnGUIClockHands myClockHandsScript;
	
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
	private GUIStyle myBoxStyle;
	private GUIStyle myBackgroundStyle;
	
	private float startMarker=99.0f;
	private float endMarker=99.0f;
	private bool setStartMarker=false;
	private bool setEndMarker=false;
	private bool setStartSliderValue=true;
	private bool setEndSliderValue=true;
	
	private Rect sliderRect = new Rect(190,480,1000,60);
	private bool one_click = false;
	private bool timer_running;
	private float timer_for_double_click;
	private const float DELAY=0.4f;
	private CreateCurves curveScript;
	
	private ClockFunctions myClockFunctions;
	
	void Start() {
		curveScript = Camera.main.GetComponent<CreateCurves>();
		myClockFunctions = new ClockFunctionsImpl ();
	}
	
	void Awake() {
		myQuestions = GameObject.Find("Questions");
		myScript = (questionScript) myQuestions.GetComponent ("questionScript");
		this.elapsedIsSnapped = myScript.elapsedIsSnapped;
		loadNextQuestion ();
	}
	
	void OnGUI ()
	{
		//double click to set the start marker 
		if (setStartMarker) {
			Event e = Event.current;
			if (e.isMouse && e.type == EventType.MouseDown && e.clickCount == 2) {
				startMarker = sliderValue;
				setStartMarker = false;
			}
		}
		setStyles ();
		//next question button
		if (remainingQuestions) {
			if (GUI.Button (new Rect (70, 60, 90, 90), "Next \nQuestion", myButtonStyle)) {
				remainingQuestions = loadNextQuestion (); 
				initialiseStartEndMarkers();
				//removeCurves();
			}
		} else {
			giveFeedback("No More Questions");
			//GUI.Label(new Rect(40, 90, 200, 40), "No More \nQuestions", myStyle);
		}
		
		//application quit button
		if (GUI.Button (new Rect (70,160,90,90), "Return", myReturnButton)) {
			Application.LoadLevel(0);
		}
		
		//position and label the timeline
		sliderValue = GUI.HorizontalSlider (sliderRect, sliderValue, minSliderValue, maxSliderValue, sliderBackgroundStyle, thumbStyle);
		assignLabels(0.0f, startMinutes, 12.0f, endMinutes, sliderRect.x, sliderRect.y, sliderRect.width, measure);
		
		//position the start and finish markers
		if (remainingQuestions) positionStartEndMarkers ();
		
		// Calculate and display the elapsed time. Nothing will be displayed if elapsed time is < 0.
		List<float> sliderTime = myClockFunctions.deriveSliderHoursMins(minSliderValue, maxSliderValue, sliderValue, measure);
		string elapsedString = myClockFunctions.deriveElapsedTimeString(sliderTime, elapsedIsSnapped, sliderValue, maxSliderValue, measure, startHours, startMinutes);
		if (!elapsedString.Equals(""))
			GUI.Box(new Rect (574,190,analogClockSize,80), elapsedString, myStyle);
		
		//position and label start clock
		Rect startPosition = new Rect(20 + analogClockSize , 70, analogClockSize, analogClockSize);
		drawClock (startPosition, startHours, startMinutes);
		string startString = myClockFunctions.timeLabel ("Start time", startHours, startMinutes);
		GUI.Label(new Rect(212, 45, 200, 20), startString, myStyle);
		
		//position and label end clock
		Rect endPosition = new Rect(Screen.width - analogClockSize - 230, 70, analogClockSize, analogClockSize);
		drawClock (endPosition, endHours, endMinutes);
		string endString = myClockFunctions.timeLabel ("End time", endHours, endMinutes);
		GUI.Label(new Rect(940, 45, 200, 20), endString, myStyle);
		
		/*maybe if mouse on the minute hand play with that else do the slider //
		myClock = GameObject.Find("AnalogClock");
		myClockHandsScript = (OnGUIClockHands) myClock.GetComponent ("OnGUIClockHands");
		Event e = Event.current;

		if (myClockHandsScript.minuteHandRect.Contains (e.mousePosition) && (e.type == EventType.mouseUp)) {
						GUI.DrawTexture (new Rect (e.mousePosition.x, e.mousePosition.y, 75, 75), myClockHandsScript.minuteHand);
		}
		end of manipulating minute hand */
		
		//position slider clock
		Rect sliderPosition = new Rect (Screen.width - analogClockSize - 600, 280, analogClockSize, analogClockSize);
		// we need to translate the slider value in hours and minutes
		drawClock (sliderPosition, sliderTime [0], sliderTime [1]);
		
		//if (GUI.Button (new Rect (370, 60, 90, 90), "add some curves", myButtonStyle)) {
		//	drawCurves ();
		//}
		
	}
	
	void drawCurves() {
		curveScript.addCurve (100,200,18);
		curveScript.addCurve (200,300,180);
	}
	
	void removeCurves() {
		curveScript.removeCurves();
	}
	
	void positionStartEndMarkers() {
		
		if (startMarker == 99.0f) {
			giveFeedback("set the start marker by double clicking over the slider");
			//GUI.Label(new Rect(90, 300, 300, 100), "set the start marker by double clicking over the slider", myStyle);
			setStartMarker=true;
		} else {
			if (!myClockFunctions.checkTime(startMarker, "Start", minSliderValue, maxSliderValue, measure, startHours, startMinutes, endHours, endMinutes)) {
				giveFeedback("sorry, that is not a correct start time, please try again");
				//GUI.Label(new Rect(90, 300, 300, 100), "sorry, that is not a correct start time, please try again", myStyle);
				setStartMarker=true;
			} else {
				// position the startMarker but first align it precisely
				startMarker = Mathf.Round(startMarker);
				if (setStartSliderValue) {
					sliderValue=startMarker;
					setStartSliderValue=false;
				}
				Rect startRect = positionMarker(startMarker);
				if (GUI.Button(startRect, "S", myMarkerStyle)) {
					sliderValue=startMarker;
				}
				if (endMarker == 99.0f) {
					//GUI.Label(new Rect(90, 300, 300, 100), "set the finish marker by double clicking over the slider", myStyle); 
					giveFeedback("set the finish marker by double clicking over the slider");
					setEndMarker=true;
				} else {
					// check if start is set to correct time
					if (!myClockFunctions.checkTime(endMarker, "End", minSliderValue, maxSliderValue, measure, startHours, startMinutes, endHours, endMinutes)) {
						//GUI.Label(new Rect(90, 300, 300, 100), "sorry, that is not a correct end time, please try again", myStyle);
						giveFeedback("sorry, that is not a correct end time, please try again");
						setEndMarker=true;
					} else {
						// position the endMarker but first align it precisely
						endMarker = Mathf.Round(endMarker);
						if (setEndSliderValue) {
							sliderValue=endMarker;
							setEndSliderValue=false;
						}
						Rect endRect = positionMarker(endMarker);
						GUI.Button(endRect, "F", myEndMarkerStyle);
					}
				}
			}
		}
	}
	
	Rect positionMarker (float marker) {
		var y = sliderRect.y + sliderRect.height;
		var progress = (marker-minSliderValue) / (maxSliderValue-minSliderValue);
		
		var xMin = sliderRect.xMin;
		var xMax = sliderRect.xMax;
		var x = Mathf.Lerp(xMin, xMax, progress);
		float width = 60.0f;
		return new Rect (x-(width/2), y - 6, width, width);
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
	void giveFeedback(string strMessage) {
		Texture2D backup = myStyle.normal.background;
		myStyle.normal.background = myAvatar;
		GUI.BeginGroup (new Rect (90, 300, 450, 300));
		GUI.Label(new Rect(0, 0, 100, 100), "", myStyle); 
		myStyle.normal.background = backup;
		GUI.Label(new Rect(100, 0, 300, 100), strMessage, myStyle); 
		GUI.EndGroup ();
	}
	
	void drawClock(Rect inPosition, float inHours, float inMinutes) {
		if (analogGuiClock != null)
		{
			//position the clock face
			if (analogClockBackground != null)
			{
				GUI.DrawTexture(inPosition, analogClockBackground);
			}
			
			// position the clock hands
			analogGuiClock.Draw(inPosition, inHours, inMinutes,0);
			
			// position the clock centre
			if (analogClockCenter != null)
			{
				GUI.DrawTexture(new Rect(inPosition.center.x - analogClockCenterSize * 0.5f, inPosition.center.y - analogClockCenterSize * 0.5f, analogClockCenterSize, analogClockCenterSize), analogClockBackground);
			}
		}
	}
	
	void assignLabels(float startHours, float startMinutes, float endHours, float endMinutes, float sliderLeft, float top, float sliderWidth, string measure) {
		// the minutes bit is whether to split into 1/2 or 1/4 hours but just do hours and half hours for now
		float currentHours = startHours;
		string myText = "";
		float left = sliderLeft+4;
		float width = sliderWidth / 12.0f;
		
		GUIStyle myStyle = new GUIStyle();
		myStyle.fontSize = 11;
		myStyle.normal.textColor = Color.yellow;
		
		for (float i=0.0f; i<=12.0f; i++) {
			myText = i.ToString () + ".00";
			float notchLeft=left+(width*i)-i;
			float textLeft=left-9 +(width*i)-i;
			GUI.Label (new Rect (notchLeft, top+6, 100, 20), "|"); // the notches
			GUI.Label (new Rect (textLeft, top+20, 100, 20), myText); // the notches
			if ( (i<12.0f) && (measure.Equals("half")) ) {
				myText = i.ToString () + ".30"; 
				GUI.Label (new Rect (notchLeft+(width/2), top+6, 100, 12), "|"); // the notches
				GUI.Label (new Rect (textLeft+(width/2), top+20, 100, 20), myText, myStyle);
			}
			if ( (i<12.0f) && (measure.Equals("quarter")) ) {
				myText = i.ToString () + ".30"; 
				GUI.Label (new Rect (notchLeft+(width/2), top+6, 100, 12), "|"); // the notches
				GUI.Label (new Rect (textLeft+(width/2), top+20, 100, 20), myText, myStyle);
				GUI.Label (new Rect (notchLeft+(width/4), top+6, 100, 12), "|"); // the notches
				GUI.Label (new Rect (notchLeft+(3*width/4), top+6, 100, 12), "|"); // the notches
			}
		}
		// the width increment segments the width of the slider by number of hours to represent
		/*float widthIncrement = sliderWidth / (endHours - startHours);
		while (currentHours <= endHours) {
			myText = currentHours.ToString () + ".00"; 
			GUI.Label (new Rect (left+2, top+12, 100, 20), "|"); // the notches
			GUI.Label (new Rect (left+2, top+6, 100, 20), "|"); // the notches
			GUI.Label (new Rect (left, top+25, 100, 20), myText);
			if ( (currentHours<endHours) && (measure.Equals("half")) ) {
				myText = currentHours.ToString () + ".30"; 
				GUI.Label (new Rect (left+(widthIncrement/2)+3, top+12, 100, 12), "|"); // the notches
				GUI.Label (new Rect (left+(widthIncrement/2)-5, top+25, 100, 20), myText, myStyle);
			}
			currentHours++;
			left=left+widthIncrement-2;
		}*/
	}	
	
	bool doubleClick() {
		if(!one_click) { // first click no previous clicks
			one_click = true;
			timer_for_double_click = Time.time; // save the current time
			return false;
		} else {
			if((Time.time - timer_for_double_click) > DELAY) {	
				//basically if thats true its been too long and we want to reset so the next click is simply a single click and not a double click.		
				one_click = false;
				return false;
			} else {
				one_click = false; // found a double click, now reset
				return true;				
			}
		}
	}
	
	void initialiseStartEndMarkers() {
		startMarker=99.0f;
		endMarker=99.0f;
		setStartMarker=false;
		setEndMarker=false;
		setStartSliderValue=true;
		setEndSliderValue=true;
	}
	
	void setStyles() {
		sliderBackgroundStyle = new GUIStyle (GUI.skin.horizontalSlider);
		myStyle = new GUIStyle(GUI.skin.label); 
		myBoxStyle = new GUIStyle(GUI.skin.box);
		myBackgroundStyle = new GUIStyle(GUI.skin.box);
		myStyle.fontSize = 18;
		myStyle.fontStyle = FontStyle.BoldAndItalic;
		myStyle.alignment = TextAnchor.MiddleCenter;
		myStyle.normal.textColor = Color.white;
		
		myBoxStyle.fontSize = 14;
		myBoxStyle.fontStyle = FontStyle.BoldAndItalic;
		myBoxStyle.alignment = TextAnchor.MiddleCenter;
		myBoxStyle.normal.textColor = Color.white;
		myBoxStyle.normal.background = boxBackground;
		myBoxStyle.fontSize = 14;
		myBackgroundStyle.fontStyle = FontStyle.BoldAndItalic;
		myBackgroundStyle.alignment = TextAnchor.MiddleCenter;
		myBackgroundStyle.normal.textColor = Color.white;
		myBackgroundStyle.normal.background = background;
		myBackgroundStyle.fontSize = 14;
	}
}
