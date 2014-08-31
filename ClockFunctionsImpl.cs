//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18033
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
namespace AssemblyCSharp
{
	public class ClockFunctionsImpl : ClockFunctions {

		public int checkTime(float marker, float minSliderValue, float maxSliderValue, string measure, float hours, float minutes) {
			// return 0 if same, 1 if slider > time, -1 if slider < time
			int returnVal;
			List<float> lSliderTime = deriveSliderHoursMins(minSliderValue, maxSliderValue, marker, measure);

			if (lSliderTime [0] > hours) {
				returnVal=1; // means a minimum of 15 minutes later
			} else { 
				if (lSliderTime [0] == hours) {
					if ((lSliderTime [1] - minutes) > 10.0f) {
						returnVal=1;
					} else {
						if ((minutes - lSliderTime [1]) > 10.0f) {
							returnVal=-1;
						} else {
							returnVal= 0;
						}
					}
				} else {
					// to get here the slider hours are less than the passed hours
					if ((hours - lSliderTime [0]) > 1.0f) {
						returnVal= -1;
					} else {
						if ((60.0f - lSliderTime [1]) > 10.0f) {
							returnVal= -1;
						} else {
							returnVal= 0;
						}
					}
				}
			}
			return returnVal;
		}

		public string compareElapsedTime(List<float> proposed, float answer) {
			string feedback;
			float hours = (int)answer;
			float difference = answer - hours;
			float minutes=0.0f;

			if (difference == 0.5f)
								minutes = 30.0f;
			if (difference == 0.25f)
								minutes = 15.0f;
			if (difference == 0.75f)
								minutes = 45.0f;

			if (proposed [0] == hours) {
				if (proposed[1] == minutes) {
					feedback = "Well done, that is correct";
				} else {
					feedback = "the hours are correct but minutes are not quite right";
				}
			} else {
				feedback = "The hours aren't quite right, try again";
			}
			return feedback;
		}

		public string deriveElapsedTimeString(List<float> sliderTime, bool elapsedIsSnapped, float sliderValue, float maxSliderValue, string measure, float startHours, float startMinutes) {
			// derives the elapsed time between the time represented on the slider and 
			// the start time. The value returned is translated into a string and 
			// is set to blanks if the slider time < start time
			if ((sliderTime [0] == startHours) && (sliderTime [1] == startMinutes)) return "Elapsed Time = 0 minutes";
			if (sliderTime [0] < startHours) return "";
			if ((sliderTime [0] == startHours) && (sliderTime [1] < startMinutes)) return "";
			
			string elapsed = "";

			List<float> elapsedTime = deriveElapsedTime(sliderTime, elapsedIsSnapped, sliderValue, maxSliderValue, measure, startHours, startMinutes);

			if (elapsedTime[0] != 0) {
				elapsed = "Elapsed Time = " + elapsedTime[0].ToString ();
				if (elapsedTime[0] == 1) elapsed = elapsed + " hour";
				else elapsed = elapsed + " hours";
				if (elapsedTime[1] != 0)
					elapsed = elapsed + " and " + elapsedTime[1].ToString () + " minutes";
			} else {
				elapsed = "Elapsed Time = " + elapsedTime[1].ToString () + " minutes";
			}
			return elapsed;
		}

		//public string deriveElapsedTimeString(List<float> answer, List<float> sliderTime, bool elapsedIsSnapped, float sliderValue, float maxSliderValue, string measure, float startHours, float startMinutes) {
		public string deriveElapsedTimeString(List<float> answer) {

			// derives the elapsed time between the time represented on the slider and 
			// the start time. The value returned is translated into a string and 
			// is set to blanks if the slider time < start time
			//if ((sliderTime [0] == startHours) && (sliderTime [1] == startMinutes)) return "Elapsed Time = 0 minutes";
			//if (sliderTime [0] < startHours) return "";
			//if ((sliderTime [0] == startHours) && (sliderTime [1] < startMinutes)) return "";
			
			string elapsed = "Elapsed Time = ";
			
			//List<float> elapsedTime = deriveElapsedTime(sliderTime, elapsedIsSnapped, sliderValue, maxSliderValue, measure, startHours, startMinutes);

			//if ((elapsedTime[0] > answer[0]) || ((elapsedTime [0] == answer[0]) && (elapsedTime [1] >= answer[1]))) {
				if (answer[0]!=0) {
					elapsed = elapsed + answer[0].ToString();
					if (answer[0] == 1) elapsed = elapsed + " hour";
					else elapsed = elapsed + " hours";
					if (answer[1] != 0)
						elapsed = elapsed + " and " + answer[1].ToString () + " minutes";
				} else {
					elapsed = "Elapsed Time = " + answer[1].ToString () + " minutes";
				}

				//return elapsed;
			//}
			return elapsed;

			/*if (elapsedTime[0] != 0) {
				elapsed = elapsed + elapsedTime[0].ToString ();
				if (elapsedTime[0] == 1) elapsed = elapsed + " hour";
				else elapsed = elapsed + " hours";
				if (elapsedTime[1] != 0)
					elapsed = elapsed + " and " + elapsedTime[1].ToString () + " minutes";
			} else {
				elapsed = "Elapsed Time = " + elapsedTime[1].ToString () + " minutes";
			}
			return elapsed;*/

		}

		public List<float> deriveElapsedTime(List<float> sliderTime, bool elapsedIsSnapped, float sliderValue, float maxSliderValue, string measure, float startHours, float startMinutes) {
			List<float> elapsedTime = new List<float> ();

			DateTime sliderDate = new DateTime (DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, (int)sliderTime [0], (int)sliderTime [1],0);
			DateTime startDate = new DateTime (DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, (int)startHours, (int)startMinutes, 0);
			
			TimeSpan difference = sliderDate.Subtract (startDate);
			int hoursDiff = difference.Hours;
			int minsDiff = difference.Minutes;

			if ( (elapsedIsSnapped) && (sliderValue!=maxSliderValue) ) {
				
				if (measure.Equals("half")) {
					if (minsDiff >= 30)
						minsDiff = 30;
					else
						minsDiff = 0;
				} else {
					if (measure.Equals("quarter")) {
						if (minsDiff < 15)
							minsDiff = 0;
						if ( (minsDiff>=15) && (minsDiff<30) )
							minsDiff = 15;
						if ( (minsDiff>=30) && (minsDiff<45) )
							minsDiff = 30;
						if ( (minsDiff>=45) && (minsDiff<60) )
							minsDiff = 45;
					} else {
						minsDiff = 0;
					}
				}
			}
			elapsedTime.Add(hoursDiff);
			elapsedTime.Add(minsDiff);
			return elapsedTime;
		}

		public List<float> deriveSliderHoursMins(float min, float max, float value, string measure) {
			List<float> sliderTime = new List<float>();
			
			// first find how far along this notch we are
			float intValue = (int) value;
			float diff = Math.Abs(value - intValue);
			float increment = 0.0f; 
			float diffFromMin = (int) Math.Abs (value - min);
			
			if (measure.Equals("half")) {
				//now need to work out if notch is first or second part of an hour
				if ( (Math.Abs(intValue-min) % 2) != 0) {
					//odd number of notches so add 30
					sliderTime.Add(min + (((diffFromMin-1)*0.5f)));
					increment = 30.0f;
				} else {
					sliderTime.Add(min + ((diffFromMin*0.5f)));
					
				}
				sliderTime.Add(increment + diff*30);
			} else {
				if (measure.Equals("whole")) {
					sliderTime.Add((int) value);
					sliderTime.Add(diff*60);
				} else {
					// each notch is quarter hours
					float calcHours = intValue*15/60;
					float intCalcHours = (int) calcHours;
					sliderTime.Add (intCalcHours);
					diff=(diff*15)+(60*(calcHours-intCalcHours));
					sliderTime.Add(diff);
				}
			}
			return sliderTime;
		}

		public string timeLabel(string strName, float inHours, float inMinutes) {
			string returnString = strName + " = " + inHours.ToString ();
			if (inMinutes == 0.0f) {
				returnString = returnString + " o'clock";
			} else {
				if (inMinutes == 15.0f) {
					returnString = returnString + ":15";
				} else {
					if (inMinutes == 30.0f) {
						returnString = returnString + ":30";
					} else {
						returnString = returnString + ":45";
					}
				}
			}
			
			return returnString;
		}

		public List<float> setTime (float workingHours, float workingMinutes, GUIStyle HrMinLabel, GUIStyle HrMinText, GUIStyle myPlusMinusButtonStyle) {
			List<float> myTime = new List<float>();
			GUI.Label (new Rect (10, 20, 60, 40), "Hours", HrMinLabel);
			GUI.Label (new Rect (75, 20, 30, 40), workingHours.ToString (), HrMinText);
			if (GUI.Button (new Rect (110,20,20,20), "+", myPlusMinusButtonStyle)) {
				if (workingHours<12.0f) workingHours++;
			}
			if (GUI.Button (new Rect (110,40,20,20), "-",myPlusMinusButtonStyle )) {
				if (workingHours>0.0f) workingHours--;
			}
			GUI.Label (new Rect (10, 65, 60, 40), "Minutes", HrMinLabel);
			GUI.Label (new Rect (75, 65, 30, 40), workingMinutes.ToString(), HrMinText);
			if (GUI.Button (new Rect (110,65,20,20), "+", myPlusMinusButtonStyle)) {
				if (workingMinutes==45.0f) {
					if (workingHours<=11.0f) {
						workingMinutes=0.0f;
						workingHours++;
					}

				}
				else workingMinutes += 15.0f;
			}
			if (GUI.Button (new Rect (110,85,20,20), "-", myPlusMinusButtonStyle)) {
				if (workingMinutes==0.0f) {
					if (workingHours>=1.0f) {
						workingMinutes=45.0f;
						workingHours--;
					}
				}
				else workingMinutes -= 15.0f;
			}
			myTime.Add(workingHours);
			myTime.Add(workingMinutes);
			return myTime;
		}

	}
}

