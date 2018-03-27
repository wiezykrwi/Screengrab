using System;
using System.Drawing;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using Size = System.Drawing.Size;

namespace Screengrab
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private AdornerLayer _adornerLayer;

		public MainWindow()
		{
			InitializeComponent();

			Loaded += OnLoaded;
			Deactivated += (s, a) => Application.Current.Shutdown();
			KeyDown += OnKeyDown;
		}

		private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
		{
			if (keyEventArgs.Key == Key.Escape)
			{
				Application.Current.Shutdown();
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			_adornerLayer = AdornerLayer.GetAdornerLayer(MainGrid);
			_adornerLayer.Add(new OverlayAdorner(MainGrid));
		}
	}

	internal class OverlayAdorner : Adorner
	{
		private readonly SolidColorBrush _solidColorBrush = new SolidColorBrush(Color.FromArgb(0x33, 0x00, 0x00, 0x00));
		private readonly SolidColorBrush _transparantBrush = new SolidColorBrush(Color.FromArgb(0x01, 0x00, 0x00, 0x00));
		private readonly Rect _fullRectangle;

		private bool _drawing;
		private Point _origin;
		private Point _current;

		public OverlayAdorner(UIElement uiElement) : base(uiElement)
		{
			_fullRectangle = new Rect(uiElement.RenderSize);

			MouseDown += UiElementOnMouseDown;
			MouseMove += UiElementOnMouseMove;
			MouseUp += UiElementOnMouseUp;

			IsHitTestVisible = true;
		}

		private void UiElementOnMouseUp(object sender, MouseButtonEventArgs mouseEventArgs)
		{
			_drawing = false;

			var smallX = Math.Min(_origin.X, _current.X);
			var largeX = Math.Max(_origin.X, _current.X);
			var smallY = Math.Min(_origin.Y, _current.Y);
			var largeY = Math.Max(_origin.Y, _current.Y);

			using (var bitmap = new Bitmap((int)(largeX - smallX), (int)(largeY - smallY)))
			{
				using (var graphics = Graphics.FromImage(bitmap))
				{
					graphics.Clear(System.Drawing.Color.HotPink);
					graphics.CopyFromScreen((int)smallX, (int)smallY, 0, 0, new Size((int)(largeX - smallX), (int)(largeY - smallY)));
				}

				bitmap.Save(@"C:\Temp\screencap.png");
			}

			using (var bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
			{
				using (var graphics = Graphics.FromImage(bitmap))
				{
					graphics.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
				}

				bitmap.Save(@"C:\Temp\fullscreencap.png");
			}
		}

		private void UiElementOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
		{
			if (!_drawing)
			{
				return;
			}

			_current = mouseEventArgs.GetPosition(AdornedElement);

			InvalidateVisual();
		}

		private void UiElementOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{
			_drawing = true;
			_origin = mouseButtonEventArgs.GetPosition(AdornedElement);
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			if (_drawing)
			{
				drawingContext.DrawRectangle(_transparantBrush, null, _fullRectangle);

				var smallX = Math.Min(_origin.X, _current.X);
				var largeX = Math.Max(_origin.X, _current.X);
				var smallY = Math.Min(_origin.Y, _current.Y);
				var largeY = Math.Max(_origin.Y, _current.Y);

				drawingContext.DrawRectangle(_solidColorBrush, null, new Rect(0, 0, AdornedElement.RenderSize.Width, smallY));

				drawingContext.DrawRectangle(_solidColorBrush, null, new Rect(0, smallY, smallX, largeY - smallY));
				drawingContext.DrawRectangle(_solidColorBrush, null, new Rect(largeX, smallY, AdornedElement.RenderSize.Width - largeX, largeY - smallY));

				drawingContext.DrawRectangle(_solidColorBrush, null, new Rect(0, largeY, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height - largeY));
			}
			else
			{
				drawingContext.DrawRectangle(_solidColorBrush, null, _fullRectangle);
			}
		}
	}
}
