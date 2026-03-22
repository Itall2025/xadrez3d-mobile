using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xadrez3D.Core;

namespace Xadrez3D.AI
{
    public class StockfishClient : IChessEngine
    {
        public Task<ChessMove> GetBestMoveAsync(string fen, DifficultyProfile profile, CancellationToken ct, IReadOnlyList<string> moveHistoryUci = null)
        {
            // TODO: Integrar processo UCI real (stockfish) e mapear "bestmove" para ChessMove.
            // Placeholder retorna lance nulo para manter o pipeline compilavel.
            _ = fen;
            _ = profile;
            _ = ct;
            _ = moveHistoryUci;
            return Task.FromResult(new ChessMove(new Square(0, 1), new Square(0, 2)));
        }
    }
}
