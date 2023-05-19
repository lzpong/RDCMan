using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace RdcMan
{
	internal class LongRunningActionForm : Form
	{
		//private const int PopupDelayInSeconds = 2;

		//private const int UpdateFrequencyInMilliseconds = 25;

		private readonly Label _statusLabel;

		private double _lastUpdateInMilliseconds;

		private DateTime _startTime;

		public bool Done { get; protected set; }

		public static LongRunningActionForm Instance { get; private set; }

		private LongRunningActionForm()
		{
			SuspendLayout();
			base.AutoScaleDimensions = new SizeF(96f, 96f);
			base.AutoScaleMode = AutoScaleMode.Dpi;
			ProgressBar progressBar = new ProgressBar
			{
				Location = FormTools.NewLocation(0, 0),
				Size = new Size(450, FormTools.ControlHeight),
				Style = ProgressBarStyle.Marquee
			};
			_statusLabel = new Label
			{
				AutoEllipsis = true,
				Location = FormTools.NewLocation(0, 1),
				Size = progressBar.Size
			};
			base.ClientSize = new Size(466, _statusLabel.Bottom + FormTools.BottomMargin);
			base.StartPosition = FormStartPosition.Manual;
			base.Location = new Point(Program.TheForm.Left + (Program.TheForm.Width - base.Width) / 2, Program.TheForm.Top + (Program.TheForm.Height - base.Height) / 2);
			base.ControlBox = false;
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.Controls.AddRange(new Control[2] { progressBar, _statusLabel });
			base.FormClosing += FormClosingHandler;
			base.Shown += ShownHandler;
			this.ScaleAndLayout();
		}

		public static void PerformOperation(string title, bool showImmediately, Action action)
		{
			LongRunningActionForm form = new LongRunningActionForm
			{
				Text = title
			};
			try
			{
				Program.TheForm.Enabled = false;
				form._startTime = DateTime.Now;
				if (showImmediately)
				{
					form.MakeVisible();
				}
				Instance = form;
				action();
			}
			finally
			{
				if (form.Visible)
				{
					form.Done = true;
					form.Invoke((MethodInvoker)delegate
					{
						form.Close();
					});
				}
				Instance = null;
				Program.TheForm.Enabled = true;
			}
		}

		public void UpdateStatus(string statusText)
		{
			TimeSpan timeSpan = DateTime.Now.Subtract(_startTime);
			if (!base.Visible && timeSpan.TotalSeconds >= 2.0)
			{
				MakeVisible();
			}
			if (base.Visible && timeSpan.TotalMilliseconds - _lastUpdateInMilliseconds >= 25.0)
			{
				_lastUpdateInMilliseconds = timeSpan.TotalMilliseconds;
				Invoke((MethodInvoker)delegate
				{
					_statusLabel.Text = statusText;
				});
			}
		}

		private void MakeVisible()
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				Application.Run(new ApplicationContext(this));
			});
			SpinWait.SpinUntil(() => base.Visible);
		}

		private void ShownHandler(object sender, EventArgs e)
		{
			BringToFront();
		}

		private void FormClosingHandler(object sender, FormClosingEventArgs e)
		{
			if (!Done && e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;
			}
		}
	}
}
