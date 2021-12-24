using System;
using System.Collections.Generic;
using TextBlockFX;
using TextBlockFX.Win2D.UWP;
using TextBlockFX.Win2D.UWP.Effects;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Sample.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        private int _index = -1;

        private string[] _sampleTexts;

        private readonly string[] _inOtherWords = new[]
        {
            "Fly me to the 🌕moon",
            "And let me play among the 🌟stars",
            "Let me see what spring is like on",
            "Jupiter and Mars",
            "In other words, hold my hand",
            "In other words, darling, kiss me",
            "Fill my heart with 🎶song",
            "And let me sing forevermore",
            "You are all I long for",
            "All I worship and adore",
            "In other words, please be true",
            "In other words, I ❤️love you",
        };

        private readonly string[] _textsOfMencius = new[]
        {
            "故天将降大任于斯人也",
            "必先苦其心志",
            "劳其筋骨",
            "饿其体肤",
            "空乏其身",
            "行拂乱其所为",
            "所以动心忍性",
            "曾益其所不能"
        };

        private ITextEffect _selectedEffect;
        private int _selectedSampleTextIndex;

        public List<BuiltInEffect> BuiltInEffects => new List<BuiltInEffect>()
        {
            new BuiltInEffect("Default", new Default()),
            new BuiltInEffect("Motion Blur", new MotionBlur()),
            new BuiltInEffect("Blur", new Blur())
        };

        public ITextEffect SelectedEffect
        {
            get => _selectedEffect;
            set
            {
                _selectedEffect = value;
                TBFX.TextEffect = _selectedEffect;
            }
        }

        public int SelectedSampleTextIndex
        {
            get => _selectedSampleTextIndex;
            set
            {
                _selectedSampleTextIndex = value;

                switch (value)
                {
                    default:
                    case 0:
                        _sampleTexts = _inOtherWords;
                        break;
                    case 1:
                        _sampleTexts = _textsOfMencius;
                        break;
                }
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += _timer_Tick;
            _sampleTexts = _inOtherWords;
        }

        private void _timer_Tick(object sender, object e)
        {
            SetSampleText();
            _timer.Stop();
        }

        private void InputBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TBFX.Text = InputBox.Text;
        }

        private void AutoPlayButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (AutoPlayButton.IsChecked == true)
            {
                _index = -1;
                SetSampleText();
                InputBox.IsEnabled = false;
            }
            else
            {
                InputBox.IsEnabled = true;
                _timer.Stop();
            }
        }

        private void SetSampleText()
        {
            _index = (_index + 1) % _sampleTexts.Length;
            string text = _sampleTexts[_index];
            TBFX.Text = text;
        }

        private void TBFX_OnRedrawStateChanged(object sender, RedrawState e)
        {
            if (AutoPlayButton.IsChecked == true && e == RedrawState.Idle)
            {
                _timer.Start();
            }
        }

        private void EffectComboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            EffectComboBox.SelectedIndex = 0;
        }

        private void TextComboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextComboBox.SelectedIndex = 0;
        }
    }

    public class BuiltInEffect
    {
        public string Name { get; }

        public ITextEffect Effect { get; }

        public BuiltInEffect(string name, ITextEffect effect)
        {
            Name = name;
            Effect = effect;
        }
    }
}
