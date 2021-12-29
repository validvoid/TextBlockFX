using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Text;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Sample.Win2D.UWP.Controls
{
    public sealed class FontPickerBox : Control
    {
        private ComboBox _comboBox;

        public FontPickerBox()
        {
            this.DefaultStyleKey = typeof(FontPickerBox);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _comboBox = GetTemplateChild("FontComboBox") as ComboBox;

            var fontFamilyNames = CanvasTextFormat.GetSystemFontFamilies(ApplicationLanguages.Languages).OrderBy(k => k);

            foreach (string fontFamilyName in fontFamilyNames)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = fontFamilyName;
                item.FontFamily = new FontFamily(fontFamilyName);
                _comboBox.Items.Add(item);
            }

            _comboBox.SelectionChanged += ComboBox_SelectionChanged;

            SelectDefaultFont();
        }

        public event SelectionChangedEventHandler SelectionChanged
        {
            add { _comboBox.SelectionChanged += value; }
            remove { _comboBox.SelectionChanged -= value; }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FontFamily = (_comboBox.SelectedItem as ComboBoxItem).FontFamily;
        }

        private void SelectDefaultFont()
        {
            SelectFont("Segoe UI");
        }

        public void SelectFont(string name)
        {
            for (int i = 0; i < _comboBox.Items.Count; ++i)
            {
                ComboBoxItem item = _comboBox.Items[i] as ComboBoxItem;
                if ((item.Content as string) == name)
                {
                    _comboBox.SelectedIndex = i;
                    return;
                }
            }
        }
    }
}
