﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI.Effects
#else
namespace TextBlockFX.Win2D.UWP.Effects
#endif
{
    public class Default : ITextEffect
    {
        public TimeSpan AnimationDuration { get; set; } = TimeSpan.FromMilliseconds(800);

        public TimeSpan DelayPerCluster { get; set; } = TimeSpan.FromMilliseconds(10);

        public void Update(string oldText,
            string newText,
            List<TextDiffResult> diffResults,
            CanvasTextLayout oldTextLayout,
            CanvasTextLayout newTextLayout,
            RedrawState state,
            ICanvasAnimatedControl canvas,
            CanvasAnimatedUpdateEventArgs args)
        {

        }

        public void DrawText(string oldText,
            string newText,
            List<TextDiffResult> diffResults,
            CanvasTextLayout oldTextLayout,
            CanvasTextLayout newTextLayout,
            CanvasTextFormat textFormat,
            Color textColor,
            CanvasLinearGradientBrush gradientBrush,
            RedrawState state,
            CanvasDrawingSession drawingSession,
            CanvasAnimatedDrawEventArgs args)
        {
            if (diffResults == null)
                return;

            var ds = args.DrawingSession;

            if (state == RedrawState.Idle)
            {
                DrawIdle(ds,
                    oldTextLayout,
                    newTextLayout,
                    textFormat,
                    textColor,
                    gradientBrush);

                return;
            }

            for (int i = 0; i < diffResults.Count; i++)
            {
                var diffResult = diffResults[i];

                switch (diffResult.Type)
                {
                    case DiffOperationType.Insert:
                        DrawInsert(ds,
                            diffResult.OldGlyphCluster,
                            diffResult.NewGlyphCluster,
                            oldTextLayout,
                            newTextLayout,
                            textFormat,
                            textColor,
                            gradientBrush);
                        break;
                    case DiffOperationType.Remove:
                        DrawRemove(ds,
                            diffResult.OldGlyphCluster,
                            diffResult.NewGlyphCluster,
                            oldTextLayout,
                            newTextLayout,
                            textFormat,
                            textColor,
                            gradientBrush);
                        break;
                    case DiffOperationType.Stay:
                    case DiffOperationType.Move:
                        DrawMove(ds,
                            diffResult.OldGlyphCluster,
                            diffResult.NewGlyphCluster,
                            oldTextLayout,
                            newTextLayout,
                            textFormat,
                            textColor,
                            gradientBrush);
                        break;
                    case DiffOperationType.Update:
                        DrawUpdate(ds,
                            diffResult.OldGlyphCluster,
                            diffResult.NewGlyphCluster,
                            oldTextLayout,
                            newTextLayout,
                            textFormat,
                            textColor,
                            gradientBrush);
                        break;
                }
            }
        }

        private void DrawIdle(CanvasDrawingSession ds,
            CanvasTextLayout oldTextLayout,
            CanvasTextLayout newTextLayout,
            CanvasTextFormat textFormat,
            Color textColor,
            CanvasLinearGradientBrush gradientBrush)
        {
            ds.Transform = Matrix3x2.Identity;
            ds.DrawTextLayout(newTextLayout, 0, 0, textColor);
        }

        private void DrawInsert(CanvasDrawingSession ds,
            GraphemeCluster oldCluster,
            GraphemeCluster newCluster,
            CanvasTextLayout oldTextLayout,
            CanvasTextLayout newTextLayout,
            CanvasTextFormat textFormat,
            Color textColor,
            CanvasLinearGradientBrush gradientBrush)
        {
            if (newCluster == null)
            {
                return;
            }

            float newProgress = Easing.UpdateProgress(newCluster.Progress, Easing.EasingFunction.CubicOut);
            using (ds.CreateLayer(newProgress))
            {
                ds.Transform = Matrix3x2.CreateScale(newProgress,
                    new Vector2((float)(newCluster.LayoutBounds.X +
                                        newCluster.LayoutBounds.Width * 0.5),
                        (float)newCluster.LayoutBounds.Bottom));

                ds.DrawText(
                    newCluster.IsTrimmed
                        ? newTextLayout.GenerateTrimmingSign()
                        : newCluster.Characters,
                    (float)newCluster.DrawBounds.X,
                    (float)newCluster.DrawBounds.Y,
                    textColor,
                    textFormat);

                ds.Transform = Matrix3x2.Identity;
            }
        }

        private void DrawMove(CanvasDrawingSession ds,
            GraphemeCluster oldCluster,
            GraphemeCluster newCluster,
            CanvasTextLayout oldTextLayout,
            CanvasTextLayout newTextLayout,
            CanvasTextFormat textFormat,
            Color textColor,
            CanvasLinearGradientBrush gradientBrush)
        {
            if (oldCluster == null || newCluster == null)
            {
                return;
            }

            float oldProgress = Easing.UpdateProgress(oldCluster.Progress, Easing.EasingFunction.CubicOut);

            var oX = oldCluster.DrawBounds.X;
            var oY = oldCluster.DrawBounds.Y;
            var nX = newCluster.DrawBounds.X;
            var nY = newCluster.DrawBounds.Y;

            var dX = nX - oX;
            var dY = nY - oY;

            ds.DrawText(
                oldCluster.IsTrimmed
                    ? oldTextLayout.GenerateTrimmingSign()
                    : oldCluster.Characters,
                (float)(oX + dX * oldProgress),
                (float)(oY + dY * oldProgress),
                textColor,
                textFormat);
        }

        private void DrawUpdate(CanvasDrawingSession ds,
            GraphemeCluster oldCluster,
            GraphemeCluster newCluster,
            CanvasTextLayout oldTextLayout,
            CanvasTextLayout newTextLayout,
            CanvasTextFormat textFormat,
            Color textColor,
            CanvasLinearGradientBrush gradientBrush)
        {
            if (oldCluster == null || newCluster == null)
            {
                return;
            }

            float oldProgress = Easing.UpdateProgress(oldCluster.Progress, Easing.EasingFunction.CubicOut);
            float newProgress = Easing.UpdateProgress(newCluster.Progress, Easing.EasingFunction.CubicOut);

            using (ds.CreateLayer(1.0f - oldProgress))
            {
                ds.Transform = Matrix3x2.CreateScale(1.0f - oldProgress,
                    new Vector2((float)(oldCluster.LayoutBounds.X +
                                        oldCluster.LayoutBounds.Width * 0.5),
                        (float)oldCluster.LayoutBounds.Bottom));

                ds.DrawText(
                    oldCluster.IsTrimmed
                        ? oldTextLayout.GenerateTrimmingSign()
                        : oldCluster.Characters,
                    (float)oldCluster.DrawBounds.X,
                    (float)oldCluster.DrawBounds.Y,
                    textColor,
                    textFormat);

                ds.Transform = Matrix3x2.Identity;
            }

            using (ds.CreateLayer(newProgress))
            {
                ds.Transform = Matrix3x2.CreateScale(newProgress,
                    new Vector2((float)(newCluster.LayoutBounds.X +
                                        newCluster.LayoutBounds.Width * 0.5),
                        (float)newCluster.LayoutBounds.Bottom));

                ds.DrawText(
                    newCluster.IsTrimmed
                        ? newTextLayout.GenerateTrimmingSign()
                        : newCluster.Characters,
                    (float)newCluster.DrawBounds.X,
                    (float)newCluster.DrawBounds.Y,
                    textColor,
                    textFormat);

                ds.Transform = Matrix3x2.Identity;
            }
        }

        private void DrawRemove(CanvasDrawingSession ds,
            GraphemeCluster oldCluster,
            GraphemeCluster newCluster,
            CanvasTextLayout oldTextLayout,
            CanvasTextLayout newTextLayout,
            CanvasTextFormat textFormat,
            Color textColor,
            CanvasLinearGradientBrush gradientBrush)
        {
            if (oldCluster == null)
            {
                return;
            }

            float oldProgress = Easing.UpdateProgress(oldCluster.Progress, Easing.EasingFunction.CubicOut);

            using (ds.CreateLayer(1.0f - oldProgress))
            {
                ds.Transform = Matrix3x2.CreateScale(1.0f - oldProgress,
                    new Vector2((float)(oldCluster.LayoutBounds.X +
                                        oldCluster.LayoutBounds.Width * 0.5),
                        (float)oldCluster.LayoutBounds.Bottom));
                ds.DrawText(
                    oldCluster.IsTrimmed
                        ? oldTextLayout.GenerateTrimmingSign()
                        : oldCluster.Characters,
                    (float)oldCluster.DrawBounds.X,
                    (float)oldCluster.DrawBounds.Y,
                    textColor,
                    textFormat);

                ds.Transform = Matrix3x2.Identity;
            }
        }
    }
}
