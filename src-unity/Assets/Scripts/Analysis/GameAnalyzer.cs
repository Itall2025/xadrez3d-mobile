using System.Collections.Generic;
using Xadrez3D.Core;

namespace Xadrez3D.Analysis
{
    public sealed class GameAnalyzer
    {
        public GameAnalysisReport Analyze(IReadOnlyList<ChessMove> moves)
        {
            var report = new GameAnalysisReport
            {
                WhiteAccuracy = 78,
                BlackAccuracy = 84
            };

            for (int i = 0; i < moves.Count; i++)
            {
                report.Moves.Add(new MoveAnalysis
                {
                    Ply = i + 1,
                    Quality = MoveQuality.Good,
                    DeltaCentipawns = 0,
                    BestLine = string.Empty
                });
            }

            return report;
        }
    }
}
