﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TextBlockFX
{
    internal class GraphemeClusterDiff
    {
        private class TableEntry
        {
            public int OldCounter { get; set; }

            public int NewCounter { get; set; }

            public Queue<int> OLNO { get; set; } = new Queue<int>();
        }

        // Based on Paul Heckel's diff algorithm
        public static List<TextDiffResult> Diff(IList<GraphemeCluster> oldClusters, IList<GraphemeCluster> newClusters)
        {
            List<TextDiffResult> results = new List<TextDiffResult>();

            if (!oldClusters.Any() && !newClusters.Any())
            {
                return results;
            }

            Dictionary<string, TableEntry> table = new Dictionary<string, TableEntry>();
            var oa = new int?[oldClusters.Count];
            var na = new int?[newClusters.Count];

            // Pass 1
            foreach (var cluster in newClusters)
            {
                if (!table.TryGetValue(cluster.Characters, out var entry))
                {
                    table[cluster.Characters] = entry = new TableEntry();
                }

                entry.NewCounter += 1;
            }

            // Pass 2
            for (int i = 0; i < oldClusters.Count; i++)
            {
                var cluster = oldClusters[i];
                if (!table.TryGetValue(cluster.Characters, out var entry))
                {
                    table[cluster.Characters] = entry = new TableEntry();
                }

                entry.OldCounter += 1;

                entry.OLNO.Enqueue(i);
            }

            // Pass 3
            for (int i = 0; i < na.Length; i++)
            {
                var te = table[newClusters[i].Characters];
                if (te.OldCounter != 0 && te.NewCounter != 0 && te.OLNO.Count > 0)
                {
                    int oldIndex = te.OLNO.Dequeue();
                    na[i] = oldIndex;
                    oa[oldIndex] = i;
                }
            }

            // Pass 4
            for (int i = 0; i < na.Length - 1; i++)
            {
                if (na[i].HasValue)
                {
                    var j = na[i].Value;

                    if ((j < oa.Length - 1) &&
                        !na[i + 1].HasValue &&
                        !oa[j + 1].HasValue &&
                        (Equals(newClusters[i + 1], oldClusters[j + 1])))
                    {
                        na[i + 1] = j + 1;
                        oa[j + 1] = i + 1;
                    }
                }
            }

            // Pass 5
            for (int i = na.Length - 1; i > 0; i--)
            {
                if (na[i].HasValue)
                {
                    var j = na[i].Value;

                    if (j > 0 &&
                        !na[i - 1].HasValue &&
                        !oa[j - 1].HasValue &&
                        (Equals(newClusters[i - 1], oldClusters[j - 1])))
                    {
                        na[i - 1] = j - 1;
                        oa[j - 1] = i - 1;
                    }
                }
            }

            //Final pass
            var removeOffsets = new int[oa.Length];
            int runningOffset = 0;

            for (int i = 0; i < oa.Length; i++)
            {
                removeOffsets[i] = runningOffset;
                if (!oa[i].HasValue)
                {
                    var removeOp = new TextDiffResult(DiffOperationType.Remove, i, i);
                    removeOp.OldGlyphCluster = oldClusters[i];
                    results.Add(removeOp);
                    runningOffset += 1;
                }
            }

            runningOffset = 0;

            for (int i = 0; i < na.Length; i++)
            {
                if (na[i].HasValue)
                {
                    var j = na[i].Value;
                    if (!Equals(oldClusters[j], newClusters[i]))
                    {
                        var updateOp = new TextDiffResult(DiffOperationType.Update, i, i);
                        updateOp.OldGlyphCluster = oldClusters[j];
                        updateOp.NewGlyphCluster = newClusters[i];
                        results.Add(updateOp);
                    }

                    if (i != j)
                    {
                        var moveOp = new TextDiffResult(DiffOperationType.Move, j, i);
                        moveOp.OldGlyphCluster = oldClusters[j];
                        moveOp.NewGlyphCluster = newClusters[i];
                        results.Add(moveOp);
                        continue;
                    }
                    else
                    {
                        var stayOp = new TextDiffResult(DiffOperationType.Stay, i, i);
                        stayOp.OldGlyphCluster = oldClusters[j];
                        stayOp.NewGlyphCluster = newClusters[i];
                        results.Add(stayOp);
                    }

                    if (i != (j + runningOffset - removeOffsets[j]))
                    {
                        var moveOp = new TextDiffResult(DiffOperationType.Move, j, i);
                        moveOp.OldGlyphCluster = oldClusters[j];
                        moveOp.NewGlyphCluster = newClusters[i];
                        results.Add(moveOp);
                    }
                }
                else
                {
                    var insertOp = new TextDiffResult(DiffOperationType.Insert, i, i);
                    insertOp.NewGlyphCluster = newClusters[i];
                    results.Add(insertOp);
                    runningOffset += 1;
                }
            }

            return results;
        }
    }
}
