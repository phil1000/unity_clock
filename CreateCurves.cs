using Vectrosity;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using AssemblyCSharp;

public class CreateCurves : MonoBehaviour {

	public Material myMaterial;
	private int segments = 250;
	private List<VectorLine> mylines;
	private List<CurvePointLabel> myCurveLabels;
	private bool startCurveSet=false;
	private float nextPoint=0.0f;
	private float nextHour=0.0f;

	// Use this for initialization
	void Start () {
		mylines = new List<VectorLine> ();
		myCurveLabels = new List<CurvePointLabel> ();
	}

	public void setStartCurveSet(bool value) {
		startCurveSet = value;
	}

	public bool getStartCurveSet() {
		return startCurveSet;
	}

	public void addCurve(float startX, float endX, float y) {
		VectorLine spline = new VectorLine ("Spline", new Vector2[251], myMaterial, 4.0f, LineType.Continuous);
		List<Vector2> splinePoints = new List<Vector2>();
		splinePoints.Add (new Vector2 (startX, y)); // anchor0
		splinePoints.Add (new Vector2 (startX + 20, y+50)); // control0
		splinePoints.Add (new Vector2 (endX, y)); // anchor 1
		splinePoints.Add (new Vector2 (endX-20, y+50)); // control 1
		
		spline.MakeCurve(splinePoints.ToArray(), segments);
		spline.Draw();
		mylines.Add (spline);
	}

	public VectorLine getCurve(int index) {
		return mylines[index];
	}

	public int getNumberOfCurves() {
		return mylines.Count;
	}

	public void removeCurves() {
		for (int i=0;i<mylines.Count;i++) {
			VectorLine myLine = mylines[i];
			VectorLine.Destroy (ref myLine);
		}
		mylines.Clear();
		myCurveLabels.Clear ();
	}

	public void addCurves(List<float> sliderTime, float left, float width, float curveHeight, float startHours, float startMinutes, float endHours, float endMinutes) {
		if (addFirstPoint (sliderTime, left, width, curveHeight, startHours, startMinutes, endHours, endMinutes) > 0)
						return;
		//drawCurves (sliderTime, left, width, curveHeight, startHours, endHours, endMinutes);
		if (nextHour!=endHours) drawCurves (sliderTime, left, width, curveHeight, startHours, endHours, endMinutes);
		addLastPoint (sliderTime, left, width, curveHeight, startHours, endHours, endMinutes);
	}

	public int addFirstPoint(List<float> sliderTime, float left, float width, float curveHeight, float startHours, float startMinutes, float endHours, float endMinutes) {
		
		float startPoint = left + (width * startHours) - startHours;;
		float increment = 0.0f;
		string text = "";
		int returnValue = 0;

		if (this.getNumberOfCurves () == 0) {
			if ((sliderTime [0] - startHours) >= 1) {
				if (endHours>startHours) {
					if (startMinutes == 15.0f) {
						startPoint = startPoint + 0.25f * width;
						increment = 0.75f * width;
						text = "45m";
					} else {
						if (startMinutes == 30.0f) {
							startPoint = startPoint + 0.5f * width;
							increment = 0.5f * width;
							text = "30m";
						} else {
							if (startMinutes == 45.0f) {
								startPoint = startPoint + 0.75f * width;
								increment = 0.25f * width;
								text = "15m";
							} else {
								increment = width;
								text = "1h";
							}
						}
					}
				} else {
					// end hours must be same as start hours, so end minutes must be different
					returnValue=1;
					if (startMinutes == 15.0f) {
						startPoint = startPoint + 0.25f * width;
						if (endMinutes==30.0f) {
							increment = 0.25f * width;
							text = "15m";
						} else {
							//end minutes must be 45
							increment = 0.5f * width;
							text = "30m";
						}
					} else {
						if (startMinutes == 30.0f) {
							startPoint = startPoint + 0.5f * width;
							if (endMinutes==45.0f) {
								increment = 0.25f * width;
								text = "15m";
							}
						} else {
							if (startMinutes == 0.0f) {
								if (endMinutes==15.0f) {
									increment = 0.25f * width;
									text = "15m";
								} else {
									if (endMinutes==30.0f) {
										increment = 0.5f * width;
										text = "30m";
									} else {
										//end minutes must be 45
										increment = 0.75f * width;
										text = "45m";
									}
								}
							}
						}
					}
				}
				this.addCurve (startPoint, startPoint + increment, curveHeight);
				CurvePointLabel myPoint = new CurvePointLabel(startPoint, increment, text);
				myCurveLabels.Add(myPoint);
				nextPoint = startPoint + increment;
				nextHour = startHours + 1.0f;
			} 
		}

		return returnValue;
	}

	public void drawCurves(List<float> sliderTime, float left, float width, float curveHeight, float startHours, float endHours, float endMinutes) {

		float increment = 0.0f;
		float iterHours = nextHour + 1.0f;
		float lNext = nextPoint;
		int difference;
		string text = "";

		while (iterHours <= sliderTime[0]) {
			if (iterHours <= endHours) {
				difference = (int)(iterHours - nextHour); // number of hours between this hour and start hour
				if (this.getNumberOfCurves () == difference) { // check whether this curve has already been set
					difference = (int)(iterHours - nextHour);
					lNext = nextPoint + (width * (difference - 1)) - difference;
					this.addCurve (lNext, lNext + width, curveHeight);
					CurvePointLabel myPoint = new CurvePointLabel(lNext, width, "1h");
					myCurveLabels.Add(myPoint);
				}
			} else break;
			iterHours = iterHours + 1.0f;
		}
		nextHour = iterHours - 1.0f; // this is set to the slider hour
		nextPoint = lNext + width;
	}
	public void addLastPoint(List<float> sliderTime, float left, float width, float curveHeight, float startHours, float endHours, float endMinutes) {
		
		float increment = 0.0f;
		float iterHours = nextHour; // set to the slider hour
		int difference;
		float lNext = nextPoint;
		string text = "";

		if (endMinutes == 0.0f)
						return;

		while (iterHours <= sliderTime[0]) {
			if (((iterHours==endHours) && (sliderTime[1]>=endMinutes)) || (iterHours>endHours) ) {
			difference = (int)(iterHours - (startHours+1.0f)); // number of hours between this hour and start hour
				if (endMinutes == 15.0f) {
					increment = 0.25f * width;
					text = "15m";
				} else {
					if (endMinutes == 30.0f) {
						increment = 0.5f * width;
						text = "30m";
					} else {
						if (endMinutes == 45.0f) {
							increment = 0.75f * width;
							text = "45m";
						} 
					}
				}
				this.addCurve (lNext, lNext + increment, curveHeight);
				CurvePointLabel myPoint = new CurvePointLabel (lNext, increment, text);
				myCurveLabels.Add (myPoint);
			}
			if ((iterHours>endHours) || ((iterHours==endHours && sliderTime[1] >= endMinutes))) break;
			iterHours = iterHours + 1.0f;
		}
	}
	/*public string addLastPoint(List<float> sliderTime, float left, float width, float curveHeight, float startHours, float endHours, float endMinutes) {
		
		float increment = 0.0f;
		float iterHours = startHours + 1.0f;
		float lNext = nextPoint;
		string temp="";
		string text = "";

		if ((sliderTime [0] == endHours) && (sliderTime[1] >= endMinutes)){ 
			int difference = (int)(sliderTime [0] - nextHour);
			if ((endMinutes != 0.0f) && (this.getNumberOfCurves () == (difference+1))) {
				lNext = lNext + (width * (difference - 1)) - difference + width;
				if (endMinutes == 15.0f) {
					increment = 0.25f * width;
					text = "15m";
				} else {
					if (endMinutes == 30.0f) {
						increment = 0.5f * width;
						text = "30m";
					} else {
						if (endMinutes == 45.0f) {
							increment = 0.75f * width;
							text = "45m";
						} 
					}
				}
				this.addCurve (lNext, lNext + increment, curveHeight);
				CurvePointLabel myPoint = new CurvePointLabel(lNext, increment, text);
				myCurveLabels.Add(myPoint);
				temp=" added last point curve";
			}
			temp=" didnt add last point curve, curve no wrong - " + this.getNumberOfCurves().ToString ();
		}
		return temp;
	}*/

	public List<CurvePointLabel> getCurvePointLables() {
		return myCurveLabels;
	}

}
