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
namespace AssemblyCSharp
{
	public class Question {
		private float startHours;
		private float startMinutes;
		private float endHours;
		private float endMinutes;
		private float sliderValue;
		private float maxSliderValue;
		private float minSliderValue;
		private string measure;
		private float answer;
		
		public Question(float startHours, float startMinutes, float endHours, float endMinutes, float sliderValue, float maxSliderValue, float minSliderValue, string measure, float answer) {
			this.startHours = startHours;
			this.startMinutes = startMinutes;
			this.endHours = endHours;
			this.endMinutes = endMinutes;
			this.sliderValue = sliderValue;
			this.maxSliderValue = maxSliderValue;
			this.minSliderValue = minSliderValue;
			this.measure = measure;
			this.answer = answer;
		}
		
		public float getStartHours() { 
			return this.startHours;
		}
		public float getsStartMinutes() { 
			return this.startMinutes;
		}
		public float getEndHours() { 
			return this.endHours;
		}
		public float getEndMinutes() { 
			return this.endMinutes;
		}
		public float getSliderValue() { 
			return this.sliderValue;
		}
		public float getMaxSliderValue() { 
			return this.maxSliderValue;
		}
		public float getMinSliderValue() { 
			return this.minSliderValue;
		}
		public string getMeasure() { 
			return this.measure;
		}
		public float getAnswer() { 
			return this.answer;
		}
	}
}

