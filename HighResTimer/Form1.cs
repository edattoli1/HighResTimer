using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime;

namespace HighResTimer
{
    public partial class Form1 : Form
    {
        private int mTimerId;
        private TimerEventHandler mHandler;  // NOTE: declare at class scope so garbage collector doesn't release it!!!
        private int mTestTick;
        private DateTime mTestStart;

        public Form1()
        {
            InitializeComponent();
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            label1 = new Label();
            label1.Location = new Point(10, 5);
            label2 = new Label();
            label2.Location = new Point(10, 30);
            this.Controls.Add(label1);
            this.Controls.Add(label2);
        }
        protected override void OnLoad(EventArgs e) {
              timeBeginPeriod(1);
              mHandler = new TimerEventHandler(TimerCallback);
              mTimerId = timeSetEvent(1, 0, mHandler, IntPtr.Zero, EVENT_TYPE);
              mTestStart = DateTime.Now;
              mTestTick = 0;
            }
            protected override void OnFormClosing(FormClosingEventArgs e) {
              mTimerId = 0;
              int err = timeKillEvent(mTimerId);
              timeEndPeriod(1);
              // Ensure callbacks are drained
              System.Threading.Thread.Sleep(100);
            }
            private delegate void TestEventHandler(int tick, TimeSpan span);

            private void TimerCallback(int id, int msg, IntPtr user, int dw1, int dw2) {
              mTestTick += 1;
              if ((mTestTick % 200) == 0 && mTimerId != 0)
                this.BeginInvoke(new TestEventHandler(ShowTick), mTestTick, DateTime.Now - mTestStart);
            }
            private void ShowTick(int msec, TimeSpan span) {
              label1.Text = msec.ToString();
              label2.Text = span.TotalMilliseconds.ToString();
            }

            // P/Invoke declarations
            private delegate void TimerEventHandler(int id, int msg, IntPtr user, int dw1, int dw2);
            private const int TIME_PERIODIC = 1;
            private const int EVENT_TYPE = TIME_PERIODIC;// + 0x100;  // TIME_KILL_SYNCHRONOUS causes a hang ?!
            [DllImport("winmm.dll")]
            private static extern int timeSetEvent(int delay, int resolution, TimerEventHandler handler, IntPtr user, int eventType);
            [DllImport("winmm.dll")]
            private static extern int timeKillEvent(int id);
            [DllImport("winmm.dll")]
            private static extern int timeBeginPeriod(int msec);
            [DllImport("winmm.dll")]
            private static extern int timeEndPeriod(int msec);
          }
}
