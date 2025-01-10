using Microsoft.Win32;
using RunAtTime.Module;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace RunAtTime
{
    public partial class Form1 : Form
    {
        private static string path = Directory.GetCurrentDirectory();
        private static string AppName = "SubscriptionRecurring";
        private static string Noti = "";
        public static Form1 formMain; 
        public Form1()
        {
            InitializeComponent();
            formMain = this;
        }
        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
        }
        private void btn_Exit_Click(object sender, EventArgs e)
        {
            deleteWithWindow();
            this.Close();
        }
        private void chk_Stop_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.ShowBalloonTip(1000);
            }
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            Hide();
            notifyIcon1.Visible = true;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            runLog();
            deleteWithWindow();
            startWithWindow();
            HideForm();
            Form1_Resize(sender, e);
            SettingModule.InitMISetting();
            doInterval();
            //doIntervalTwilio();
        }
        private void runLog()
        {
            LogModule.flushLog();
            LogModule.flushDay();

        }
        private void HideForm()
        {
            System.Threading.Thread.Sleep(1000);
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }
        private void startWithWindow()
        {
            rkApp.SetValue(AppName, System.Windows.Forms.Application.ExecutablePath.ToString());
        }
        private void deleteWithWindow()
        {
            rkApp.DeleteValue(AppName, false);
        }
        private async void doInterval()
        {

            var timeNow = DateTime.UtcNow;
            var timeConfig = (SettingModule.TimeRun ?? "00:00").Split(':');
            var timeRun = new DateTime(timeNow.Year, timeNow.Month, timeNow.Day, int.Parse(timeConfig[0]), int.Parse(timeConfig[1]), int.Parse(timeConfig[2]));
            var scheduler = (timeRun - timeNow).Duration();
            if (timeNow > timeRun)
            {
                timeRun = timeRun.AddDays(1);
                scheduler = (timeNow - timeRun).Duration();
            }
            LogModule.AddInfo($"Execute recurring service [{timeNow}]-[{timeRun}], next run affter {scheduler.TotalMinutes} minutes");
            int seccondDlay = (int)scheduler.TotalSeconds;
            await Task.Delay(seccondDlay * 1000);

            foreach(var item in SettingModule.listRunApi)
            {
                Interval.Set(async () =>
                {
                    await callBrower(item.Domain);
                }, item.RecurringMinute * 60 * 1000);
            }

        }

       

        private async Task callBrower(string url)
        {
            if (chk_Stop.Checked == false)
            {
                Noti += $"--Running at {DateTime.UtcNow.ToString("MMM dd, yyyy hh:mm:ss")} utc" + Environment.NewLine;
                LogModule.AddInfo($"----Start recurring '{url}' at {DateTime.UtcNow.ToString("MMM dd, yyyy hh:mm:ss")} utc");
                await CallApiModule.CallApiRecurring(url);
                LogModule.AddInfo($"----Complete recurring at {DateTime.UtcNow.ToString("MMM dd, yyyy hh:mm:ss")} utc");
            }
            notiTextBox.Text = Noti;
        }
        private async void btn_Chck_Click(object sender, EventArgs e)
        {
            foreach (var item in SettingModule.listRunApi)
            {
                await callBrower(item.Domain);
            }
            //await callSyncTwilio();
        }
        public void updateNotiTextBox(string text)
        {
            Noti = Noti + "\r\n" + text;
        }
        private void notiTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
