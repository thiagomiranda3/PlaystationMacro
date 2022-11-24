// PlaystationMacro (File: Classes/MacroPlayer.cs)
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
    public class MacroPlayer
    {
        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsRecording { get; private set; }
        public bool IsLooping { get; private set; }
        public int CurrentTick { get; private set; }

        public MacroPlayer()
        {
            IsPlaying = false;
            IsPaused = false;
            IsRecording = false;
            CurrentTick = 0;
            Sequence = new List<byte[]>();
        }

        #region Properties
        public void Loop(bool loop)
        {
            IsLooping = loop;
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

        public void Record()
        {
            IsRecording = !IsRecording;
        }

        public void Stop()
        {
            IsPlaying = false;
            IsPaused = false;
            CurrentTick = 0;
        }

        public void Clear()
        {
            Sequence = new List<byte[]>();
            CurrentTick = 0;
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
                }
            }
        }
        #endregion

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
                    if (!IsLooping && !IsRecording)
                    {
                        Stop();
                    }
                }
            }
        }
    }
}
