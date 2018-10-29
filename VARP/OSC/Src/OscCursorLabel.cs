// =============================================================================
// MIT License
// 
// Copyright (c) [2018] [Valeriya Pudova]
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// =============================================================================

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
