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
	[Serializable]
	public class AnsweredQuestionImpl : AnsweredQuestion
	{
		private Question question;
		private List<float> givenAnswer;
		private bool isCorrect = false;

		public AnsweredQuestionImpl (Question question, List<float> givenAnswer, bool isCorrect )
		{
			this.question = question;
			this.givenAnswer = givenAnswer;
			this.isCorrect = isCorrect;
		}

		public Question getQuestion() {
			return this.question;
		}

		public bool isCorrectAnswer () {
			return this.isCorrect;
		}

		public List<float> getGivenAnswer () {
			return this.givenAnswer;
		}
	}
}

