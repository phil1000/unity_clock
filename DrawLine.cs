using Vectrosity;
using UnityEngine;
using System.Collections;

public class DrawLine : MonoBehaviour {

	// Use this for initialization
	void Start () {
		VectorLine.SetLine (Color.green, new Vector2(0, 0), new Vector2(Screen.width-1, Screen.height-1));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
