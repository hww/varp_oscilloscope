using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VARP.OSC;

public class OscilloscopeDemo : MonoBehaviour
{
	// =================================================================================================================
	// Fields
	// =================================================================================================================
	
	public Camera demoCamera;
	public Canvas canvas;
	public VARP.OSC.Oscilloscope oscilloscope;
	
	private OscProbe oscHeartBeatProbe;
	private OscProbe oscSinwaveProbe;
	private OscProbe oscLastDifficultyForce;
	private OscProbe oscDistanceXZ;
	private OscProbe oscEvents;
	private bool heartBeat;

	// =================================================================================================================
	// Mono behaviour
	// =================================================================================================================

	private void Start()
	{
		oscilloscope.Initialize();
		
		// -- Setup heart beat synchronization probe input -- 
		oscHeartBeatProbe = new OscProbe("HeartBeat");
		oscHeartBeatProbe.readSample = (OscProbe probe) =>
		{
			probe.Log(heartBeat ? 1f : 0f);
		};
		oscHeartBeatProbe.autoGain = true;
		oscHeartBeatProbe.autoGainDivisions = 2;
		oscHeartBeatProbe.position = 2;
		oscHeartBeatProbe.Mode = OscTrigger.TriggerMode.Normal;
		oscHeartBeatProbe.Edge = OscTrigger.TriggerEdge.Rising;
		oscHeartBeatProbe.triggerLevel = 0.5f;
		oscilloscope.GetChannel(OscChannel.Name.C1).Plug(oscHeartBeatProbe);
		oscilloscope.trigger.SetChannel(OscChannel.Name.C1);
		
		// -- Setup simple sine wave form probe input --
		oscSinwaveProbe = new OscProbe("Sinus+1V");
		oscSinwaveProbe.readSample = (OscProbe probe) =>
		{
			probe.Log(Mathf.Sin(Time.time*3f) + 1f); // 1V DC
		};
		oscSinwaveProbe.autoGain = true;
		oscSinwaveProbe.autoGainDivisions = 2;
		oscSinwaveProbe.decoupling = true;
		oscSinwaveProbe.position = -2;
		oscSinwaveProbe.renderLabels = (OscChannel channel) =>
		{
			channel.AddValueLabel("-1", -1f);
			channel.AddValueLabel("0", 0f);
			channel.AddValueLabel(" 1", 1f);
		};
		oscilloscope.GetChannel(OscChannel.Name.C2).Plug(oscSinwaveProbe);
	
		// -- Setup vector 3 probe input -- 
		var oscVecProbe = new OscProbe("Vector3");
		oscVecProbe.readSample = (OscProbe probe) =>
		{
			probe.Log(Mathf.Sin((Time.time)*3f), 
				Mathf.Sin((Time.time+0.5f)*3f),
				Mathf.Sin((Time.time+1.0f)*3f));
		};
		oscVecProbe.autoGain = true;
		oscVecProbe.Gain = 2f;
		oscVecProbe.autoGainDivisions = 2;
		oscVecProbe.position = 0;
		oscVecProbe.format = OscProbe.Format.Vector3;
		oscVecProbe.Mode = OscTrigger.TriggerMode.Normal;
		oscVecProbe.Edge = OscTrigger.TriggerEdge.Rising;
		oscVecProbe.triggerLevel = 0.5f;
		oscilloscope.GetChannel(OscChannel.Name.C3).Plug(oscVecProbe);
		
		// -- Setup vector 3 probe input -- 
		var oscLogicProbe = new OscProbe("LogicProbe");
		oscLogicProbe.readSample = (OscProbe probe) =>
		{
			probe.Log((int)Time.time);
		};
		oscLogicProbe.autoGain = true;
		oscLogicProbe.Gain = 2f;
		oscLogicProbe.autoGainDivisions = 2;
		oscLogicProbe.position = -5;
		oscLogicProbe.style = OscProbe.Style.Logic;
		oscLogicProbe.Mode = OscTrigger.TriggerMode.Normal;
		oscLogicProbe.Edge = OscTrigger.TriggerEdge.Rising;
		oscLogicProbe.triggerLevel = 0.5f;
		oscilloscope.GetChannel(OscChannel.Name.C4).Plug(oscLogicProbe);
		
		
		// -- Plug them to scope --
		oscilloscope.trigger.SecondsDivision = 0.5f;
		
		SetEnabledInternal(true);
	}

	private void Update()
	{
		heartBeat = (1 & (int) Time.time) > 0; // demo pulses
	}
	
	// =================================================================================================================
	// Api
	// =================================================================================================================

	/* Set enable or disabled */
	private void SetEnabledInternal(bool state)
	{
		oscilloscope.enabled = state;
		canvas.enabled = state;
		if (demoCamera != null) 
			demoCamera.enabled = state;
	}
}
