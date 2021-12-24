using System.Collections.Generic;
using System.Linq;
using Microsoft.Graphics.Canvas.Text;

#if WINDOWS
namespace TextBlockFX.Win2D.WinUI
#else
namespace TextBlockFX.Win2D.UWP
#endif
{
    internal static class TextRenderingHelper
    {
        public static List<GraphemeCluster> GenerateGraphemeClusters(string source, CanvasTextLayout textLayout)
        {
            List<GraphemeCluster> graphemeClusters = new List<GraphemeCluster>();

            int clusterCount = textLayout.ClusterMetrics.Length;
            int offset = 0;

            for (int i = 0; i < clusterCount; i++)
            {
                GraphemeCluster cluster = new GraphemeCluster();
                textLayout.GetCaretPosition(offset, true, out CanvasTextLayoutRegion clusterLayoutRegion);

                // Detect trimmed clusters
                if (i > 0 && offset > 0 && clusterLayoutRegion.CharacterCount == 0)
                {
                    graphemeClusters.Last().IsTrimmed = true;
                    break;
                }
                else
                {
                    cluster.Characters = source.Substring(clusterLayoutRegion.CharacterIndex, clusterLayoutRegion.CharacterCount);
                    cluster.Offset = clusterLayoutRegion.CharacterIndex;
                    cluster.Length = clusterLayoutRegion.CharacterCount;
                    cluster.Bounds = clusterLayoutRegion.LayoutBounds;
                    graphemeClusters.Add(cluster);

                    offset += clusterLayoutRegion.CharacterCount;
                }
            }

            return graphemeClusters;
        }
    }
}
