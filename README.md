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


## Channel names

The cnannels named A,B,C,D can be used for record samples and draw oscillogram on screen. Additional channel EXT can be used only for synhronixation.

## Horizontal Scale

