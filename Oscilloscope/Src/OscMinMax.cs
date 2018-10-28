using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VARP.OSC
{
	public struct OscMinMax
	{
		private bool isZero;
		private float min; // minimum
		private float max; // maximum
		private float p2p; // peak to peak					

		public float Min => isZero ? 0 : min;
		public float Max => isZero ? 0 : max;
		public float P2P => p2p;
		public float Mid => (Max - Min) / 2f;
		
		public void Reset()
		{
			isZero = true;
			min = float.MaxValue;
			max = float.MinValue;
			max = 0;
		}

		public bool CalculateMinMax(Vector3[] buffer, int smpStart, int smpEnd, OscProbe.Format format)
		{
			isZero = false;
			switch (format)
			{
				case OscProbe.Format.Float:
					return  CalculateMinMaxFloat(buffer, smpStart, smpEnd);
				case OscProbe.Format.Vector2:
					return  CalculateMinMaxVector2(buffer, smpStart, smpEnd);
				case OscProbe.Format.Vector3:
					return  CalculateMinMaxVector3(buffer, smpStart, smpEnd);
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}
		
		/// <summary>Calculate min and max value of the capturing samples</summary>
		/// <param name="buffer">Samples buffer</param>
		/// <param name="smpStart">Start from sample</param>
		/// <param name="smpEnd">End with samples</param>
		/// <param name="smpEnd">Result true if peak to peak is modifyed</param>
		private bool CalculateMinMaxFloat(Vector3[] buffer, int smpStart, int smpEnd)
		{
			for (var i = smpStart; i <= smpEnd; i++)
			{
				var sample = buffer[i & OscSettings.BUFFER_INDEX_MASK].x;
				if (sample < min) min = sample;
				if (sample > max) max = sample;
			}
			var ppOld = p2p;
			p2p = max - min;
			return p2p != ppOld;
		}
		
		/// <summary>Calculate min and max value of the capturing samples</summary>
		/// <param name="buffer">Samples buffer</param>
		/// <param name="smpStart">Start from sample</param>
		/// <param name="smpEnd">End with samples</param>
		/// <param name="smpEnd">Result true if peak to peak is modifyed</param>
		private bool CalculateMinMaxVector2(Vector3[] buffer, int smpStart, int smpEnd)
		{
			for (var i = smpStart; i <= smpEnd; i++)
			{
				var sample = buffer[i & OscSettings.BUFFER_INDEX_MASK].x;
				if (sample < min) min = sample;
				if (sample > max) max = sample;
				sample = buffer[i & OscSettings.BUFFER_INDEX_MASK].y;
				if (sample < min) min = sample;
				if (sample > max) max = sample;
			}
			var ppOld = p2p;
			p2p = max - min;
			return p2p != ppOld;
		}

		/// <summary>Calculate min and max value of the capturing samples</summary>
		/// <param name="buffer">Samples buffer</param>
		/// <param name="smpStart">Start from sample</param>
		/// <param name="smpEnd">End with samples</param>
		/// <param name="smpEnd">Result true if peak to peak is modifyed</param>
		private bool CalculateMinMaxVector3(Vector3[] buffer, int smpStart, int smpEnd)
		{
			for (var i = smpStart; i <= smpEnd; i++)
			{
				var sample = buffer[i & OscSettings.BUFFER_INDEX_MASK].x;
				if (sample < min) min = sample;
				if (sample > max) max = sample;
				sample = buffer[i & OscSettings.BUFFER_INDEX_MASK].y;
				if (sample < min) min = sample;
				if (sample > max) max = sample;
				sample = buffer[i & OscSettings.BUFFER_INDEX_MASK].z;
				if (sample < min) min = sample;
				if (sample > max) max = sample;
			}
			var ppOld = p2p;
			p2p = max - min;
			return p2p != ppOld;
		}
	}
}
