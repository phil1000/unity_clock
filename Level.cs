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
	public interface Level
	{
		void questionCompleted (Question question, List<float> givenAnswer, bool isCorrect) ;
		void reset();
		int getLevel ();
		int getnumberOfQuestions () ;
		int getnumberOfQuestionsCompleted ();
		int getnumberAnswerCorrectly ();
		List<Question> getUnansweredQuestions ();
		Question getNextUnansweredQuestion();
		List<AnsweredQuestion> getAnsweredQuestions ();
	}
}

