﻿// =============================================================================
// MIT License
// 
// Copyright (c) 2018 Valeriya Pudova (hww.github.io)
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
    public class OscChannelLabel : MonoBehaviour
    {
        public RectTransform rect;
        public Text label;
        public Image image;
        public float position;
        public int frameCount; 
        
        private bool _visible;
        public bool visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
                label.enabled = value;
                image.enabled = value;
            }
        }

        private Color _color;
        public Color color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                image.color = label.color = _color;
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
