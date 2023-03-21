// PlaystationMacro (File: Forms/MainForm.cs)
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

using PlaystationMacro.Classes;
using PS4RemotePlayInterceptor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PlaystationMacro.Forms
{
    enum ControlMode
    {
        Macro
    }

    public partial class MainForm : Form
    {
        private const string CURRENT_TICK_DEFAULT_TEXT = "-";

        private MacroPlayer m_MacroPlayer;

        private ControlMode m_ControlMode;

        private SaveLoadHelper m_SaveLoadHelper;

        /* Constructor */
        public MainForm()
        {
            InitializeComponent();

            // Create macro player
            m_MacroPlayer = new MacroPlayer();
            m_MacroPlayer.Loop = true;
            m_MacroPlayer.PropertyChanged += MacroPlayer_PropertyChanged;

            // Set control mode
            SetControlMode(ControlMode.Macro);

            // Create save/load helper
            m_SaveLoadHelper = new SaveLoadHelper(this, m_MacroPlayer);
            m_SaveLoadHelper.PropertyChanged += SaveLoadHelper_PropertyChanged;

            // Initialize interceptor
            InitInterceptor();
        }

        private void InitInterceptor()
        {
            Interceptor.InjectionMode = InjectionMode.Compatibility;

            // Inject if not bypassed
            if (!Program.Settings.BypassInjection)
            {
                // Attempt to inject into PS4 Remote Play
                try
                {
                    Interceptor.Inject();
                }
                // Injection failed
                catch (InterceptorException ex)
                {
                    // Only handle when PS4 Remote Play is in used by another injection
                    if (ex.InnerException != null && ex.InnerException.Message.Equals("STATUS_INTERNAL_ERROR: Unknown error in injected C++ completion routine. (Code: 15)"))
                    {
                        MessageBox.Show("The process has been injected by another executable. Restart PS4 Remote Play and try again.", "Injection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(-1);
                    }
                    else
                    {
                        MessageBox.Show(string.Format("[{0}] - {1}", ex.GetType(), ex.Message), "Injection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(-1);
                    }
                }
            }
        }

        private void SetControlMode(ControlMode controlMode)
        {
            m_ControlMode = controlMode;

            // Setup callback to interceptor
            Interceptor.Callback = new InterceptionDelegate(m_MacroPlayer.OnReceiveData);

            recordButton.Enabled = true;
            recordToolStripMenuItem.Enabled = true;
            loopToolStripMenuItem.Enabled = true;
            saveToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;
            clearMacroToolStripMenuItem.Enabled = true;
        }

        private void TemporarilySetControlMode(ControlMode controlMode, Action action)
        {
            // Store current control mode and temporarily set it
            ControlMode oldControlMode = m_ControlMode;
            SetControlMode(controlMode);

            // Invoke action
            action?.Invoke();

            // Restore control mode
            SetControlMode(oldControlMode);
        }

        public void LoadMacro(string path)
        {
            SetControlMode(ControlMode.Macro);
            m_MacroPlayer.LoadFile(path);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Load startup file
            if (!string.IsNullOrWhiteSpace(Program.Settings.StartupFile))
                m_SaveLoadHelper.DirectLoad(Program.Settings.StartupFile);
        }

        /* Macro Player */
        #region MacroPlayer_PropertyChanged
        private void UpdateCurrentTick()
        {
            BeginInvoke((MethodInvoker)delegate
            {
                // Invalid sequence
                if (m_MacroPlayer.Sequence == null || m_MacroPlayer.Sequence.Count <= 0)
                {
                    currentTickToolStripStatusLabel.Text = CURRENT_TICK_DEFAULT_TEXT;
                }
                // Valid sequence
                else
                {
                    currentTickToolStripStatusLabel.Text = string.Format("{0}/{1}",
                        m_MacroPlayer.CurrentTick.ToString(),
                        m_MacroPlayer.Sequence.Count.ToString()
                    );
                }
            });
        }

        private void MacroPlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsPlaying":
                    {
                        playButton.ForeColor = m_MacroPlayer.IsPlaying ? Color.Green : DefaultForeColor;
                        UpdateCurrentTick();
                        break;
                    }

                case "IsPaused":
                    {
                        playButton.ForeColor = m_MacroPlayer.IsPaused ? DefaultForeColor : playButton.ForeColor;
                        UpdateCurrentTick();
                        break;
                    }

                case "IsRecording":
                    {
                        recordButton.ForeColor = m_MacroPlayer.IsRecording ? Color.Red : DefaultForeColor;
                        currentTickToolStripStatusLabel.ForeColor = m_MacroPlayer.IsRecording ? Color.Red : DefaultForeColor;
                        UpdateCurrentTick();
                        break;
                    }

                case "CurrentTick":
                    {
                        UpdateCurrentTick();
                        break;
                    }

                case "Sequence":
                    {
                        UpdateCurrentTick();
                        break;
                    }

                case "Loop":
                    {
                        loopCheckBox.Checked = m_MacroPlayer.Loop;
                        loopToolStripMenuItem.Checked = m_MacroPlayer.Loop;
                        break;
                    }
            }
        }
        #endregion

        /* Save/Load Helper */
        #region SaveLoadHelper_PropertyChanged
        private void SaveLoadHelper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentFile")
            {
                if (m_SaveLoadHelper.CurrentFile == null)
                {
                    fileNameToolStripStatusLabel.Text = SaveLoadHelper.DEFAULT_FILE_NAME;
                    currentTickToolStripStatusLabel.Text = CURRENT_TICK_DEFAULT_TEXT;
                }
                else
                {
                    fileNameToolStripStatusLabel.Text = System.IO.Path.GetFileName(m_SaveLoadHelper.CurrentFile);
                }
            }
        }
        #endregion

        /* Playback buttons methods */
        #region Playback Buttons

        private void playButton_Click(object sender, EventArgs e)
        {
            m_MacroPlayer.Play();
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            m_MacroPlayer.Pause();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            m_MacroPlayer.Stop();
        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            m_MacroPlayer.Record();
        }

        private void loopCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            m_MacroPlayer.Loop = loopCheckBox.Checked;
        }
        #endregion

        /* Menu strip methods */
        #region Menu Strip

        #region File
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetControlMode(ControlMode.Macro);
            m_MacroPlayer.Clear();
            m_SaveLoadHelper.ClearCurrentFile();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_SaveLoadHelper.Load();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_SaveLoadHelper.Save();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_SaveLoadHelper.SaveAs();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region Edit
        private void clearMacroToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_MacroPlayer.Clear();
        }
        #endregion

        #region Playback
        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_MacroPlayer.Play();
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_MacroPlayer.Pause();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_MacroPlayer.Stop();
        }

        private void recordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_MacroPlayer.Record();
        }

        private void loopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_MacroPlayer.Loop = !loopToolStripMenuItem.Checked;
        }
        #endregion

        #region Help
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var aboutForm = new AboutForm();
            aboutForm.ShowDialog(this);
        }
        #endregion

        #endregion
    }
}
