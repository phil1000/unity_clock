//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18033
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;
namespace AssemblyCSharp
{
	public class SliderFunctionsImpl : SliderFunctions
	{
		private bool setStartSliderValue=true;
		private float startMarker=99.0f;

		public void initialiseStartMarker() {
			startMarker=99.0f;
			setStartSliderValue=true;
		}

		public void setStartMarkerValue(float sliderValue) {
			startMarker = sliderValue;
		}

		public void assignLabels(float startHours, float startMinutes, float endHours, float endMinutes, float sliderLeft, float top, float sliderWidth, string measure) {
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

		public float getStartMarkerValue() {
			return startMarker;
		}

		public float positionStartMarker(float startMarker, Rect sliderRect, float minSliderValue, float maxSliderValue, GUIStyle myMarkerStyle) {
			// that start has already been set so you just want to position it
			float sliderValue = 0.0f;
			Rect startRect = positionMarker(startMarker, sliderRect, minSliderValue, maxSliderValue);
			if (GUI.Button(startRect, "S", myMarkerStyle)) {
				sliderValue=startMarker;
			}
			return sliderValue;
		}

		public float positionStartMarker(string feedbackPrefix, Rect feedbackRect, Rect sliderRect, float minSliderValue, float maxSliderValue, string measure, float startHours, float startMinutes, float endHours, float endMinutes, ClockFunctions myClockFunctions, GUIStyle myMarkerStyle, UserFeedbackFunctions myFeedbackFunctions) {

			float mySliderValue = 0.0f;
			int myResult = 1; // it is assumed to be negative unless positively set otherwise

			if (startMarker == 99.0f) {
				string tempStr = feedbackPrefix + " set the start marker by double clicking where you think the start time is on the timeline";
				myFeedbackFunctions.giveFeedback(feedbackRect, tempStr);
			} else {
				startMarker = Mathf.Round(startMarker);
				myResult = myClockFunctions.checkTime(startMarker, minSliderValue, maxSliderValue, measure, startHours, startMinutes);

				if (myResult<0) {
					myFeedbackFunctions.giveFeedback(feedbackRect, "That's not quite right, you are a bit early. Try again");
					mySliderValue = 0.0f;
				} else {
					if (myResult > 0) {
						myFeedbackFunctions.giveFeedback(feedbackRect, "That's not quite right, you are a bit late. Try again");
						mySliderValue = 0.0f;
					} else {
						// position the startMarker but first align it precisely
						if (setStartSliderValue) {
							mySliderValue=startMarker;
							setStartSliderValue=false;
						}
						Rect startRect = positionMarker(startMarker, sliderRect, minSliderValue, maxSliderValue);
						if (GUI.Button(startRect, "S", myMarkerStyle)) {
							mySliderValue=startMarker;
						}
					}
				}
			}
			return mySliderValue;
		}
		
		public Rect positionMarker (float marker, Rect sliderRect, float minSliderValue, float maxSliderValue) {
			var y = sliderRect.y + sliderRect.height;
			var progress = (marker-minSliderValue) / (maxSliderValue-minSliderValue);
			
			var xMin = sliderRect.xMin;
			var xMax = sliderRect.xMax;
			var x = Mathf.Lerp(xMin, xMax, progress);
			float width = 60.0f;
			return new Rect (x-(width/2), y - 6, width, width);
		}
	}
}

