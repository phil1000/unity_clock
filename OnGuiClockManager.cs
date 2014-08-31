using Vectrosity;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;

public class OnGuiClockManager : MonoBehaviour {
	
	// these variables are used to assign the images and font styles used within this controller. 
	// They need to be specified as public variables so they can be assigned within the Unity 
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
	public GUIStyle timerStyle;
	public GUIStyle myPlusMinusButtonStyle;
	public GUIStyle myOKButtonStyle;
	public GUIStyle HrMinLabel;
	public GUIStyle HrMinText;
	public GUIStyle thumbStyle;
	private GUIStyle sliderBackgroundStyle;
	
	// A game object with an attached script is required in order to maintain state across the 
	// different scenes - the state maintained is the Player object which provide a hashset of
	// all the current game players and each player object maintains the player state e.g. level,
	// questions answered, current score
	private GameObject myPlayers;
	private PlayerScript myPlayerScript;
	private Player myPlayer;
	private bool elapsedIsSnapped;
	
	private float startHours;
	private float startMinutes;
	private float endHours;
	private float endMinutes;
	
	private Question myQuestion;
	private float workingHours;
	private float workingMinutes;
	private List<float> answer;
	private bool validAnswer=false;
	private bool answerSet=false;
	private string answerFeedback;
	private const float PASS_LEVEL = 0.6f;
	
	private float sliderValue;
	private float maxSliderValue;
	private float minSliderValue;
	private string measure;
	
	private GUIStyle myStyle; 
	private GUIStyle myBackgroundStyle;
	
	// these member variables are used to access the clock, slider, userr feedback and curve functionality
	private CreateCurves curveScript;
	private ClockFunctions myClockFunctions;
	private DrawClockFunctions myDrawClocks;
	private SliderFunctions mySliderFunctions;
	private UserFeedbackFunctions myFeedbackFunctions;
	private Rect feedbackRect;
	private string feedbackString;
	
	void Start() {
		curveScript = Camera.main.GetComponent<CreateCurves>();
		myClockFunctions = new ClockFunctionsImpl ();
		mySliderFunctions = new SliderFunctionsImpl ();
		myDrawClocks = new DrawClockFunctionsImpl ();
		
		feedbackRect = new Rect (20, 20, 450, 300);
		myFeedbackFunctions = new UserFeedbackFunctionsImpl ();
	}
	
	void Awake() {
		myPlayers = GameObject.Find("Player");
		myPlayerScript = (PlayerScript) myPlayers.GetComponent ("PlayerScript");
		this.elapsedIsSnapped = myPlayerScript.elapsedIsSnapped;
		myPlayer = myPlayerScript.getCurrentPlayer ();
		answerFeedback = "";
		feedbackString = "";
		myQuestion = loadNextQuestion ();
	}
	
	void OnGUI ()
	{
		// Make a background box
		GUI.Box(new Rect((Screen.width-820)/2,20,820,410), "", myBoxStyle);
		
		setStyles ();
		GUIStyle feedBox = new GUIStyle(GUI.skin.box);
		feedBox.normal.background = myAvatar;
		myFeedbackFunctions.updateStyles (feedBox, myStyle);
		
		addButtons (new Rect(20, 150, 150, 450));
		
		if (!feedbackString.Equals(""))
			myFeedbackFunctions.giveFeedback (feedbackRect, feedbackString);
		
		//present the time selector widget 
		GUI.BeginGroup (new Rect (825, 210, 141, 175),timerStyle);
		
		answer = myClockFunctions.setTime (workingHours, workingMinutes, HrMinLabel, HrMinText, myPlusMinusButtonStyle);
		workingHours = answer [0];
		workingMinutes = answer [1];
		
		if (GUI.Button (new Rect (10,110,121,40), "Check", myOKButtonStyle)) {
			if ((answer [0] != 0.0f) || (answer [1] != 0.0f)) {
				answerSet=true;
				//List<float> qAnswer = myPlayer.getcurrentQuestionLevel ().getNextUnansweredQuestion ().getAnswer();
				List<float> qAnswer = myQuestion.getAnswer();
				if ((qAnswer[0]==answer [0]) && (qAnswer[1]==answer [1])) {
					//Question myQuestion = myPlayer.getcurrentQuestionLevel ().getNextUnansweredQuestion ();
					validAnswer=true;
					myPlayer.getcurrentQuestionLevel().questionCompleted(myQuestion, answer, validAnswer);
					
					answerFeedback="Great, that's correct. Now click Next Question to get another question";
				} else {
					//Question myQuestion = myPlayer.getcurrentQuestionLevel ().getNextUnansweredQuestion ();
					validAnswer=false;
					myPlayer.getcurrentQuestionLevel().questionCompleted(myQuestion, answer, validAnswer);
					
					answerFeedback="That's not quite right. Try again or click Next Question to skip to the next question";
				}
			}
		}
		GUI.EndGroup ();
		
		if (!answerFeedback.Equals(""))
			myFeedbackFunctions.giveFeedback (feedbackRect, answerFeedback);
		
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
		//if (remainingQuestions) {
		if (myQuestion!=null) {
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
	
	void initialiseForNewQuestion() {
		mySliderFunctions.initialiseStartMarker();
		curveScript.removeCurves();
		validAnswer=false;
		answerFeedback="";
		workingHours=0.0f;
		workingMinutes=0.0f;
	}
	
	void addButtons(Rect buttonRect) {
		//group buttons together .. then all button coordinates are relative to the group coordinates
		GUI.BeginGroup(buttonRect);
		//application quit button
		if (GUI.Button (new Rect (10,120,90,90), "Return", myReturnButton)) {
			Application.LoadLevel(0);
		}
		
		//next question button
		//if (remainingQuestions) {
		//if (myQuestion!=null) {
		if (GUI.Button (new Rect (10, 20, 90, 90), "Next \nQuestion", myButtonStyle)) {
			/*if (!answerSet) {
					// need to discard the current question as the player wants to 
					// ignore it and move to the next question
					myPlayer.getcurrentQuestionLevel().questionCompleted(myQuestion, null, false);
				}
				answerSet=false;
				myQuestion = loadNextQuestion (); 
				resetForNewQuestion();*/
			evaluateAnswer();
		} 
		//}
		GUI.EndGroup();
	}
	
	void evaluateAnswer() {
		int correctAnswers = myPlayer.getcurrentQuestionLevel().getnumberAnswerCorrectly();
		int noQuestions = myPlayer.getcurrentQuestionLevel().getnumberOfQuestions();
		
		if (!answerSet) {
			// need to discard the current question as the player wants to 
			// ignore it and move to the next question
			myPlayer.getcurrentQuestionLevel().questionCompleted(myQuestion, null, false);
		}
		answerSet=false;
		myQuestion = loadNextQuestion (); 
		
		if (myQuestion==null) {
			
			feedbackString = "You have completed all the questions in this level. \nYou got " + correctAnswers + " out of " + noQuestions;
			if ((correctAnswers/noQuestions)>= PASS_LEVEL) {
				if (myPlayer.updatePlayerLevel()) {
					feedbackString= feedbackString + "You have now moved to level" + myPlayer.getcurrentPlayerLevel().ToString () + "." + myPlayer.getcurrentQuestionLevel().getLevel().ToString(); 
					myQuestion = loadNextQuestion (); 
					initialiseForNewQuestion();
				} else {
					feedbackString= feedbackString + "You have finished all the levels in this game. Well done!";
				}
			} else {
				feedbackString= feedbackString + "You just need a little bit more practice at this level. \n";
				feedbackString = feedbackString + "Why not try using the Practice area to check your understanding before trying again";
				myPlayer.getcurrentQuestionLevel().reset();
				myQuestion = loadNextQuestion (); 
				initialiseForNewQuestion();
			}
			
		} else {
			feedbackString="";
		}
		myPlayerScript.Save ();
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
	
	Question loadNextQuestion() {
		Question myQuestion = myPlayer.getcurrentQuestionLevel ().getNextUnansweredQuestion ();
		
		if (myQuestion != null) {
			this.startHours = myQuestion.getStartHours ();
			this.startMinutes = myQuestion.getsStartMinutes ();
			this.endHours = myQuestion.getEndHours ();
			this.endMinutes = myQuestion.getEndMinutes ();
			this.sliderValue = myQuestion.getSliderValue ();
			this.maxSliderValue = myQuestion.getMaxSliderValue ();
			this.minSliderValue = myQuestion.getMinSliderValue ();
			this.measure = myQuestion.getMeasure ();
			return myQuestion;
		} else {
			return null;
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
