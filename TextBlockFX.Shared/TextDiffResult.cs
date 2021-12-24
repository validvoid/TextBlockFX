using System.Diagnostics;

namespace TextBlockFX
{
    [DebuggerDisplay("{Type} - {OldGlyphCluster.Characters} | {NewGlyphCluster.Characters}")]
    public class TextDiffResult
    {
        public DiffOperationType Type { get; set; }

        public GraphemeCluster OldGlyphCluster { get; set; }

        public GraphemeCluster NewGlyphCluster { get; set; }

        public int OldClusterOffset { get; set; }
        
        public int NewClusterOffset { get; set; }

        public TextDiffResult( DiffOperationType type)
        {
            Type = type;
        }

        public TextDiffResult(DiffOperationType type, int oldClusterOffset, int newClusterOffset)
        {
            Type = type;
            OldClusterOffset = oldClusterOffset;
            NewClusterOffset = newClusterOffset;
        }

        public TextDiffResult(DiffOperationType type, GraphemeCluster oldGlyphCluster, GraphemeCluster newGlyphCluster)
        {
            Type = type;
            OldGlyphCluster = oldGlyphCluster;
            NewGlyphCluster = newGlyphCluster;
        }

        public TextDiffResult(DiffOperationType type, GraphemeCluster oldGlyphCluster, GraphemeCluster newGlyphCluster, int oldClusterOffset, int newClusterOffset)
        {
            Type = type;
            OldGlyphCluster = oldGlyphCluster;
            NewGlyphCluster = newGlyphCluster;
            OldClusterOffset = oldClusterOffset;
            NewClusterOffset = newClusterOffset;
        }
    }
}
