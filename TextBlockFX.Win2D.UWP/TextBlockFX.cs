using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#else
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#endif

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI
#else
namespace TextBlockFX.Win2D.UWP
#endif
{
    [TemplatePart(Name = "ContentBorder", Type = typeof(Border))]
    [TemplatePart(Name = "AnimatedCanvas", Type = typeof(CanvasAnimatedControl))]
    public sealed class TextBlockFX : Control
    {
        private CanvasAnimatedControl _animatedCanvas = null;

        private string _oldText = string.Empty;
        private string _newText = string.Empty;

        private RedrawState _currentState = RedrawState.Idle;
        private TimeSpan _animationBeginTime;

        private List<TextDiffResult> _diffResults = null;

        private CanvasTextFormat _textFormat = new CanvasTextFormat();
        private CanvasLinearGradientBrush _textBrush;
        private Color _textColor = Colors.Black;

        private CanvasTextLayout _oldTextLayout;
        private CanvasTextLayout _newTextLayout;

        private ITextEffect _textEffect;

        private float _fontSize = 14;
        private string _fontFamily = FontFamily.XamlAutoFontFamily.Source;
        private FontStretch _fontStretch = FontStretch.Normal;
        private FontStyle _fontStyle = FontStyle.Normal;
        private FontWeight _fontWeight = FontWeights.Normal;

        private TextAlignment _textAlignment = TextAlignment.Left;
        private TextDirection _textDirection = TextDirection.LeftToRightThenTopToBottom;
        private TextTrimming _textTrimming = TextTrimming.None;
        private TextWrapping _textWrapping = TextWrapping.NoWrap;

        #region Properties

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(TextBlockFX), new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                _oldText = _newText ?? string.Empty;
                _newText = value ?? string.Empty;

                SetRedrawState(RedrawState.TextChanged, false);

                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty TextEffectProperty = DependencyProperty.Register(
            "TextEffect", typeof(ITextEffect), typeof(TextBlockFX), new PropertyMetadata(default(ITextEffect)));

        public ITextEffect TextEffect
        {
            get { return (ITextEffect)GetValue(TextEffectProperty); }
            set
            {
                _textEffect = value;
                SetValue(TextEffectProperty, value);
            }
        }

        public new static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
            "FontFamily", typeof(FontFamily), typeof(TextBlockFX), new PropertyMetadata(default(FontFamily)));

        public new FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set
            {
                _fontFamily = value.Source;
                SetValue(FontFamilyProperty, value);
            }
        }

        public new static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            "FontSize", typeof(double), typeof(TextBlockFX), new PropertyMetadata(default(double)));

        public new double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set
            {
                _fontSize = (float)value;
                SetValue(FontSizeProperty, value);
            }
        }

        public new static readonly DependencyProperty FontStretchProperty = DependencyProperty.Register(
            "FontStretch", typeof(FontStretch), typeof(TextBlockFX), new PropertyMetadata(default(FontStretch)));

        public new FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set
            {
                _fontStretch = value;
                SetValue(FontStretchProperty, value);
            }
        }

        public new static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register(
            "FontStyle", typeof(FontStyle), typeof(TextBlockFX), new PropertyMetadata(default(FontStyle)));

        public new FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set
            {
                _fontStyle = value;
                SetValue(FontStyleProperty, value);
            }
        }

        public new static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(
            "FontWeight", typeof(FontWeight), typeof(TextBlockFX), new PropertyMetadata(default(FontWeight)));

        public new FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set
            {
                _fontWeight = value;
                SetValue(FontWeightProperty, value);
            }
        }

        public new static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof(Brush), typeof(TextBlockFX), new PropertyMetadata(default(Brush)));

        public new Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set
            {
                if (value is SolidColorBrush colorBrush)
                {
                    _textColor = colorBrush.Color;
                    _textBrush = null;
                }
                else if (value is LinearGradientBrush linearGradientBrush)
                {
                    if (_animatedCanvas != null)
                    {
                        var stops = new CanvasGradientStop[linearGradientBrush.GradientStops.Count];

                        foreach (var gradientStop in linearGradientBrush.GradientStops)
                        {
                            var stop = new CanvasGradientStop()
                            {
                                Color = gradientStop.Color,
                                Position = (float)gradientStop.Offset
                            };
                        }

                        _textBrush = new CanvasLinearGradientBrush(_animatedCanvas, stops);
                    }
                }

                SetValue(ForegroundProperty, value);
            }
        }

        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
            "TextAlignment", typeof(TextAlignment), typeof(TextBlockFX), new PropertyMetadata(default(TextAlignment)));

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set
            {
                _textAlignment = value;
                SetValue(TextAlignmentProperty, value);
            }
        }

        public static readonly DependencyProperty TextDirectionProperty = DependencyProperty.Register(
            "TextDirection", typeof(TextDirection), typeof(TextBlockFX), new PropertyMetadata(default(TextDirection)));

        public TextDirection TextDirection
        {
            get { return (TextDirection)GetValue(TextDirectionProperty); }
            set
            {
                _textDirection = value;
                SetValue(TextDirectionProperty, value);
            }
        }

        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
            "TextTrimming", typeof(TextTrimming), typeof(TextBlockFX), new PropertyMetadata(default(TextTrimming)));

        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set
            {
                _textTrimming = value;
                SetValue(TextTrimmingProperty, value);
            }
        }

        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
            "TextWrapping", typeof(TextWrapping), typeof(TextBlockFX), new PropertyMetadata(default(TextWrapping)));

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set
            {
                _textWrapping = value;
                SetValue(TextWrappingProperty, value);
            }
        }

        public bool IsAnimating => _currentState != RedrawState.Idle;

        #endregion

        public event EventHandler<RedrawState> RedrawStateChanged;

        public TextBlockFX()
        {
            this.DefaultStyleKey = typeof(TextBlockFX);

            _textFormat.TrimmingSign = CanvasTrimmingSign.Ellipsis;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _animatedCanvas = GetTemplateChild("AnimatedCanvas") as CanvasAnimatedControl;
            this.SizeChanged += TextBlockFX_SizeChanged;

            ApplyTextFormat();

            if (_animatedCanvas != null)
            {
                _animatedCanvas.CreateResources += AnimatedCanvas_CreateResources;
                _animatedCanvas.Update += AnimatedCanvas_Update;
                _animatedCanvas.Draw += AnimatedCanvas_Draw;
            }
        }

        private void TextBlockFX_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetRedrawState(RedrawState.LayoutChanged);
        }

        #region Canvas Events

        private void AnimatedCanvas_CreateResources(CanvasAnimatedControl sender,
            CanvasCreateResourcesEventArgs args)
        {
            // Generate CanvasGradientStops from LinearGradientBrush
            if (Foreground is LinearGradientBrush linearGradientBrush)
            {
                var stops = new CanvasGradientStop[linearGradientBrush.GradientStops.Count];

                for (int i = 0; i < linearGradientBrush.GradientStops.Count; i++)
                {
                    var gradientStop = linearGradientBrush.GradientStops[i];
                    stops[i].Color = gradientStop.Color;
                    stops[i].Position = (float)gradientStop.Offset;
                }

                _textBrush = new CanvasLinearGradientBrush(_animatedCanvas, stops);
            }
        }

        private void AnimatedCanvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            if (_textEffect == null)
            {
                SetRedrawState(RedrawState.Idle);
                return;
            }

            if (_currentState == RedrawState.LayoutChanged)
            {
                ApplyTextFormat();

                if (_newTextLayout == null)
                {
                    SetRedrawState(RedrawState.Idle);
                }
                else
                {
                    _oldText = _newText;
                    _oldTextLayout = _newTextLayout;

                    _newTextLayout = new CanvasTextLayout(sender, _newText, _textFormat,
                        (float)sender.Size.Width,
                        (float)sender.Size.Height);
                    _newTextLayout.Options = CanvasDrawTextOptions.EnableColorFont;
                    _newTextLayout.VerticalAlignment = CanvasVerticalAlignment.Center;

                    GenerateDiffResults();

                    _animationBeginTime = args.Timing.TotalTime;

                    SetRedrawState(RedrawState.Animating);
                }
            }

            if (_currentState == RedrawState.TextChanged)
            {
                ApplyTextFormat();

                _oldTextLayout = new CanvasTextLayout(sender, _oldText, _textFormat,
                    (float)sender.Size.Width,
                    (float)sender.Size.Height);
                _oldTextLayout.Options = CanvasDrawTextOptions.EnableColorFont;
                _oldTextLayout.VerticalAlignment = CanvasVerticalAlignment.Center;

                _newTextLayout = new CanvasTextLayout(sender, _newText, _textFormat,
                    (float)sender.Size.Width,
                    (float)sender.Size.Height);
                _newTextLayout.Options = CanvasDrawTextOptions.EnableColorFont;
                _newTextLayout.VerticalAlignment = CanvasVerticalAlignment.Center;

                GenerateDiffResults();

                _animationBeginTime = args.Timing.TotalTime;

                SetRedrawState(RedrawState.Animating);
            }

            if (_currentState == RedrawState.Animating)
            {
                UpdateAllClusterProgress(args.Timing);
            }

            _textEffect.Update(_oldText,
                _newText,
                _diffResults,
                _oldTextLayout,
                _newTextLayout,
                _currentState,
                sender,
                args);
        }

        private void AnimatedCanvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            args.DrawingSession.Clear(Colors.Transparent);

            if (_textEffect == null)
            {
                CanvasTextLayout ctl = new CanvasTextLayout(sender,
                    _newText,
                    _textFormat,
                    (float)sender.Size.Width,
                    (float)sender.Size.Height);

                args.DrawingSession.DrawTextLayout(ctl, 0, 0, _textColor);
            }
            else
            {
                _textEffect?.DrawText(_oldText,
                    _newText,
                    _diffResults,
                    _oldTextLayout,
                    _newTextLayout,
                    _textFormat, _textColor,
                    _textBrush,
                    _currentState,
                    args.DrawingSession,
                    args);
            }
        }

        #endregion

        #region Helpers

        private void ApplyTextFormat()
        {
            _textFormat.FontSize = _fontSize;
            _textFormat.FontFamily = _fontFamily;
            _textFormat.FontStretch = _fontStretch;
            _textFormat.FontStyle = _fontStyle;
            _textFormat.FontWeight = _fontWeight;
            _textFormat.Options = CanvasDrawTextOptions.EnableColorFont;
            _textFormat.HorizontalAlignment = Win2dHelpers.MapCanvasHorizontalAlignment(_textAlignment);
            _textFormat.VerticalAlignment = CanvasVerticalAlignment.Center;
            _textFormat.Direction = Win2dHelpers.MapTextDirection(_textDirection);
            _textFormat.TrimmingGranularity = Win2dHelpers.MapTrimmingGranularity(_textTrimming);
            _textFormat.WordWrapping = Win2dHelpers.MapWordWrapping(_textWrapping);
        }

        private void GenerateDiffResults()
        {
            var oldGraphemeClusters = TextRenderingHelper.GenerateGraphemeClusters(_oldText, _oldTextLayout);
            var newGraphemeClusters = TextRenderingHelper.GenerateGraphemeClusters(_newText, _newTextLayout);

            _diffResults = GraphemeClusterDiff.Diff(oldGraphemeClusters, newGraphemeClusters);
        }

        private void UpdateAllClusterProgress(CanvasTimingInformation timing)
        {
            var animationDuration = _textEffect?.AnimationDuration ?? TimeSpan.FromMilliseconds(600);
            var delayPerCluster = _textEffect?.DelayPerCluster ?? TimeSpan.FromMilliseconds(10);

            float step = (float)(1 / (animationDuration.TotalMilliseconds / timing.ElapsedTime.TotalMilliseconds));

            var delay = delayPerCluster <= animationDuration ? delayPerCluster : animationDuration;

            int insertDelayOffset = 0;
            int moveDelayOffset = 0;
            int removeDelayOffset = 0;
            int updateDelayOffset = 0;

            int ongoingAnimations = 0;

            for (int i = 0; i < _diffResults.Count; i++)
            {
                var diffResult = _diffResults[i];
                var oldCluster = diffResult.OldGlyphCluster;
                var newCluster = diffResult.NewGlyphCluster;

                int delayOffset = 0;

                switch (diffResult.Type)
                {
                    default:
                    case DiffOperationType.Move:
                        delayOffset = removeDelayOffset;
                        removeDelayOffset += 1;
                        break;
                    case DiffOperationType.Insert:
                        delayOffset = insertDelayOffset;
                        insertDelayOffset += 1;
                        break;
                    case DiffOperationType.Remove:
                        delayOffset = moveDelayOffset;
                        moveDelayOffset += 1;
                        break;
                    case DiffOperationType.Update:
                        delayOffset = updateDelayOffset;
                        updateDelayOffset += 1;
                        break;
                }

                if (!UpdateClusterProgress(oldCluster, delayOffset, step, delay, timing))
                {
                    ongoingAnimations += 1;
                }

                if (!UpdateClusterProgress(newCluster, delayOffset, step, delay, timing))
                {
                    ongoingAnimations += 1;
                }
            }

            if (ongoingAnimations < 1)
            {
                SetRedrawState(RedrawState.Idle);
            }
        }

        /// <summary>
        /// Update progress of every cluster.
        /// </summary>
        /// <param name="cluster">Target cluster.</param>
        /// <param name="offset">Index of target cluster</param>
        /// <param name="step">Incremental step of the progress</param>
        /// <param name="delay">Duration of delay</param>
        /// <param name="timing">Timing info</param>
        /// <returns>If the animation of the cluster is finished</returns>
        private bool UpdateClusterProgress(GraphemeCluster cluster,
            int offset,
            float step,
            TimeSpan delay,
            CanvasTimingInformation timing)
        {
            if (cluster == null)
                return true;

            var delayPerCluster = _textEffect?.DelayPerCluster ?? TimeSpan.FromMilliseconds(10);

            float progress = cluster.Progress + step;

            if (delayPerCluster.TotalMilliseconds > 0)
            {
                if ((timing.TotalTime.TotalMilliseconds - _animationBeginTime.TotalMilliseconds <
                    delay.TotalMilliseconds * offset))
                {
                    progress = 0;
                }
            }
            cluster.Progress = progress;

            return cluster.Progress >= 1.0f;
        }

        private void ResetAllClusterProgress()
        {
            foreach (var diffResult in _diffResults)
            {
                var oldCluster = diffResult.OldGlyphCluster;
                var newCluster = diffResult.NewGlyphCluster;

                oldCluster.Progress = 0;
                newCluster.Progress = 0;
            }
        }

        private void SetRedrawState(RedrawState state, bool fireEvent = true)
        {
            _currentState = state;

            if (fireEvent)
            {
#if WINDOWS
			    DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () => RedrawStateChanged?.Invoke(this, _currentState));
#else
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => RedrawStateChanged?.Invoke(this, _currentState)
                );
#endif
            }
        }

        #endregion
    }
}