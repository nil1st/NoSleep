using System;
using System.Drawing;
using System.Windows.Forms;

namespace NoSleep
{
    public class TrayIcon : ApplicationContext
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
        private bool _followRDPStatus;
        private NotifyIcon _TrayIcon;
        private Timer _RefreshTimer;
        private const string NOTRUNNING_TEXT = "NoSleep (Not Running)";

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
            _TrayIcon.BalloonTipTitle = NOTRUNNING_TEXT;
            _TrayIcon.Text = NOTRUNNING_TEXT;
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
            //_TrayIcon.ContextMenuStrip.Items.Add("Connect", null, this.StopAfterTime);
            //_TrayIcon.ContextMenuStrip.Items.Add("Disconnect", null, this.StopAfterTime);
            _TrayIcon.ContextMenuStrip.Items.Add("RDP", null, this.StopAfterTime);
            _TrayIcon.ContextMenuStrip.Items.Add("Keep Running", null, this.StopAfterTime);
            _TrayIcon.ContextMenuStrip.Items.Add("Stop After 10 Seconds", null, this.StopAfterTime);
            _TrayIcon.ContextMenuStrip.Items.Add("Stop After 30 Minutes", null, this.StopAfterTime);
            _TrayIcon.ContextMenuStrip.Items.Add("Stop After 1 hour", null, this.StopAfterTime);
            _TrayIcon.ContextMenuStrip.Items.Add("Stop After 2 hours", null, this.StopAfterTime);
            _TrayIcon.ContextMenuStrip.Items.Add("Stop After 4 hours", null, this.StopAfterTime);
            _TrayIcon.ContextMenuStrip.Items.Add("-");
            _TrayIcon.ContextMenuStrip.Items.Add("Close", null, this.CloseMenuItem_Click);
        }

        private void StopAfterTime(object sender, EventArgs e)
        {
            long minute = 1000 * 60;
            long intervalToStopAfter = 0;

            switch (sender.ToString())
            {
                //case "Connect": RDPConnectionStatus(true); break;
                //case "Disconnect": RDPConnectionStatus(false); break;
                case "RDP": _followRDPStatus = true; intervalToStopAfter = 0; _TrayIcon.Text = "NoSleep (RDP: Keep Running till Disconncted)"; break;
                case "Keep Running": _followRDPStatus = false; intervalToStopAfter = 0; _TrayIcon.Text = "NoSleep (Keep Running)"; break;
                case "Stop After 10 Seconds": _followRDPStatus = false; intervalToStopAfter = 1000 * 10; break;
                case "Stop After 30 Minutes": _followRDPStatus = false; intervalToStopAfter = minute * 30; break;
                case "Stop After 1 hour": _followRDPStatus = false; intervalToStopAfter = minute * 60; break;
                case "Stop After 2 hours": _followRDPStatus = false; intervalToStopAfter = minute * 120; break;
                case "Stop After 4 hours": _followRDPStatus = false; intervalToStopAfter = minute * 240; break;
                default: _followRDPStatus = false; intervalToStopAfter = minute * 240; break;
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
            if (ExecutionMode.HasFlag(EXECUTION_STATE.ES_CONTINUOUS)) WinU.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e) { _TrayIcon.ShowBalloonTip(10000); }
        private void CloseMenuItem_Click(object sender, EventArgs e) { Application.Exit(); }

        long elapsedTime = -1;
        long totalTime = 60 * 1000 * 120; //default is 2hrs
        TimeSpan sleepingTimeStart = new TimeSpan(0, 0, 1); //12:00:01 AM
        TimeSpan sleepingTimeEnd = new TimeSpan(5, 0, 0); //5:00:00 AM
        public enum RDPStatus
        {
            Connected,
            Disconncted
        }
        private static RDPStatus lastRDPStatus = RDPStatus.Disconncted;
        public void RDPConnectionStatus(bool isRDP)
        {
            //System.Diagnostics.Debug.WriteLine("IsRDP: " + isRDP);
            //System.Diagnostics.Debug.WriteLine("IsTS: " + System.Windows.Forms.SystemInformation.TerminalServerSession);
            
            if (!_followRDPStatus) return;

            //if ((lastRDPStatus == RDPStatus.Disconncted) && (RDPSession.IsRDP()))
            if ((lastRDPStatus == RDPStatus.Disconncted) && (isRDP))
            {
                _RefreshTimer.Start();
                lastRDPStatus = RDPStatus.Connected;
                _TrayIcon.ShowBalloonTip(60000, "Remote Session Connected", "Remote Session Connected", ToolTipIcon.Info);
            }
            //else if ((lastRDPStatus == RDPStatus.Connected) && (!RDPSession.IsRDP()))
            else if ((lastRDPStatus == RDPStatus.Connected) && (!isRDP))
            {
                lastRDPStatus = RDPStatus.Disconncted;
                _TrayIcon.ShowBalloonTip(60000, "Remote Session Disconnected", "Remote Session Disconnected", ToolTipIcon.Info);
                StopRefreshTimer();
            }
        }

        private void _RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (_followRDPStatus)
            {
                RDPConnectionStatus(RDPSession.IsRDP());
                return;
            }
            if (totalTime != 0) elapsedTime += _RefreshTimer.Interval;
            if (elapsedTime >= totalTime)
            {
                StopRefreshTimer();
            }
            else if ((DateTime.Now.TimeOfDay >= sleepingTimeStart) && (DateTime.Now.TimeOfDay <= sleepingTimeEnd))
            {
                _RefreshTimer.Stop();
                _TrayIcon.Text = NOTRUNNING_TEXT;
                _TrayIcon.ShowBalloonTip(3500, "Go to sleep", "Exiting NoSleep App, its time for you to goto sleep.", ToolTipIcon.Info);
                System.Threading.Thread.Sleep(10000);
                Application.Exit();
            }
            else
            {
                _TrayIcon.Text = $"NoSleep Timer Running (Elapsed {elapsedTime / 60000} of {totalTime / 60000} minutes)";
                WinU.SetThreadExecutionState(ExecutionMode);
            }
        }

        private void StopRefreshTimer()
        {
            _RefreshTimer.Stop();
            _TrayIcon.Text = NOTRUNNING_TEXT;
            _TrayIcon.ShowBalloonTip(2000, "Stay Awake Timer Stopped", "Your Stay awake timer has stopped", ToolTipIcon.Info);
        }
    }
}