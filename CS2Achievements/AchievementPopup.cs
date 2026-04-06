using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CS2Achievements
{
	public class AchievementPopup : Form
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int TargetY { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsLeaving { get; set; }

		protected override bool ShowWithoutActivation => true;

		protected override CreateParams CreateParams {
			get {
				var cp = base.CreateParams;
				cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
				cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW
				return cp;
			}
		}

		private readonly System.Windows.Forms.Timer closeTimer;
		public string Title => _title;
		private readonly string _title;
		public string AchievementName { get; }
		private string _description;
		private readonly Image? _icon;
		private int _progress;
		private int _maxProgress;
		private float _scale;

		private static readonly Color BgColor = Color.FromArgb(22, 25, 31);
		private static readonly Color IconBgColor = Color.FromArgb(13, 15, 19);
		private static readonly Color TitleColor = Color.FromArgb(220, 220, 215);
		private static readonly Color DescColor = Color.FromArgb(160, 150, 120);
		private static readonly Color BarBgColor = Color.FromArgb(42, 46, 54);
		private static readonly Color BarFillColor = Color.FromArgb(195, 158, 35);
		private static readonly Color BorderColor = Color.FromArgb(52, 56, 65);

		public AchievementPopup(string achievementName, string title, string description, Image? icon = null, int progress = 0, int maxProgress = 0) {
			AchievementName = achievementName;
			_title = title;
			_description = description;
			_icon = icon;
			_progress = progress;
			_maxProgress = maxProgress;
			_scale = Math.Clamp(AppConfig.Instance.PopupScale, 0.5f, 3.0f);

			DoubleBuffered = true;
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
			FormBorderStyle = FormBorderStyle.None;
			StartPosition = FormStartPosition.Manual;
			ShowInTaskbar = false;
			TopMost = true;
			BackColor = BgColor;
			Width = (int)(360 * _scale);
			Height = (int)(90 * _scale);

			var cfg = AppConfig.Instance;
			var bounds = PopupStack.GetCS2Bounds();
			bool isRight = cfg.PopupPosition == PopupPosition.BottomRight || cfg.PopupPosition == PopupPosition.TopRight;
			bool isBottom = cfg.PopupPosition == PopupPosition.BottomRight || cfg.PopupPosition == PopupPosition.BottomLeft;
			int x = isRight ? bounds.Right - Width - 10 : bounds.Left + 10;
			int spawnY = isBottom ? bounds.Bottom : bounds.Top - Height;
			TargetY = spawnY;
			Location = new Point(x, spawnY);

			closeTimer = new System.Windows.Forms.Timer { Interval = 4000 };
			closeTimer.Tick += (s, e) => {
				closeTimer.Stop();
				closeTimer.Dispose();
				if (!IsDisposed) PopupStack.StartLeave(this);
			};
			closeTimer.Start();
		}

		public void Rescale(float scale) {
			_scale = scale;
			Width = (int)(360 * _scale);
			Height = (int)(90 * _scale);
			Invalidate();
		}

		public void Update(string description, int progress, int maxProgress) {
			_description = description;
			_progress = progress;
			_maxProgress = maxProgress;
			closeTimer.Stop();
			closeTimer.Start();
			IsLeaving = false;
			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e) {
			var g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

			g.Clear(BgColor);

			int iconAreaW = (int)(80 * _scale);
			int pad = (int)(7 * _scale);
			g.FillRectangle(new SolidBrush(IconBgColor), 0, 0, iconAreaW, Height);
			if (_icon != null) {
				g.DrawImage(_icon, new Rectangle(pad, pad, iconAreaW - pad * 2, Height - pad * 2));
			}

			g.DrawLine(new Pen(BorderColor, 1), iconAreaW, 0, iconAreaW, Height);

			int cx = iconAreaW + (int)(12 * _scale);
			int cw = Width - cx - (int)(10 * _scale);

			using var titleFont = new Font("Segoe UI", 10.5f * _scale, FontStyle.Bold, GraphicsUnit.Point);
			g.DrawString(_title, titleFont, new SolidBrush(TitleColor), cx, (int)(10 * _scale));

			using var descFont = new Font("Segoe UI", 8.5f * _scale, FontStyle.Regular, GraphicsUnit.Point);
			string descText = _maxProgress > 0 ? $"{_description}  ({_progress}/{_maxProgress})" : _description;
			int descBottomReserve = _maxProgress > 0 ? (int)(20 * _scale) : (int)(10 * _scale);
			var descRect = new RectangleF(cx, (int)(34 * _scale), cw, Height - (int)(34 * _scale) - descBottomReserve);
			var fmt = new StringFormat { Trimming = StringTrimming.Word };
			g.DrawString(descText, descFont, new SolidBrush(DescColor), descRect, fmt);

			if (_maxProgress > 0) {
				int barH = Math.Max(1, (int)(4 * _scale));
				int barY = Height - (int)(14 * _scale);
				float fraction = Math.Clamp((float)_progress / _maxProgress, 0f, 1f);
				g.FillRectangle(new SolidBrush(BarBgColor), cx, barY, cw, barH);
				if (fraction > 0)
					g.FillRectangle(new SolidBrush(BarFillColor), cx, barY, (int)(cw * fraction), barH);
			}

			g.DrawRectangle(new Pen(BorderColor, 1), 0, 0, Width - 1, Height - 1);
		}
	}

	public static class PopupStack
	{
		private static readonly List<AchievementPopup> popups = new();
		private static readonly int popupSpacing = 10;
		private static SynchronizationContext? _syncContext;
		private static Thread? _uiThread;
		private static System.Windows.Forms.Timer? _animationTimer;
		private const float LerpSpeed = 0.18f;

		[DllImport("user32.dll")]
		private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT { public int Left, Top, Right, Bottom; }

		public static Rectangle GetCS2Bounds() {
			var process = Process.GetProcessesByName("cs2").FirstOrDefault();
			if (process != null) {
				var hwnd = process.MainWindowHandle;
				if (hwnd != IntPtr.Zero && GetWindowRect(hwnd, out RECT r))
					return Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
			}
			return Screen.PrimaryScreen!.WorkingArea;
		}

		[DllImport("user32.dll")]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
		private const uint SWP_NOMOVE = 0x0002;
		private const uint SWP_NOSIZE = 0x0001;
		private const uint SWP_NOACTIVATE = 0x0010;
		private static readonly IntPtr HWND_TOPMOST = new(-1);

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

		private static void EnsureAnimationTimer() {
			if (_animationTimer != null) return;
			_animationTimer = new System.Windows.Forms.Timer { Interval = 16 };
			_animationTimer.Tick += AnimationTick;
			_animationTimer.Start();
		}

		private static void AnimationTick(object? sender, EventArgs e) {
			var bounds = GetCS2Bounds();
			var pos = AppConfig.Instance.PopupPosition;
			bool isRight = pos == PopupPosition.BottomRight || pos == PopupPosition.TopRight;
			bool isBottom = pos == PopupPosition.BottomRight || pos == PopupPosition.BottomLeft;

			foreach (var popup in popups.ToList()) {
				if (popup.IsDisposed) continue;

				int targetX = isRight ? bounds.Right - popup.Width - 10 : bounds.Left + 10;
				if (popup.Left != targetX)
					popup.Left = targetX;

				int current = popup.Top;
				int target = popup.TargetY;
				int next = Math.Abs(current - target) > 1
					? (int)Math.Round(current + (target - current) * LerpSpeed)
					: target;
				popup.Top = next;

				int clipTop = Math.Max(0, bounds.Top - next);
				int clipBottom = Math.Max(0, Math.Min(popup.Height, bounds.Bottom - next));
				int clipLeft = Math.Max(0, bounds.Left - popup.Left);
				int clipRight = Math.Max(0, Math.Min(popup.Width, bounds.Right - popup.Left));
				if (clipBottom > clipTop && clipRight > clipLeft)
					popup.Region = new Region(new Rectangle(clipLeft, clipTop, clipRight - clipLeft, clipBottom - clipTop));
				else
					popup.Region = new Region(Rectangle.Empty);

				if (isBottom && popup.IsLeaving && next >= bounds.Bottom + popup.Height) {
					popups.Remove(popup);
					popup.Close();
					UpdatePositions();
				}
				else if (!isBottom && popup.IsLeaving && next <= bounds.Top - popup.Height) {
					popups.Remove(popup);
					popup.Close();
					UpdatePositions();
				}
			}
			UpdateZOrder();
		}

		private static void UpdateZOrder() {
			IntPtr insertAfter = HWND_TOPMOST;
			var staying = popups.Where(p => !p.IsDisposed && !p.IsLeaving).ToList();
			var leaving = popups.Where(p => !p.IsDisposed && p.IsLeaving).ToList();
			foreach (var p in staying) {
				SetWindowPos(p.Handle, insertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
				insertAfter = p.Handle;
			}
			foreach (var p in leaving) {
				SetWindowPos(p.Handle, insertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
				insertAfter = p.Handle;
			}
		}

		public static void StartLeave(AchievementPopup popup) {
			if (popup.IsDisposed || popup.IsLeaving) return;
			popup.IsLeaving = true;
			var pos = AppConfig.Instance.PopupPosition;
			bool isBottom = pos == PopupPosition.BottomRight || pos == PopupPosition.BottomLeft;
			var bounds = GetCS2Bounds();
			popup.TargetY = isBottom
				? bounds.Bottom + popup.Height + 10
				: bounds.Top - popup.Height - 10;
			UpdatePositions();
		}

		public static void Show(string achievementName, string title, string description, Image? icon = null, int progress = 0, int maxProgress = 0) {
			EnsureUIThread();
			_syncContext!.Post(_ => {
				EnsureAnimationTimer();
				var existing = popups.FirstOrDefault(p => !p.IsDisposed && !p.IsLeaving && p.AchievementName == achievementName);
				if (existing != null) {
					existing.Update(description, progress, maxProgress);
					return;
				}
				var popup = new AchievementPopup(achievementName, title, description, icon, progress, maxProgress);
				popup.FormClosed += (s, e) => { popups.Remove(popup); UpdatePositions(); };
				popups.Add(popup);
				UpdatePositions();
				popup.Show();
			}, null);
		}

		public static void ApplyScale() {
			if (_syncContext == null) return;
			float scale = Math.Clamp(AppConfig.Instance.PopupScale, 0.5f, 3.0f);
			_syncContext.Post(_ => {
				foreach (var p in popups.ToList()) {
					if (!p.IsDisposed) p.Rescale(scale);
				}
				UpdatePositions();
			}, null);
		}

		internal static void UpdatePositions() {
			var bounds = GetCS2Bounds();
			var pos = AppConfig.Instance.PopupPosition;
			bool isBottom = pos == PopupPosition.BottomRight || pos == PopupPosition.BottomLeft;

			if (isBottom) {
				int y = bounds.Bottom - 10;
				for (int i = popups.Count - 1; i >= 0; i--) {
					var p = popups[i];
					if (p.IsLeaving) continue;
					y -= p.Height;
					p.TargetY = y;
					y -= popupSpacing;
				}
			} else {
				int y = bounds.Top + 10;
				for (int i = 0; i < popups.Count; i++) {
					var p = popups[i];
					if (p.IsLeaving) continue;
					p.TargetY = y;
					y += p.Height + popupSpacing;
				}
			}
		}
	}
}