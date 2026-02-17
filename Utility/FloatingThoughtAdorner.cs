using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OLLM.Utility;

using static MdFd;

public sealed class FloatingThoughtAdorner : Adorner {
	private readonly VisualCollection _visuals;
	private readonly Border _bubble;

	public FloatingThoughtAdorner(UIElement adornedElement) : base(adornedElement) {
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

	public void ShowAtTopRight(double margin = 14) {
		FrameworkElement fe = (FrameworkElement)AdornedElement;
		double x = Math.Max(margin, fe.ActualWidth - margin - _bubble.ActualWidth);
		double y = margin;

		TransformGroup tg = (TransformGroup)_bubble.RenderTransform;
		TranslateTransform tt = (TranslateTransform)tg.Children[1];
		tt.X = x;
		tt.Y = y;

		InvalidateArrange();
	}

	public void AnimateIn() {
		Storyboard sb = new();

		DoubleAnimation fade = new(0, 1, TimeSpan.FromMilliseconds(227)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
		Storyboard.SetTarget(fade, _bubble);
		Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));
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
	}

	public void AnimateOut() {
		DoubleAnimation fade = new(0, TimeSpan.FromMilliseconds(140));
		_bubble.BeginAnimation(UIElement.OpacityProperty, fade);
	}

	protected override Size ArrangeOverride(Size finalSize) {
		_bubble.Measure(finalSize);
		_bubble.Arrange(new Rect(new Point(0, 0), _bubble.DesiredSize));
		return finalSize;
	}

	protected override int VisualChildrenCount => _visuals.Count;
	protected override Visual GetVisualChild(int index) => _visuals[index];
}
