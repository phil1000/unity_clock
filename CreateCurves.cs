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
	private bool startCurveSet=false;
	private float nextPoint=0.0f;
	private float nextHour=0.0f;

	// Use this for initialization
	void Start () {
		mylines = new List<VectorLine> ();
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
		splinePoints.Add (new Vector2 (startX + 20, y+60)); // control0
		splinePoints.Add (new Vector2 (endX, y)); // anchor 1
		splinePoints.Add (new Vector2 (endX-20, y+60)); // control 1
		
		spline.MakeCurve(splinePoints.ToArray(), segments);
		spline.Draw();
		mylines.Add (spline);
	}
	
	public void removeCurve(int index) {
		VectorLine myLine = mylines[index];
		VectorLine.Destroy (ref myLine);
		mylines.Remove (myLine);
	}

	public void removeCurves(int index) {
		for (int i=index; i<mylines.Count; i++) {
			VectorLine myLine = mylines [i];
			VectorLine.Destroy (ref myLine);
			mylines.Remove(myLine);
		}
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
	}

	public void addFirstPoint(List<float> sliderTime, float left, float width, float curveHeight, float startHours, float startMinutes) {
		
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
			this.addCurve (startPoint,startPoint+increment,curveHeight);
			startCurveSet=true;
			nextPoint = startPoint+increment;
			nextHour = startHours+1.0f;
		}
	}

	public void drawCurves(List<float> sliderTime, float left, float width, float curveHeight, float startHours, float endHours, float endMinutes) {

		float increment = 0.0f;
		float iterHours = startHours + 1.0f;
		int index = (int)(sliderTime [0] - nextHour);
		index=index+1;
		float lNext = nextPoint;

		if (sliderTime [0] < endHours) {
			int difference = (int)(sliderTime [0] - nextHour);
			if (this.getNumberOfCurves () == difference) {
				lNext = lNext + (width * (difference - 1)) - difference;
				this.addCurve (lNext, lNext + width, curveHeight);
			}
		}
			
		if (sliderTime [0] == endHours) {
			int difference = (int)(sliderTime [0] - nextHour);
			if (this.getNumberOfCurves () == difference) {
				lNext = lNext + (width * (difference - 1)) - difference;
				this.addCurve (lNext, lNext + width, curveHeight);
				if (endMinutes > 0.0f) {
					lNext = lNext + (width * (difference - 1)) - difference;
					/*if (endMinutes == 15.0f) {
						increment = 0.25f * width;
					} else {
						if (endMinutes == 30.0f) {
							increment = 0.5f * width;
						} else {
							if (endMinutes == 45.0f) {
								increment = 0.75f * width;
							} 
						}
					}*/
					//this.addCurve (lNext, lNext + increment, curveHeight);
					this.addCurve (lNext, lNext + width, curveHeight);
				}
			}
		}
		this.removeCurves(index);
		if ((sliderTime [0] - startHours) <= 0.0f) startCurveSet = false;
	}
}
