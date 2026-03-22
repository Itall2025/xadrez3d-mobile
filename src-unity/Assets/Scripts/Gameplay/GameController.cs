using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Xadrez3D.AI;
using Xadrez3D.Core;

namespace Xadrez3D.Gameplay
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private int difficultyIndex = 1;

        private readonly BoardState _board = new BoardState();
        private IChessEngine _engine;
        private CancellationTokenSource _turnToken;

        private void Awake()
        {
            _engine = new StockfishClient();
            _board.ResetToInitialPosition();
        }

        public void NewGame(int newDifficultyIndex)
        {
            difficultyIndex = Mathf.Clamp(newDifficultyIndex, 0, DifficultyCatalog.Levels.Length - 1);
            _board.ResetToInitialPosition();
        }

        public async Task<bool> PlayHumanMoveAsync(ChessMove move)
        {
            if (!_board.TryApplyMove(move))
            {
                return false;
            }

            await PlayEngineTurnAsync();
            return true;
        }

        private async Task PlayEngineTurnAsync()
        {
            _turnToken?.Cancel();
            _turnToken = new CancellationTokenSource();

            var profile = DifficultyCatalog.Levels[difficultyIndex];
            var fen = ToFen();
            var bestMove = await _engine.GetBestMoveAsync(fen, profile, _turnToken.Token);
            _board.TryApplyMove(bestMove);
        }

        private string ToFen()
        {
            // TODO: gerar FEN real a partir de BoardState.
            return "startpos";
        }
    }
}
