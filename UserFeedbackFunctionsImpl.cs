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
	public class UserFeedbackFunctionsImpl : UserFeedbackFunctions
	{
		private GUIStyle textStyle;
		private GUIStyle boxStyle;

		public void updateStyles(GUIStyle boxStyle, GUIStyle textStyle) {
			this.textStyle = textStyle;
			this.boxStyle = boxStyle;
		}

		public void giveFeedback(Rect feedbackRect, string strMessage) {
			GUI.BeginGroup (feedbackRect);
			GUI.Label(new Rect(0, 0, 100, 100), "", boxStyle); 
			GUI.Label(new Rect(100, 0, 300, 150), strMessage, textStyle); 
			GUI.EndGroup ();
		}
	}
}

