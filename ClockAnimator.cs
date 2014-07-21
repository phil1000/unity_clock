using UnityEngine;
using System.Collections;
using System;

public class ClockAnimator : MonoBehaviour {
	
	public Transform hours, minutes;
	//public Transform seconds;
	public bool analog;
	public bool setTime;
	private const float 
		hoursToDegrees = 360f / 12f,
		minutesToDegrees = 360f / 60f;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (setTime) {
			hours.localRotation = Quaternion.Euler (0f, 0f, (float)2f * -hoursToDegrees);
			minutes.localRotation = Quaternion.Euler (0f, 0f, (float)15f * -minutesToDegrees);
			return;
		}
		if (analog) {
			TimeSpan timespan = DateTime.Now.TimeOfDay;
			hours.localRotation = Quaternion.Euler (0f, 0f, (float) timespan.TotalHours * -hoursToDegrees);
			minutes.localRotation = Quaternion.Euler (0f, 0f, (float) timespan.TotalMinutes * -minutesToDegrees);
		} else {
			DateTime time = DateTime.Now;
			hours.localRotation = Quaternion.Euler (0f, 0f, time.Hour * -hoursToDegrees);
			minutes.localRotation = Quaternion.Euler (0f, 0f, time.Minute * -minutesToDegrees);
		}
	}
}
