// =============================================================================
// MIT License
// 
// Copyright (c) [2018] [Valeriya Pudova] https://github.com/hww
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
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace VARP.OSC
{
	[RequireComponent(typeof(RawImage))]
	public class OscGrid : MonoBehaviour
	{
		public Color bgColor;
		public Color gridColor;
		private OscSettings oscSettings;
		private RawImage screenImage;			//< The image to draw oscilloscope
		private Texture2D screenTexture; 		//< The texture used for screenImage
		private Color[] clearColors; 			//< The colors to clear screenTexture
		private bool redraw;
		
		// ============================================================================================
		// Initialization
		// ============================================================================================

		/// <summary>Use this for initialization</summary>
		/// <param name="divisionsX">Divisions of X axis</para>
		/// <param name="diviaionsY">Divisions of Y axis</para>
		/// <param name="subdivisions">Subdivisions</para>
		public void Initialize(OscSettings oscSettings)
		{
			this.oscSettings = oscSettings;
			screenImage = GetComponent<RawImage>();
			drawGrid = oscSettings.drawGrid;
			drawRulerX = oscSettings.drawRulerX;
			drawRulerY = oscSettings.drawRulerY;
			redraw = true;
		}

		void Update()
		{
			if (redraw)
			{
				redraw = false;
				Clear(bgColor);
				PlotGrid(gridColor);
				Apply();
			}
		}

		private bool drawGrid;
		public bool DrawGrid
		{
			get { return drawGrid; }
			set { drawGrid = value; redraw = true; }
		}
		
		private bool drawRulerX;
		public bool DrawRulerX
		{
			get { return drawRulerX; }
			set { drawRulerX = value; redraw = true; }
		}
		
		private bool drawRulerY;
		public bool DrawRulerY
		{
			get { return drawRulerY; }
			set { drawRulerY = value; redraw = true; }
		}
		
		// ============================================================================================
		// Clear screen
		// ============================================================================================

		/// <summary>Clear screen with color</summary>
		/// <param name="color">Grid color</para>
		public void Clear(Color color)
		{
			var w = (int) oscSettings.textureSize.x;
			var h = (int) oscSettings.textureSize.y;
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

		// ============================================================================================
		// Display texture
		// ============================================================================================

		/// <summary>Apply texture to screen image</summary>
		public void Apply()
		{
			screenTexture.Apply();
			screenImage.texture = screenTexture;
			screenImage.color = Color.white;
		}

		// ============================================================================================
		// Draw grid
		// ============================================================================================

		/// <summary>
		/// Draw oscilloscope grid
		/// </summary>
		/// <param name="color"></param>
		public void PlotGrid(Color color)
		{
			var w = oscSettings.textureSize.x;
			var h = oscSettings.textureSize.y;
			var xcenter = oscSettings.textureCenter.x;
			var ycenter = oscSettings.textureCenter.y;
			var pxDiv = oscSettings.pixelsPerDivision;
			var pxSub = oscSettings.pixelsPerSubdivision;

			if (drawGrid)
			{
				// -- draw main grid 
				for (var x = xcenter; x<w; x+=pxDiv)
					PlotDotedLineVertical(screenTexture, x, ycenter, pxSub, color);
				for (var x = xcenter - pxDiv; x>=0; x-=pxDiv)
					PlotDotedLineVertical(screenTexture, x, ycenter, pxSub, color);
				for (var y = ycenter; y<h; y+=pxDiv)
					PlotDotedLineHorizontal(screenTexture, xcenter, y, pxSub, color);
				for (var y = ycenter - pxDiv; y>=0; y-=pxDiv)
					PlotDotedLineHorizontal(screenTexture, xcenter, y, pxSub, color);
			}

			if (drawRulerX)
			{
				// -- draw horizontal ruler bar in center
				PlotDotedLineHorizontal(screenTexture, xcenter, ycenter + 2, pxSub, color);
				PlotDotedLineHorizontal(screenTexture, xcenter, ycenter + 1, pxSub, color);
				PlotDotedLineHorizontal(screenTexture, xcenter, ycenter - 1, pxSub, color);
				PlotDotedLineHorizontal(screenTexture, xcenter, ycenter - 2, pxSub, color);
			}

			if (drawRulerY)
			{
				// -- draw verticals ruler bar in center
				PlotDotedLineVertical(screenTexture, xcenter + 2, ycenter, pxSub, color);
				PlotDotedLineVertical(screenTexture, xcenter + 1, ycenter, pxSub, color);
				PlotDotedLineVertical(screenTexture, xcenter - 1, ycenter, pxSub, color);
				PlotDotedLineVertical(screenTexture, xcenter - 2, ycenter, pxSub, color);
			}

			// -- draw frame around
			OscUtils.PlotRectangle(screenTexture, 0, 0, w - 1, h - 1, color);
		}
		
		// ============================================================================================
		// Draw dotted lines
		// ============================================================================================
		
		/// <summary>
		/// Draw horizontally multiple dots as the ruler's divisions. Arguments are in pixel Units.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="x">Line position</param>
		/// <param name="y">Line position</param>
		/// <param name="step">Step between divisions</param>
		public static void PlotDotedLineHorizontal(Texture2D texture, int x, int y, int step, Color color)
		{
			var w = texture.width;
			var ix = x;
			while (ix < w)
			{
				texture.SetPixel(ix, y, color);
				ix += step;
			}

			ix = x;
			while (ix >= 0)
			{
				texture.SetPixel(ix, y, color);
				ix -= step;
			}
		}

		/// <summary>
		/// Draw vertically multiple dots as the ruler's divisions 
		/// </summary>
		/// <param name="color"></param>
		/// <param name="x">Line position</param>
		/// <param name="y">Line position</param>
		/// <param name="step">Step between divisions</param>
		public static void PlotDotedLineVertical(Texture2D texture, int x, int y, int step, Color color)
		{
			var h = texture.height;
			var iy = y;
			while (iy < h)
			{
				texture.SetPixel(x, iy, color);
				iy += step;
			}
			iy = y;
			while (iy >= 0)
			{
				texture.SetPixel(x, iy, color);
				iy -= step;
			}
		}
	}
}
