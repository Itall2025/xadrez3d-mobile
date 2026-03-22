using System.Threading;
using System.Threading.Tasks;
using Xadrez3D.Core;

namespace Xadrez3D.AI
{
    public interface IChessEngine
    {
        Task<ChessMove> GetBestMoveAsync(string fen, DifficultyProfile profile, CancellationToken ct);
    }
}
