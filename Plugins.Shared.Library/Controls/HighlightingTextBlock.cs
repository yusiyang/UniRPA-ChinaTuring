using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Plugins.Shared.Library.Controls
{
    [TemplatePart(Name = HighlighttextblockName, Type = typeof(TextBlock))]
    public class HighlightingTextBlock : Control
    {
        private const string HighlighttextblockName = "PART_HighlightTextblock";

        private static readonly DependencyPropertyKey MatchCountPropertyKey
            = DependencyProperty.RegisterReadOnly("MatchCount", typeof(int), typeof(HighlightingTextBlock),
                new PropertyMetadata(0));

        public static readonly DependencyProperty MatchCountProperty
            = MatchCountPropertyKey.DependencyProperty;

        public static readonly DependencyProperty HighlightTextProperty =
            DependencyProperty.Register("HighlightText", typeof(string), typeof(HighlightingTextBlock),
                new PropertyMetadata(string.Empty, OnHighlightTextPropertyChanged));

        public static readonly DependencyProperty TextProperty =
            TextBlock.TextProperty.AddOwner(
                typeof(HighlightingTextBlock),
                new PropertyMetadata(string.Empty, OnTextPropertyChanged));

        public static readonly DependencyProperty TextWrappingProperty = TextBlock.TextWrappingProperty.AddOwner(
            typeof(HighlightingTextBlock),
            new PropertyMetadata(TextWrapping.NoWrap));

        public static readonly DependencyProperty TextTrimmingProperty = TextBlock.TextTrimmingProperty.AddOwner(
            typeof(HighlightingTextBlock),
            new PropertyMetadata(TextTrimming.None));

        public static readonly DependencyProperty HighlightForegroundProperty =
            DependencyProperty.Register("HighlightForeground", typeof(Brush),
                typeof(HighlightingTextBlock),
                new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty HighlightBackgroundProperty =
            DependencyProperty.Register("HighlightBackground", typeof(Brush),
                typeof(HighlightingTextBlock),
                new PropertyMetadata(Brushes.Blue));

        private TextBlock highlightTextBlock;

        static HighlightingTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HighlightingTextBlock),
                new FrameworkPropertyMetadata(typeof(HighlightingTextBlock)));
        }

        public int MatchCount
        {
            get => (int) GetValue(MatchCountProperty);
            protected set => SetValue(MatchCountPropertyKey, value);
        }

        public Brush HighlightBackground
        {
            get => (Brush) GetValue(HighlightBackgroundProperty);
            set => SetValue(HighlightBackgroundProperty, value);
        }

        public Brush HighlightForeground
        {
            get => (Brush) GetValue(HighlightForegroundProperty);
            set => SetValue(HighlightForegroundProperty, value);
        }

        public string HighlightText
        {
            get => (string) GetValue(HighlightTextProperty);
            set => SetValue(HighlightTextProperty, value);
        }

        public string Text
        {
            get => (string) GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public TextWrapping TextWrapping
        {
            get => (TextWrapping) GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }

        public TextTrimming TextTrimming
        {
            get => (TextTrimming) GetValue(TextTrimmingProperty);
            set => SetValue(TextTrimmingProperty, value);
        }

        private static void OnHighlightTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textblock = (HighlightingTextBlock) d;
            textblock.ProcessTextChanged(textblock.Text, e.NewValue as string);
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textblock = (HighlightingTextBlock) d;
            textblock.ProcessTextChanged(e.NewValue as string, textblock.HighlightText);
        }

        private void ProcessTextChanged(string mainText, string highlightText)
        {
            if (highlightTextBlock == null)
                return;
            highlightTextBlock.Inlines.Clear();
            SetValue(MatchCountPropertyKey, 0);
            if (highlightTextBlock == null || string.IsNullOrWhiteSpace(mainText)) return;
            if (string.IsNullOrWhiteSpace(highlightText))
            {
                var completeRun = new Run(mainText);
                highlightTextBlock.Inlines.Add(completeRun);
                return;
            }

            var find = 0;
            var searchTextLength = highlightText.Length;
            while (true)
            {
                var oldFind = find;
                find = mainText.IndexOf(highlightText, find, StringComparison.InvariantCultureIgnoreCase);
                if (find == -1)
                {
                    highlightTextBlock.Inlines.Add(
                        oldFind > 0
                            ? GetRunForText(mainText.Substring(oldFind, mainText.Length - oldFind), false)
                            : GetRunForText(mainText, false));
                    break;
                }

                if (oldFind == find)
                {
                    highlightTextBlock.Inlines.Add(GetRunForText(mainText.Substring(oldFind, searchTextLength), true));
                    SetValue(MatchCountPropertyKey, MatchCount + 1);
                    find = find + searchTextLength;
                    continue;
                }

                highlightTextBlock.Inlines.Add(GetRunForText(mainText.Substring(oldFind, find - oldFind), false));
            }
        }

        private Run GetRunForText(string text, bool isHighlighted)
        {
            var textRun = new Run(text)
            {
                Foreground = isHighlighted ? HighlightForeground : Foreground,
                Background = isHighlighted ? HighlightBackground : Background
            };
            return textRun;
        }

        public override void OnApplyTemplate()
        {
            highlightTextBlock = GetTemplateChild(HighlighttextblockName) as TextBlock;
            if (highlightTextBlock == null)
                return;
            ProcessTextChanged(Text, HighlightText);
        }
    }
}