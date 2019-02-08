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

using UnityEngine;
using UnityEngine.UI;

namespace VARP.OSC
{

    /// <summary>
    /// Simple GUI for oscilloscope
    /// </summary>
    public class OscGuiManager : MonoBehaviour
    {

        public Canvas oscCanvas;
        public Text helpText;
        public Image keyboard;
        private Oscilloscope oscilloscope;

        /**********************************************************
         * Main button to activate or deactivate oscilloscope
         **********************************************************/
        private const KeyCode buttonMain = KeyCode.BackQuote;

        /**********************************************************
         * Channel selection
         **********************************************************/
        private const KeyCode buttonSelectC1 = KeyCode.Alpha1;
        private const KeyCode buttonSelectC2 = KeyCode.Alpha2;
        private const KeyCode buttonSelectC3 = KeyCode.Alpha3;
        private const KeyCode buttonSelectC4 = KeyCode.Alpha4;
        private const KeyCode buttonSelectC5 = KeyCode.Alpha5;
        private const KeyCode buttonSelectC6 = KeyCode.Alpha6;
        private const KeyCode buttonSelectC7 = KeyCode.Alpha7;
        private const KeyCode buttonSelectC8 = KeyCode.Alpha8;
        private const KeyCode buttonSelectTrigger = KeyCode.Alpha0;
        // used for changing function buttonSelectCN, when shift pressed
        // the channel attached as trigger's source
        private const KeyCode buttonSelectModifyer1 = KeyCode.LeftShift;
        private const KeyCode buttonSelectModifyer2 = KeyCode.LeftShift;
        
        // (computed) list of all above
        private KeyCode[] buttonSelectKeys;

        /**********************************************************
         * Cursors will be used to control magnitude 
         **********************************************************/
        // horizontal position and level
        private const KeyCode buttonTimePosPlus = KeyCode.RightArrow;
        private const KeyCode buttonTimePosMinus = KeyCode.LeftArrow;
        private const KeyCode buttonTimeScalePlus = KeyCode.Equals;
        private const KeyCode buttonTimeScaleMinus = KeyCode.Minus;
        private const KeyCode buttonLevelPlus = KeyCode.UpArrow;
        private const KeyCode buttonLevelMinus = KeyCode.DownArrow;
        // channel settings
        private const KeyCode buttonPosPlus = KeyCode.UpArrow;
        private const KeyCode buttonPosMinus = KeyCode.DownArrow;
        private const KeyCode buttonGainPlus = KeyCode.Equals;
        private const KeyCode buttonGainMinus = KeyCode.Minus;

        /**********************************************************
         * Channel settings
         **********************************************************/
        private const KeyCode buttonChannelAuto = KeyCode.A;
        private const KeyCode buttonChannelAcDC = KeyCode.C;
        private const KeyCode buttonChannelUnplug = KeyCode.U;
        private const KeyCode buttonChannelView = KeyCode.V;

        /**********************************************************
         * Trigger settings
         **********************************************************/
        private const KeyCode buttonTriggerMode = KeyCode.M;
        private const KeyCode buttonTriggerEdge = KeyCode.E;
        private const KeyCode buttonTriggerPause = KeyCode.Pause;
        private const KeyCode buttonTriggerForceStart = KeyCode.S;

        /**********************************************************
         * Grid settings
         **********************************************************/
        private const KeyCode buttonGridSettings = KeyCode.G;
        private int gridSetting;
        
        public void Initialize(Oscilloscope osc)
        {
            oscilloscope = osc;
            buttonSelectKeys = new KeyCode[]
            {
                buttonSelectC1, buttonSelectC2, buttonSelectC3, buttonSelectC4,
                buttonSelectC5, buttonSelectC6, buttonSelectC7, buttonSelectC8,
            };
            SelectChannel(OscChannel.Name.C1);
            UpdateHelp();
            IsVisible = true;
        }


        void Update()
        {
            // -- Test main button --
            var main = Input.GetKeyDown(buttonMain);
            if (IsVisible)
            {
                var trigger = oscilloscope.trigger;

                if (main)
                {
                    // when it is focused main button close OSC
                    if (isInFocus)
                        IsVisible = false;
                    else
                        IsInFocus = true;
                }
                else if (isInFocus)
                {
                    // select channel
                    if (Input.GetKeyDown(buttonSelectTrigger))
                    {
                        SelectTriggerChannel();
                    }
                    else
                    {
                        var shift = Input.GetKey(buttonSelectModifyer1) || Input.GetKey(buttonSelectModifyer2);
                        for (var i = 0; i < buttonSelectKeys.Length; i++)
                        {
                            if (Input.GetKeyDown(buttonSelectKeys[i]))
                            {
                                if (shift)
                                    // enable trigger input channel
                                    oscilloscope.trigger.SetChannel((OscChannel.Name) i);
                                else
                                    // select channel
                                    SelectChannel((OscChannel.Name) i);
                            }
                        }
                    }

                    // global evens and buttons
                    if (Input.GetKeyDown(buttonTriggerPause))
                        trigger.Pause = !trigger.Pause;
                    else if (Input.GetKeyDown(buttonTriggerForceStart))
                        trigger.ForceTrigger();
                    
                    // -- trigger control --
                    else if (Input.GetKeyDown(buttonTriggerMode))
                        trigger.Mode = GetNextEnum<OscTrigger.TriggerMode>(trigger.Mode);
                    else if (Input.GetKeyDown(buttonTriggerEdge))
                        trigger.Edge = GetNextEnum<OscTrigger.TriggerEdge>(trigger.Edge);
                    
                    else if (Input.GetKeyDown(buttonGridSettings))
                    {
                        gridSetting++;
                        oscilloscope.grid.DrawGrid = (gridSetting & 1) > 0;
                        oscilloscope.grid.DrawRulerY = oscilloscope.grid.DrawRulerX = (gridSetting & 2) > 0;
                    }
                    
                    // the event for selected or trigger channel
                    if (selectedChannel == null)
                    {
                        // Joy pad control
                        if (Input.GetKeyDown(buttonTimeScalePlus))
                            trigger.SecondsDivisionPlus();
                        else if (Input.GetKeyDown(buttonTimeScaleMinus))
                            trigger.SecondsDivisionMinus();
                        else if (Input.GetKeyDown(buttonLevelPlus))
                            trigger.Level += 0.5f;
                        else if (Input.GetKeyDown(buttonLevelMinus))
                            trigger.Level -= 0.5f;
                        else if (Input.GetKeyDown(buttonTimePosPlus))
                            trigger.Position += 0.5f;
                        else if (Input.GetKeyDown(buttonTimePosMinus))
                            trigger.Position -= 0.5f;
                    }
                    else
                    {
                        // ------------------------------------------------------
                        // selected channel 
                        // ------------------------------------------------------
                        var channel = SelectedChannel;
                        
                        if (Input.GetKeyDown(buttonChannelAuto))
                            channel.AutoGain = !channel.AutoGain;
                        else if (Input.GetKeyDown(buttonChannelAcDC))
                            channel.Decoupling = !channel.Decoupling;
                        else if (Input.GetKeyDown(buttonChannelUnplug))
                            channel.Unplug();
                        else if (Input.GetKeyDown(buttonChannelView))
                            channel.Style = GetNextEnum<OscProbe.Style>(channel.Style);

                        // Joy pad control
                        if (Input.GetKeyDown(buttonPosPlus))
                            channel.Position += 0.5f;
                        else if (Input.GetKeyDown(buttonPosMinus))
                            channel.Position -= 0.5f;
                        else if (Input.GetKeyDown(buttonGainPlus))
                            channel.GainPlus();
                        else if (Input.GetKeyDown(buttonGainMinus))
                            channel.GainMinus();
                    }
                }
            }
            else
            {
                // when unfocused main button make focus
                if (main)
                    IsVisible = true;
            }

            if (updateHelp)
                UpdateHelp();
        }

        // =============================================================================================================
        // Visibility control
        // =============================================================================================================

        public bool IsVisible
        {
            get { return oscCanvas.enabled; }
            set
            {
                oscCanvas.enabled = isInFocus = value;
                updateHelp = true;
            }
        }

        // =============================================================================================================
        // Focus control
        // =============================================================================================================

        private bool isInFocus; //< TRUE means inputs tested

        /// <summary>
        /// When oscilloscope is in focus the oscilloscope controlled by keys.
        /// In other case the keyboard ignored.
        /// </summary>
        public bool IsInFocus
        {
            get { return isInFocus; }
            set
            {
                keyboard.enabled = isInFocus = value;
                updateHelp = true;
            }
        }

        // =============================================================================================================
        // Choose and print help
        // =============================================================================================================

        private bool updateHelp;

        /// <summary>Update help texts</summary>
        void UpdateHelp()
        {
            updateHelp = false;

            if (IsVisible)
            {
                if (!isInFocus)
                {
                    // not in focus 
                    helpText.text = toFocusHelp;
                }
                else
                {
                    var help = "";
                    if (selectedChannel == null)
                    {
                        // selected trigger channgel
                        help += "TRIGGER:\n";
                        help += triggerHelp;
                    }
                    else
                    {
                        // selected channel
                        help += $"CHANNEL: {selectedChannel.channelName}\n";
                        help += channelHelp;
                    }
                    // display this help in any case 
                    help += persistHelp;
                    helpText.text = help;
                }
            }
        }

        // =============================================================================================================
        // The text messages for help
        // =============================================================================================================

        private readonly string toFocusHelp = "` activate keyboard shortcuts\n";
        private readonly string channelHelp = "1,2,..,8 select input\nA auto, C coupling\nU unplug, V view\nUP,DOWN position\n+,- gain\n";
        private readonly string triggerHelp = "SHIFT+1,2,..,8 select input\nM mode, E edge\nUP,DOWN level\nLEFT,RIGHT time position\n+,- time scale";
        private readonly string persistHelp = "PAUSE pause, S force start\n";
        
        // =============================================================================================================
        // Current channel can be used for keyboard shortcuts
        // =============================================================================================================

        private OscChannel selectedChannel; //< Channel to get messages

        public void SelectChannel(OscChannel.Name channelName)
        {
            oscilloscope.trigger.ledSelected.State = false;
            if (selectedChannel != null)
                selectedChannel.ledSelected.State = false;
            selectedChannel = oscilloscope.GetChannel(channelName);
            selectedChannel.ledSelected.State = true;
            updateHelp = true;
        }

        /// <summary>Mark channel selected</summary>
        public void SelectTriggerChannel()
        {
            if (selectedChannel != null)
                selectedChannel.ledSelected.State = false;
            selectedChannel = null;
            oscilloscope.trigger.ledSelected.State = true;
            updateHelp = true;
        }

        public OscChannel SelectedChannel => selectedChannel;

        // =============================================================================================================
        // Increment/Decrement enum value
        // =============================================================================================================

        public T GetNextEnum<T>(T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new System.ArgumentException(string.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] values = (T[]) System.Enum.GetValues(src.GetType());
            var j = System.Array.IndexOf<T>(values, src) + 1;
            return (values.Length == j) ? values[0] : values[j];
        }

        public T GetPrevEnum<T>(T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new System.ArgumentException(string.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] values = (T[]) System.Enum.GetValues(src.GetType());
            var j = System.Array.IndexOf<T>(values, src) - 1;
            return (0 > j) ? values[values.Length - 1] : values[j];
        }
    }
}
