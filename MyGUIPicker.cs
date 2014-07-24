using UnityEngine;
using System.Collections;


public class MyGUIPicker : MonoBehaviour {
	
	public Rect positionAndSize = new Rect(10f,150f,300f, 200f);
	
	public bool vertical = true;
	public GUISkin guiSkin;
	public Texture2D uiPickerBar;
	public Texture2D uiPickerBorderLeft;
	public Texture2D uiPickerTopAndBottom;
	public Texture2D uiPickerBackground;
	public Texture2D uiPickerDelimiter;	
	
	public bool shouldCentralBarScale = false;
	public bool displayDelimiter = true;
	// Use this for initialization
	void Start () {
		//pickerParams[0] = new GUIPickerView.PickerParams();
	}
	
	
	//For future use only.
	void DrawIntoRectHorizontal(Rect area, string[] column, int columnIndex, float smoothFactor, GUISkin guiSkin,Texture2D[] icons){
		
		float centerPosition = positionAndSize.x + positionAndSize.width/2;
		float espacement = positionAndSize.width/5; //textOffset.height;
		for (int i=0; i<column.Length; i++){
			float currentPosition = centerPosition + (i-columnIndex) * espacement; //textOffset.height;  
			currentPosition += smoothFactor; 
			
			float x = ((currentPosition - (positionAndSize.width/2+positionAndSize.x)))/(positionAndSize.width);
			
			float transparency = 0f;
			
			if ( Mathf.Abs(x)>1|| currentPosition < positionAndSize.x|| currentPosition > positionAndSize.x+positionAndSize.width){
				transparency = 0f;
			} else {
				transparency = 1f-2f*Mathf.Abs(x);  //= Mathf.Lerp(1f, 0f, x);
				Color oldColor = GUI.color;
				GUI.color = new Color(oldColor.r, oldColor.g,oldColor.b, transparency);
				if (guiSkin==null){
					GUI.Label(new Rect ( currentPosition , area.y , 0, 0 ),column[i]); //Should be an icon
				} else {
					GUISkin oldSkin = GUI.skin;
					GUI.skin = guiSkin;
					GUI.Label(new Rect ( currentPosition, area.y , 0, 0 ),column[i]); //Should be an icon
					GUI.skin = oldSkin;
				}
				GUI.color = oldColor;
			}
		}
	}
	
	
	//Set the GUISkin to change the offset of the label.
	void DrawIntoRectRotatedVertical(Rect area, string[] column, int columnIndex, float smoothFactor, GUISkin guiSkin, Texture2D[] icons){
		
		float centerPosition = positionAndSize.x + positionAndSize.height/2;
		float espacement = positionAndSize.height/5; //textOffset.height;
		for (int i=0; i<column.Length; i++){
			float currentPosition = centerPosition + (i-columnIndex) * espacement; //textOffset.height;  
			currentPosition += smoothFactor; 
			
			float x = ((currentPosition - (positionAndSize.height/2+positionAndSize.x)))/(positionAndSize.height);
			
			float transparency = 0f;
			
			if ( Mathf.Abs(x)>1|| currentPosition < positionAndSize.x|| currentPosition > positionAndSize.x+positionAndSize.height){
				transparency = 0f;
			} else {
				transparency = 1f-2f*Mathf.Abs(x);  //= Mathf.Lerp(1f, 0f, x);
				Color oldColor = GUI.color;
				GUI.color = new Color(oldColor.r, oldColor.g,oldColor.b, transparency);
				if (guiSkin==null){
					GUI.Label(new Rect ( currentPosition , area.y , 0, 0 ),column[i]); 
				} else {
					GUISkin oldSkin = GUI.skin;
					GUI.skin = guiSkin;
					GUI.Label(new Rect ( currentPosition, area.y , 0, 0 ),column[i]); 
					if (icons != null && icons.Length >i && icons[i]!= null){
						GUI.Box(new Rect ( currentPosition  , area.y , icons[i].width, icons[i].height),icons[i]);
						GUI.Label(new Rect (currentPosition , area.y, 0, 0 ),column[i], guiSkin.label);
					} else {
						GUI.Label(new Rect ( currentPosition , area.y , 0, 0 ),column[i], guiSkin.label); 
					}
					GUI.skin = oldSkin;
				}
				GUI.color = oldColor;
			}
		}
	}	
	
	
	void DrawIntoRectVertical(Rect area, string[] column, int columnIndex, float smoothFactor, GUISkin guiSkin, Texture2D[] icons){
		
		float centerPosition = positionAndSize.y + positionAndSize.height/2;
		float espacement = positionAndSize.height/5; 
		for (int i=0; i<column.Length; i++){
			float currentPosition = centerPosition + (i-columnIndex) * espacement; 
			currentPosition += smoothFactor; 
			
			float x = ((currentPosition - (positionAndSize.height/2+positionAndSize.y)))/(positionAndSize.height);
			
			float transparency = 0f;
			
			if ( Mathf.Abs(x)>1|| currentPosition < positionAndSize.y|| currentPosition > positionAndSize.y+positionAndSize.height){
				transparency = 0f;
			} else {
				transparency = 1f-2f*Mathf.Abs(x);  //= Mathf.Lerp(1f, 0f, x);
				Color oldColor = GUI.color;
				GUI.color = new Color(oldColor.r, oldColor.g,oldColor.b, transparency);
				if (guiSkin==null){
					GUI.Label(new Rect ( area.x , currentPosition , 0, 0 ),column[i]); //Should be an icon
				} else {
					GUISkin oldSkin = GUI.skin;
					GUI.skin = guiSkin;
					if (icons != null && icons.Length >i && icons[i]!= null){
						GUI.Box(new Rect ( area.x , currentPosition , icons[i].width, icons[i].height),icons[i]);
						GUI.Label(new Rect ( area.x , currentPosition , 0, 0 ),column[i], guiSkin.label);
					} else {
						GUI.Label(new Rect ( area.x , currentPosition , 0, 0 ),column[i], guiSkin.label); //Should be an icon
					}
					GUI.skin = oldSkin;
				}
				GUI.color = oldColor;
			}
		}
	}
	
	void OnGUI(){
		if (guiSkin != null){
			GUI.skin = guiSkin;
		}
		
		Vector2 rotationCenter = new Vector2(positionAndSize.x+positionAndSize.width/2, positionAndSize.y+positionAndSize.width/2);
		float rotationAngle = 90f;
		if (!vertical)
			GUIUtility.RotateAroundPivot (-rotationAngle, rotationCenter); 
		
		GUI.DrawTexture(new Rect (positionAndSize.x,positionAndSize.y,positionAndSize.width,positionAndSize.height), uiPickerBackground,ScaleMode.StretchToFill);
		
		if (!vertical)
			GUIUtility.RotateAroundPivot (rotationAngle, rotationCenter); 
		
		
		if (!vertical){
			for (int i=0; i<this.pickerParams.Length; i++){
				DrawIntoRectRotatedVertical(this.touchAreas[i], this.pickerParams[i].column, this.pickerParams[i].initialIndex, this.pickerParams[i].smoothFactor, this.pickerParams[i].guiSkin, this.pickerParams[i].icons);
			}		
		} else {
			for (int i=0; i<this.pickerParams.Length; i++){
				DrawIntoRectVertical(this.touchAreas[i], this.pickerParams[i].column, this.pickerParams[i].initialIndex, this.pickerParams[i].smoothFactor, this.pickerParams[i].guiSkin, this.pickerParams[i].icons);
			}
		}
		
		
		if (!vertical)
			GUIUtility.RotateAroundPivot (-rotationAngle,rotationCenter); 
		
		//Display Delimiter
		if (displayDelimiter == true){
			float incPercent = 0;
			for (int i=0; i<pickerParams.Length-1; i++){
				incPercent += pickerParams[i].widthPercent/100f;
				GUI.DrawTexture(new Rect (positionAndSize.x + positionAndSize.width*incPercent, positionAndSize.y, 8, positionAndSize.height), 
				                uiPickerDelimiter,ScaleMode.StretchToFill);
			}
		}
		
		
		GUI.DrawTexture(new Rect (positionAndSize.x,positionAndSize.y,positionAndSize.width,positionAndSize.height), 
		                uiPickerTopAndBottom,ScaleMode.StretchToFill);
		GUI.DrawTexture(new Rect (positionAndSize.x,positionAndSize.y,uiPickerBorderLeft.width,positionAndSize.height), 
		                uiPickerBorderLeft,ScaleMode.StretchToFill);
		GUI.DrawTexture(new Rect (positionAndSize.x+positionAndSize.width,positionAndSize.y,-uiPickerBorderLeft.width,positionAndSize.height), 
		                uiPickerBorderLeft,ScaleMode.StretchToFill);
		
		if (shouldCentralBarScale){
			GUI.DrawTexture(new Rect (
				positionAndSize.x+uiPickerBorderLeft.width-4,
				(positionAndSize.height/2+positionAndSize.y)-(uiPickerBar.height-16)*positionAndSize.height/200/2,
				positionAndSize.width-2*uiPickerBorderLeft.width+8,
				uiPickerBar.height*positionAndSize.height/200
				),uiPickerBar,ScaleMode.StretchToFill);
		} else {
			GUI.DrawTexture(new Rect (
				positionAndSize.x+uiPickerBorderLeft.width-4,
				(positionAndSize.height/2+positionAndSize.y)-(uiPickerBar.height-16)/2,
				positionAndSize.width-2*uiPickerBorderLeft.width+8,
				uiPickerBar.height
				),uiPickerBar,ScaleMode.StretchToFill);
		}
		if (!vertical)
			GUIUtility.RotateAroundPivot (rotationAngle,rotationCenter); 
		
		if (debugMode){
			for (int i=0;i<touchAreas.Length; i++){
				Color oldColor2 = GUI.color;
				GUI.color = new Color(1.0f , 1f/(i+1), 0, 0.2f);//Set color
				GUI.DrawTexture(new Rect(touchAreas[i].x, touchAreas[i].y, touchAreas[i].width, touchAreas[i].height),uiPickerBackground);
				GUI.color = oldColor2;//Reset color 
			}
		}
	}
	//*/
	public bool debugMode = false;
	private Rect[] touchAreas = new Rect[2];
	private Vector2 lastMousePosition = Vector2.zero;
	private Vector2 deltaMousePosition = Vector2.zero;
	private bool mouseDown = false;
	public PickerParams[] pickerParams ; //Max of 5 pickers
	
	
	[System.Serializable]
	public class PickerParams {
		
		public GUISkin guiSkin;
		public int widthPercent = 0;
		public int initialIndex = 2;
		[HideInInspector]
		public float smoothFactor = 0f;
		[HideInInspector]
		public float deltaSmooth = 0f;
		[HideInInspector]
		public int indexChange = 0;
		[HideInInspector]
		public float timer = 0f;
		public string[] column;
		public Texture2D[] icons;
		public int selectedIndexReadOnly = 0;
	}
	
	
	void ManageTouchAreas(){
		int total = 0;
		for (int i=0; i<this.pickerParams.Length; i++){
			total += this.pickerParams[i].widthPercent;
		}
		if (total > 100){
			Debug.LogError("The sum of the width/height percent of your pickers exceeds 100 %");
		}
		this.touchAreas = new Rect[this.pickerParams.Length];
		float incPerc= 0;
		for (int i=0; i<pickerParams.Length; i++){
			float percent = this.pickerParams[i].widthPercent/100f;
			if (vertical){
				this.touchAreas[i] = new Rect(positionAndSize.x+positionAndSize.width*incPerc,
				                              positionAndSize.y,
				                              positionAndSize.width*percent,
				                              positionAndSize.height);
				
			}
			else {
				
				//This is for rotated touch only...
				this.touchAreas[i] = new Rect(positionAndSize.x,
				                              positionAndSize.y+positionAndSize.width*incPerc,
				                              positionAndSize.height,
				                              positionAndSize.width*percent);
			}
			//*/
			incPerc += percent;
		}
	}
	
	// Update is called once per frame
	void Update () {
		float ratio;
		if (vertical){
			ratio = 5/positionAndSize.height;
		} else {
			ratio = 5/positionAndSize.height; //This for rotated only... will keep this branch for future use.
		}
		this.ManageTouchAreas();
		
		for (int i=0; i<pickerParams.Length; i++){
			
			this.pickerParams[i].indexChange = Mathf.RoundToInt(this.pickerParams[i].deltaSmooth*ratio);
			this.pickerParams[i].selectedIndexReadOnly = this.pickerParams[i].initialIndex - this.pickerParams[i].indexChange;
			
		}
		if (Input.GetMouseButtonDown(0)){
			this.lastMousePosition = Input.mousePosition;
			this.mouseDown = true;
		}
		if (Input.GetMouseButtonUp(0)){ //Or not contained.
			this.mouseDown = false;
			for (int i=0;i<pickerParams.Length; i++){
				if (this.touchAreas[i].Contains(new Vector2(Input.mousePosition.x,Screen.height-Input.mousePosition.y))){
					this.pickerParams[i].timer = 0;
					break;
				}
			}
		}
		
		if (this.mouseDown == true) {
			this.deltaMousePosition = new Vector2(Input.mousePosition.x-this.lastMousePosition.x,
			                                      Input.mousePosition.y-this.lastMousePosition.y);
			this.lastMousePosition = Input.mousePosition;
			
			for (int i=0;i<this.touchAreas.Length; i++){
				if (this.touchAreas[i].Contains(new Vector2(Input.mousePosition.x,Screen.height-Input.mousePosition.y))){
					int smooth = 0;
					if (vertical){
						smooth = Mathf.RoundToInt((this.pickerParams[i].smoothFactor - (int)this.deltaMousePosition.y)*ratio);
					} else {
						smooth = Mathf.RoundToInt((this.pickerParams[i].smoothFactor + (int)this.deltaMousePosition.x)*ratio);
						
					}
					if (this.pickerParams[i].initialIndex - smooth < this.pickerParams[i].column.Length  && this.pickerParams[i].initialIndex - smooth >= 0){
						if (vertical){
							this.pickerParams[i].smoothFactor -= (int)this.deltaMousePosition.y;
						} else {
							this.pickerParams[i].smoothFactor += (int)this.deltaMousePosition.x;
							//Debug.Log(this.pickerParams[i].smoothFactor);
						}
						this.pickerParams[i].deltaSmooth = this.pickerParams[i].smoothFactor;
					}
				}
			}
			
		}
		
		if (this.mouseDown == false){
			for (int i=0; i<pickerParams.Length; i++){
				this.pickerParams[i].timer += 3f*Time.deltaTime;
				this.pickerParams[i].smoothFactor = Mathf.Lerp(this.pickerParams[i].deltaSmooth, (float)this.pickerParams[i].indexChange/ratio, this.pickerParams[i].timer); 
			}
		}
	}
}
