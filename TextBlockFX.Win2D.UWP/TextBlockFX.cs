using System;
using System.Collections.Generic;
using Windows.Foundation;
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
    /// <summary>
    /// A lightweight control for displaying small amounts of animated text.
    /// </summary>
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

        /// <summary>
        /// Identifies the Text dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(TextBlockFX), new PropertyMetadata(default(string)));

        /// <summary>
        /// Gets or sets the text contents of a TextBlockFX.
        /// </summary>
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

        /// <summary>
        /// Identifies the TextEffect dependency property.
        /// </summary>
        public static readonly DependencyProperty TextEffectProperty = DependencyProperty.Register(
            "TextEffect", typeof(ITextEffect), typeof(TextBlockFX), new PropertyMetadata(default(ITextEffect)));

        /// <summary>
        /// Gets or sets the effect for animating text.
        /// </summary>
        public ITextEffect TextEffect
        {
            get { return (ITextEffect)GetValue(TextEffectProperty); }
            set
            {
                _textEffect = value;
                SetValue(TextEffectProperty, value);
            }
        }

        /// <summary>
        /// Identifies the TextAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
            "TextAlignment", typeof(TextAlignment), typeof(TextBlockFX), new PropertyMetadata(default(TextAlignment)));

        /// <summary>
        /// Gets or sets a value that indicates the horizontal alignment of text content.
        /// </summary>
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set
            {
                _textAlignment = value;
                SetValue(TextAlignmentProperty, value);
            }
        }

        /// <summary>
        /// Identifies the TextDirection dependency property.
        /// </summary>
        public static readonly DependencyProperty TextDirectionProperty = DependencyProperty.Register(
            "TextDirection", typeof(TextDirection), typeof(TextBlockFX), new PropertyMetadata(default(TextDirection)));

        /// <summary>
        /// Gets or sets a value that indicates direction in which the text is read.
        /// </summary>
        public TextDirection TextDirection
        {
            get { return (TextDirection)GetValue(TextDirectionProperty); }
            set
            {
                _textDirection = value;
                SetValue(TextDirectionProperty, value);
            }
        }

        /// <summary>
        /// Identifies the TextTrimming dependency property.
        /// </summary>
        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
            "TextTrimming", typeof(TextTrimming), typeof(TextBlockFX), new PropertyMetadata(default(TextTrimming)));

        /// <summary>
        /// Gets or sets the text trimming behavior to employ when content overflows the content area.
        /// </summary>
        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set
            {
                _textTrimming = value;
                SetValue(TextTrimmingProperty, value);
            }
        }

        /// <summary>
        /// Identifies the TextWrapping  dependency property.
        /// </summary>
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
            "TextWrapping", typeof(TextWrapping), typeof(TextBlockFX), new PropertyMetadata(default(TextWrapping)));

        /// <summary>
        /// Gets or sets how the TextBlockFX wraps text.
        /// </summary>
        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set
            {
                _textWrapping = value;
                SetValue(TextWrappingProperty, value);
            }
        }

        /// <summary>
        /// Gets whether TextBlockFX is animating the text.
        /// </summary>
        public bool IsAnimating => _currentState != RedrawState.Idle;

        #endregion

        /// <summary>
        /// Occurs when the redraw state has changed.
        /// </summary>
        public event EventHandler<RedrawState> RedrawStateChanged;

        /// <summary>
        /// Initializes a new instance of the TextBlockFX class.
        /// </summary>
        public TextBlockFX()
        {
            this.DefaultStyleKey = typeof(TextBlockFX);

            this.Loaded += TextBlockFX_Loaded;
            this.RegisterPropertyChangedCallback(TextBlockFX.ForegroundProperty, ForegroundChangedCallback);
            this.RegisterPropertyChangedCallback(TextBlockFX.FontFamilyProperty, FontFamilyChangedCallback);
            this.RegisterPropertyChangedCallback(TextBlockFX.FontSizeProperty, FontSizeChangedCallback);
            this.RegisterPropertyChangedCallback(TextBlockFX.FontStretchProperty, FontStretchChangedCallback);
            this.RegisterPropertyChangedCallback(TextBlockFX.FontStyleProperty, FontStyleChangedCallback);
            this.RegisterPropertyChangedCallback(TextBlockFX.FontWeightProperty, FontWeightChangedCallback);

            _textFormat.TrimmingSign = CanvasTrimmingSign.Ellipsis;
        }
        /// <inheritdoc />
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _animatedCanvas = GetTemplateChild("AnimatedCanvas") as CanvasAnimatedControl;
            this.SizeChanged += TextBlockFX_SizeChanged;

            ApplyTextFormat();
            ApplyTextForeground();

            if (_animatedCanvas != null)
            {
                _animatedCanvas.CreateResources += AnimatedCanvas_CreateResources;
                _animatedCanvas.Update += AnimatedCanvas_Update;
                _animatedCanvas.Draw += AnimatedCanvas_Draw;
            }
        }

        private void TextBlockFX_Loaded(object sender, RoutedEventArgs e)
        {
            _newText = Text ?? string.Empty;

            SetRedrawState(RedrawState.TextChanged, false);
        }

        private void TextBlockFX_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetRedrawState(RedrawState.LayoutChanged);
        }

        #region Property Changed Callbacks

        private void ForegroundChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            ApplyTextForeground();
        }
        private void FontFamilyChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            _fontFamily = FontFamily.Source;
        }
        private void FontSizeChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            _fontSize = (float)FontSize;
        }
        private void FontStretchChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            _fontStretch = FontStretch;
        }

        private void FontStyleChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            _fontStyle = FontStyle;
        }
        private void FontWeightChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            _fontWeight = FontWeight;
        }
        #endregion

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

                    GenerateNewTextLayout(sender);

                    GenerateDiffResults();

                    _animationBeginTime = args.Timing.TotalTime;

                    SetRedrawState(RedrawState.Animating);
                }
            }

            if (_currentState == RedrawState.TextChanged)
            {
                ApplyTextFormat();
                
                GenerateOldTextLayout(sender);
                
                GenerateNewTextLayout(sender);
                
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
                ctl.Options = CanvasDrawTextOptions.EnableColorFont;

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
            _textFormat.Options = CanvasDrawTextOptions.EnableColorFont | CanvasDrawTextOptions.NoPixelSnap;
            _textFormat.HorizontalAlignment = Win2dHelpers.MapCanvasHorizontalAlignment(_textAlignment);
            _textFormat.VerticalAlignment = CanvasVerticalAlignment.Center;
            _textFormat.Direction = Win2dHelpers.MapTextDirection(_textDirection);
            _textFormat.TrimmingGranularity = Win2dHelpers.MapTrimmingGranularity(_textTrimming);
            _textFormat.WordWrapping = Win2dHelpers.MapWordWrapping(_textWrapping);
        }

        private void ApplyTextForeground()
        {
            if (Foreground is SolidColorBrush colorBrush)
            {
                _textColor = colorBrush.Color;
                _textBrush = null;
            }
            else if (Foreground is LinearGradientBrush linearGradientBrush)
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
            else
            {
                if (Application.Current.Resources["DefaultTextForegroundThemeBrush"] is SolidColorBrush defaultForegroundBrush)
                {
                    _textColor = defaultForegroundBrush.Color;
                    _textBrush = null;
                }
            }
        }

        private void GenerateOldTextLayout(ICanvasAnimatedControl resourceCreator)
        {
            _oldTextLayout = new CanvasTextLayout(resourceCreator, _oldText, _textFormat,
                (float)(resourceCreator.Size.Width),
                (float)(resourceCreator.Size.Height));
            _oldTextLayout.Options = CanvasDrawTextOptions.EnableColorFont | CanvasDrawTextOptions.NoPixelSnap;
            _oldTextLayout.VerticalAlignment = CanvasVerticalAlignment.Center;
        }
        
        private void GenerateNewTextLayout(ICanvasAnimatedControl resourceCreator)
        {
            _newTextLayout = new CanvasTextLayout(resourceCreator, _newText, _textFormat,
                (float)(resourceCreator.Size.Width),
                (float)(resourceCreator.Size.Height));
            _newTextLayout.Options = CanvasDrawTextOptions.EnableColorFont | CanvasDrawTextOptions.NoPixelSnap;
            _newTextLayout.VerticalAlignment = CanvasVerticalAlignment.Center;
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
            var delayPerCluster = _textEffect?.DelayPerCluster ?? TimeSpan.FromMilliseconds(0);

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

            var duration = _textEffect?.AnimationDuration ?? TimeSpan.FromMilliseconds(0);

            bool isFinished = timing.TotalTime.TotalMilliseconds >=
                              (_animationBeginTime.TotalMilliseconds +
                               delay.TotalMilliseconds * offset +
                               duration.TotalMilliseconds);

            if (isFinished)
            {
                cluster.Progress = 1.0f;
                cluster.IsAnimationFinished = true;
                return true;
            }

            float progress = cluster.Progress + step;

            if ((timing.TotalTime.TotalMilliseconds - _animationBeginTime.TotalMilliseconds <
                 delay.TotalMilliseconds * offset))
            {
                progress = 0;
            }

            progress = Math.Clamp(progress, 0, 1.0f);

            cluster.Progress = progress;

            return false;
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