using Windows.Foundation;
using Microsoft.Graphics.Canvas.Text;

#if WINDOWS
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI
#else
namespace TextBlockFX.Win2D.UWP
#endif
{
    internal static class Win2dHelpers
    {
        public static CanvasHorizontalAlignment MapCanvasHorizontalAlignment(TextAlignment alignment)
        {
            switch (alignment)
            {
                case TextAlignment.DetectFromContent:
                case TextAlignment.Center:
                    return CanvasHorizontalAlignment.Center;
                default:
                case TextAlignment.Left:
                    return CanvasHorizontalAlignment.Left;
                case TextAlignment.Right:
                    return CanvasHorizontalAlignment.Right;
                case TextAlignment.Justify:
                    return CanvasHorizontalAlignment.Justified;
            }
        }

        public static CanvasTextDirection MapTextDirection(TextDirection textDirection)
        {
            switch (textDirection)
            {
                default:
                case TextDirection.LeftToRightThenTopToBottom:
                    return CanvasTextDirection.LeftToRightThenTopToBottom;
                case TextDirection.RightToLeftThenTopToBottom:
                    return CanvasTextDirection.RightToLeftThenTopToBottom;
                case TextDirection.LeftToRightThenBottomToTop:
                    return CanvasTextDirection.LeftToRightThenBottomToTop;
                case TextDirection.RightToLeftThenBottomToTop:
                    return CanvasTextDirection.RightToLeftThenBottomToTop;
                case TextDirection.TopToBottomThenLeftToRight:
                    return CanvasTextDirection.TopToBottomThenLeftToRight;
                case TextDirection.BottomToTopThenLeftToRight:
                    return CanvasTextDirection.BottomToTopThenLeftToRight;
                case TextDirection.TopToBottomThenRightToLeft:
                    return CanvasTextDirection.TopToBottomThenRightToLeft;
                case TextDirection.BottomToTopThenRightToLeft:
                    return CanvasTextDirection.BottomToTopThenRightToLeft;
            }
        }

        public static CanvasTextTrimmingGranularity MapTrimmingGranularity(TextTrimming textTrimming)
        {
            switch (textTrimming)
            {
                default:
                case TextTrimming.None:
                    return CanvasTextTrimmingGranularity.None;
                case TextTrimming.CharacterEllipsis:
                    return CanvasTextTrimmingGranularity.Character;
                case TextTrimming.WordEllipsis:
                    return CanvasTextTrimmingGranularity.Word;
                case TextTrimming.Clip:
                    return CanvasTextTrimmingGranularity.None;
            }
        }

        public static CanvasWordWrapping MapWordWrapping(TextWrapping textWrapping)
        {
            switch (textWrapping)
            {
                default:
                case TextWrapping.NoWrap:
                    return CanvasWordWrapping.NoWrap;
                case TextWrapping.Wrap:
                    return CanvasWordWrapping.Character;
                case TextWrapping.WrapWholeWords:
                    return CanvasWordWrapping.WholeWord;
            }
        }

        public static string GenerateTrimmingSign(this CanvasTextLayout layout)
        {
            return "\u2026";
        }
    }
}
