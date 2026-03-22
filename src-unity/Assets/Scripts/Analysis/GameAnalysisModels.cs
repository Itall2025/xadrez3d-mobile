using System.Collections.Generic;

namespace Xadrez3D.Analysis
{
    public enum MoveQuality
    {
        Excellent,
        Good,
        Inaccuracy,
        Mistake,
        Blunder
    }

    public sealed class MoveAnalysis
    {
        public int Ply { get; set; }
        public MoveQuality Quality { get; set; }
        public int DeltaCentipawns { get; set; }
        public string BestLine { get; set; } = string.Empty;
    }

    public sealed class GameAnalysisReport
    {
        public int WhiteAccuracy { get; set; }
        public int BlackAccuracy { get; set; }
        public List<MoveAnalysis> Moves { get; } = new List<MoveAnalysis>();
    }
}
