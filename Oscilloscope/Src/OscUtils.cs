using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VARP.OSC
{
	public static class OscUtils
	{

		// ============================================================================================
		// Draw Line on screen
		// ============================================================================================

		/// <summary>
		/// Draw line on the texture. Argumens are in pixel Units.
		/// 
		/// Bresenham's line algorithm is an algorithm that determines the points of an n-dimensional
		/// raster that should be selected in order to form a close approximation to a straight line
		/// between two points. It is commonly used to draw line primitives in a bitmap image
		/// (e.g. on a computer screen), as it uses only integer addition, subtraction and bit shifting,
		/// all of which are very cheap operations in standard computer architectures. It is an
		/// incremental error algorithm. It is one of the earliest algorithms developed in the field
		/// of computer graphics. An extension to the original algorithm may be used for drawing circles.
		/// </summary>
		/// <param name="texture">The texture</param>
		/// <param name="x1">Source position X</param>
		/// <param name="y1">Source position Y</param>
		/// <param name="x2">Target position X</param>
		/// <param name="y2">Targte position Y</param>
		/// <param name="color">Color of line</param>
		public static void PlotLine(Texture2D texture, int x0, int y0, int x1, int y1, Color color)
		{
			if (System.Math.Abs(y1 - y0) < System.Math.Abs(x1 - x0))
			{
				if (x0 > x1)
					PlotLineLow(texture, x1, y1, x0, y0, color);
				else
					PlotLineLow(texture, x0, y0, x1, y1, color);
			}
			else
			{
				if (y0 > y1)
					PlotLineHigh(texture, x1, y1, x0, y0, color);
				else
					PlotLineHigh(texture, x0, y0, x1, y1, color);
			}
		}
		
		private static void PlotLineLow(Texture2D texture, int x0, int y0, int x1, int y1, Color color)
		{
			var dx = x1 - x0;
			var dy = y1 - y0;
			var yi = 1;
			if (dy < 0)
			{
				yi = -1;
				dy = -dy;
			}

			var D = 2 * dy - dx;
			var y = y0;

			for (var x = x0; x <= x1; x++)
			{
				texture.SetPixel(x, y, color);
				if (D > 0)
				{
					y = y + yi;
					D = D - 2 * dx;
				}
				D = D + 2 * dy;
			}
		}

		private static void PlotLineHigh(Texture2D texture, int x0, int y0, int x1, int y1, Color color)
		{
			var dx = x1 - x0;
			var dy = y1 - y0;
			var xi = 1;
			if (dx < 0)
			{
				xi = -1;
				dx = -dx;
			}

			var D = 2 * dx - dy;
			var x = x0;

			for (var y = y0; y <= y1; y++)
			{
				texture.SetPixel(x, y, color);
				if (D > 0)
				{
					x = x + xi;
					D = D - 2 * dy;
				}

				D = D + 2 * dx;
			}
		}

		/// <summary>
		/// Draw rectangle. Argumens are in pixel Units.
		/// </summary>
		/// <param name="texture">Target texture</param>
		/// <param name="x1">Source position X</param>
		/// <param name="y1">Source position Y</param>
		/// <param name="x2">Target position X</param>
		/// <param name="y2">Targte position Y</param>
		/// <param name="color">Color of line</param>
		public static void PlotRectangle(Texture2D texture, int x1, int y1, int x2, int y2, Color color)
		{
			PlotLine(texture, x1, y1, x1, y2, color); // left 
			PlotLine(texture, x1, y1, x2, y1, color); // bottom
			PlotLine(texture, x2, y2, x1, y2, color); // top
			PlotLine(texture, x2, y2, x2, y1, color); // right
		}


	}

	public class OscFormatter
	{
		private static readonly string[] timePerDivisionFormats = new string[]
		{
			"{0:0.0}h/",
			"{0:0.0}m/",
			"{0:0.0}s/",
			"{0:0.0}ms/",
			"{0:0.0}us/",
			"{0:0.0}ns/",
			"{0:0.0}ps/",
		};
		private static readonly string[] timeFormats = new string[]
		{
			"{0:0.0}h",
			"{0:0.0}m",
			"{0:0.0}s",
			"{0:0.0}ms",
			"{0:0.0}us",
			"{0:0.0}ns",
			"{0:0.0}ps",
		};
		
		private static readonly string[] valuePerDivisionFormats = new string[]
		{
			"{0:0.0}MV/",
			"{0:0.0}kV/",
			"{0:0.0}V/",
			"{0:0.0}mV/",
			"{0:0.0}uV/",
			"{0:0.0}nV/",
			"{0:0.0}pV/",
		};
		private static readonly string[] valueFormats = new string[]
		{
			"{0:0.0}MV",
			"{0:0.0}kV",
			"{0:0.0}V",
			"{0:0.0}mV",
			"{0:0.0}uV",
			"{0:0.0}nV",
			"{0:0.0}pV",
		};

		
		private const float Minute = 60f;
		private const float Hour = 60f * 60f;
		
		public static string FormatTimePerDiv(float time)
		{
			var abs = Mathf.Abs(time);
			if (abs >= Hour)
				return string.Format(timePerDivisionFormats[0], time / Hour);
			if (abs >= Minute)
				return string.Format(timePerDivisionFormats[1], time / Hour);
			if (abs >= 1f)
				return string.Format(timePerDivisionFormats[2], time);
			if (abs >= 0.001f)
				return string.Format(timePerDivisionFormats[3], time * 1000f);
			if (abs >= 0.000001)
				return string.Format(timePerDivisionFormats[4], time * 1000000f);
			if (abs >= 0.000000001)
				return string.Format(timePerDivisionFormats[5], time * 1000000000f);
			return time.ToString();
		}
		
		public static string FormatTime(float time)
		{
			var abs = Mathf.Abs(time);
			if (abs >= Hour)
				return string.Format(timeFormats[0], time / Hour);
			if (abs >= Minute)
				return string.Format(timeFormats[1], time / Hour);
			if (abs >= 1f)
				return string.Format(timeFormats[2], time);
			if (abs >= 0.001f)
				return string.Format(timeFormats[3], time * 1000f);
			if (abs >= 0.000001)
				return string.Format(timeFormats[4], time * 1000000f);
			if (abs >= 0.000000001)
				return string.Format(timeFormats[5], time * 1000000000f);
			return time.ToString();
		}

		public static string FormatValuePerDiv(float value)
		{
			var abs = Mathf.Abs(value);
			if (abs >= 1000000f)
				return string.Format(valuePerDivisionFormats[0], value / 1000000f);
			if (abs >= 1000f)
				return string.Format(valuePerDivisionFormats[1], value / 1000f);
			if (abs >= 1f)
				return string.Format(valuePerDivisionFormats[2], value);
			if (abs >= 0.001f)
				return string.Format(valuePerDivisionFormats[3], value * 1000f);
			if (abs >= 0.000001f)
				return string.Format(valuePerDivisionFormats[4], value * 1000000f);
			if (abs >= 0.000000001f)
				return string.Format(valuePerDivisionFormats[5], value * 1000000000f);
			return value.ToString();
		}
		
		
		public static string FormatValue(float value)
		{
			var abs = Mathf.Abs(value);
			if (abs >= 1000000f)
				return string.Format(valueFormats[0], value / 1000000f);
			if (abs >= 1000f)
				return string.Format(valueFormats[1], value / 1000f);
			if (abs >= 1f)
				return string.Format(valueFormats[2], value);
			if (abs >= 0.001f)
				return string.Format(valueFormats[3], value * 1000f);
			if (abs >= 0.000001f)
				return string.Format(valueFormats[4], value * 1000000f);
			if (abs >= 0.000000001f)
				return string.Format(valueFormats[5], value * 1000000000f);
			return value.ToString();
		}
	}
	
	
	/// <summary>
	/// The gain or time control class. Round any value to the human
	/// friendly format aka: ..., 0.1, 0.2, 0.5, 1, 2, 5, 10, 20, ...
	/// </summary>
	public struct OscValue
	{
		private readonly float[] availableGains;
		
		
		/// <summary>Counstructor</summary>
		/// <param name="minGain">Minimum ratio: 0.1, 0.01, ..., 0.000001</param>
		/// <param name="maxGain">Maximum ratio: 1000, 10000, ..., 100000</param>
		/// <param name="gains">The requested row: [1, 2, 5] or [1, 2.5, 5]</param>
		public OscValue(float[] gains, float[] availableFactors)
		{
			availableGains = new float[gains.Length * availableFactors.Length]; 
			for (var i = 0; i<availableFactors.Length; i++)
			{
				var factor = availableFactors[i];
				for (var j=0; j<gains.Length;j++)
					availableGains[i * gains.Length + j] = factor * gains[j];
			}
		}
		
		/// <summary>
		/// I do not want to have strange numbers in autogain feature.
		/// The strange like 0.21245 etc. It is hard read for human.
		/// That is why this function can find nearest gain value.
		/// </summary>
		/// <param name="value">Requested value</param>
		/// <returns>Result value (V.DIVISION or SEC.DIVISIONS)</returns>
		public float GetValue(float value)
		{
			var bestIndex = GetValueIndex(value);
			return availableGains[bestIndex];
		}

		/// <summary>
		/// Find best gain for given peak to peak amplitude,
		/// </summary>
		/// <param name="peak2Peak">Peak to peak value</param>
		/// <param name="autoDivisions">Requested amount of grid divisions</param>
		/// <returns>Result gain</returns>
		public float GetAutoGain(float peak2Peak, float autoDivisions)
		{
			for (var i = availableGains.Length-1; i >=0 ; i--)
			{
				var scaled = peak2Peak * (1f / availableGains[i]);
				if (scaled <= autoDivisions)
					return availableGains[i];
			}
			return availableGains[0];
		}
		
		/// <summary>
		/// I do not want to have strange numbers in autogain feature.
		/// The strange like 0.21245 etc. It is hard read for human.
		/// That is why this function can find nearest gain value.
		/// </summary>
		/// <param name="value">Requested value</param>
		/// <returns>Result value's index</returns>
		public int GetValueIndex(float value)
		{
			var bestIndex= -1;
			var bestDifference = float.MaxValue;

			for (var i = availableGains.Length-1; i >= 0; i--)
			{
				var gain = availableGains[i];
				var diff = Mathf.Abs(value - gain);
				if (diff < bestDifference)
				{
					bestDifference = diff;
					bestIndex = i;
				}
			}
			return bestIndex;
		}

		/// <summary>
		/// Get value by index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public float GetValueByIndex(int index)
		{
			if (index < 0) index = 0;
			if (index >= availableGains.Length) index = availableGains.Length-1;
			return availableGains[index];
		}
		
		/// <summary>Gain (values per division) object</summary>
		public static OscValue Gain = new OscValue(new float[] {5f,2.0f,1.0f}, new float[] {1000f,100f,10f,1f,0.1f,0.01f,0.001f,0.0001f,0.00001f,0.000001f});
		/// <summary>Time (seconds per division) value</summary>
		public static OscValue Time = new OscValue(new float[] {5f,2.5f,1.0f}, new float[] {10f,1f,0.1f,0.01f,0.001f,0.0001f,0.00001f,0.000001f});
	}
}