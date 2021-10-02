using System;
using System.Drawing;
using System.Windows.Forms;

namespace NoSleep
{
    class TrayIcon : ApplicationContext
    {
        /// <summary>
        /// Interval between timer ticks (in ms) to refresh Windows idle timers. Shouldn't be too small to avoid resources consumption. Must be less then Windows screensaver/sleep timer.
        /// Default = 10 000 ms (10 seconds).
        /// </summary>
        const int RefreshInterval = 10000;
        /// <summary>
        /// ExecutionMode defines how blocking is made. See details at https://msdn.microsoft.com/en-us/library/aa373208.aspx?f=255&MSPPError=-2147217396
        /// </summary>
        const EXECUTION_STATE ExecutionMode = EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_AWAYMODE_REQUIRED;

        // PRIVATE VARIABLES
        private NotifyIcon _TrayIcon;
        private Timer _RefreshTimer;

        // CONSTRUCTOR
        public TrayIcon()
        {
            // Initialize application
            Application.ApplicationExit += this.OnApplicationExit;
            InitializeComponent();
            _TrayIcon.Visible = true;

            // Set timer to tick to refresh idle timers
            _RefreshTimer = new Timer() { Interval = RefreshInterval, Enabled = true };
            _RefreshTimer.Tick += _RefreshTimer_Tick;
        }

        private void InitializeComponent()
        {
            // Initialize Tray icon
            _TrayIcon = new NotifyIcon();
            _TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
            _TrayIcon.BalloonTipText = "A tiny C# application to prevent windows screensaver/sleep.";
            _TrayIcon.BalloonTipTitle = "NoSleep";
            _TrayIcon.Text = "NoSleep";
            _TrayIcon.Icon = Properties.Resources.TrayIcon;
            _TrayIcon.DoubleClick += TrayIcon_DoubleClick;

            //Initialize context Menu
            TrayMenuContext();
            //// Initialize Close menu item for context menu
            //ToolStripMenuItem _CloseMenuItem = new ToolStripMenuItem() { Text = "Close" };
            //_CloseMenuItem.Click += this.CloseMenuItem_Click;

            //// Initialize context menu
            //_TrayIcon.ContextMenuStrip = new ContextMenuStrip();
            //_TrayIcon.ContextMenuStrip.Items.Add(_CloseMenuItem);
        }

        private void TrayMenuContext()
        {
            _TrayIcon.ContextMenuStrip = new ContextMenuStrip();
            _TrayIcon.ContextMenuStrip.Items.Add("Keep Running", null, this.StopAfterTime);
            _TrayIcon.ContextMenuStrip.Items.Add("Stop After 10 Seconds", null, this.StopAfterTime);
            _TrayIcon.ContextMenuStrip.Items.Add("Stop After 30 Minutes", null, this.StopAfterTime);
            _TrayIcon.ContextMenuStrip.Items.Add("Stop After 1 hour", null, this.StopAfterTime);
            _TrayIcon.ContextMenuStrip.Items.Add("Stop After 2 hours", null, this.StopAfterTime);
            _TrayIcon.ContextMenuStrip.Items.Add("Stop After 4 hours", null, this.StopAfterTime);
            _TrayIcon.ContextMenu.MenuItems.Add("-");
            _TrayIcon.ContextMenuStrip.Items.Add("Close", null, this.CloseMenuItem_Click);
        }

        private void StopAfterTime(object sender, EventArgs e)
        {
            long minute = 1000 * 60;
            long intervalToStopAfter = 0;
            
            switch (sender.ToString())
            {
                case "Keep Running": intervalToStopAfter = 0; break;
                case "Stop After 10 Seconds": intervalToStopAfter = 1000 * 10; break;
                case "Stop After 30 Minutes": intervalToStopAfter = minute * 30; break;
                case "Stop After 1 hour": intervalToStopAfter = minute * 60; break;
                case "Stop After 2 hours": intervalToStopAfter = minute * 120; break;
                case "Stop After 4 hours": intervalToStopAfter = minute * 240; break;
                default: intervalToStopAfter = minute * 240; break;
            }
            StopTimerAfterCertainInterval(intervalToStopAfter);
        }

        private void StopTimerAfterCertainInterval(long intervalToStopAfter)
        {
            _RefreshTimer.Stop();
            elapsedTime = -1;
            totalTime = intervalToStopAfter;
            _RefreshTimer.Start();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            // Clean up things on exit
            _TrayIcon.Visible = false;
            _RefreshTimer.Enabled = false;
            // Clean up continuous state, if required
            if(ExecutionMode.HasFlag(EXECUTION_STATE.ES_CONTINUOUS)) WinU.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e) { _TrayIcon.ShowBalloonTip(10000); }
        private void CloseMenuItem_Click(object sender, EventArgs e) { Application.Exit(); }

        long elapsedTime = -1;
        long totalTime = 60*1000*120; //default is 2hrs
        private void _RefreshTimer_Tick(object sender, EventArgs e)
        {
            if(totalTime != 0) elapsedTime += _RefreshTimer.Interval;
            if (elapsedTime >= totalTime)
            {
                _RefreshTimer.Stop();
                _TrayIcon.ShowBalloonTip(2000, "Stay Awake Timer Stopped", "Your Stay awake timer has stopped", ToolTipIcon.Info);
            }
            else
            {
                WinU.SetThreadExecutionState(ExecutionMode);
            }
        }
    }
}