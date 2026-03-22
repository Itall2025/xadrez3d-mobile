using System.Collections.Generic;
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
        [SerializeField] private string backendApiBaseUrl = "https://xadrez3d-mobile-api.onrender.com";

        private readonly BoardState _board = new BoardState();
        private readonly List<string> _uciMoveHistory = new List<string>();
        private IChessEngine _engine;
        private CancellationTokenSource _turnToken;

        private void Awake()
        {
            var localFallback = new StockfishClient();
            _engine = new BackendChessEngine(backendApiBaseUrl, localFallback);
            _board.ResetToInitialPosition();
        }

        public void NewGame(int newDifficultyIndex)
        {
            _turnToken?.Cancel();
            difficultyIndex = Mathf.Clamp(newDifficultyIndex, 0, DifficultyCatalog.Levels.Length - 1);
            _board.ResetToInitialPosition();
            _uciMoveHistory.Clear();
        }

        public async Task<bool> PlayHumanMoveAsync(ChessMove move)
        {
            if (!_board.TryApplyMove(move))
            {
                return false;
            }

            _uciMoveHistory.Add(MoveNotation.ToUci(move));
            await PlayEngineTurnAsync();
            return true;
        }

        private async Task PlayEngineTurnAsync()
        {
            _turnToken?.Cancel();
            _turnToken = new CancellationTokenSource();

            var profile = DifficultyCatalog.Levels[difficultyIndex];
            var fen = ToFen();
            var bestMove = await _engine.GetBestMoveAsync(fen, profile, _turnToken.Token, _uciMoveHistory);
            if (_board.TryApplyMove(bestMove))
            {
                _uciMoveHistory.Add(MoveNotation.ToUci(bestMove));
            }
        }

        private string ToFen()
        {
            return _board.ToFen();
        }
    }
}
