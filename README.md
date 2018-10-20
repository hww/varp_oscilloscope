# VARP Oscilloscope
_Documentation for Untiy asset_ 

## Getting Stated

 VARP Oscilloscope are small, easy to use Unity asset, that you can use to record and analyze values modifyed by script, physcis or animation in real time. The oscilloscopes have four-channels but can be extended without proggraming.
In addition to the list of general features, this section covers the following topics:

- How to add asset to your project
- How to add extended functions
- How to perform a brief functional check
- How to compensate probes
- How to use the self calibration routine
- How to match your probe attenuation factor

## Features

- Single time base digital real-time oscilloscope
- Every frame or every fixed update sample rate<sup>1</sup> and 1000<sup>2</sup> point record lenght for each channel. 
- Four<sup>3</sup> independed recording channels.
- One additional external channel can be used for trigger sampling 
- Each buffer is aray of floating point values
- Each channel has it's own color tag.
- Screen 550x550pixels and 11x11 divisions grid<sup>4</sup> 
- Four automated measurements (min,max,peak,average)
- Autoset for quick setup
- Cursors with readout
- Custom markers OSD.

<sup>1</sup> The sampling rate is fully configurable and can be replaced to other time steps for example 1second.

<sup>2</sup> Can be modifyed to another size.

<sup>4</sup> Can be modifyed to another channels quantity.

<sup>3</sup> Can be modifyed to another dimentions.

## Additional Features 

- Does not require custom Unity GUI tools and learing.
- Fully configurable with script for different measurements. 
- Human friendly attenuation gain and time per division values. 

## Installation

Drop asset folder inside Assets/Plugins folder. After that you can instantiate prefab Oscilloscope in the sceene of your project. Now you can write your own script to control the oscilloscope with your game events or data.

## Basic Concepts

To use your oscilloscope effectively, you must understand the
following basic concepts:

- Triggering
- Acquiring data
- Scaling and positioning waveforms
- Measuring waveforms
- Setting Up the oscilloscope

The figure below shows a block diagram of the various functions of
an oscilloscope and their relationship to each other.

![Basic Concept Diagram](images/varp_oscilloscope_basic_concept.png)

**GameValue** _Any variable or class member can be captured by pushing it to the probe every frame or only when it was changed. As alternative the value can be pulled by lambda function assigned to the probe. Before recording the value should be converted to floating point type_

**OscProbe** _Container of sample and configuration settings for the channel or trigger. Avery time when the probe connected to the oscilloscope channel, the values will be copyied to the channel and to trigger (if this channel connected to trigger)_

**OscChannel** _This class contains data for data recording and rendering it on the screen_

**OscBuffer** _The buffer for recorded samples_

**Oscilloscope** _Main code for the oscilloscope_

**OscGrid** _Rendering of grid on the screen_

**OscRenderer** _Renderer of waveforms_

**OscTrigger** _Class which monitoring one of the channels and can be used to the start/stop acquiring data.


## Channel Names

The cnannels named A,B,C,D can be used for record samples and draw oscillogram on screen. Additional channel EXT can be used only for triggering recording samples. The channel's name will be displayed on sceen display and can be used as argumen of functions.

## Probe Names

The name of probe in just a string value will be displayed on scree to inform user about which probe connected to this or that channel.

## OsdProbe

Lets create simple probe and connect it to oscilloscope channel A.

```C#
var characterVelocityProbe = new OscProbe("CharacterVelocity");
Oscilloscope.I.ActivateProbe(OscChannel.Name.A, characterVelocityProbe);
```

### Push Value to Probe

As probe created we can push value to the probe with setting the sample field.

```C#
characterVelocityProbe.sample = rigidbody.velocity.magnitude;
```

### Pull Value by Probe

Alternative way to read values is to assign the lambda method to the deligate of probe.

```C#
oscEvents.readSample = () => { return rigidbody.velocity.magnitude };
```

### Probe Fields

| Type | Field | Info |
|---:|:----|:-----|
| RenderLabelsDelegate | renderLabels |	Render labels or markers |
| ReadSampleDelegate | readSample | Read sample from this probe	 |
| readonly | string name | Input's name will be displayed on screen |
| float | position | Change vertical position of this diagram |
| bool | autoGain | Make this input autoscaled verticaly |
| int | autoGainDivisions | Auto gain wil fit to X divisions |
| OscTrigger.Mode | triggerMode | Trigger mode |
| float | triggerLevel | Trigger threshold |
| float| sample | Curent sample value |



