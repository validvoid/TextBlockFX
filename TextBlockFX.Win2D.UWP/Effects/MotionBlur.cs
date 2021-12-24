﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI.Effects
#else
namespace TextBlockFX.Win2D.UWP.Effects
#endif
{
    public class MotionBlur : ITextEffect
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

            foreach (var diffResult in diffResults)
            {
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

            CanvasCommandList cl = new CanvasCommandList(ds);
            float newProgress = 1.0f - Easing.UpdateProgress(newCluster.Progress, Easing.EasingFunction.CubicOut);

            using (CanvasDrawingSession clds = cl.CreateDrawingSession())
            {
                clds.Transform = Matrix3x2.CreateTranslation(0,
                    (float)(newCluster.LayoutBounds.Height * newProgress));

                clds.DrawText(
                    newCluster.IsTrimmed
                        ? newTextLayout.GenerateTrimmingSign()
                        : newCluster.Characters,
                    (float)newCluster.DrawBounds.X,
                    (float)newCluster.DrawBounds.Y,
                    textColor,
                    textFormat);

                clds.Transform = Matrix3x2.Identity;
            }

            using (ds.CreateLayer(1.0f - newProgress))
            {
                using (var blurEffect = new DirectionalBlurEffect())
                {
                    blurEffect.Source = cl;
                    blurEffect.Angle = DegreesToRadians(90);
                    blurEffect.BlurAmount = (float)(newProgress * newCluster.DrawBounds.Height * 0.5f);
                    blurEffect.Optimization = EffectOptimization.Speed;

                    ds.DrawImage(blurEffect);
                }
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
            float newProgress = 1.0f - Easing.UpdateProgress(newCluster.Progress, Easing.EasingFunction.CubicOut);

            CanvasCommandList oCl = new CanvasCommandList(ds);

            using (CanvasDrawingSession clds = oCl.CreateDrawingSession())
            {
                clds.Transform = Matrix3x2.CreateTranslation(0,
                    (float)(-oldCluster.LayoutBounds.Height * 0.5 * oldProgress));

                clds.DrawText(
                    oldCluster.IsTrimmed
                        ? oldTextLayout.GenerateTrimmingSign()
                        : oldCluster.Characters,
                    (float)oldCluster.DrawBounds.X,
                    (float)oldCluster.DrawBounds.Y,
                    textColor,
                    textFormat);

                clds.Transform = Matrix3x2.Identity;
            }

            using (ds.CreateLayer(1.0f - oldProgress))
            {
                using (var blurEffect = new DirectionalBlurEffect())
                {
                    blurEffect.Source = oCl;
                    blurEffect.Angle = DegreesToRadians(90);
                    blurEffect.BlurAmount = (float)(oldProgress * oldCluster.DrawBounds.Height * 0.5f);
                    blurEffect.Optimization = EffectOptimization.Speed;

                    ds.DrawImage(blurEffect);
                }
            }

            CanvasCommandList nCl = new CanvasCommandList(ds);

            using (CanvasDrawingSession clds = nCl.CreateDrawingSession())
            {
                clds.Transform = Matrix3x2.CreateTranslation(0,
                    (float)(newCluster.LayoutBounds.Height * newProgress));

                clds.DrawText(
                    newCluster.IsTrimmed
                        ? newTextLayout.GenerateTrimmingSign()
                        : newCluster.Characters,
                    (float)newCluster.DrawBounds.X,
                    (float)newCluster.DrawBounds.Y,
                    textColor,
                    textFormat);

                clds.Transform = Matrix3x2.Identity;
            }

            using (ds.CreateLayer(1.0f - newProgress))
            {
                using (var blurEffect = new DirectionalBlurEffect())
                {
                    blurEffect.Source = nCl;
                    blurEffect.Angle = DegreesToRadians(90);
                    blurEffect.BlurAmount = (float)(newProgress * newCluster.DrawBounds.Height * 0.5f);
                    blurEffect.Optimization = EffectOptimization.Speed;

                    ds.DrawImage(blurEffect);
                }
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

            CanvasCommandList cl = new CanvasCommandList(ds);
            float oldProgress = Easing.UpdateProgress(oldCluster.Progress, Easing.EasingFunction.CubicOut);

            using (CanvasDrawingSession clds = cl.CreateDrawingSession())
            {
                clds.Transform = Matrix3x2.CreateTranslation(0,
                    (float)(-oldCluster.LayoutBounds.Height * 0.5 * oldProgress));

                clds.DrawText(
                    oldCluster.IsTrimmed
                        ? oldTextLayout.GenerateTrimmingSign()
                        : oldCluster.Characters,
                    (float)oldCluster.DrawBounds.X,
                    (float)oldCluster.DrawBounds.Y,
                    textColor,
                    textFormat);

                clds.Transform = Matrix3x2.Identity;
            }

            using (ds.CreateLayer(1.0f - oldProgress))
            {
                using (var blurEffect = new DirectionalBlurEffect())
                {
                    blurEffect.Source = cl;
                    blurEffect.Angle = DegreesToRadians(90);
                    blurEffect.BlurAmount = (float)(oldProgress * oldCluster.DrawBounds.Height * 0.5f);
                    blurEffect.Optimization = EffectOptimization.Speed;

                    ds.DrawImage(blurEffect);
                }
            }
        }

        private static float DegreesToRadians(float degrees)
        {
            float radians = ((MathF.PI / 180) * degrees);
            return (radians);
        }
    }
}
