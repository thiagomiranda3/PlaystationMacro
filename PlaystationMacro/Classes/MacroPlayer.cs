﻿// PlaystationMacro (File: Classes/MacroPlayer.cs)
//
// Copyright (c) 2018 Komefai
//
// Visit http://komefai.com for more information
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using PS4RemotePlayInterceptor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PlaystationMacro.Classes
{
    public class MacroPlayer : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Properties
        private bool m_Loop = false;
        public bool Loop
        {
            get { return m_Loop; }
            set
            {
                if (value != m_Loop)
                {
                    m_Loop = value;
                    NotifyPropertyChanged("Loop");
                }
            }
        }

        private bool m_RecordShortcut = false;
        public bool RecordShortcut
        {
            get { return m_RecordShortcut; }
            set
            {
                if (value != m_RecordShortcut)
                {
                    m_RecordShortcut = value;
                    NotifyPropertyChanged("RecordShortcut");
                }
            }
        }

        private bool m_IsPlaying = false;
        public bool IsPlaying
        {
            get { return m_IsPlaying; }
            private set
            {
                if (value != m_IsPlaying)
                {
                    m_IsPlaying = value;
                    NotifyPropertyChanged("IsPlaying");
                }
            }
        }

        private bool m_IsPaused = false;
        public bool IsPaused
        {
            get { return m_IsPaused; }
            private set
            {
                if (value != m_IsPaused)
                {
                    m_IsPaused = value;
                    NotifyPropertyChanged("IsPaused");
                }
            }
        }

        private bool m_IsRecording = false;
        public bool IsRecording
        {
            get { return m_IsRecording; }
            private set
            {
                if (value != m_IsRecording)
                {
                    m_IsRecording = value;
                    NotifyPropertyChanged("IsRecording");
                }
            }
        }

        private int m_CurrentTick = 0;
        public int CurrentTick
        {
            get { return m_CurrentTick; }
            private set
            {
                if (value != m_CurrentTick)
                {
                    m_CurrentTick = value;
                    //NotifyPropertyChanged("CurrentTick");
                }
            }
        }

        private List<byte[]> m_Sequence = new List<byte[]>();
        public List<byte[]> Sequence
        {
            get { return m_Sequence; }
            set
            {
                if (value != m_Sequence)
                {
                    m_Sequence = value;
                    //NotifyPropertyChanged("Sequence");
                }
            }
        }
        #endregion

        private bool m_RecordShortcutDown = false;

        /* Constructor */
        public MacroPlayer()
        {
            Loop = false;
            IsPlaying = false;
            IsPaused = false;
            IsRecording = false;
            CurrentTick = 0;
            Sequence = new List<byte[]>();
        }


        public void Play()
        {
            IsPlaying = true;
            IsPaused = false;
        }

        public void Pause()
        {
            IsPlaying = true;
            IsPaused = true;
        }

        public void Stop()
        {
            IsPlaying = false;
            IsPaused = false;
            CurrentTick = 0;
        }

        public void Record()
        {
            IsRecording = !IsRecording;
        }

        public void Clear()
        {
            Sequence = new List<byte[]>();
            CurrentTick = 0;
        }

        public void LoadFile(string path)
        {
            Sequence = DualShockState.Deserialize(path);
        }

        public void SaveFile(string path)
        {
            DualShockState.Serialize(path, Sequence);
        }

        public void OnReceiveData(ref byte[] state)
        {
            // Playback
            if (IsPlaying && !IsPaused)
            {
                // Recording
                if (IsRecording)
                {
                    Sequence.Add(state);
                }
                // Playing
                else
                {
                    byte[] newState = Sequence[CurrentTick];

                    if (newState != null)
                    {
                        // Update the state
                        state = newState;
                    }
                }

                // Increment tick
                CurrentTick++;

                // Reset tick if out of bounds
                if (CurrentTick >= Sequence.Count)
                {
                    CurrentTick = 0;

                    // Stop if looping is disabled
                    if (!Loop && !IsRecording)
                    {
                        Stop();
                    }
                }
            }
        }
    }
}
