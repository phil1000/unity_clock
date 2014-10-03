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

	private bool invalidFiles = false;

	private string playerFile;
	private string playerLevelFile;
	private string questionFile;
	private static string CONFIG_FILE = "config.txt";

	private Player currentPlayer;
	private bool loggedIn = false;
	private float passThreshold=0.0f;
	
	void Start () {
		elapsedIsSnapped = true;
		//load the config data in order to get location for player and question files
		if (!this.loadConfigData ()) { 
			// something wrong with config file so need to abort
			invalidFiles=true;
			return;
		}

		// pre load in case this is a new player
		if (!this.loadQuestions ()) {
			// something wrong with question file so need to abort
			invalidFiles=true;
			return;
		}

		// load player levels to see how many levels and which tools available in each level
		playerLevels = this.loadPlayerLevels ();
		if (playerLevels == null) {
			// something wrong with player levels file so need to abort
			invalidFiles=true;
			return;
		}

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

	public float getPassThreshold() {
		return passThreshold;
	}
	
	public bool isInvalidFiles() {
		return invalidFiles;
	}

	public bool isLoggedIn() {
		return loggedIn;
	}

	private Hashtable loadPlayers() {
		try
		{
			Stream s = File.Open (playerFile, FileMode.Open, FileAccess.Read);
			BinaryFormatter b = new BinaryFormatter ();
			//start of alternative code
			Hashtable myHash = (Hashtable)b.Deserialize (s);
			s.Close();
			return myHash;
			//return (Hashtable)b.Deserialize (s);
		} catch (Exception e) {
			Console.WriteLine("{0}\n", e.Message);
			return null;
		}
	}

	public void Save()
	{
		if (players==null) return;

		try {
			Stream s = File.Open(playerFile, FileMode.Create, FileAccess.ReadWrite);
			BinaryFormatter b = new BinaryFormatter();
			b.Serialize(s, players);
			s.Close();
		} catch (Exception e) {
			Console.WriteLine("{0}\n", e.Message);
		}     
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

		List<string> playerLevelsArray = loadFromFile (playerLevelFile);

		if (playerLevelsArray.Count == 0)
			return null; // the file read was unsuccessful

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

	private bool loadConfigData() {

		bool returnVal = true; // assume the file will be found

		playerFile = "";
		playerLevelFile = "";
		questionFile = "";
		List<String> configArray = loadFromFile (CONFIG_FILE);
		if (configArray.Count == 0)
						return false; // the file read was unsuccessful
		
		string[] entries;
		foreach (string line in configArray) {
			if (!(line.Substring(0,2).Equals("//"))) {
				entries = line.Split(',');
				if (entries[0].Equals("PLAYER_FILE")) {
					playerFile=entries[1];
				} else {
					if (entries[0].Equals("PLAYER_LEVEL_FILE")) {
						playerLevelFile=entries[1];
					} else {
						if (entries[0].Equals("QUESTION_FILE")) {
							questionFile=entries[1];
						} else {
							if (entries[0].Equals("PASS_THRESHOLD")) {
								passThreshold=float.Parse(entries[1]);
								passThreshold=passThreshold/100.0f; // to make it a percentage
							}
						}
					}
				}
			}
		}
		return returnVal;
	}

	private bool loadQuestions() {
		bool returnVal = true; // assume the file will be found

		Question myQuestion = null;
		questions = new List<Question>();
		//List<string> questionsArray = loadFromFile ("questionsNew1.txt");
		List<string> questionsArray = loadFromFile (questionFile);
		if (questionsArray.Count == 0)
			return false; // the file read was unsuccessful

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
		return returnVal;
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
			return fileLines; //will be have a count of zero if no questions
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
