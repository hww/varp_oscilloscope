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
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


namespace Plugins.VARP.OSC
{
    [System.Serializable]
	public class OscChannel : MonoBehaviour, IRenderableGUI, IRenderable, IPluggable
	{
		/// <summary>Channel names</summary>
		public enum Name
		{
			C1, //< Channel 1
			C2, //< Channel 2
			C3, //< Channel 3
			C4, //< Channel 4
			C5, //< Channel 5 (Expansion)
			C6, //< Channel 6 (Expansion)
			C7, //< Channel 7 (Expansion)
			C8  //< Channel 8 (Expansion)
		}

		public Name channelName;				//< Channel's name will be displayed on screen
		public Text configText;		  			//< The channel's text message
		public Text statusText;		  			//< The channel's measurements text message
		public OscChannelLabel label;			//< Label at left side of screen
		public OscLed ledSelected;				//< LED button shows active channel
		public OscLed ledPlugged;				//< LED button shows active channel
		public Color color;						//< Channel color
		public OscTrigger.TriggerMode TrigTriggerMode;		//< Trigger mode
		public OscTrigger.TriggerEdge TrigTriggerEdge;		//< Trigger edge

		public float trigLevel;					//< Trigger threshold
		public bool isPlugged;
		
		private Oscilloscope oscilloscope;		//< Oscilloscope reference
		private OscSettings oscSettings;
		private OscRenderer oscRenderer;
		private float chanLabelPosX;			//< (Calculate) Coordinate of markers (Grid Divisions)
		private float valsLabelPosX;			//< (Calculate) Coordinate of markers (Grid Divisions)

		private float autoDivisions = 2;		//< Auto attenuation fit to X divisions

		/// <summary>Initialize oscilloscope channel</summary>
		/// <param name="oscProbe">Default input connected to this channel</param>
		/// <param name="bufferSize">Buffer capacity</param>
		public void Initialize(Oscilloscope osc, OscProbe oscProbe, int bufferSize)
		{
			oscilloscope = osc;
			oscSettings = osc.oscSettings;
			oscRenderer = osc.oscRenderer;
			chanLabelPosX = oscSettings.rectangle.xMin;
			valsLabelPosX = oscSettings.rectangle.xMax;
			label.color = color;
			statusText.color = color;
			ledPlugged.colorOn = color;
			ledPlugged.message = label.text = channelName.ToString();
			buffer = new Vector3[bufferSize];
			Plug(oscProbe);
			RenderGUI();					
		}
		
		// =============================================================================================================
		// Probe input
		// =============================================================================================================
		
		public OscProbe probe;					//< The input of this channels

		/// <summary>Plug input to the oscilloscope channel</summary>
		/// <param name="oscProbe">Default input connected to this channel</param>
		public void Plug(OscProbe oscProbe)
		{
			if (oscProbe.Gui != null) oscProbe.Gui.Unplug();
			probe = oscProbe;
			OnPlugHandle();
		}

		/// <summary>Unplug probe from channel</summary>
		public void Unplug()
		{
			if (probe == null) return;
			probe.Gui = null;
			probe = OscProbe.Null;
			OnPlugHandle();
		}
		
		/// <summary>Call to update channels by probes</summary>
		public void OnPlugHandle()
		{
			ClearLabels();
			probe.renderLabels?.Invoke(this);
			Gain = probe.Gain;
			decoupling = probe.decoupling;
			format = probe.format;
			position = probe.position;
			autoGain = probe.autoGain;
			autoDivisions = probe.autoGainDivisions;
			style = probe.style;
			TrigTriggerMode = probe.Mode;
			TrigTriggerEdge = probe.Edge;
			readSample = probe.readSample;
			readTriggerSample = probe.readTriggerSample;
			trigLevel = probe.triggerLevel;
			isPlugged = !probe.IsNullProbe;
			label.visible = isPlugged;
			UpdateLeds();
			isDirtyConfigText = isDirtyStatusText = true;
			if (oscilloscope.trigger.channel == this)
				oscilloscope.trigger.OnPlugHandle();
			sample = dclevel = new Vector3();
		}

		private void UpdateLeds()
		{
			ledPlugged.State = isPlugged;
		}
		
		// =============================================================================================================
		// Acquire sample
		// =============================================================================================================
		
		public Vector3 sample;											//< Current sample
		public Vector3 dclevel;											//< DC level for decoupling
		public OscProbe.ReadTriggerSampleDelegate readTriggerSample; 	//< Read sample for trigger	
		public OscProbe.ReadSampleDelegate readSample;					//< Read sample from this probe	
		
		/// <summary>Acquire sample with curent probe</summary>
		public void AcquireSample(int dmaWrIdx, float dt)
		{
			if (isPlugged)
			{
				readSample?.Invoke(probe);
				if (decoupling)
				{
					var sampleIn = probe.sample;
					dclevel = Vector3.Lerp(dclevel, sampleIn, dt);
					sample = sampleIn - dclevel;
				}
				else
				{
					sample = probe.sample;
				}
				this[dmaWrIdx] = sample;
			}
		}

		/// <summary>Acquire sample for trigger</summary>
		public float AcquireTriggerSample()
		{
			return isPlugged ? (readTriggerSample?.Invoke(this) ?? sample.x) : 0f;
		}
		
		// =============================================================================================================
		// The renderer
		// =============================================================================================================

		/// <summary>Render this channel</summary>
		/// <param name="renderer"></param>
		/// <param name="smpBeg">Start sample</param>
		/// <param name="smpEnd">End sample</param>
		/// <param name="smpCenter">Samples quantity</param>
		/// <param name="pixPerSample">Scale samples to pixels</param>
		public void Render(OscRenderer renderer, int smpBeg, int smpEnd, float pixStart, float pixPerSample)
		{
			
			// Do not render external channelor if "Off" probe used
			if (isPlugged)
			{
				if (CalculateMinMax(smpBeg, smpEnd))
					isDirtyStatusText = true;

				switch (style)
				{
					case OscProbe.Style.Default:
						switch (format)
						{
							case OscProbe.Format.Float:
								renderer.PlotFloat(this, smpBeg, smpEnd, pixStart, pixPerSample, color);
								break;
							case OscProbe.Format.Vector2:
								renderer.PlotVector2(this, smpBeg, smpEnd, pixStart, pixPerSample, color);
								break;
							case OscProbe.Format.Vector3:
								renderer.PlotVector3(this, smpBeg, smpEnd, pixStart, pixPerSample, color);
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
						break;
					case OscProbe.Style.Logic:
						renderer.PlotFloatLogic(this, smpBeg, smpEnd, pixStart, pixPerSample, color);
						break;
				}

			}
		}

		// =============================================================================================================
		// Statistics
		// =============================================================================================================

		/// <summary>Calculate min, max and peak to peak values</summary>
		public OscMinMax minMax = new OscMinMax();						//< (Calculated) min value

		/// <summary>Calculate min max value</summary>
		public void OnTrigger()
		{
			minMax.Reset();
		}

		/// <summary>Calculate min max value</summary>
		public bool CalculateMinMax(int smpStart, int smpEnd)
		{
			return minMax.CalculateMinMax(buffer, smpStart, smpEnd, format);
		}

		/// <summary>Tune ato gain</summary>
		public void ApplyAutoGain()
		{
			if (isPlugged && autoGain)
				gain = OscValue.Gain.GetAutoGain(minMax.P2P, autoDivisions);
		}
		
		// =============================================================================================================
		// Text message for this channel
		// =============================================================================================================
				
		private const string STR_AUTO_ON = "AUTO";
		private const string STR_AUTO_OFF = "";
		private const string STR_DECOUPLING_ON = "AC";
		private const string STR_DECOUPLING_OFF = "DC";
		
		private bool isDirtyConfigText;
		private bool isDirtyStatusText;
		
		/// <summary>Render OSD labels. Can be called per frame</summary>
		public void RenderGUI()
		{
			if (isPlugged)
			{
				// channel's zero level label at the left side of screen
				label.anchoredPosition = oscSettings.GetPixelPositionClamped(chanLabelPosX, position);
				// channel's custom value labels at the right side of screen
				var valsNum = valueLabels.Count;
				for (var i = 0; i < valsNum; i++)
				{
					var label = valueLabels[i];
					label.anchoredPosition = oscSettings.GetPixelPositionClamped(valsLabelPosX, label.position + position);
				}

			}
			
			if (isDirtyConfigText)
			{
				isDirtyConfigText = false;
				oscilloscope.trigger.RequestRedraw();
				if (probe != null)
					configText.text =
						$"{probe.name} {(autoGain ? STR_AUTO_ON : STR_AUTO_OFF)} {style}\n{(decoupling ? STR_DECOUPLING_ON : STR_DECOUPLING_OFF)} {OscFormatter.FormatValuePerDiv(gain)}\nPos:{position} divs ({OscFormatter.FormatValue(position * scale)})";	
				else
					configText.text = string.Empty;
			}
			
			if (isDirtyStatusText)
			{
				isDirtyStatusText = false;
				if (probe != null)
					statusText.text = $"Vpp: {OscFormatter.FormatValue(minMax.P2P),5}";	
				else
					statusText.text = string.Empty;
			}
		}

		// =============================================================================================================
		// Waveform buffer
		// =============================================================================================================
		
		/// <summary>Samples buffer</summary>
		private Vector3[] buffer;
		/// <summary>Buffer format</summary>
		private OscProbe.Format format;
		
		/// <summary>Read or write buffer</summary>
		/// <param name="idx"></param>
		public Vector3 this[int idx]
		{
			get { return buffer[idx]; }
			set { buffer[idx] = value; }
		}

		/// <summary>Read sample</summary>
		public float GetFloat(int idx)
		{
			return buffer[idx & OscSettings.BUFFER_INDEX_MASK].x * scale + position;
		}


		public Vector3 GetVector3(int idx)
		{
			var i = idx & OscSettings.BUFFER_INDEX_MASK;
			return new Vector3(
				buffer[i].x * scale + position, 
				buffer[i].y * scale + position, 
				buffer[i].z * scale + position);
		}
		
		public Vector2 GetVector2(int idx)
		{
			var i = idx & OscSettings.BUFFER_INDEX_MASK;
			return new Vector2(
				buffer[i].x * scale + position, 
				buffer[i].y * scale + position);
		}
		
		/// <summary>Clear samples of this channel</summary>
		public void Clear()
		{
			for (var i=0; i<buffer.Length; i++)
				buffer[0] = new Vector3();
		}

		// =============================================================================================================
		// Gain & other controls
		// =============================================================================================================

		private float gain = 1; //< Values per division
		private float scale = 1f;
		private float position = 0f;			//< Change vertical position of this diagram
		private bool autoGain = false;			//< Make this input auto-scaled vertically
		private bool decoupling;				//< AC/DC coupling mode
		private OscProbe.Style style;			//< Rendering style
		
		/// <summary>Set or get gain of the channel (values per division)</summary>
		public float Gain
		{
			get { return gain; }
			set
			{
				gain = OscValue.Gain.GetValue(value);
				scale = 1f / gain;
				isDirtyConfigText = true;
			}
		}

		/// <summary>Inrease gain</summary>
		public void GainPlus()
		{
			var index = OscValue.Gain.GetValueIndex(gain);
			Gain = OscValue.Gain.GetValueByIndex(index + 1);
		}
		
		/// <summary>Decrease gain</summary>
		public void GainMinus()
		{
			var index = OscValue.Gain.GetValueIndex(gain);
			Gain = OscValue.Gain.GetValueByIndex(index - 1);
		}

		/// <summary>Set or get gain of the channel</summary>
		public float Scale
		{
			get { return scale; }
			set { Gain = 1f / value; }
		}
		
		/// <summary>Change position of the level</summary>
		public float Position
		{
			get { return position; }
			set { position = value; isDirtyConfigText = true; }
		}
		
		/// <summary>Change AC/DC mode</summary>
		public bool Decoupling
		{
			get { return decoupling; }
			set { decoupling = value; isDirtyConfigText = true; }
		}
		
		/// <summary>Change AutoGain controll</summary>
		public bool AutoGain
		{
			get { return autoGain; }
			set { autoGain = value; isDirtyConfigText = true; }
		}
		
		/// <summary>Rendering style</summary>
		public OscProbe.Style Style
		{
			get { return style; }
			set { style = value; isDirtyConfigText = true; }
		}
		
		// =============================================================================================================
		// Labels
		// Small time or value markers rendered on the screen and repositioned together with 
		// channel's oscillogram
		// =============================================================================================================
	
		private List<OscChannelLabel> valueLabels = new List<OscChannelLabel>(10);

		
		/// <summary>Add vertical (value) label</summary>
		public void AddValueLabel(string text, float y)
		{
			var label = oscilloscope.valueLables.SpawnLabel();
			label.text = text;
			label.color = color;
			label.position = y;
			label.anchoredPosition = oscSettings.GetPixelPositionClamped(valsLabelPosX, label.position + position);
			label.visible = true;
			valueLabels.Add(label);	
		}
		
		/// <summary>Clear all labels/summary>
		private void ClearLabels()
		{
			for (var i=0; i<valueLabels.Count; i++)
				oscilloscope.valueLables.Release(valueLabels[i]);
		}
	}

	public interface IRenderable
	{
		/// <summary>Render this channel</summary>
		/// <param name="renderer"></param>
		/// <param name="smpBeg">Start sample</param>
		/// <param name="smpEnd">Samples quantity</param>
		/// <param name="pixStart">Sample at the screen center</param>
		/// <param name="smpScaleToPixels">Scale samples to pixels</param>
		void Render(OscRenderer renderer, int smpBeg, int smpEnd, float pixStart, float smpScaleToPixels);
	}

	public interface IRenderableGUI
	{
		/// <summary>
		/// Render gui widgets of this channel
		/// </summary>
		void RenderGUI();
	}

	public interface IPluggable
	{
		/// <summary>Update parameters of channel from the probe</summary>
		void Plug(OscProbe probe);
		/// <summary>Update parameters of channel from the probe</summary>
		void Unplug();
	}
}