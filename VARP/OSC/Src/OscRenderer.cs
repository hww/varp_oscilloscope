// =============================================================================
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VARP.OSC
{
	[RequireComponent(typeof(RawImage))]
	public class OscRenderer : MonoBehaviour
	{
		public enum Mode
		{
			Default,							//< Default oscilloscope diagram
		}

		public Color colorX = Color.red;        //< Draw vector data with this colors
		public Color colorY = Color.green;      //< Draw vector data with this colors  
		public Color colorZ = Color.blue;       //< Draw vector data with this colors 
		
		public OscChannelLabel[] labels;		//< User programmable labels
		private RawImage screenImage;			//< Image to render oscilloscope diagram
		private Texture2D screenTexture;		//< The texture used for screenImage
		private Color[] clearColors;			//< The colors to clear screenTexture
		private OscSettings oscSettings;		//< Oscilloscope settings
		public bool heartBeat;					//< To make GUI elements blink
		
		/// <summary>Use this for initialization</summary>
		/// <param name="divisionsX">Divisions of X axis</para>
		/// <param name="diviaionsY">Divisions of Y axis</para>
		/// <param name="subdivisions">Subdivisions</para>
		public void Initialize(OscSettings oscSettings)
		{
			this.oscSettings = oscSettings;
			screenImage = GetComponent<RawImage>();
			screenImage.color = Color.white;	
		}

		/// <summary>Before reader</summary>
		public void OnBeforeRenderer()
		{
			heartBeat = 0 < (1 & (int) (Time.unscaledTime * 2));
		}
		
		// ============================================================================================
		// Clear screen
		// ============================================================================================

		/// <summary>Clear screen by vlack color</summary>
		public void Clear(Color color)
		{
			var w = oscSettings.textureSize.x;
			var h = oscSettings.textureSize.y;
			if (screenTexture == null)
			{
				CreateTexture(w, h, color);
			}
			else if (screenTexture.width != w || screenTexture.height != h)
			{
				Destroy(screenTexture);
				CreateTexture(w, h, color);
			}
			screenTexture.SetPixels(clearColors, 0);
		}
		
		// Create new texture and the buffer of colors to clear it
		private void CreateTexture(int w, int h, Color color)
		{
			clearColors = new Color[w * h];
			for (var i = 0; i < clearColors.Length; i++)
				clearColors[i] = color;
			screenTexture = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
			screenTexture.filterMode = FilterMode.Bilinear;
			screenTexture.wrapMode = TextureWrapMode.Clamp;
		}
		
		/// <summary>
		/// Clear horizontal block of texture
		/// </summary>
		/// <param name="x1">Start pixel</param>
		/// <param name="x2">End pixel</param>
		/// <param name="color">Color used to clear</param>
		public void ClearHorizFrame(int x1, int x2, Color color)
		{
			var h = screenTexture.height;
			for (var x = x1; x <= x2; x++)
			{
				for (var y = 0; y < h; y++)
					screenTexture.SetPixel(x, y, color);
			}
		}
		
		// ============================================================================================
		// Display texture
		// ============================================================================================
	
		/// <summary>Apply texture to screen image</summary>
		public void Apply()
		{
			screenTexture.Apply();
			screenImage.texture = screenTexture;

		}
		
		// ============================================================================================
		// Draw time diagram on screen
		// ============================================================================================
	
		/// <summary>
		/// Draw waveform. Argument's units are grid divisions (GD)
		/// </summary>
		/// <param name="channel">Channel</param>
		/// <param name="smpBeg">Index of first sample</param>
		/// <param name="smpEnd">Index of last sample</param>
		/// <param name="pxStart">Start pixel</param>
		/// <param name="pxPerSample">Pixels per sample</param>
		/// <param name="color">Waveform color</param>
		public void PlotFloat(
			OscChannel channel, 
			int smpBeg,
			int smpEnd,
			float pxStart,
			float pxPerSample,  
			Color color)
		{
			// do not render empty buffer
			var texCenterY = oscSettings.textureCenter.y;
			var pixPerDivs = oscSettings.pixelsPerDivision;
			// first sample position
			var fpx = pxStart;
			var x1 = Mathf.RoundToInt(pxStart-pxPerSample);
			var y1 = Mathf.RoundToInt (channel.GetFloat(smpBeg-1) * pixPerDivs + texCenterY);
			// iterate over all samples (starts from sample 2)
			for (var i=smpBeg; i<=smpEnd; i++)
			{
				var x2 = Mathf.RoundToInt(fpx);
				var y2 = Mathf.RoundToInt(channel.GetFloat(i) * pixPerDivs + texCenterY);
				// first line at the sample 2
				var absDx = Math.Abs(x1 - x2);
				var absDy = Math.Abs(y1 - y2);
				if (absDx > 1 || absDy > 1)
					OscUtils.PlotLine(screenTexture, x1, y1, x2, y2, color);
				else
					screenTexture.SetPixel(x2,y2,color);
				x1 = x2;
				y1 = y2;
				fpx += pxPerSample;
			}
		}
		
		/// <summary>
		/// Draw waveform. Argument's units are grid divisions (GD)
		/// </summary>
		/// <param name="channel">Channel</param>
		/// <param name="smpBeg">Index of first sample</param>
		/// <param name="smpEnd">Index of last sample</param>
		/// <param name="pxStart">Start pixel</param>
		/// <param name="pxPerSample">Pixels per sample</param>
		/// <param name="color">Waveform color</param>
		public void PlotVector2(
			OscChannel channel, 
			int smpBeg,
			int smpEnd,
			float pxStart,
			float pxPerSample, 
			Color color)
		{
			// do not render empty buffer
			var texCenterY = oscSettings.textureCenter.y;
			var pixPerDivs = oscSettings.pixelsPerDivision;
			// first sample position
			var fpx = pxStart;
			var x1 =  Mathf.RoundToInt(pxStart-pxPerSample);
			var sample = channel.GetVector2(smpBeg-1);
			var y1a = Mathf.RoundToInt (sample.x * pixPerDivs + texCenterY);
			var y1b = Mathf.RoundToInt (sample.y * pixPerDivs + texCenterY);
			// iterate over all samples (starts from sample 2)
			for (var i=smpBeg; i<=smpEnd; i++)
			{
				var x2 = Mathf.RoundToInt(fpx);
				sample = channel.GetVector2(i);
				var y2a = Mathf.RoundToInt(sample.x * pixPerDivs + texCenterY);
				var y2b = Mathf.RoundToInt(sample.y * pixPerDivs + texCenterY);
				// first line at the sample 2
				var dx = Math.Abs(x1 - x2);
				
				var dya = Math.Abs(y1a - y2a);
				if (dx > 1 || dya > 1)
					OscUtils.PlotLine(screenTexture, x1, y1a, x2, y2a, colorX);
				else
					screenTexture.SetPixel(x2,y2a,colorX);
				
				var dyb = Math.Abs(y1b - y2b);
				if (dx > 1 || dyb > 1)
					OscUtils.PlotLine(screenTexture, x1, y1b, x2, y2b, colorY);
				else
					screenTexture.SetPixel(x2,y2b,colorY);

				x1 = x2;
				y1a = y2a;
				y1b = y2b;
				fpx += pxPerSample;
			}
		}

		/// <summary>
		/// Draw waveform. Argument's units are grid divisions (GD)
		/// </summary>
		/// <param name="channel">Channel</param>
		/// <param name="smpBeg">Index of first sample</param>
		/// <param name="smpEnd">Index of last sample</param>
		/// <param name="pxStart">Start pixel</param>
		/// <param name="pxPerSample">Pixels per sample</param>
		/// <param name="color">Waveform color</param>
		public void PlotVector3(
			OscChannel channel, 
			int smpBeg,
			int smpEnd,
			float pxStart,
			float pxPerSample, 
			Color color)
		{
			// do not render empty buffer
			var texCenterY = oscSettings.textureCenter.y;
			var pixPerDivs = oscSettings.pixelsPerDivision;
			// first sample position
			var fpx = pxStart;
			var x1 =  Mathf.RoundToInt(pxStart-pxPerSample);
			var sample = channel.GetVector3(smpBeg-1);
			var y1a = Mathf.RoundToInt (sample.x * pixPerDivs + texCenterY);
			var y1b = Mathf.RoundToInt (sample.y * pixPerDivs + texCenterY);
			var y1c = Mathf.RoundToInt (sample.z * pixPerDivs + texCenterY);
			// iterate over all samples (starts from sample 2)
			for (var i=smpBeg; i<=smpEnd; i++)
			{
				var x2 = Mathf.RoundToInt(fpx);
				sample = channel.GetVector3(i);
				var y2a = Mathf.RoundToInt(sample.x * pixPerDivs + texCenterY);
				var y2b = Mathf.RoundToInt(sample.y * pixPerDivs + texCenterY);
				var y2c = Mathf.RoundToInt(sample.z * pixPerDivs + texCenterY);
				// first line at the sample 2
				var dx = Math.Abs(x1 - x2);
				
				var dya = Math.Abs(y1a - y2a);
				if (dx > 1 || dya > 1)
					OscUtils.PlotLine(screenTexture, x1, y1a, x2, y2a, colorX);
				else
					screenTexture.SetPixel(x2,y2a,colorX);
				
				var dyb = Math.Abs(y1b - y2b);
				if (dx > 1 || dyb > 1)
					OscUtils.PlotLine(screenTexture, x1, y1b, x2, y2b, colorY);
				else
					screenTexture.SetPixel(x2,y2b,colorY);

				var dyc = Math.Abs(y1c - y2c);
				if (dx > 1 || dyc > 1)
					OscUtils.PlotLine(screenTexture, x1, y1c, x2, y2c, colorZ);
				else
					screenTexture.SetPixel(x2,y2c,colorZ);
				
				x1 = x2;
				y1a = y2a;
				y1b = y2b;
				y1c = y2c;
				fpx += pxPerSample;
			}
		}		
		
		/// <summary>
		/// Draw waveform. Argument's units are grid divisions (GD)
		/// </summary>
		/// <param name="channel">Channel</param>
		/// <param name="smpBeg">Index of first sample</param>
		/// <param name="smpEnd">Index of last sample</param>
		/// <param name="pxStart">Start pixel</param>
		/// <param name="pxPerSample">Pixels per sample</param>
		/// <param name="color">Waveform color</param>
		public void PlotFloatLogic(
			OscChannel channel, 
			int smpBeg,
			int smpEnd,
			float pxStart,
			float pxPerSample,  
			Color color)
		{
			// do not render empty buffer
			const int CHAR_OFFSET_Y = -10;
			const int CHAR_OFFSET_X = -66;
			var position = channel.Position;
			var texCenterY = oscSettings.textureCenter.y;
			var pixPerDivs = oscSettings.pixelsPerDivision;
			var topY = Mathf.RoundToInt((position + channel.Scale) * pixPerDivs + texCenterY);
			var botY = Mathf.RoundToInt(position * pixPerDivs + texCenterY);
			// first sample position
			var fpx = pxStart;
			var x1 = Mathf.RoundToInt(pxStart-pxPerSample);
			var v1 = (int)(channel.GetFloat(smpBeg - 1) - position);
			// iterate over all samples (starts from sample 2)
			for (var i=smpBeg; i<=smpEnd; i++)
			{
				var x2 = Mathf.RoundToInt(fpx);
				var v2 = (int)(channel.GetFloat(i) - position);
				if (v1 == v2)
				{
					if (i != smpEnd)
					{
						v1 = v2;
						fpx += pxPerSample;
						continue; // skip	
					}
					else
					{
						// horizontal lines
						OscUtils.PlotLine(screenTexture, x1, topY, x2-1, topY, color);
						OscUtils.PlotLine(screenTexture, x1, botY, x2-1, botY, color);
						return;
					}
				}
				// horizontal lines
				OscUtils.PlotLine(screenTexture, x1, topY, x2-2, topY, color);
				OscUtils.PlotLine(screenTexture, x1, botY, x2-2, botY, color);
				// cross over lines
				OscUtils.PlotLine(screenTexture, x2-2, topY, x2, botY, color);
				OscUtils.PlotLine(screenTexture, x2-2, botY, x2, topY, color);
				// display value (previous value)
				OscFont.DrawText(screenTexture, v1.ToString("x8"), x2 + CHAR_OFFSET_X, botY + CHAR_OFFSET_Y, color);
				x1 = x2;
				v1 = v2;
				fpx += pxPerSample;
			}
		}
		
		/// <summary>
		/// Draw marker at the x,y position in the division units
		/// </summary>
		/// <param name="text"></param>
		/// <param name="color"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="scale">Vertical scale</param>
		/// <param name="position">Vertical position</param>
		/// <param name="clampPosition">If label does not fit on screen render it with blinking</param>
		public void DrawLabel(string text, float x, float y, Color color, bool clampPosition = false)
		{
			var pxX = oscSettings.GetPixelPositionIntX(x);
			var pxY = oscSettings.GetPixelPositionIntY(y);
			var isNotFit = !oscSettings.TestPixelInsideScreenY(pxY);
			if (isNotFit)
			{
				// if the label outside it modify name
				if (clampPosition)
				{
					pxY = oscSettings.ClampPixelInsideScreenY(pxY);
					text += '!'; // mark it clamped
				}
				else
				{
					return; // do not render label outside
				}
			}
			// Render little cross hor. and vert. lines.			
			OscUtils.PlotLine(screenTexture, pxX, pxY, pxX+16, pxY, color);
			OscUtils.PlotLine(screenTexture, pxX, pxY-8, pxX, pxY+8, color);
			// Render text (if text is not fit move it below the line)
			const int CHAR_HEIGHT = 8;
			const int CHAR_OFFSET = 2;
			if (pxY < oscSettings.textureSize.y - (CHAR_HEIGHT+CHAR_OFFSET))
				// fit on screen 
				OscFont.DrawText(screenTexture, text, pxX + 1, pxY + CHAR_OFFSET, color);
			else
				// does not fit on screen
				OscFont.DrawText(screenTexture, text, pxX + 1, pxY - (CHAR_HEIGHT+CHAR_OFFSET), color);
		}
		
		
		/// <summary>
		/// Draw horizontal line at position Y
		/// </summary>
		/// <param name="color"></param>
		/// <param name="y"></param>
		/// <param name="scale"></param>
		/// <param name="position"></param>
		public void DrawHorizontalLine(float y, Color color)
		{
			var pxY = oscSettings.GetPixelPositionIntY(y);
			OscUtils.PlotLine(screenTexture, 1, pxY, oscSettings.textureSize.x-1, pxY, color);
		}
	}	
}

