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
using System.Collections;
using System.Collections.Generic;
namespace AssemblyCSharp
{
		public interface ClockFunctions
		{
		bool checkTime(float marker, string startOrEnd, float minSliderValue, float maxSliderValue, string measure, float startHours, float startMinutes, float endHours, float endMinutes);
		List<float> deriveSliderHoursMins(float min, float max, float value, string measure);
		string deriveElapsedTime(List<float> sliderTime, bool elapsedIsSnapped, float sliderValue, float maxSliderValue, string measure, float startHours, float startMinutes);
		string timeLabel(string strName, float inHours, float inMinutes);
		string compareElapsedTime(List<float> proposed, float answer);
		}
}
