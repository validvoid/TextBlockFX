using System;
using System.Collections.Generic;
using System.Linq;
using TextBlockFX;
using TextBlockFX.Win2D.UWP;
using TextBlockFX.Win2D.UWP.Effects;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Pivot = TextBlockFX.Win2D.UWP.Effects.Pivot;

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

        private readonly string[] _textsOfMakenaide = new[]
        {
            "ふとした瞬間に 視線がぶつかる",
            "幸福のときめき 覚えているでしょ",
            "パステルカラーの季節に恋した",
            "あの日のように 輝いてる",
            "あなたでいてね",
            "負けないで もう少し",
            "最後まで 走り抜けて",
            "どんなに 離れてても",
            "心は そばにいるわ",
            "追いかけて 遥かな夢を",
        };

        private readonly string[] _textsOfOdeToJoy = new[]
        {
            "Wem der große Wurf gelungen,",
            "Eines Freundes Freund zu sein;",
            "Wer ein holdes Weib errungen,",
            "Mische seinen Jubel ein!",
            "Ja, wer auch nur eine Seele",
            "Sein nennt auf dem Erdenrund!",
            "Und wer's nie gekonnt, der stehle",
            "Weinend sich aus diesem Bund!",
        };

        private ITextEffect _selectedEffect;
        private int _selectedSampleTextIndex;

        public List<BuiltInEffect> BuiltInEffects => new List<BuiltInEffect>()
        {
            new BuiltInEffect("Default", new Default()),
            new BuiltInEffect("Motion Blur", new MotionBlur()),
            new BuiltInEffect("Blur", new Blur()),
            new BuiltInEffect("Elastic", new Elastic()),
            new BuiltInEffect("Zoom", new Zoom()),
            new BuiltInEffect("Pivot", new Pivot())
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
                    case 2:
                        _sampleTexts = _textsOfMakenaide;
                        break;
                    case 3:
                        _sampleTexts = _textsOfOdeToJoy;
                        break;
                }
            }
        }

        public List<ComboWrapper<FontStretch>> FontStretches => GetEnumAsList<FontStretch>();

        public List<ComboWrapper<FontStyle>> FontStyles => GetEnumAsList<FontStyle>();

        public List<ComboWrapper<FontWeight>> FontWeightsList => new List<ComboWrapper<FontWeight>>()
        {
            new ComboWrapper<FontWeight>("ExtraBlack", FontWeights.ExtraBlack),
            new ComboWrapper<FontWeight>("Black", FontWeights.Black),
            new ComboWrapper<FontWeight>("ExtraBold", FontWeights.ExtraBold),
            new ComboWrapper<FontWeight>("Bold", FontWeights.Bold),
            new ComboWrapper<FontWeight>("SemiBold", FontWeights.SemiBold),
            new ComboWrapper<FontWeight>("Medium", FontWeights.Medium),
            new ComboWrapper<FontWeight>("Normal", FontWeights.Normal),
            new ComboWrapper<FontWeight>("SemiLight", FontWeights.SemiLight),
            new ComboWrapper<FontWeight>("Light", FontWeights.Light),
            new ComboWrapper<FontWeight>("ExtraLight", FontWeights.ExtraLight),
            new ComboWrapper<FontWeight>("Thin", FontWeights.Thin),
        };

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += _timer_Tick;
            _sampleTexts = _inOtherWords;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < FontStretches.Count; i++)
            {
                if (FontStretches[i].Value == FontStretch.Normal)
                {
                    FontStretchComboBox.SelectedIndex = i;
                }
            }

            for (int i = 0; i < FontStyles.Count; i++)
            {
                if (FontStyles[i].Value == FontStyle.Normal)
                {
                    FontStyleComboBox.SelectedIndex = i;
                }
            }

            for (int i = 0; i < FontWeightsList.Count; i++)
            {
                if (FontWeightsList[i].Value.Weight == FontWeights.Normal.Weight)
                {
                    FontWeightComboBox.SelectedIndex = i;
                }
            }
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

        private static List<ComboWrapper<T>> GetEnumAsList<T>()
        {
            var names = Enum.GetNames(typeof(T)).ToList();
            var values = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            return names.Zip(values, (k, v) => new ComboWrapper<T>(k, v)).ToList();
        }
    }

    public class ComboWrapper<T>
    {
        public string Name { get; }

        public T Value { get; }

        public ComboWrapper(string name, T value)
        {
            Name = name;
            Value = value;
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
