using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OscLed : MonoBehaviour
{
	public Image image;
	public Text text;
	public Color colorOff;
	public Color colorOn;
	
	private bool state;
	
	void Start ()
	{
		State = state;
	}
	
	public bool State
	{
		get { return state; }
		set { 
			state = value;
			image.color = state ? colorOn : colorOff;
		}
	}

	public string message
	{
		set { if (text!=null) text.text = value; }
	}
}
