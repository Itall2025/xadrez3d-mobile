using System;
using System.Threading;
using System.Threading.Tasks;
using Xadrez3D.Core;

namespace Xadrez3D.AI
{
    public class StockfishClient : IChessEngine
    {
        public Task<ChessMove> GetBestMoveAsync(string fen, DifficultyProfile profile, CancellationToken ct)
        {
            // TODO: Integrar processo UCI real (stockfish) e mapear "bestmove" para ChessMove.
            // Placeholder retorna lance nulo para manter o pipeline compilavel.
            _ = fen;
            _ = profile;
            _ = ct;
            return Task.FromResult(new ChessMove(new Square(0, 1), new Square(0, 2)));
        }
    }
}
