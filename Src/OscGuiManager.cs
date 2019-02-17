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
    ///     Simple GUI for oscilloscope
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

        private const KeyCode ButtonMain = KeyCode.F11;

        /**********************************************************
         * Channel selection
         **********************************************************/

        private const KeyCode ButtonSelectC1 = KeyCode.Alpha1;
        private const KeyCode ButtonSelectC2 = KeyCode.Alpha2;
        private const KeyCode ButtonSelectC3 = KeyCode.Alpha3;
        private const KeyCode ButtonSelectC4 = KeyCode.Alpha4;
        private const KeyCode ButtonSelectC5 = KeyCode.Alpha5;
        private const KeyCode ButtonSelectC6 = KeyCode.Alpha6;
        private const KeyCode ButtonSelectC7 = KeyCode.Alpha7;
        private const KeyCode ButtonSelectC8 = KeyCode.Alpha8;
        private const KeyCode ButtonSelectTrigger = KeyCode.Alpha0;
        // used for changing function buttonSelectCN, when shift pressed
        // the channel attached as trigger's source
        private const KeyCode ButtonSelectModifier1 = KeyCode.LeftShift;
        private const KeyCode ButtonSelectModifier2 = KeyCode.LeftShift;
        
        // (computed) list of all above
        private KeyCode[] buttonSelectKeys;

        /**********************************************************
         * Cursors will be used to control magnitude 
         **********************************************************/
 
        // horizontal position and level
        private const KeyCode ButtonTimePosPlus = KeyCode.RightArrow;
        private const KeyCode ButtonTimePosMinus = KeyCode.LeftArrow;
        private const KeyCode ButtonTimeScalePlus = KeyCode.Equals;
        private const KeyCode ButtonTimeScaleMinus = KeyCode.Minus;
        private const KeyCode ButtonLevelPlus = KeyCode.UpArrow;
        private const KeyCode ButtonLevelMinus = KeyCode.DownArrow;
        // channel settings
        private const KeyCode ButtonPosPlus = KeyCode.UpArrow;
        private const KeyCode ButtonPosMinus = KeyCode.DownArrow;
        private const KeyCode ButtonGainPlus = KeyCode.Equals;
        private const KeyCode ButtonGainMinus = KeyCode.Minus;

        /**********************************************************
         * Channel settings
         **********************************************************/

        private const KeyCode ButtonChannelAuto = KeyCode.A;
        private const KeyCode ButtonChannelAcDc = KeyCode.C;
        private const KeyCode ButtonChannelUnplug = KeyCode.U;
        private const KeyCode ButtonChannelView = KeyCode.V;

        /**********************************************************
         * Trigger settings
         **********************************************************/

        private const KeyCode ButtonTriggerMode = KeyCode.M;
        private const KeyCode ButtonTriggerEdge = KeyCode.E;
        private const KeyCode ButtonTriggerPause = KeyCode.Pause;
        private const KeyCode ButtonTriggerForceStart = KeyCode.S;

        /**********************************************************
         * Grid settings
         **********************************************************/
        
        private const KeyCode ButtonGridSettings = KeyCode.G;
        private int gridSetting;
        
        public void Initialize(Oscilloscope osc)
        {
            oscilloscope = osc;
            buttonSelectKeys = new[]
            {
                ButtonSelectC1, ButtonSelectC2, ButtonSelectC3, ButtonSelectC4,
                ButtonSelectC5, ButtonSelectC6, ButtonSelectC7, ButtonSelectC8
            };
            SelectChannel(OscChannel.Name.C1);
            UpdateHelp();
            IsVisible = false;
        }


        void Update()
        {
            // -- Test main button --
            var main = Input.GetKeyDown(ButtonMain);
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
                    if (Input.GetKeyDown(ButtonSelectTrigger))
                    {
                        SelectTriggerChannel();
                    }
                    else
                    {
                        var shift = Input.GetKey(ButtonSelectModifier1) || Input.GetKey(ButtonSelectModifier2);
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
                    if (Input.GetKeyDown(ButtonTriggerPause))
                        trigger.Pause = !trigger.Pause;
                    else if (Input.GetKeyDown(ButtonTriggerForceStart))
                        trigger.ForceTrigger();
                    
                    // -- trigger control --
                    else if (Input.GetKeyDown(ButtonTriggerMode))
                        trigger.Mode = GetNextEnum<OscTrigger.TriggerMode>(trigger.Mode);
                    else if (Input.GetKeyDown(ButtonTriggerEdge))
                        trigger.Edge = GetNextEnum<OscTrigger.TriggerEdge>(trigger.Edge);
                    
                    else if (Input.GetKeyDown(ButtonGridSettings))
                    {
                        gridSetting++;
                        oscilloscope.grid.DrawGrid = (gridSetting & 1) > 0;
                        oscilloscope.grid.DrawRulerY = oscilloscope.grid.DrawRulerX = (gridSetting & 2) > 0;
                    }
                    
                    // the event for selected or trigger channel
                    if (selectedChannel == null)
                    {
                        // Joy pad control
                        if (Input.GetKeyDown(ButtonTimeScalePlus))
                            trigger.SecondsDivisionPlus();
                        else if (Input.GetKeyDown(ButtonTimeScaleMinus))
                            trigger.SecondsDivisionMinus();
                        else if (Input.GetKeyDown(ButtonLevelPlus))
                            trigger.Level += 0.5f;
                        else if (Input.GetKeyDown(ButtonLevelMinus))
                            trigger.Level -= 0.5f;
                        else if (Input.GetKeyDown(ButtonTimePosPlus))
                            trigger.Position += 0.5f;
                        else if (Input.GetKeyDown(ButtonTimePosMinus))
                            trigger.Position -= 0.5f;
                    }
                    else
                    {
                        // ------------------------------------------------------
                        // selected channel 
                        // ------------------------------------------------------
                        var channel = SelectedChannel;
                        
                        if (Input.GetKeyDown(ButtonChannelAuto))
                            channel.AutoGain = !channel.AutoGain;
                        else if (Input.GetKeyDown(ButtonChannelAcDc))
                            channel.Decoupling = !channel.Decoupling;
                        else if (Input.GetKeyDown(ButtonChannelUnplug))
                            channel.Unplug();
                        else if (Input.GetKeyDown(ButtonChannelView))
                            channel.Style = GetNextEnum<OscProbe.Style>(channel.Style);

                        // Joy pad control
                        if (Input.GetKeyDown(ButtonPosPlus))
                            channel.Position += 0.5f;
                        else if (Input.GetKeyDown(ButtonPosMinus))
                            channel.Position -= 0.5f;
                        else if (Input.GetKeyDown(ButtonGainPlus))
                            channel.GainPlus();
                        else if (Input.GetKeyDown(ButtonGainMinus))
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
            get => oscCanvas.enabled;
            set
            {
                oscCanvas.enabled = isInFocus = value;
                oscCanvas.scaleFactor = 1f;
                updateHelp = true;
            }
        }

        // =============================================================================================================
        // Focus control
        // =============================================================================================================

        private bool isInFocus; //< TRUE means inputs tested

        /// <summary>
        ///     When oscilloscope is in focus the oscilloscope controlled by keys.
        ///     In other case the keyboard ignored.
        /// </summary>
        public bool IsInFocus
        {
            get => isInFocus;
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
                    helpText.text = ToFocusHelp;
                }
                else
                {
                    var help = "";
                    if (selectedChannel == null)
                    {
                        // selected trigger channel
                        help += "TRIGGER:\n";
                        help += TriggerHelp;
                    }
                    else
                    {
                        // selected channel
                        help += $"CHANNEL: {selectedChannel.channelName}\n";
                        help += ChannelHelp;
                    }
                    // display this help in any case 
                    help += PersistHelp;
                    helpText.text = help;
                }
            }
        }

        // =============================================================================================================
        // The text messages for help
        // =============================================================================================================

        private const string ToFocusHelp = "` activate keyboard shortcuts\n";
        private const string ChannelHelp = "1,2,..,8 select input\nA auto, C coupling\nU unplug, V view\nUP,DOWN position\n+,- gain\n";
        private const string TriggerHelp = "SHIFT+1,2,..,8 select input\nM mode, E edge\nUP,DOWN level\nLEFT,RIGHT time position\n+,- time scale";
        private const string PersistHelp = "PAUSE pause, S force start\n";
        
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

        private T GetNextEnum<T>(T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new System.ArgumentException($"Argument {typeof(T).FullName} is not an Enum");

            T[] values = (T[]) System.Enum.GetValues(src.GetType());
            var j = System.Array.IndexOf<T>(values, src) + 1;
            return values.Length == j ? values[0] : values[j];
        }

        private T GetPrevEnum<T>(T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new System.ArgumentException($"Argument {typeof(T).FullName} is not an Enum");

            T[] values = (T[]) System.Enum.GetValues(src.GetType());
            var j = System.Array.IndexOf<T>(values, src) - 1;
            return 0 > j ? values[values.Length - 1] : values[j];
        }
    }
}
