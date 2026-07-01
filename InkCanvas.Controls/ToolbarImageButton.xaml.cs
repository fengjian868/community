using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Ink_Canvas.Controls
{
    public partial class ToolbarImageButton : UserControl
    {
        private static ToolbarImageButton _lastPressedButton;
        private bool _isVerticalOrientation;
        private bool _isSelected;

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(ToolbarImageButton),
            new PropertyMetadata(string.Empty, (d, e) => ((ToolbarImageButton)d).LabelTextBlock.Text = (string)e.NewValue));

        public static readonly DependencyProperty UseBoardStyleProperty = DependencyProperty.Register(
            nameof(UseBoardStyle), typeof(bool), typeof(ToolbarImageButton),
            new PropertyMetadata(false, OnUseBoardStyleChanged));

        private static void OnUseBoardStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (ToolbarImageButton)d;
            button.ApplyBoardStyle((bool)e.NewValue);
        }

        public bool UseBoardStyle
        {
            get => (bool)GetValue(UseBoardStyleProperty);
            set => SetValue(UseBoardStyleProperty, value);
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty IconGeometryDrawingProperty = DependencyProperty.Register(
            nameof(IconGeometryDrawing), typeof(GeometryDrawing), typeof(ToolbarImageButton),
            new PropertyMetadata(null, OnIconGeometryDrawingChanged));

        private static void OnIconGeometryDrawingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (ToolbarImageButton)d;
            if (e.NewValue is GeometryDrawing newDrawing)
            {
                button.IconGeometryInternal.Geometry = newDrawing.Geometry;
                button.IconGeometryInternal.Brush = newDrawing.Brush;
            }
        }

        public GeometryDrawing IconGeometryDrawing
        {
            get => (GeometryDrawing)GetValue(IconGeometryDrawingProperty);
            set => SetValue(IconGeometryDrawingProperty, value);
        }

        public GeometryDrawing Icon => IconGeometryInternal;

        public GeometryDrawing Badge => BadgeGeometryInternal;

        public GeometryDrawing GeometryDrawing => IconGeometryInternal;

        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(
            nameof(IconBrush), typeof(Brush), typeof(ToolbarImageButton),
            new PropertyMetadata(null, (d, e) => ((ToolbarImageButton)d).IconGeometryInternal.Brush = (Brush)e.NewValue));

        public Brush IconBrush
        {
            get => (Brush)GetValue(IconBrushProperty);
            set => SetValue(IconBrushProperty, value);
        }

        public static readonly DependencyProperty LabelBrushProperty = DependencyProperty.Register(
            nameof(LabelBrush), typeof(Brush), typeof(ToolbarImageButton),
            new PropertyMetadata(null, (d, e) =>
            {
                var b = (ToolbarImageButton)d;
                if (e.NewValue is Brush brush) b.LabelTextBlock.Foreground = brush;
            }));

        public Brush LabelBrush
        {
            get => (Brush)GetValue(LabelBrushProperty);
            set => SetValue(LabelBrushProperty, value);
        }

        public new Brush Background
        {
            get => ButtonBorder.Background;
            set => ButtonBorder.Background = value;
        }

        public double LabelFontSize
        {
            get => LabelTextBlock.FontSize;
            set => LabelTextBlock.FontSize = value;
        }

        public double IconHeight
        {
            get => ButtonImage.Height;
            set => ButtonImage.Height = value;
        }

        public void ApplyOrientation(bool isVertical)
        {
            _isVerticalOrientation = isVertical;
            if (UseBoardStyle)
            {
                // 白板风格：固定 60×50，无方向偏移
                ButtonPanel.Width = 60;
                ButtonPanel.Height = 50;
                ButtonBorder.Margin = new Thickness(0);
                return;
            }
            if (isVertical)
            {
                ButtonPanel.Width = 43;
                ButtonPanel.Height = 44;
                ButtonBorder.Margin = new Thickness(2, 0, 7, 0);
            }
            else
            {
                ButtonPanel.Width = 44;
                ButtonPanel.Height = 43;
                ButtonBorder.Margin = new Thickness(0, 2, 0, 7);
            }
        }

        /// <summary>
        /// 切换白板风格视觉：匹配 BoardToolbarButton 的 60×50 尺寸、圆角 5、图标 20×20、文字 12px。
        /// </summary>
        private void ApplyBoardStyle(bool useBoardStyle)
        {
            if (useBoardStyle)
            {
                // 匹配 BoardToolbarButton: Width=60, Height=50, CornerRadius=5
                ButtonBorder.Width = 60;
                ButtonBorder.Height = 50;
                ButtonBorder.CornerRadius = new CornerRadius(5);
                ButtonBorder.Background = TryFindResource("BoardFloatBarBackground") as Brush ?? Brushes.Transparent;
                ButtonBorder.Margin = new Thickness(0);
                ButtonBorder.BorderThickness = new Thickness(0);

                ButtonPanel.Width = 60;
                ButtonPanel.Height = 50;

                // 内部 Grid 边距匹配 BoardToolbarButton: Margin="0,6,0,4"
                ButtonContent.Margin = new Thickness(0, 6, 0, 4);

                // 图标 20×20，顶部对齐
                ButtonImage.Width = 20;
                ButtonImage.Height = 20;
                ButtonImage.HorizontalAlignment = HorizontalAlignment.Center;
                ButtonImage.VerticalAlignment = VerticalAlignment.Top;
                ButtonImage.Margin = new Thickness(0);
                ButtonImage.Stretch = Stretch.Uniform;

                // 文字 12px，底部对齐
                LabelTextBlock.FontSize = 12;
                LabelTextBlock.Margin = new Thickness(0);
                LabelTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
                LabelTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                LabelTextBlock.Visibility = Visibility.Visible;

                PressedBackground.CornerRadius = new CornerRadius(5);
                ApplyBoardStyleSelectionVisual();
            }
            else
            {
                ButtonBorder.Width = double.NaN;
                ButtonBorder.Height = double.NaN;
                ButtonBorder.CornerRadius = new CornerRadius(4);
                ButtonBorder.Background = Brushes.Transparent;
                ButtonBorder.BorderThickness = new Thickness(0);
                ApplyOrientation(_isVerticalOrientation);

                ButtonPanel.Width = _isVerticalOrientation ? 43 : 44;
                ButtonPanel.Height = _isVerticalOrientation ? 44 : 43;

                ButtonContent.Margin = new Thickness(0);

                ButtonImage.Width = 24;
                ButtonImage.Height = 24;
                ButtonImage.HorizontalAlignment = HorizontalAlignment.Center;
                ButtonImage.VerticalAlignment = VerticalAlignment.Top;
                ButtonImage.Stretch = Stretch.Uniform;
                ButtonImage.Margin = new Thickness(0, 1, 0, 0);

                LabelTextBlock.FontSize = 13;
                LabelTextBlock.Margin = new Thickness(0, 0, 0, 2);
                LabelTextBlock.VerticalAlignment = VerticalAlignment.Bottom;

                PressedBackground.CornerRadius = new CornerRadius(4);
                ButtonBorder.Background = Brushes.Transparent;
            }
        }

        /// <summary>
        /// 应用紧凑浮动栏模式：开启后隐藏常驻文字标签，并让图标在保持纵横比的前提下拉伸填满空出的区域。
        /// 白板风格下保持显示标签并等比缩小。
        /// </summary>
        public void ApplyCompactMode(bool compact)
        {
            if (UseBoardStyle)
            {
                // 白板风格下紧凑模式仅轻微缩小尺寸，保持标签可见
                if (compact)
                {
                    ButtonBorder.Width = 46;
                    ButtonBorder.Height = 42;
                    ButtonPanel.Width = 46;
                    ButtonPanel.Height = 42;
                    ButtonImage.Width = 18;
                    ButtonImage.Height = 18;
                    ButtonImage.Margin = new Thickness(0, 4, 0, 0);
                    LabelTextBlock.FontSize = 10;
                }
                else
                {
                    ApplyBoardStyle(true);
                }
                return;
            }
            if (compact)
            {
                LabelTextBlock.Visibility = Visibility.Collapsed;
                ButtonImage.HorizontalAlignment = HorizontalAlignment.Stretch;
                ButtonImage.VerticalAlignment = VerticalAlignment.Stretch;
                ButtonImage.Stretch = Stretch.Uniform;
                ButtonImage.Width = double.NaN;
                ButtonImage.Height = double.NaN;
                ButtonImage.Margin = new Thickness(2);
            }
            else
            {
                LabelTextBlock.Visibility = Visibility.Visible;
                ButtonImage.HorizontalAlignment = HorizontalAlignment.Center;
                ButtonImage.VerticalAlignment = VerticalAlignment.Top;
                ButtonImage.Stretch = Stretch.Uniform;
                ButtonImage.Width = 24;
                ButtonImage.Height = 24;
                ButtonImage.Margin = new Thickness(0, 1, 0, 0);
            }
        }

        private void ApplyBoardStyleSelectionVisual()
        {
            if (!UseBoardStyle) return;
            if (_isSelected)
            {
                // 选中态：蓝色背景 + 白色图标/文字（匹配白板工具栏）
                var selectedBg = TryFindResource("BoardFloatBarSelectedBackground") as Brush;
                var selectedFg = TryFindResource("BoardFloatBarSelectedForeground") as Brush;
                if (selectedBg != null) ButtonBorder.Background = selectedBg;
                if (selectedFg != null)
                {
                    IconGeometryInternal.Brush = selectedFg;
                    LabelTextBlock.Foreground = selectedFg;
                }
            }
            else
            {
                // 非选中态：恢复白板背景 + 正常图标/文字颜色
                var boardBg = TryFindResource("BoardFloatBarBackground") as Brush;
                var iconFg = TryFindResource("IconForeground") as Brush ?? TryFindResource("FloatBarForeground") as Brush;
                if (boardBg != null) ButtonBorder.Background = boardBg;
                if (iconFg != null)
                {
                    IconGeometryInternal.Brush = iconFg;
                    LabelTextBlock.Foreground = iconFg;
                }
            }
        }

        public void SetSelectedVisualOffset(bool isSelected)
        {
            _isSelected = isSelected;
            if (UseBoardStyle)
            {
                ApplyBoardStyleSelectionVisual();
                return;
            }

            if (ButtonContent == null) return;

            var transform = ButtonContent.RenderTransform as TranslateTransform;

            if (_isVerticalOrientation)
            {
                // 选定项：左边距归零，内容不动
                // 非选定项：恢复左边距，内容向左偏移2px居中
                ButtonBorder.Margin = isSelected
                    ? new Thickness(0, 0, 7, 0)
                    : new Thickness(2, 0, 7, 0);

                double targetX = isSelected ? 0 : -2;
                double fromX = transform?.X ?? 0;
                if (Math.Abs(fromX - targetX) < 0.5) return;

                var newTransform = new TranslateTransform(fromX, 0);
                ButtonContent.RenderTransform = newTransform;
                var animX = new DoubleAnimation(targetX, new Duration(TimeSpan.FromMilliseconds(120)))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                newTransform.BeginAnimation(TranslateTransform.XProperty, animX);
            }
            else
            {
                // 选定项：上边距归零，内容不动
                // 非选定项：恢复上边距，内容下移2px居中
                ButtonBorder.Margin = isSelected
                    ? new Thickness(0, 0, 0, 7)
                    : new Thickness(0, 2, 0, 7);

                double targetY = isSelected ? 0 : 2;
                double fromY = transform?.Y ?? 0;
                if (Math.Abs(fromY - targetY) < 0.5) return;

                var newTransform = new TranslateTransform(0, fromY);
                ButtonContent.RenderTransform = newTransform;
                var animY = new DoubleAnimation(targetY, new Duration(TimeSpan.FromMilliseconds(120)))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                newTransform.BeginAnimation(TranslateTransform.YProperty, animY);
            }
        }

        public event MouseButtonEventHandler ButtonMouseDown;
        public event MouseEventHandler ButtonMouseLeave;
        public event MouseButtonEventHandler ButtonMouseUp;

        public ToolbarImageButton()
        {
            InitializeComponent();
            ButtonPanel.Background = Brushes.Transparent;
            IsEnabledChanged += ToolbarImageButton_IsEnabledChanged;
        }

        private void ToolbarImageButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var isEnabled = (bool)e.NewValue;
            ButtonImage.Opacity = isEnabled ? 1.0 : 0.5;
            LabelTextBlock.Opacity = isEnabled ? 1.0 : 0.5;
            ButtonPanel.IsEnabled = isEnabled;
        }

        private void ButtonPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;
            if (_lastPressedButton != null && _lastPressedButton != this)
            {
                _lastPressedButton.PressedBackground.Background = Brushes.Transparent;
            }
            _lastPressedButton = this;
            PressedBackground.Background = new SolidColorBrush(Color.FromArgb(28, 24, 24, 27));
            ButtonMouseDown?.Invoke(this, e);
        }

        private void ButtonPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsEnabled) return;
            if (_lastPressedButton == this)
            {
                PressedBackground.Background = Brushes.Transparent;
                _lastPressedButton = null;
            }
            ButtonMouseLeave?.Invoke(this, e);
        }

        private void ButtonPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;
            if (_lastPressedButton == this)
            {
                PressedBackground.Background = Brushes.Transparent;
                _lastPressedButton = null;
            }
            ButtonMouseUp?.Invoke(this, e);
        }
    }
}
