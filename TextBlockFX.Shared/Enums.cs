namespace TextBlockFX
{
    public enum RedrawState
    {
        Idle,
        Animating,
        TextChanged,
        LayoutChanged,
    }

    public enum TextDirection
    {
        /// <summary>Text is read from left to right and lines flow from top to bottom.</summary>
        LeftToRightThenTopToBottom,
        /// <summary>Text is read from right to left and lines flow from top to bottom.</summary>
        RightToLeftThenTopToBottom,
        /// <summary>Text is read from left to right and lines flow from bottom to top.</summary>
        LeftToRightThenBottomToTop,
        /// <summary>Text is read from right to left and lines flow from bottom to top.</summary>
        RightToLeftThenBottomToTop,
        /// <summary>Text is read from top to bottom and lines flow from left to right.</summary>
        TopToBottomThenLeftToRight,
        /// <summary>Text is read from bottom to top and lines flow from left to right.</summary>
        BottomToTopThenLeftToRight,
        /// <summary>Text is read from top to bottom and lines flow from right to left.</summary>
        TopToBottomThenRightToLeft,
        /// <summary>Text is read from bottom to top and lines flow from right to left.</summary>
        BottomToTopThenRightToLeft,
    }
}
