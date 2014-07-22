using Vectrosity;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;

public class OnGuiClockManager : MonoBehaviour {
	
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
	private bool remainingQuestionClicked=false;
	// I don't want these to be member variables because they are only
	// relevant to the slider but I need their values to persist 
	// until a new question is loaded and the only way to do this once
	// per session is by defining them as member variables
	private bool setStartSliderValue=true;
	private float startMarker=99.0f;
	private bool setStartMarker=false;
	private bool startCurveSet=false;
	private float nextPoint=0.0f;
	private float nextHour=0.0f;
	
	private GUIStyle myStyle; 
	private GUIStyle myBackgroundStyle;
	
	private CreateCurves curveScript;
	private ClockFunctions myClockFunctions;
	private DrawClockFunctions myDrawClocks;
	private Rect feedbackRect;
	
	void Start() {
		curveScript = Camera.main.GetComponent<CreateCurves>();
		myClockFunctions = new ClockFunctionsImpl ();
		myDrawClocks = new DrawClockFunctionsImpl ();
		feedbackRect = new Rect (20, 300, 450, 300);
	}
	
	void Awake() {
		myQuestions = GameObject.Find("Questions");
		myScript = (questionScript) myQuestions.GetComponent ("questionScript");
		this.elapsedIsSnapped = myScript.elapsedIsSnapped;
		loadNextQuestion ();
	}
	
	void OnGUI ()
	{
		setStyles ();
		
		// Make a background box
		//GUI.Box(new Rect(170,40,1024,512), "", myBoxStyle);
		
		//group buttons together .. then all button coordinates are relative to the group coordinates
		GUI.BeginGroup(new Rect(20, 20, 150, 450));
		
		//application quit button
		if (GUI.Button (new Rect (10,120,90,90), "Return", myReturnButton)) {
			Application.LoadLevel(0);
		}
		
		//next question button
		if (remainingQuestions) {
			if (GUI.Button (new Rect (10, 20, 90, 90), "Next \nQuestion", myButtonStyle)) {
				remainingQuestions = loadNextQuestion (); 
				remainingQuestionClicked=true;
			} else {
				remainingQuestionClicked=false;;
			}
		}
		
		GUI.EndGroup();
		
		if (!remainingQuestions) giveFeedback(feedbackRect, "No More Questions");
		
		//position and label start and end clocks
		//myDrawClocks.positionClock (20 + analogClockSize, 70, analogClockSize, startHours, startMinutes, 0.0f, "Start time", myStyle, analogGuiClock, analogClockBackground, analogClockCenter, analogClockCenterSize);
		//myDrawClocks.positionClock (Screen.width - analogClockSize - 230, 70, analogClockSize, endHours, endMinutes, 0.0f, "End time", myStyle, analogGuiClock, analogClockBackground, analogClockCenter, analogClockCenterSize);
		GUI.BeginGroup(new Rect(Screen.width/2 - 148.0f, 20, 350, 450));
		float startEndClockSize = 128.0f;
		float centreSize = 12.0f;
		int temp = myStyle.fontSize;
		myStyle.fontSize = 12;
		myDrawClocks.positionClock (20, 20, startEndClockSize, startHours, startMinutes, 0.0f, "Start time", myStyle, analogGuiClock, analogClockBackground, analogClockCenter, centreSize);
		myDrawClocks.positionClock (20 + startEndClockSize + 20, 20, startEndClockSize, endHours, endMinutes, 0.0f, "End time", myStyle, analogGuiClock, analogClockBackground, analogClockCenter, centreSize);
		myStyle.fontSize = temp;
		GUI.EndGroup();
		
		//load the timeline and slider clock
		sliderProcessing (remainingQuestions, remainingQuestionClicked, feedbackRect);
	}
	
	void sliderProcessing(bool remainingQuestions, bool remainingQuestionsClicked, Rect feedbackRect) {
		Rect sliderRect = new Rect(190,480,1000,60);
		
		if (remainingQuestions && remainingQuestionClicked) initialiseStartMarker();
		
		//double click to set the start marker 
		if (setStartMarker) {
			Event e = Event.current;
			if (e.isMouse && e.type == EventType.MouseDown && e.clickCount == 2) {
				startMarker = sliderValue;
				setStartMarker = false;
			}
		}
		
		//position and label the timeline
		sliderValue = GUI.HorizontalSlider (sliderRect, sliderValue, minSliderValue, maxSliderValue, sliderBackgroundStyle, thumbStyle);
		assignLabels(0.0f, startMinutes, 12.0f, endMinutes, sliderRect.x, sliderRect.y, sliderRect.width, measure);
		
		//position the start marker
		if (remainingQuestions) positionStartMarker (feedbackRect, sliderRect);
		
		// Calculate and display the elapsed time. Nothing will be displayed if elapsed time is < 0.
		List<float> sliderTime = myClockFunctions.deriveSliderHoursMins(minSliderValue, maxSliderValue, sliderValue, measure);
		string elapsedString = myClockFunctions.deriveElapsedTimeString(sliderTime, elapsedIsSnapped, sliderValue, maxSliderValue, measure, startHours, startMinutes);
		if (!elapsedString.Equals ("")) {
			GUI.Box (new Rect (574, 100, analogClockSize, 80), elapsedString, myStyle);
			if (!startCurveSet) addFirstPoint(sliderTime, sliderRect.x+4,sliderRect.width/12.0f, 120);
			drawCurves (sliderTime, sliderRect.x+4,sliderRect.width/12.0f, 120);
		}
		
		//position slider clock
		myDrawClocks.positionClock (Screen.width - analogClockSize - 600, 180, analogClockSize, sliderTime [0], sliderTime [1], 0.0f, "", myStyle, analogGuiClock, analogClockBackground, analogClockCenter, analogClockCenterSize);
	}
	
	void addFirstPoint(List<float> sliderTime, float left, float width, float curveHeight) {
		
		float startPoint = left + (width * startHours) - startHours;;
		float increment = 0.0f;
		
		if ((sliderTime[0]-startHours)>=1) {
			if (startMinutes == 15.0f) {
				startPoint = startPoint + 0.25f * width;
				increment = 0.75f * width;
			} else {
				if (startMinutes == 30.0f) {
					startPoint = startPoint + 0.5f * width;
					increment = 0.5f * width;
				} else {
					if (startMinutes == 45.0f) {
						startPoint = startPoint + 0.75f * width;
						increment = 0.25f * width;
					} else {
						increment = width;
					}
				}
			}
			curveScript.addCurve (startPoint,startPoint+increment,curveHeight);
			startCurveSet=true;
			nextPoint = startPoint+increment;
			nextHour = startHours+1.0f;
		}
	}
	
	void drawCurves(List<float> sliderTime, float left, float width, float curveHeight) {
		
		float increment = 0.0f;
		float iterHours = startHours + 1.0f;
		int index = (int)(sliderTime [0] - nextHour);
		index=index+1;
		float lNext = nextPoint;
		
		if (sliderTime [0] < endHours) {
			int difference = (int)(sliderTime [0] - nextHour);
			if (curveScript.getNumberOfCurves () == difference) {
				lNext = lNext + (width * (difference - 1)) - difference;
				curveScript.addCurve (lNext, lNext + width, curveHeight);
			}
		}
		
		if (sliderTime[0] == endHours) {
			int difference = (int)(sliderTime [0] - nextHour);
			if (curveScript.getNumberOfCurves () == difference) {
				lNext = lNext + (width * (difference - 1)) - difference;
				curveScript.addCurve (lNext, lNext + width, curveHeight);
			}
			if (endMinutes>0.0f) {
				if (curveScript.getNumberOfCurves () == difference+1) {
					if (endMinutes == 15.0f) {
						increment = 0.25f * width;
					} else {
						if (endMinutes == 30.0f) {
							increment = 0.5f * width;
						} else {
							if (endMinutes == 45.0f) {
								increment = 0.75f * width;
							} 
						}
					}
					curveScript.addCurve (lNext,lNext+increment,curveHeight);
				}
			}
		}
		
		//curveScript.removeCurves(index);
		//if ((sliderTime [0] - startHours) <= 0.0f) startCurveSet = false;
	}
	
	void removeCurves() {
		curveScript.removeCurves();
	}
	
	void positionStartMarker(Rect feedbackRect, Rect sliderRect) {
		
		if (startMarker == 99.0f) {
			giveFeedback(feedbackRect, "set the start marker by double clicking over the slider");
			setStartMarker=true;
		} else {
			if (!myClockFunctions.checkTime(startMarker, "Start", minSliderValue, maxSliderValue, measure, startHours, startMinutes, endHours, endMinutes)) {
				giveFeedback(feedbackRect, "sorry, that is not a correct start time, please try again");
				setStartMarker=true;
			} else {
				// position the startMarker but first align it precisely
				startMarker = Mathf.Round(startMarker);
				if (setStartSliderValue) {
					sliderValue=startMarker;
					setStartSliderValue=false;
				}
				Rect startRect = positionMarker(startMarker, sliderRect);
				if (GUI.Button(startRect, "S", myMarkerStyle)) {
					sliderValue=startMarker;
				}
			}
		}
	}
	
	Rect positionMarker (float marker, Rect sliderRect) {
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
	void giveFeedback(Rect feedbackRect, string strMessage) {
		Texture2D backup = myStyle.normal.background;
		myStyle.normal.background = myAvatar;
		GUI.BeginGroup (feedbackRect);
		GUI.Label(new Rect(0, 0, 100, 100), "", myStyle); 
		myStyle.normal.background = backup;
		GUI.Label(new Rect(100, 0, 300, 100), strMessage, myStyle); 
		GUI.EndGroup ();
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
	}	
	
	void initialiseStartMarker() {
		startMarker=99.0f;
		setStartMarker=false;
		setStartSliderValue=true;
		startCurveSet=false;
		removeCurves();
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
