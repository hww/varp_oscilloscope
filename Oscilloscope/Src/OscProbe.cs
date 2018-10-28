using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VARP.OSC
{
    /// <summary>
    /// The base class for all input. It is model of the oscilloscope's cable probe.
    /// Also it has default settings for oscilloscope's channel. And when we will
    /// plug input to the channel those settings will be applayed to the channel.
    /// </summary>
	public class OscProbe
	{
		public readonly string name;					//< Input's name will be displayed on screen
		
		/***********************************************
		 * Attenuator
		 ***********************************************/

		public float position = 0f;						//< Change vertical position of this diagram (divisions)
		public bool autoGain = false;					//< Audoset best gain for this probe
		public int autoGainDivisions = 2;				//< Audoset gain will try to fit diagram to X divisions
		
		/***********************************************
		 * Settings for the OscTrigger
		 ***********************************************/
		
		public OscTrigger.TriggerMode Mode;				//< Trigger mode will be copyied to the OscTrigger
		public OscTrigger.TriggerEdge Edge;				//< Trigger edge will be copyied to the OscTrigger
		public float triggerLevel;						//< Trigger level will be copyied to the OscTrigger
		
		/***********************************************
		 * Decoupling 
		 ***********************************************/
		
		public bool decoupling;							//< Decoupling mode
		public Vector3 dclevel;							//< Dc level for decouplig
		
		/***********************************************
		 * Sample format
		 ***********************************************/

		public enum Format { Float, Vector2, Vector3 }
		public Format format;							//< Format os sample
		public enum Style { Default, Logic }
		public Style style;								//< Rendering style
		
		/***********************************************
		 * Delegates
		 ***********************************************/
		
		/// <summary>Return current sample</summary>
		public delegate void ReadSampleDelegate(OscProbe probe);
		public ReadSampleDelegate readSample;				//< Read sample from this probe	

		/// <summary>Return trigger sample</summary>
		public delegate float ReadTriggerSampleDelegate();
		public ReadTriggerSampleDelegate readTriggerSample; //< Read sample for trigger	

		/// <summary>Oscilloscope will call it after rendering</summary>
		public delegate void RenderLabelsDelegate(OscChannel gui);
		public RenderLabelsDelegate renderLabels;			//< Render additional markers

		// =============================================================================================================
		// Gain control
		// =============================================================================================================
		
		private float gain = 1f; 						//< Manual scale for inputs
		
		/// <summary>The gain (values per division)</summary>
		public float Gain
		{
			get { return gain; }
			set { gain = OscValue.Gain.GetValue(value); }
		}
		
		// =============================================================================================================
		// Oscilloscope channel
		// =============================================================================================================
		
		/// <summary>Connected to this channel</summary>
		public OscChannel Gui;	

		/// <summary>Apply the plug's settings to the connected channel</summary>
		public void Apply()
		{
			Gui?.OnPlugHandle();
		}
		
		// =============================================================================================================
		// Log data api
		// =============================================================================================================
		
		/// <summary>Curent sample value</summary>
		private Vector3 sampleIn;
		private Vector3 sample;

		public Vector3 GetSample(float dt)
		{
			readSample?.Invoke(this);
			dclevel = Vector3.Lerp(dclevel, sampleIn, dt);
			return sample = sampleIn - dclevel;
		}

		public Vector3 GetSample()
		{
			readSample?.Invoke(this);
			return sample = sampleIn;
		}

		/// <summary>Write floating point value to probe</summary>
		public void Log(float value) { sampleIn.x = value; }
		/// <summary>Write integer value to probe</summary>
		public void Log(int value) { sampleIn.x = (float)value; }
		/// <summary>Write bool value to probe</summary>
		public void Log(bool value) { sampleIn.x = value ? 1f : 0f; }
		/// <summary>Write vector 2 to probe</summary>
		public void Log(Vector2 value) { sampleIn.x = value.x; sampleIn.y = value.y; }
		/// <summary>Write vector 2 to probe</summary>
		public void Log(float x, float y) { sampleIn.x = x; sampleIn.y = y; }
		/// <summary>Write vector3 value to probe</summary>
		public void Log(Vector3 value)
		{
			sampleIn.x = value.x; sampleIn.y = value.y; sampleIn.z = value.z;
		}
		/// <summary>Write vector3 value to probe</summary>
		public void Log(float x, float y, float z)
		{
			sampleIn.x = x; sampleIn.y = y; sampleIn.z = z;
		}
		/// <summary>Write color value to probe</summary>
		public void Log(Color value) { 
			sampleIn.x = value.r;
			sampleIn.y = value.g;
			sampleIn.z = value.b;
		}
		
		// =============================================================================================================
		// Constructors
		// =============================================================================================================
		
		/// <summary>Void input for the oscilloscope</summary>
		/// <param name="name">The input's name</param>
		public OscProbe(string name)
		{
			this.name = name;
			// defalut method read all sample
			this.readSample = null;
			// default method read x value
			this.readTriggerSample = () => sample.x;	
		}

		/// <summary>Null probe used as default frobe for unused channels</summary>
		public bool IsNullProbe => this == Null;
		
		/// <summary>Default probe with constant 0 value</summary>
		public static OscProbe Null = new OscProbe("Null");
	}

	/// <summary>
	/// Simple sine wave input
	/// </summary>
	public class OscSineProbe : OscProbe
	{
		private float amplitude;
		private float timeScale;
		
		public OscSineProbe(string name, float frequence = 10f, float amplitude = 1f) : base(name)
		{
			this.amplitude = amplitude;
			this.timeScale = Mathf.PI * 2f * frequence;
			this.readSample = (OscProbe probe) =>
			{
				probe.Log(Mathf.Sin(Time.unscaledTime * timeScale) * this.amplitude);
			}; 
		}

		public static OscProbe Default = new OscSineProbe("Sin");
	}
	
	/// <summary>
	/// Simple square form wave input
	/// </summary>
	public class OscSquareProbe : OscProbe
	{
		private float amplitude;
		private float timeScale;
		
		public OscSquareProbe(string name, float frequence = 10f, float amplitude = 1f) : base(name)
		{
			this.amplitude = amplitude;
			this.timeScale = frequence * 2f;
			this.readSample = (OscProbe probe) =>
			{
				var ival = (int) (Time.unscaledTime * timeScale);
				probe.Log((float) (ival & 1) * this.amplitude);
			};
		}
		
		public static OscProbe Default = new OscSquareProbe("Sqrt");
	}

}
