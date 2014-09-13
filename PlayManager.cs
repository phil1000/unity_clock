using Vectrosity;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;

public class PlayManager : MonoBehaviour {
	
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
	public GUIStyle feedbackStyle;
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
	private bool levelCompleted = false;
	//private string answerFeedback;
	private float passThreshold;
	private Rect timeSelectorRect;

	private float sliderValue;
	private float maxSliderValue;
	private float minSliderValue;
	private string measure;
	private bool sliderGTElapsed=false;

	private GUIStyle myStyle; 
	private GUIStyle myBackgroundStyle;

	// these member variables are used to access the clock, slider, user feedback and curve functionality
	private CreateCurves curveScript;
	private ClockFunctions myClockFunctions;
	private DrawClockFunctions myDrawClocks;
	private SliderFunctions mySliderFunctions;
	private UserFeedbackFunctions myFeedbackFunctions;
	private Rect feedbackRect;
	//private string answerString;
	private string feedbackString;
	private bool sliderSet = false;
	private PlayerLevelFlags myPlayerLevelFlags;
	private bool hintsOn;
	private string myPersistedLevelCompleted;
	
	void Start() {
		curveScript = Camera.main.GetComponent<CreateCurves>();
		myClockFunctions = new ClockFunctionsImpl ();
		mySliderFunctions = new SliderFunctionsImpl ();
		myDrawClocks = new DrawClockFunctionsImpl ();

		feedbackRect = new Rect (20, 20, 475, 375);
		timeSelectorRect = new Rect (825, 210, 141, 175);
		myFeedbackFunctions = new UserFeedbackFunctionsImpl ();
	}
	
	void Awake() {
		myPlayers = GameObject.Find("Player");
		myPlayerScript = (PlayerScript) myPlayers.GetComponent ("PlayerScript");
		this.elapsedIsSnapped = myPlayerScript.elapsedIsSnapped;
		this.passThreshold = myPlayerScript.getPassThreshold ();
		myPlayer = myPlayerScript.getCurrentPlayer ();
		feedbackString = "";
		myPlayerLevelFlags = myPlayer.getPlayerLevelFlags ();
		myQuestion = loadNextQuestion ();
		hintsOn = false;
	}
	
	void OnGUI ()
	{
		// Make a background box
		GUI.Box(new Rect((Screen.width-820)/2,20,820,410), "", myBoxStyle);

		setStyles ();
		GUIStyle feedBox = new GUIStyle(GUI.skin.box);
		feedBox.normal.background = myAvatar;
		myFeedbackFunctions.updateStyles (feedBox, feedbackStyle);

		string myLevelString = addButtons (new Rect(20, 150, 150, 450));
		if (!myLevelString.Equals ("")) {
			feedbackString = myLevelString;
			myPersistedLevelCompleted = myLevelString;
		}

		if (myPlayer.isFinished ()) {
			myFeedbackFunctions.giveFeedback (feedbackRect, "Well Done. You have finished all questions and levels.");
		} else {

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

			if (myPlayerLevelFlags.sliderTool) {
				if ((!feedbackString.Equals ("")) && (sliderSet))
					// the slider has been set and so give any non slider related feedback here
					// - if the slider has not been set then  feedback messages are provided by slider set
					myFeedbackFunctions.giveFeedback (feedbackRect, feedbackString);

				Rect sliderRect = new Rect (190, 480, 1000, 60);
				//present the time selector widget if the slider has been set
				if (sliderSet) {
					if ((!answerSet) && (!levelCompleted)) {
						//feedbackString = "Great. Now move the slider button up and down and see what happens to the clock ";
						feedbackString = "Great. Now enter your answer for the elasped time between the start and end times. Click Check when you are happy with your answer";
						if ((myPlayerLevelFlags.textTips) && (hintsOn) ) 
							feedbackString = feedbackString + "\n\n\nTIPS: * Move the slider button up and down and see what happens to the clock ";
					}
					//position the start marker based on previously set value
					float tempSliderValue = 0.0f;
					float startMarker = mySliderFunctions.getStartMarkerValue ();
					tempSliderValue = mySliderFunctions.positionStartMarker (startMarker, sliderRect, minSliderValue, maxSliderValue, myMarkerStyle);
					if (tempSliderValue != 0.0f)
						sliderValue = tempSliderValue;
					// Calculate and display the elapsed time. Nothing will be displayed if elapsed time is < 0.
					List<float> sliderTime = myClockFunctions.deriveSliderHoursMins (minSliderValue, maxSliderValue, sliderValue, measure);
					string elapsedString = myClockFunctions.deriveElapsedTimeString (sliderTime, elapsedIsSnapped, sliderValue, maxSliderValue, measure, startHours, startMinutes);
					if (!elapsedString.Equals ("")) {
						if (myPlayerLevelFlags.textTips) {
							GUI.Box (new Rect (Screen.width / 2 - 165.0f, 370, 350, 80), elapsedString, myStyle);
							if ( (!answerSet) && (!levelCompleted) && (hintsOn) )
								feedbackString = feedbackString + "and the elapsed time.";
						}

						if (myPlayerLevelFlags.arcTools) {
							curveScript.removeCurves ();
							// this works in game/test mode where the y coordinate resolves to 120
							//curveScript.addCurves(sliderTime, sliderRect.x + 4, sliderRect.width / 12.0f, 120, startHours, startMinutes, endHours, endMinutes);
							//curveScript.addCurves(sliderTime, sliderRect.x + 4, sliderRect.width / 12.0f, sliderRect.y-360, startHours, startMinutes, endHours, endMinutes);
							//this works in app.exe mode where the y coordinate resolves to 290
							//curveScript.addCurves(sliderTime, sliderRect.x + 4, sliderRect.width / 12.0f, 290, startHours, startMinutes, endHours, endMinutes);
							curveScript.addCurves (sliderTime, sliderRect.x + 4, sliderRect.width / 12.0f, sliderRect.y - 190, startHours, startMinutes, endHours, endMinutes);
							writeCurveLabels (sliderRect.y - 24);
							if ((!answerSet) && (!levelCompleted) && ((myPlayerLevelFlags.textTips) && (hintsOn)) ) {
								feedbackString = feedbackString + " \n * Also notice how the arcs are added as you increase the elapsed time.";
								feedbackString = feedbackString + " \n * Try counting up the times on the arcs to work out the total elasped time";
							}
						}
					} else {
						if (myPlayerLevelFlags.arcTools) 
							curveScript.removeCurves ();
					}

					//position slider clock
					myDrawClocks.positionClock (Screen.width - analogClockSize - 580, 200, analogClockSize, sliderTime [0], sliderTime [1], 0.0f, "", myStyle, analogGuiClock, analogClockBackground, analogClockCenter, analogClockCenterSize);

					//string tempStr = setTimeandCheckAnswerProvided(timeSelectorRect);
					//if (!tempStr.Equals("")) feedbackString = tempStr;
					string answerString = setTimeandCheckAnswerProvided (timeSelectorRect);
					if (!answerString.Equals (""))
						feedbackString = answerString;
				} else {
					// slider has not been set 
					Event e = Event.current;
					if (e.isMouse && e.type == EventType.MouseDown && e.clickCount == 2) {
						mySliderFunctions.setStartMarkerValue (sliderValue);
					}

					//position the start marker for the first time
					if (myQuestion != null) {
						//feedbackString = feedbackString + " questions at this level " + myQuestion.getDescription();
						//feedbackString = myQuestion.getDescription();
						//feedbackString = "You have been given a start and end time. First ";
						float tempSliderValue = mySliderFunctions.positionStartMarker (feedbackString, feedbackRect, sliderRect, minSliderValue, maxSliderValue, measure, startHours, startMinutes, endHours, endMinutes, myClockFunctions, myMarkerStyle, myFeedbackFunctions);
						if (tempSliderValue != 0.0f) { 
							sliderValue = tempSliderValue;
							sliderSet = true;
							levelCompleted = false;
						}
					}
				}

				//position and label all the slider widgets... clock and timeline
				//load the timeline and slider clock
				//position and label the timeline
				sliderValue = GUI.HorizontalSlider (sliderRect, sliderValue, minSliderValue, maxSliderValue, sliderBackgroundStyle, thumbStyle);
				mySliderFunctions.assignLabels (0.0f, startMinutes, 12.0f, endMinutes, sliderRect.x, sliderRect.y, sliderRect.width, measure);
			} else {
				//No tools provided

				if (!feedbackString.Equals (""))
					myFeedbackFunctions.giveFeedback (feedbackRect, feedbackString);

				if (!answerSet) {
					if  (!levelCompleted)
						feedbackString = "Please enter your answer for the elasped time between the start and end times. Click Check when you are happy with your answer";
					else
						feedbackString = myPersistedLevelCompleted + " Please enter your answer for the elasped time between the start and end times. Click Check when you are happy with your answer";
				}

				string answerString = setTimeandCheckAnswerProvided (timeSelectorRect);
				if (!answerString.Equals ("")) {
					feedbackString = answerString;
					levelCompleted=false;
				}
				//else feedbackString = myLevelString + " Please enter your answer for the elasped time between the start and end times. Click Check when you are happy with your answer";

			}
		}
	}

	string setTimeandCheckAnswerProvided(Rect timeSelectorRect) {
		string myStr = "";
		GUI.BeginGroup (timeSelectorRect,timerStyle);
		
		answer = myClockFunctions.setTime (workingHours, workingMinutes, HrMinLabel, HrMinText, myPlusMinusButtonStyle);
		workingHours = answer [0];
		workingMinutes = answer [1];
		
		if (GUI.Button (new Rect (10,110,121,40), "Check", myOKButtonStyle)) {
			if ((answer [0] != 0.0f) || (answer [1] != 0.0f)) {
				answerSet=true;

				List<float> qAnswer = myQuestion.getAnswer();
				if ((qAnswer[0]==answer [0]) && (qAnswer[1]==answer [1])) {

					validAnswer=true;
					myPlayer.getcurrentQuestionLevel().questionCompleted(myQuestion, answer, validAnswer);
					//myStr="Great, that's correct. Now click Next Question to get another question";
					// new code here
					myStr="Great, that's correct.\n";
					Question tempQuestion = myPlayer.getcurrentQuestionLevel ().getNextUnansweredQuestion ();
					if (tempQuestion==null)
						myStr = myStr + checkIfLevelCompleted();
					myStr = myStr + "\nNow click Next Question to get another question";
					// end of new code
				} else {
					validAnswer=false;
					//myPlayer.getcurrentQuestionLevel().questionCompleted(myQuestion, answer, validAnswer);
					myStr="That's not quite right. Try again or click Next Question to skip to the next question";
				}
			}
		}
		GUI.EndGroup ();

		return myStr;
		
		//if (!answerFeedback.Equals(""))
		//	myFeedbackFunctions.giveFeedback (feedbackRect, answerFeedback);
	}

	void initialiseForNewQuestion() {
		mySliderFunctions.initialiseStartMarker();
		curveScript.removeCurves();
		validAnswer=false;
		sliderSet = false;
		feedbackString="";
		//answerString = "";
		workingHours=0.0f;
		workingMinutes=0.0f;
		myPlayerLevelFlags = myPlayer.getPlayerLevelFlags ();
		hintsOn = false;
	}

	string addButtons(Rect buttonRect) {
		string myString = "";
		//group buttons together .. then all button coordinates are relative to the group coordinates
		GUI.BeginGroup(buttonRect);
		//application quit button
		if (GUI.Button (new Rect (10,120,90,90), "Return", myReturnButton)) {
			// before exiting to start scene, just check if a level has been completed so that
			// the questions from the next level can be preloaded in anticipation for next 
			// time the user comes into play environment
			// load next question does not remove any items from the unanswered question list
			// and so can be called multiple times without moving the user on
			myQuestion=loadNextQuestion();
			if (myQuestion==null) {
				myString = checkIfLevelCompleted();
			}
			myPlayerScript.Save ();
			Application.LoadLevel(0);
		}
		
		//next question button
		if (!myPlayer.isFinished ()) {
			if (GUI.Button (new Rect (10, 20, 90, 90), "Next \nQuestion", myButtonStyle)) {
				myString = evaluateAnswer();
				myPlayerScript.Save ();
			} 
		}

		//hint button
		if ((myPlayerLevelFlags.textTips) && (sliderSet) && (!answerSet)) {
			if (hintsOn) {
				if (GUI.Button (new Rect (10, 220, 90, 90), "Hint \nOff?", myButtonStyle)) {
					hintsOn=false;
				} 
			} else {
				if (GUI.Button (new Rect (10, 220, 90, 90), "Hint \nOn?", myButtonStyle)) {
					hintsOn=true;
				} 
			}
		}

		GUI.EndGroup();

		return myString;
	}

	string evaluateAnswer() {
		string myString = "";
		//int correctAnswers = myPlayer.getcurrentQuestionLevel().getnumberAnswerCorrectly();
		//int noQuestions = myPlayer.getcurrentQuestionLevel().getnumberOfQuestions();

		if (!answerSet) {
			// need to discard the current question as the player wants to 
			// ignore it and move to the next question
			myPlayer.getcurrentQuestionLevel().questionCompleted(myQuestion, null, false);
		}

		if ((answerSet) && (!validAnswer)) // an answer but it was wrong so skip question but save answer given for information
			myPlayer.getcurrentQuestionLevel().questionCompleted(myQuestion, answer, validAnswer);

		answerSet=false;
		myQuestion = loadNextQuestion (); 

		if (myQuestion==null) {
			levelCompleted=true;
			myString = checkIfLevelCompleted();

			myString = myString + "\n\n Please ";
			myQuestion = loadNextQuestion (); 
		} 

		if (!myPlayer.isFinished()) {
			initialiseForNewQuestion();
		}

		return myString;
	}

	public string checkIfLevelCompleted() {
		string myString = "";
		float correctAnswers = (float) myPlayer.getcurrentQuestionLevel().getnumberAnswerCorrectly();
		float noQuestions = (float) myPlayer.getcurrentQuestionLevel().getnumberOfQuestions();

		myString = "You have completed all the questions in this level. You got " + correctAnswers + " out of " + noQuestions;
		if ((correctAnswers/noQuestions)>= passThreshold) {
			if (myPlayer.updatePlayerLevel()) {
				myString= myString + "\nYou have now moved to level " + myPlayer.getcurrentPlayerLevel().ToString () + "." + myPlayer.getcurrentQuestionLevel().getLevel().ToString() + " "; 

			} else {
				myPlayer.setToFinished();
			}
		} else {
			myString= myString + "\nYou just need a little bit more practice at this level. ";
			myString = myString + "You could also try using the Practice area to check your understanding before trying again";

			myPlayer.getcurrentQuestionLevel().reset();

		}

		return myString;
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
