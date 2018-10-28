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
        public Vector2 gridPosition;

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
