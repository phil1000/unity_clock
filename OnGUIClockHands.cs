using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Draws the hands of an analog clock on OnGUI.
/// </summary>
public class OnGUIClockHands : MonoBehaviour 
{
	//public ClockTimer timer;
	
	/// <summary>
	/// The texture for the hour hand.
	/// A vertical texture pointing upward is assumed.
	/// </summary>
	public Texture hourHand;
	
	/// <summary>
	/// The texture for the minute hand.
	/// A vertical texture pointing upward is assumed.
	/// </summary>
	public Texture minuteHand;
	public Rect minuteHandRect;
	
	/// <summary>
	/// The texture for the second hand.
	/// A vertical texture pointing upward is assumed.
	/// </summary>
	public Texture secondHand;
	
	/// <summary>
	/// Pivot point for the hour hand.
	/// (0.5f, 0.5f) results in the hand rotating
	/// around the center of the texture.
	/// (0.5f, 1) results in the hand rotating
	/// around the bottom center of the texture.
	/// </summary>
	public Vector2 hourHandPivot;
	
	/// <summary>
	/// Pivot point for the minute hand.
	/// (0.5f, 0.5f) results in the hand rotating
	/// around the center of the texture.
	/// (0.5f, 1) results in the hand rotating
	/// around the bottom center of the texture.
	/// </summary>
	public Vector2 minuteHandPivot;
	
	/// <summary>
	/// Pivot point for the second hand.
	/// (0.5f, 0.5f) results in the hand rotating
	/// around the center of the texture.
	/// (0.5f, 1) results in the hand rotating
	/// around the bottom center of the texture.
	/// </summary>
	public Vector2 secondHandPivot;
	
	public Vector2 hourHandScale = new Vector2(0.25f, 0.35f);
	public Vector2 minuteHandScale = new Vector2(0.2f, 0.45f);
	public Vector2 secondHandScale = new Vector2(0.15f, 0.4f);
	
	public int cyclesPerDay = 2;

	private const float hoursToDegrees = 360f/12f,
	minutesToDegrees = 360f/60f, secondsToDegrees = 360f/60f;
	
	public void Draw(Rect position, float inHours, float inMinutes, float inSeconds)
	{
		Matrix4x4 matrixBackup = GUI.matrix;
			
		float angle = 0;
		//TimeSpan timespan = DateTime.Now.TimeOfDay;
			
		if (hourHand != null)
		{
			//angle = (float) timespan.TotalHours * hoursToDegrees;
			// the following line sets the time to a specific hour
			angle = (float) (inHours + (inMinutes/60f))* hoursToDegrees;

			GUI.matrix = matrixBackup;
			GUIUtility.RotateAroundPivot(angle, position.center);
				
			float width = position.width * hourHandScale.x;
			float height = position.height * hourHandScale.y;
			float leftStartPos = position.center.x - width * hourHandPivot.x;
			float topStartPos = position.center.y - height * hourHandPivot.y;
				
			GUI.DrawTexture(new Rect(leftStartPos, topStartPos, width, height), hourHand);
		}
		if (minuteHand != null)
		{
			//angle = (float) timespan.TotalMinutes * minutesToDegrees;
			angle = (float) inMinutes * minutesToDegrees;
				
			GUI.matrix = matrixBackup;
				
			GUIUtility.RotateAroundPivot(angle, position.center);
				
			float width = position.width * minuteHandScale.x;
			float height = position.height * minuteHandScale.y;
			float leftStartPos = position.center.x - width * minuteHandPivot.x;
			float topStartPos = position.center.y - height * minuteHandPivot.y;

			minuteHandRect = new Rect(leftStartPos, topStartPos, width, height);
			GUI.DrawTexture(minuteHandRect, minuteHand);
		}
		if ((inSeconds!=0.0f))
		{
			//angle = (float) timespan.TotalSeconds * secondsToDegrees;
			angle = (float) inSeconds * secondsToDegrees;
				
			GUI.matrix = matrixBackup;
				
			GUIUtility.RotateAroundPivot(angle, position.center);

			float width = position.width * secondHandScale.x;
			float height = position.height * secondHandScale.y;
			float leftStartPos = position.center.x - width * secondHandPivot.x;
			float topStartPos = position.center.y - height * secondHandPivot.y;
				
			GUI.DrawTexture(new Rect(leftStartPos, topStartPos, width, height), secondHand);
		}
		GUI.matrix = matrixBackup;
	}	
}
