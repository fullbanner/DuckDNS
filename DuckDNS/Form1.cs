﻿using DuckDNS.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace DuckDNS
{
    public partial class Form1 : Form
    {
        private DDns ddns = new DDns();
        private int intervalMS;
        private bool allowshowdisplay = false;
        private bool canClose = false;
        private Icon icoTray = Resources.tray;
        private Icon icoTrayC = Resources.tray_checking;

        public Form1()
        {
            InitializeComponent();
            notifyIcon.Icon = Icon;
            //Top = Screen.PrimaryScreen.WorkingArea.Bottom - (Height + 200);
            //Left = Screen.PrimaryScreen.WorkingArea.Right - (Width + 25);
            ddns.Load();
            tbDomain.Text = ddns.Domain;
            tbToken.Text = ddns.Token;
            cbInterval.Text = ddns.Interval;
            ParseInterval();
            RefreshTimer();
            notifyIcon.Icon = icoTray;
            allowshowdisplay = tbDomain.Text.Length == 0 || tbToken.Text.Length == 0;
            if (!allowshowdisplay)
                UpdateDNS();
        }

        protected override void SetVisibleCore(bool value)
        {

            if (!allowshowdisplay)
            {
                allowshowdisplay = true;
                if (!IsHandleCreated && value)
                    CreateHandle();
            }
            else
                base.SetVisibleCore(value);
        }

        private void UpdateDNS()
        {
            try
            {
                notifyIcon.Icon = icoTrayC;
                bool update = ddns.Update();
                lblInfo.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " (" + (update ? "OK" : "FAILED") + ")";
                if (!update)
                {
                    MessageBox.Show("Error updating Duck DNS domain", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Show();
                }
            }
            finally
            {
                notifyIcon.Icon = icoTray;
            }
        }

        private void ParseInterval()
        {
            string istr = cbInterval.Text.ToLower();
            int iint=0;
            bool error=false;

            if (istr.Length==0 || !int.TryParse(istr.Substring(0, istr.Length - 1), out iint))
                error = true;
            else
            {
                switch (istr[istr.Length - 1])
                {
                    case 's':
                        intervalMS = iint * 1000;
                        break;
                    case 'm':
                        intervalMS = iint * 60000;
                        break;
                    case 'h':
                        intervalMS = iint * 3600000;
                        break;
                    case 'd':
                        intervalMS = iint * 86400000;
                        break;
                    default:
                        error = true;
                        break;
                }
            }
            cbInterval.BackColor = error ? Color.LightPink : SystemColors.Window;
        }

        private void RefreshTimer()
        {
            timer.Enabled = false;
            timer.Interval = intervalMS;
            timer.Enabled = true;
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            ddns.Domain = tbDomain.Text;
            ddns.Token = tbToken.Text;
            ddns.Interval = cbInterval.Text;
            ddns.Save();
            Hide();
            UpdateDNS();
            RefreshTimer();
        }

        private void cbInterval_TextChanged(object sender, EventArgs e)
        {
            ParseInterval();
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !canClose)
            {
                e.Cancel = true;
                Hide();
                // Reset values (discard)
                tbDomain.Text = ddns.Domain;
                tbToken.Text = ddns.Token;
                cbInterval.Text = ddns.Interval;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            UpdateDNS();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            canClose = true;
            Close();
        }

        private void updateNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateDNS();
        }

        private void installStartupShortcutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string linkPath = Windows.GetStartupPath() + Path.DirectorySeparatorChar + "DuckDNS.lnk";
            WShellLink.CreateLink(linkPath,"Duck DNS Updater",Assembly.GetExecutingAssembly().Location);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FAbout.Execute();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            icoTray.Dispose();
            icoTrayC.Dispose();
        }
    }
}
