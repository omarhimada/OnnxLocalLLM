using OLLM.Utility;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OLLM.State.Thinking;

using static MdFd;

/// <summary>
/// Represents a visual adorner that displays a floating bubble over an adorned element, providing contextual
/// information during the model's thinking process.
/// </summary>
public sealed class FloatingAdorner : Adorner {
	private readonly VisualCollection _visuals;
	private readonly Border _bubble;

	public FloatingAdorner(UIElement adornedElement) : base(adornedElement) {
		_visuals = new VisualCollection(this);
		IsHitTestVisible = false;
		_bubble = new Border {
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,

			CornerRadius = new CornerRadius(0),
			Padding = new Thickness(8, 8, 8, 8),
			Background = _black,
			BorderBrush = _readyDosPurple,

			BorderThickness = new Thickness(1),
			Effect = new System.Windows.Media.Effects.DropShadowEffect {
				BlurRadius = 16,
				ShadowDepth = 2,
				Opacity = 0.35
			},
			Child = new TextBlock {
				Foreground = _white,
				FontSize = 14,
				TextWrapping = TextWrapping.Wrap,
				MaxWidth = 680,
				MaxHeight = 227,

			},
			RenderTransformOrigin = new Point(0.5, 0.5),
			RenderTransform = new TransformGroup {
				Children =
				[
					new ScaleTransform(1,1),
					new TranslateTransform(0,0)
				]

			},
			Opacity = 0
		};
		_visuals.Add(_bubble);
	}

	public void SetText(string text)
		=> ((TextBlock)_bubble.Child).Text = text;

	/// <summary>
	/// Displays the bubble in the top-right corner of the adorned element, offset by the specified margin.
	/// </summary>
	/// <remarks>This method must be called on the UI thread. The bubble's position is updated based on the actual
	/// width of the adorned element and the specified margin to ensure proper alignment and spacing.</remarks>
	/// <param name="margin">The margin, in pixels, to maintain between the bubble and the edges of the adorned element. Defaults to 14.</param>
	/// <returns>A task that represents the asynchronous operation of positioning the bubble.</returns>
	public async Task ShowAtTopRight(double margin = 14) {
		await Application.Current.Dispatcher.InvokeAsync(new Action(() => {
			FrameworkElement fe = (FrameworkElement)AdornedElement;
			double x = Math.Max(margin, fe.ActualWidth - margin - _bubble.ActualWidth);
			double y = margin;

			TransformGroup tg = (TransformGroup)_bubble.RenderTransform;
			TranslateTransform tt = (TranslateTransform)tg.Children[1];
			tt.X = x;
			tt.Y = y;

			InvalidateArrange();
		}));
	}

	/// <summary>
	/// Animates the appearance of the UI element by fading it in and scaling it to its original size.
	/// </summary>
	/// <remarks>The animation combines a fade-in effect with a scaling transition for a smooth entrance. The
	/// fade-in duration is 227 milliseconds, and the scaling uses a back easing function to enhance visual appeal. This
	/// method must be called from a context where the UI element is available and ready to be displayed.</remarks>
	/// <returns>A task that represents the asynchronous operation of the animation.</returns>
	public async Task AnimateIn() {
		await Application.Current.Dispatcher.InvokeAsync(new Action(() => {
			Storyboard sb = new();

			DoubleAnimation fade = new(0, 1, TimeSpan.FromMilliseconds(227)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
			Storyboard.SetTarget(fade, _bubble);
			Storyboard.SetTargetProperty(fade, new PropertyPath(OpacityProperty));
			sb.Children.Add(fade);

			TransformGroup tg = (TransformGroup)_bubble.RenderTransform;
			ScaleTransform scale = (ScaleTransform)tg.Children[0];

			DoubleAnimation sx = new(1, 1, TimeSpan.FromMilliseconds(227)) { EasingFunction = new BackEase { Amplitude = 0.25, EasingMode = EasingMode.EaseOut } };
			Storyboard.SetTarget(sx, scale);
			Storyboard.SetTargetProperty(sx, new PropertyPath(ScaleTransform.ScaleXProperty));
			sb.Children.Add(sx);

			DoubleAnimation sy = sx.Clone();
			Storyboard.SetTargetProperty(sy, new PropertyPath(ScaleTransform.ScaleYProperty));
			sb.Children.Add(sy);

			sb.Begin();
		}));
	}

	/// <summary>
	/// Animates the opacity of the bubble element to fade out over a short duration.
	/// </summary>
	/// <remarks>This method ensures that the animation is executed on the application's UI thread by using the
	/// dispatcher. The opacity is animated from its current value to zero, making the bubble element invisible after the
	/// animation completes.</remarks>
	/// <returns>A task that represents the asynchronous operation of the fade-out animation.</returns>
	public async Task AnimateOut() {
		await Application.Current.Dispatcher.InvokeAsync(new Action(() => {
			DoubleAnimation fade = new(0, TimeSpan.FromMilliseconds(140));
			_bubble.BeginAnimation(OpacityProperty, fade);
		}));
	}

	protected override Size ArrangeOverride(Size finalSize) {
		_bubble.Measure(finalSize);
		_bubble.Arrange(new Rect(new Point(0, 0), _bubble.DesiredSize));
		return finalSize;
	}

	protected override int VisualChildrenCount => _visuals.Count;

	protected override Visual GetVisualChild(int index) => _visuals[index];
}
