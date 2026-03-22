using System;
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
        [SerializeField] private PieceColor humanColor = PieceColor.White;

        private readonly BoardState _board = new BoardState();
        private readonly List<string> _uciMoveHistory = new List<string>();
        private IChessEngine _engine;
        private CancellationTokenSource _turnToken;

        public event Action BoardChanged;

        public BoardState Board => _board;
        public PieceColor HumanColor => humanColor;
        public bool IsEngineThinking { get; private set; }
        public int DifficultyIndex => difficultyIndex;

        private void Awake()
        {
            var localFallback = new StockfishClient();
            _engine = new BackendChessEngine(backendApiBaseUrl, localFallback);
            _board.ResetToInitialPosition();
            NotifyBoardChanged();
        }

        public void NewGame(int newDifficultyIndex)
        {
            _turnToken?.Cancel();
            IsEngineThinking = false;
            difficultyIndex = Mathf.Clamp(newDifficultyIndex, 0, DifficultyCatalog.Levels.Length - 1);
            _board.ResetToInitialPosition();
            _uciMoveHistory.Clear();
            NotifyBoardChanged();
        }

        public async Task<bool> PlayHumanMoveAsync(ChessMove move)
        {
            if (IsEngineThinking || _board.SideToMove != humanColor)
            {
                return false;
            }

            if (!_board.TryApplyMove(move))
            {
                return false;
            }

            _uciMoveHistory.Add(MoveNotation.ToUci(move));
            NotifyBoardChanged();
            await PlayEngineTurnAsync();
            return true;
        }

        private async Task PlayEngineTurnAsync()
        {
            _turnToken?.Cancel();
            _turnToken = new CancellationTokenSource();
            IsEngineThinking = true;
            NotifyBoardChanged();

            try
            {
                var profile = DifficultyCatalog.Levels[difficultyIndex];
                var fen = ToFen();
                var bestMove = await _engine.GetBestMoveAsync(fen, profile, _turnToken.Token, _uciMoveHistory);
                if (_board.TryApplyMove(bestMove))
                {
                    _uciMoveHistory.Add(MoveNotation.ToUci(bestMove));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Engine move failed: {ex.Message}");
            }
            finally
            {
                IsEngineThinking = false;
                NotifyBoardChanged();
            }
        }

        private string ToFen()
        {
            return _board.ToFen();
        }

        private void NotifyBoardChanged()
        {
            BoardChanged?.Invoke();
        }
    }
}
