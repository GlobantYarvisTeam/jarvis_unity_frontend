using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class TextSize : MonoBehaviour {

	public bool executeInEditMode = false;
	public float wantedWidth;
	private TextMesh textMesh;
	private Dictionary<char, float> dict;
	private string _lastText = "";
	

	public void Start()
	{
		textMesh = gameObject.GetComponent<TextMesh> ();
		dict = new Dictionary<char, float> ();
	}

	public void Update()
	{
		if (_lastText != textMesh.text) {
			FitText ();
			_lastText = textMesh.text;
		}
	}

	public void FitText()
	{
		FitToWidth (wantedWidth);
	}

	private void FitToWidth(float wantedWidth) {
		string oldText = textMesh.text;
		textMesh.text = "";
		
		string[] lines = oldText.Split('\n');
		
		foreach(string line in lines){
			textMesh.text += wrapLine(line, wantedWidth * 0.0000001f);
			textMesh.text += "\n";
		}
	}
	
	private string wrapLine(string s, float w)
	{
		// need to check if smaller than maximum character length, really...
		if(w == 0 || s.Length <= 0) return s;
		
		char c;
		char[] charList = s.ToCharArray();
		
		float charWidth = 0;
		float wordWidth = 0;
		float currentWidth = 0;
		
		string word = "";
		string newText = "";
		string oldText = textMesh.text;
		
		for (int i=0; i<charList.Length; i++){
			c = charList[i];
			
			if (dict.ContainsKey(c)){
				charWidth = (float)dict[c];
			} else {
				textMesh.text = ""+c;
				charWidth = gameObject.GetComponent<Renderer>().bounds.size.x;
				dict.Add(c, charWidth);
				//here check if max char length
			}
			
			if(c == ' ' || i == charList.Length - 1){
				if(c != ' '){
					word += c.ToString();
					wordWidth += charWidth;
				}
				
				if(currentWidth + wordWidth < w){
					currentWidth += wordWidth;
					newText += word;
				} else {
					currentWidth = wordWidth;
					newText += word.Replace(" ", "\n");
				}
				
				word = "";
				wordWidth = 0;
			} 
			
			word += c.ToString();
			wordWidth += charWidth;
		}
		
		textMesh.text = oldText;
		return newText;
	}
}
