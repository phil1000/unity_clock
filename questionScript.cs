using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using System.Text;
using System.IO; 
using System;

public class questionScript : MonoBehaviour {

	private List<Question> questions;
	private int currentQuestion = 0;
	public bool elapsedIsSnapped;
	
	void Start () {
		elapsedIsSnapped = true;
		this.loadQuestions ();
	}

	void Awake() {
		// this class is available across all scenes and so should not be destroyed
		// when a new scene is loaded
		DontDestroyOnLoad(transform.gameObject);
	}
	
	private void loadQuestions() {
		Question myQuestion = null;
		questions = new List<Question>();
		List<string> questionsArray = loadFromFile ("questionsNew.txt");
		string[] entries;

		// The questions file contain multiple question lines,
		// and each question line is comma delimited. The format 
		// of each line is as follows (all float unless specified otherwise):
		// startHours, startMinutes, endHours, endMinutes, measure (a string), answer
		foreach (string line in questionsArray) {
			// ignore comment lines in the file i.e. those starting with "//"
			if (!(line.Substring(0,2).Equals("//"))) {
				entries = line.Split (',');
				//float minSliderValue=float.Parse(entries[0])-2.0f;
				float minSliderValue=0.0f;
				float SliderValue=minSliderValue;
				//float increment=0.0f;
				float maxSliderValue;
				if (entries[4].Equals("whole")) {
					maxSliderValue=12.0f;
				} else {
					if (entries[4].Equals("half")) {
						maxSliderValue=24.0f;
					} else {
						maxSliderValue=48.0f;
					}
				}
				//float maxSliderValue=minSliderValue + ((float.Parse(entries[2])+2.0f-minSliderValue)*increment);*/
				myQuestion = new Question (float.Parse(entries[0]), 
				                           float.Parse(entries[1]), 
				                           float.Parse(entries[2]), 
				                           float.Parse(entries[3]), 
				                           SliderValue, 
				                           maxSliderValue, 
				                           minSliderValue, 
				                           entries[4], 
				                           float.Parse(entries[5]));
				questions.Add (myQuestion);
			}
		}
	}
	
	private List<string> loadFromFile(string fileName)
	{
		// Handle any problems that might arise when reading the text
		List<string> fileLines = new List<string>();
		try
		{
			string line;
			// Create a new StreamReader, stating fileName and encoding
			StreamReader theReader = new StreamReader(fileName, Encoding.Default);
			
			// Immediately clean up the reader after this block of code is done.
			// You generally use the "using" statement for potentially memory-intensive objects
			// instead of relying on garbage collection.

			using (theReader)
			{
				// While there's lines left in the text file, do this:
				do
				{
					line = theReader.ReadLine();
					
					if (line != null)
					{
						fileLines.Add(line);
					}
				}
				while (line != null);
				
				// Done reading, close the reader and return the file contents to broadcast success    
				// null is returned if no questions or no file found
				theReader.Close();
				return fileLines;
			}
		}
		
		// If anything broke in the try block, we throw an exception with information
		// on what didn't work
		catch (Exception e)
		{
			Console.WriteLine("{0}\n", e.Message);
			return fileLines; //will be null if no questions
		}
	}
	//

	public Question getNextQuestion() {
		if (currentQuestion > (questions.Count - 1)) {
			return null;
		}
		else {
			Question myQuestion = questions[currentQuestion];
			currentQuestion++;
			return myQuestion;
		}
	}		
}
