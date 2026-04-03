using System.Drawing;
using System.Windows.Forms;

namespace CS2Achievements
{
	public class AchievementPopup : Form
	{
		protected override bool ShowWithoutActivation => true;

		protected override CreateParams CreateParams {
			get {
				var cp = base.CreateParams;
				cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
				cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW
				return cp;
			}
		}

		private System.Windows.Forms.Timer closeTimer;

		public AchievementPopup(string title, string description, Image? icon = null) {
			DoubleBuffered = true;
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			FormBorderStyle = FormBorderStyle.None;
			StartPosition = FormStartPosition.Manual;
			ShowInTaskbar = false;
			TopMost = true;
			BackColor = Color.FromArgb(32, 32, 32);
			Width = 350;
			Height = 100;

			var screen = Screen.PrimaryScreen!.WorkingArea;
			Location = new Point(screen.Right - Width - 10, screen.Bottom - Height - 10);

			if (icon != null) {
				var pic = new PictureBox {
					Image = icon,
					SizeMode = PictureBoxSizeMode.Zoom,
					Location = new Point(10, 10),
					Size = new Size(80, 80)
				};
				Controls.Add(pic);
			}

			Controls.Add(new Label {
				Text = title,
				Font = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold),
				Location = new Point(icon != null ? 100 : 10, 10),
				AutoSize = true,
				ForeColor = Color.Gold,
				BackColor = Color.FromArgb(32, 32, 32)
			});

			Controls.Add(new Label {
				Text = description,
				Font = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Regular),
				Location = new Point(icon != null ? 100 : 10, 40),
				Size = new Size(Width - (icon != null ? 110 : 20), 50),
				ForeColor = Color.White,
				BackColor = Color.FromArgb(32, 32, 32)
			});

			closeTimer = new System.Windows.Forms.Timer { Interval = 3500 };
			closeTimer.Tick += (s, e) => {
				closeTimer.Stop();
				closeTimer.Dispose();
				if (!IsDisposed) Close();
			};
			closeTimer.Start();
		}
	}

	public static class PopupStack
	{
		private static readonly List<AchievementPopup> popups = new();
		private static readonly int popupSpacing = 10;
		private static SynchronizationContext? _syncContext;
		private static Thread? _uiThread;

		private static void EnsureUIThread() {
			if (_uiThread != null && _uiThread.IsAlive) return;
			var ready = new System.Threading.ManualResetEventSlim(false);
			_uiThread = new Thread(() => {
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				var syncCtx = new WindowsFormsSynchronizationContext();
				SynchronizationContext.SetSynchronizationContext(syncCtx);
				_syncContext = syncCtx;
				ready.Set();
				Application.Run(new ApplicationContext());
			});
			_uiThread.SetApartmentState(ApartmentState.STA);
			_uiThread.IsBackground = true;
			_uiThread.Start();
			ready.Wait();
		}

		public static void Show(string title, string description, Image? icon = null) {
			EnsureUIThread();
			_syncContext!.Post(_ => {
				var popup = new AchievementPopup(title, description, icon);
				popup.FormClosed += (s, e) => { popups.Remove(popup); UpdatePositions(); };
				popups.Add(popup);
				UpdatePositions();
				popup.Show();
			}, null);
		}

		private static void UpdatePositions() {
			var screen = Screen.PrimaryScreen!.WorkingArea;
			int y = screen.Bottom - 10;
			for (int i = popups.Count - 1; i >= 0; i--) {
				var p = popups[i];
				y -= p.Height;
				p.Location = new Point(screen.Right - p.Width - 10, y);
				y -= popupSpacing;
			}
		}
	}
}
