using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace VARP.OSC
{
    /// <summary>
    /// Oscilloscope's trigger. 
    /// </summary>
    [System.Serializable]
    public class OscTrigger : MonoBehaviour, IRenderableGUI
    {
        // =============================================================================================================
        // Constants
        // =============================================================================================================
		
        /// <summary>Size of buffer should be 2^N</summary>
        public const int BUFFER_SIZE = 1024;
        /// <summary>Size of buffer should be 2^N / 2</summary>
        public const int BUFFER_HALF_SIZE = BUFFER_SIZE / 2; 
        /// <summary>Size of buffer should be 2^N-1</summary>
        public const int BUFFER_INDEX_MASK = BUFFER_SIZE - 1;
        /// <summary>How many samples per single draw</summary>
        public const int SAMPLES_PER_DRAW = 3;
        /// <summary>Display debugging OSD</summary>
        public const bool DEBUG_DISPLAY = true;
        /// <summary>Will be rendered on screen as label</summary>
        public const string TRIGGER_CHANNEL_NAME = "T";
        
        // =============================================================================================================
        // Initialization 
        // =============================================================================================================
        
        /// <summary>Create trigger with default channel</summary>
        /// <param name="oscilloscope"></param>
        public void Initialize(Oscilloscope osc, OscChannel oscChannel)
        {
            oscilloscope = osc;
            oscSettings = osc.oscSettings;
            oscRenderer = osc.oscRenderer;
            configText.color = color;
            valueLabel.color = color;
            valueLabel.text = TRIGGER_CHANNEL_NAME;
            timeCursor.color = color;
            timeCursor.text = TRIGGER_CHANNEL_NAME;
            ledPlugged.message = TRIGGER_CHANNEL_NAME;
            pause = true;
            timeLabelPosY = oscSettings.rectangle.yMax;
            debugText.enabled = DEBUG_DISPLAY;
            SetChannel(oscChannel);
            RenderGUI();
        }

        // =============================================================================================================
        // Trigger's input port can be connected to any channel
        // =============================================================================================================
        
        /// <summary>Set channel for this trigger</summary>
        public void SetChannel(OscChannel oscChannel)
        {
            channel = oscChannel;
            OnPlugHandle();
        }
        
        /// <summary>Set channel for this trigger</summary>
        public void SetChannel(OscChannel.Name channelName)
        {
            channel = oscilloscope.GetChannel(channelName);
            OnPlugHandle();
        }
        
        /// <summary>Set channel for this trigger. (Call it to update params of this channel)</summary>
        public void OnPlugHandle()
        {
            mode = channel.TrigTriggerMode;
            triggerEdge = channel.TrigTriggerEdge;
            level = channel.trigLevel;
            isDirtyConfigText = isDirtyStatusText = true;
        }

        // =============================================================================================================
        // Trigger screen renderer
        // =============================================================================================================

        /// <summary>Redraw grpah from left side of screen to current time</summary>
        public void Redraw()
        {
            dmaDrawBeg = dmaWriteTrggrd - numSamplesBeforeTrigger;
            dmaDrawEnd = dmaWriteTrggrd + numSamplesAfterTrigger;
            dmaDraw = dmaDrawBeg;
        }
        
        /// <summary>Render samples</summary>
        /// <param name="smpStart"></param>
        /// <param name="smpEnd"></param>
        private int RenderSamples(int smpStart, int smpEnd)
        {
            var smpCenter = dmaWriteTrggrd - sampAtCenterRel;
            var pixelsPerSample = oscSettings.pixelsPerDivision * divsPerSample;
            var pixStart = (smpStart - smpCenter) * pixelsPerSample + oscSettings.textureCenter.x;
            oscilloscope.Render(smpStart, smpEnd, pixStart, pixelsPerSample);
            return smpEnd;
        }
        
        /// <summary>Update text at the channel's label</summary>
        public void RenderGUI()
        {
            // channel's custom time labels at the bottom side of screen
            var timesNum = timeLabels.Count;
            for (var i = 0; i < timesNum ; i++)
            {
                var label = timeLabels[i];
                label.anchoredPosition = oscSettings.GetPixelPositionClamped(label.gridPosition.x + position, timeLabelPosY);
            }
            
            if (isDirtyConfigText)
            {
                isDirtyConfigText = false;
                configText.text = string.Format("In:{0} | T:{1,5} | Pos:{2,5} | Lev:{3,5} | Mode:{4,5} | Edge:{5,5} |", 
                    channel.channelName, 
                    OscFormatter.FormatTimePerDiv(secondsDivision), 
                    OscFormatter.FormatTime(timeAtCenterRel), 
                    OscFormatter.FormatValue(level * channel.Scale), 
                    mode, 
                    triggerEdge);
            }

            if (isDirtyStatusText)
            {
                isDirtyStatusText = false;
                iconStateArmed.enabled = false;
                iconStateAuto.enabled = false;
                iconStateReady.enabled = false;
                iconStateTriggered.enabled = false;
                iconStatePause.enabled = false;
                switch (status)
                {
                    case Status.Armed:
                        iconStateArmed.enabled = true;
                        break;
                    case Status.Ready:
                        iconStateReady.enabled = true;
                        break;
                    case Status.Triggered:
                        iconStateTriggered.enabled = true;
                        break;
                    case Status.Auto:
                        iconStateAuto.enabled = true;
                        break;
                    case Status.Stop:
                        iconStatePause.enabled = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
            
            // -- render horizontal line of threshold but only in case of non-ext channels
            if (valueLabel != null)
            {          
                var valueLabelY = level * channel.Scale + channel.Position;
                valueLabel.anchoredPosition = oscSettings.GetPixelPositionClamped(oscSettings.rectangle.xMin, valueLabelY);
            }


            
            if (DEBUG_DISPLAY)
            {
                debugText.text = string.Format(
                    "dmaWrt: {0}\ndmaTrg: {1}\ndmaEnd: {2}\nsmpLft: {3}\nsmpRht: {4}\ndmaDrw: {5}\ndrwBeg: {6}\ndrwEnd: {7}\nsmpDiv: {8}",
                    dmaWrite,
                    dmaWriteTrggrd,
                    dmaWriteEnd,
                    numSamplesBeforeTrigger,
                    numSamplesAfterTrigger,
                    dmaDraw,
                    dmaDrawBeg,
                    dmaDrawEnd,
                    sampPerDivision);
            }
        }
        
        // =============================================================================================================
        // Trigger Finite State Machine
        // =============================================================================================================
        
        /// <summary>Update trigger every frame</summary>
        public void UpdateTrigger()
        {
            switch (status)
            {
                case Status.Armed:                     /* Collect samples before trigger */
                    AquireSampe();
                    /* waiting to acquire amount of samples at left side of trigger */
                    if (++armingSamplesCount > numSamplesBeforeTrigger)
                    {
                        forceTrigger = false;
                        status = Status.Ready;
                        isDirtyConfigText = isDirtyStatusText = true;
                    }
                    break;
                case Status.Ready:                     /* Ready to test trigger */
                    AquireSampe();
                    if (ReadTrigger())
                    {
                        TriggerIt();
                        status = Status.Triggered;
                    }
                    break;
                case Status.Triggered: /* Record samples after trigger */
                    AquireSampe();
                    // test if we fill the buffer for all screen then finish render of screen
                    if (dmaWrite > dmaWriteEnd)
                    {
                        // finish render of screen 
                        dmaDraw = RenderSamples(dmaDraw, dmaWrite-1);
                        dmaWrite &= BUFFER_INDEX_MASK;
                        // go to nexr state depend on curent mode
                        switch (mode)
                        {
                            case TriggerMode.Auto:
                                dmaDraw = dmaWriteTrggrd = dmaWrite;
                                status = Status.Auto;
                                break;
                            case TriggerMode.Normal:
                                status = Status.Ready;
                                break;
                            case TriggerMode.Single:
                                status = Status.Ready;
                                break;
                        }
                        isDirtyConfigText = isDirtyStatusText = true;
                    }
                    else
                    {
                        // if enought samples to render - do it
                        if ((dmaWrite - dmaDraw) > SAMPLES_PER_DRAW)
                            dmaDraw = RenderSamples(dmaDraw, dmaWrite-1);
                    }
                    break;
                case Status.Auto:  /* Record and display on screen all what acuired */
                    AquireSampe();
                    // test if we fill the buffer for all screen then finish render of screen
                    if (dmaWrite > dmaWriteEnd)
                    {
                        dmaWrite &= BUFFER_INDEX_MASK;
                        // finish render of screen 
                        dmaDraw = RenderSamples(dmaDraw, dmaWrite-1);
                        // restart acquiring 
                        dmaDraw = dmaWriteTrggrd = dmaWrite;
                        TriggerIt();
                    }
                    else
                    {
                        // if enought samples to render - do it
                        if ((dmaWrite - dmaDraw) > SAMPLES_PER_DRAW)
                            dmaDraw = RenderSamples(dmaDraw, dmaWrite-1);
                    }
                    break;
                case Status.Stop:
                    isDirtyConfigText = isDirtyStatusText = true;
                    numSamplesBeforeTrigger = 0; 
                    break;
            }
            RenderGUI();
        }

        private void AquireSampe()
        {
            oscilloscope.AquireSampe(dmaWrite++);
            trigSample1 = trigSample2;
            trigSample2 = channel.probe.readTriggerSample.Invoke();
        }
        
        /// <summary>Start trigger</summary>
        private void TriggerIt()
        {
            forceTrigger = false;
            dmaWriteTrggrd = dmaWrite;
            dmaWriteEnd = dmaWrite + numSamplesAfterTrigger;
            isDirtyConfigText = isDirtyStatusText = true;
            Redraw();
        }
        
        /// <summary>Detect the start/stop events for oscilloscope</summary>
        private bool ReadTrigger()
        {
            // try to trigger the sampling it depends on curent mode.
            switch (mode)
            {
                case TriggerMode.Auto: 
                    return true;                
                case TriggerMode.Normal: 
                    switch (triggerEdge)
                    {
                        case TriggerEdge.Rising:
                            return trigSample2 > level && trigSample1 <= level;
                        case TriggerEdge.Falling:
                            return trigSample2 < level && trigSample1 >= level;
                    }
                    break;
                case TriggerMode.Single:
                    if (forceTrigger)
                    {
                        switch (triggerEdge)
                        {
                            case TriggerEdge.Rising:
                                return trigSample2 > level && trigSample1 <= level;
                            case TriggerEdge.Falling:
                                return trigSample2 < level && trigSample1 >= level;
                        }
                        return false;
                    }
                    break;
            }
            return false;
        }

        /// <summary>Available trigger modes (When we start/stop capturing)</summary>
        public enum TriggerMode
        {
            /// <summary>
            /// This trigger mode allows the oscilloscope to acquire a
            /// waveform even when it does not detect a trigger condition. If no
            /// trigger condition occurs while the oscilloscope waits for a specific
            /// period (as determined by the time-base setting), it will force itself to
            /// trigger.
            /// </summary>
            Auto, 
            /// <summary>
            /// The Normal mode allows the oscilloscope to acquire a
            /// waveform only when it is triggered. If no trigger occurs, the
            /// oscilloscope will not acquire a new waveform, and the previous
            /// waveform, if any, will remain on the display.
            /// </summary>
            Normal,
            /// <summary>
            /// The Single mode allows the oscilloscope to acquire one
            /// waveform each time you call ForceTrigger method, and the trigger
            /// condition is detected.
            /// </summary>
            Single
        }

        /// <summary>Available trigger edge detection modes</summary>
        public enum TriggerEdge
        {
            Rising,     //< When signal more than treshold
            Falling,    //< When signal less than treshold
        }

        /// <summary>Trigger status</summary>
        public enum Status
        {            
            /// <summary>
            /// The instrument is acquiring pretrigger data. All
            /// triggers are ignored in this state.
            /// </summary>
            Armed,    
            /// <summary>
            /// All pretrigger data has been acquired and the
            /// instrument is ready to accept a trigger.
            /// </summary>
            Ready,
            /// <summary>
            /// The instrument has seen a trigger and is acquiring the
            /// posttrigger data.
            /// </summary>
            Triggered,
            /// <summary>
            /// The instrument is in auto mode and is acquiring
            /// waveforms in the absence of triggers.
            /// </summary>
            Auto,
            /// <summary>
            /// The instrument has stopped acquiring waveform data.
            /// </summary>
            Stop
        }


        public Status status;                        //< Curent status of trigger
        public float level;                          //< The treshold value to detect trigger
        public Text configText;                      //< The channel's text message
        public Image iconStateArmed;
        public Image iconStateAuto;
        public Image iconStateReady;
        public Image iconStateTriggered;
        public Image iconStatePause;
        public Text debugText;
        public OscLed ledSelected;				     //< LED button shows active channel
        public OscLed ledPlugged;				     //< LED button shows active channel
        public OscCursorLabel valueLabel;            //< Channel's zero label
        public OscCursorLabel timeCursor;            //< Channel's trigger label
        public Color color;                          //< Trigger color
        public Oscilloscope oscilloscope;            //< Pointer to the oscilloscope
        public OscChannel channel;                   //< Connected channel to this trigger
        public OscRenderer oscRenderer;              //< The OSC renderer
        private OscSettings oscSettings;             //< Oscilloscope settings
        private bool forceTrigger;                   //< Pressed start button
        private TriggerMode mode;                    //< When we start/stop samples capturing
        private TriggerEdge triggerEdge;             //< The edge detection mode
        
        // =============================================================================================================
        // Dirty flags
        // =============================================================================================================
		
        private bool isDirtyConfigText;    //< Required rebuld config text
        private bool isDirtyStatusText;   //< Required rebuld statuc text
        
        // =============================================================================================================
        // Timing labels
        // =============================================================================================================
        		
        private List<OscChannelLabel> timeLabels = new List<OscChannelLabel>(10);
        private float timeLabelPosY;		        //< (Calculate) Coordinate of markers (Grid Divisions)
        
        /// <summary>Add horizontal (time) label</summary>
        public void AddTimeLabel(string text, float x)
        {
            var label = oscilloscope.timeLables.SpawnLabel();
            label.text = text;
            label.color = color;
            label.gridPosition = new Vector2(x, 0);
            label.anchoredPosition = oscSettings.GetPixelPositionClamped(label.gridPosition.x + position, timeLabelPosY);
            label.visible = true;
            timeLabels.Add(label);				
        }

        /// <summary>Add horizontal (time) label now</summary>
        public void AddTimeLabel(string text)
        {
        }

        /// <summary>Clear all labels/summary>
        private void ClearLabels()
        {
            for (var i=0; i<timeLabels.Count; i++)
                oscilloscope.timeLables.Release(timeLabels[i]);
        }
        
        // =============================================================================================================
        // Timing settings
        // =============================================================================================================
        
        /// <summary>Set or get horizontal time scale setting</summary>
        public float SecondsDivision
        {
            get { return secondsDivision; }
            set
            {
                value = OscValue.Time.GetValue(value);
                secondsDivision = value;
                sampPerDivision = secondsDivision / oscSettings.timePerSample;
                sampPerScreen = oscSettings.rectangle.width * sampPerDivision;
                divsPerSample = 1f / sampPerDivision;
                SetPosition(position);
            }
        }

        /// <summary>Inrease seconds per division</summary>
        public void SecondsDivisionPlus()
        {
            var index = OscValue.Time.GetValueIndex(secondsDivision);
            SecondsDivision = OscValue.Time.GetValueByIndex(index + 1);
        }
		
        /// <summary>Decrease seconds per division</summary>
        public void SecondsDivisionMinus()
        {
            var index = OscValue.Time.GetValueIndex(secondsDivision);
            SecondsDivision = OscValue.Time.GetValueByIndex(index - 1);
        }

        // =============================================================================================================
        // Vertical position
        // =============================================================================================================
        
        /// <summary>
        /// Positive values move diagram to the left. Untits DIVISIONS
        /// Other way to think about is the time in center of screen.
        /// </summary>
        public float Position
        {
            get { return position; }
            set { SetPosition(value); }
        }

        /// <summary>Set position. But before test for minimum and maximum value</summary>
        /// <param name="value">Vertical position</param>
        private void SetPosition(float value)
        {
            // compute minimum maximum for horizontal position
            var halfScreen = sampPerScreen / 2;
            var maxPosition = (BUFFER_HALF_SIZE - halfScreen) * divsPerSample ;
            // calculate actual position
            position = Mathf.Clamp(value, -maxPosition, maxPosition);
            // calculate relative position 
            sampAtCenterRel = Mathf.RoundToInt(position * sampPerDivision);
            timeAtCenterRel = position * secondsDivision;
            // caclulate samples before and after trigger
            numSamplesBeforeTrigger = Mathf.RoundToInt((int)halfScreen + sampAtCenterRel);
            numSamplesAfterTrigger = Mathf.RoundToInt((int)halfScreen - sampAtCenterRel);
            isDirtyConfigText = isDirtyStatusText = true;
            timeCursor.anchoredPosition = oscSettings.GetPixelPositionClamped(position, oscSettings.rectangle.yMin);
            Redraw(); /* request redraw */
        }

        // =============================================================================================================
        // Edge detection settings
        // =============================================================================================================

        /// <summary>
        /// Change the level of trigger
        /// </summary>
        public float Level
        {
            get { return level; }
            set { 
                level = value;
                isDirtyStatusText = isDirtyConfigText = true;
            }
        }

        /// <summary>
        /// Change the dge detection of trigger
        /// </summary>
        public TriggerEdge Edge
        {
            get { return triggerEdge; }
            set { triggerEdge = value; isDirtyStatusText = isDirtyConfigText = true; }
        }
        
        /// <summary>
        /// SET LEVEL TO 50%. The trigger level is set to the vertical midpoint between the peaks of the trigger signal.
        /// </summary>
        public void AutoSetLevel()
        {
            Level = channel.minMax.Mid;
        }
        
        // =============================================================================================================
        // Trigger mode
        // =============================================================================================================

        /// <summary>
        /// Set the mode of trigger
        /// </summary>
        public TriggerMode Mode
        {
            get { return mode; }
            set { SetMode(value); }
        }
        
        /// <summary>Set curent mode</summary>
        public void SetMode(TriggerMode triggerMode)
        {
            // change the mode
            this.pause = true;
            this.mode = triggerMode;
            // on mode enter
            switch (mode)
            {
                case TriggerMode.Auto:
                    TriggerIt();
                    status = Status.Auto;
                    break;
                case TriggerMode.Normal:
                    status = Status.Armed;
                    break;
                case TriggerMode.Single:
                    status = Status.Armed;
                    break;
            }
            isDirtyConfigText = isDirtyStatusText = true;
        }

        /// <summary>Start acquiring data</summary>
        public bool Pause
        {
            get { return pause; }
            set { pause = value; status = pause ? Status.Stop : Status.Armed; }
        }
        
        /// <summary>Start acquiring data</summary>
        public void ForceTrigger()
        {
            if (mode == TriggerMode.Single)
                forceTrigger = true;
        }

        // =============================================================================================================
        // Fields
        // =============================================================================================================
        
        private int dmaDraw;                    //< Number of sample for rendering
        private int dmaDrawBeg;                 //< Begin of rendering
        private int dmaDrawEnd;                 //< End of rendering
        private int dmaWrite;                   //< Number of sample for aquiring
        private int dmaWriteTrggrd;             //< Store dmaWrite when triggered 
        private int dmaWriteEnd;                //< Store dmaWrite when triggered 
        private float trigSample1;              //< Previous trigger sample
        private float trigSample2;              //< Next trigger sample
        private float secondsDivision = 1;    	//< (Knob) Horizontal scale
        private float position;			  		//< (Knob) Horizontal position
        private float sampPerScreen;	  		//< (Calculated) Width of screen in samples
        private float sampPerDivision; 		    //< (Calculated) How many samples in single division
        private float divsPerSample;            //< (Calculated) Divisions per samples
        private float timeAtCenterRel;          //< (Calculated) Relative sample number at center of screen
        private int sampAtCenterRel;            //< (Calculated) Relative time at center of screen
        private int armingSamplesCount;         //< (Calculated) How many samples before trigger
        private int numSamplesBeforeTrigger;    //< (Calculated) Counter of samples in armed mode
        private int numSamplesAfterTrigger;     //< (Calculated) Counter of samples in armed mode
        private bool pause = false;             //< Pause state

    }
}