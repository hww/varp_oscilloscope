using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VARP.OSC
{
	public class OscCursorLabel : MonoBehaviour
	{
		public RectTransform rect;
		public Text label;
		public Image image;
		public RawImage line;
		public Vector2 gridPosition;
        
		public bool visible
		{
			get
			{
				return label.enabled;
			}
			set
			{
				label.enabled = value;
				image.enabled = value;
				line.enabled = value;
			}
		}
        
		public Color color
		{
			get
			{
				return label.color;
			}
			set
			{
				label.color = value;
				image.color = value;
				line.color = value;
			}
		}
        
		public string text
		{
			get
			{
				return label.text;
			}
			set
			{
				label.text = value;
			}
		}
        
		public Vector2 anchoredPosition
		{
			get
			{
				return rect.anchoredPosition;
			}
			set
			{
				rect.anchoredPosition = value;
			}
		}
	}
}
