using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using AssemblyCSharp;
using System.Text;
using System.IO; 
using System;

public class PlayerScript : MonoBehaviour {

	private List<Question> questions;
	private Hashtable players;
	private List<PlayerLevelFlags> playerLevels;
	private int currentQuestion = 0;
	public bool elapsedIsSnapped;
	private static string PLAYER_FILE = "player.txt";
	private static string PLAYER_LEVEL_FILE = "playerLevel.txt";
	//private static string QUESTION_FILE = "questionsNew1.txt";
	private static string QUESTION_FILE = "questionsNew3.txt";
	private Player currentPlayer;
	private bool loggedIn = false;
	
	void Start () {
		elapsedIsSnapped = true;
		this.loadQuestions (); // pre load in case this is a new player
		playerLevels = this.loadPlayerLevels ();

		players = this.loadPlayers (); // load existing players
		if (players == null) {
			players = new Hashtable();
		}
	}

	void Awake() {
		// this class is available across all scenes and so should not be destroyed
		// when a new scene is loaded
		DontDestroyOnLoad(transform.gameObject);
	}

	private void addPlayer(Player myPlayer) {
		if (players == null)
			players = new Hashtable ();
		players.Add (myPlayer.getId(), myPlayer);
	}

	public Player getCurrentPlayer() {
		return currentPlayer;
	}

	public Player getPlayer(string name) {
		// first check if existing player and if so return a pointer
		// to the player
		if (players.ContainsKey(name.GetHashCode())) {
			currentPlayer = (Player) players [name.GetHashCode()];
		} else {
			// else create a new player, add it to the player hashtable
			currentPlayer = new PlayerImpl(name, questions, playerLevels);
			this.addPlayer(currentPlayer);
			questions = currentPlayer.getcurrentQuestionLevel().getUnansweredQuestions();
		}
		loggedIn = true;
		return currentPlayer;
	}

	public bool isLoggedIn() {
		return loggedIn;
	}

	private Hashtable loadPlayers() {
		try
		{
			Stream s = File.Open (PLAYER_FILE, FileMode.Open, FileAccess.Read);
			BinaryFormatter b = new BinaryFormatter ();
			return (Hashtable)b.Deserialize (s);
		} catch (Exception e) {
			Console.WriteLine("{0}\n", e.Message);
			return null;
		}

		/*if (File.Exists (PLAYER_FILE)) {
			Stream s = File.Open (PLAYER_FILE, FileMode.Open, FileAccess.Read);
			BinaryFormatter b = new BinaryFormatter ();
			return (Hashtable)b.Deserialize (s);
		} else
			return null;*/
	}

	public void Save()
	{
		if (players==null) return;
		Stream s = File.Open(PLAYER_FILE, FileMode.Create, FileAccess.ReadWrite);
		BinaryFormatter b = new BinaryFormatter();
		b.Serialize(s, players);
		s.Close();      
	}	

	private List<PlayerLevelFlags> loadPlayerLevels() {
		//// the read file contains the player levels
		// it should contain one line for each level
		// Each line should contain 4 items delimited by commas e.g. 
		// 1, true, false, false
		// which represents the values for: 
		// levelId, textTips, ArcTools, SliderTools
		// levelId: can be any integer e.g. 1-4
		// textTips: can be true or false
		// ArcTools: can be true or false
		// SliderTools: can be true or false
		List<PlayerLevelFlags> myPlayerLevels = new List<PlayerLevelFlags>();

		List<string> playerLevelsArray = loadFromFile (PLAYER_LEVEL_FILE);
		string [] entries;
		foreach (string line in playerLevelsArray) {
			// ignore comment lines in the file i.e. those starting with "//"
			if (!(line.Substring(0,2).Equals("//"))) {
				entries = line.Split(',');
				//PlayerLevelFlags (int id, bool textTips, bool arcTools, bool sliderTool)
				int id = int.Parse(entries[0]);
				bool textTips = (entries[1].Equals("true")) ? true : false;
				bool arcTools = (entries[2].Equals("true")) ? true : false;
				bool sliderTools = (entries[3].Equals("true")) ? true : false;
				PlayerLevelFlags flags = new PlayerLevelFlags(id, textTips, arcTools, sliderTools);
				myPlayerLevels.Add(flags);
			}
		}
		return myPlayerLevels;
	}

	private void loadQuestions() {
		Question myQuestion = null;
		questions = new List<Question>();
		//List<string> questionsArray = loadFromFile ("questionsNew1.txt");
		List<string> questionsArray = loadFromFile (QUESTION_FILE);
		string[] entries;

		// The questions file contain multiple question lines,
		// and each question line is comma delimited. The format 
		// of each line is as follows (all float unless specified otherwise):
		// startHours, startMinutes, endHours, endMinutes, measure (a string), answer
		foreach (string line in questionsArray) {
			// ignore comment lines in the file i.e. those starting with "//"
			if (!(line.Substring(0,2).Equals("//"))) {
				entries = line.Split (',');
				float minSliderValue=0.0f;
				float SliderValue=minSliderValue;
				float maxSliderValue;
				if (entries[6].Equals("whole")) {
					maxSliderValue=12.0f;
				} else {
					if (entries[6].Equals("half")) {
						maxSliderValue=24.0f;
					} else {
						maxSliderValue=48.0f;
					}
				}

				List<float> answer = new List<float>();
				answer.Add (float.Parse(entries[7]));
				answer.Add (float.Parse(entries[8]));

				myQuestion = new QuestionImpl (int.Parse (entries[0]),
				                          entries[1],
				                          float.Parse(entries[2]), 
				                          float.Parse(entries[3]), 
				                          float.Parse(entries[4]), 
				                          float.Parse(entries[5]), 
				                          SliderValue, 
				                          maxSliderValue, 
				                          minSliderValue, 
				                          entries[6], 
				                          answer);
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
