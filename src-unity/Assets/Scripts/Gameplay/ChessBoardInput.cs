using System.Threading.Tasks;
using UnityEngine;
using Xadrez3D.Core;

namespace Xadrez3D.Gameplay
{
    public sealed class ChessBoardInput : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private GameController gameController;
        [SerializeField] private BoardRenderer3D boardRenderer;

        private bool _hasSelection;
        private Square _selectedSquare;
        private bool _isSubmitting;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (gameController == null)
            {
                gameController = FindObjectOfType<GameController>();
            }

            if (boardRenderer == null)
            {
                boardRenderer = FindObjectOfType<BoardRenderer3D>();
            }
        }

        private void OnEnable()
        {
            if (gameController != null)
            {
                gameController.BoardChanged += OnBoardChanged;
            }
        }

        private void OnDisable()
        {
            if (gameController != null)
            {
                gameController.BoardChanged -= OnBoardChanged;
            }
        }

        private void Update()
        {
            if (_isSubmitting || gameController == null || boardRenderer == null || targetCamera == null)
            {
                return;
            }

            if (!TryGetPointerDownRay(targetCamera, out var ray))
            {
                return;
            }

            if (!Physics.Raycast(ray, out var hit, 200f))
            {
                return;
            }

            if (!boardRenderer.TryGetSquareFromHit(hit, out var clickedSquare))
            {
                return;
            }

            HandleSquareClicked(clickedSquare);
        }

        private void HandleSquareClicked(Square clickedSquare)
        {
            var board = gameController.Board;
            var pieceOnClicked = board.Get(clickedSquare.File, clickedSquare.Rank);

            if (!_hasSelection)
            {
                if (pieceOnClicked.IsEmpty || pieceOnClicked.Color != gameController.HumanColor || board.SideToMove != gameController.HumanColor)
                {
                    return;
                }

                _selectedSquare = clickedSquare;
                _hasSelection = true;
                boardRenderer.SetSelectedSquare(_selectedSquare);
                return;
            }

            if (_selectedSquare.File == clickedSquare.File && _selectedSquare.Rank == clickedSquare.Rank)
            {
                ClearSelection();
                return;
            }

            if (!pieceOnClicked.IsEmpty && pieceOnClicked.Color == gameController.HumanColor)
            {
                _selectedSquare = clickedSquare;
                boardRenderer.SetSelectedSquare(_selectedSquare);
                return;
            }

            var promotion = InferPromotion(_selectedSquare, clickedSquare);
            var move = new ChessMove(_selectedSquare, clickedSquare, promotion);
            _ = SubmitMoveAsync(move);
        }

        private async Task SubmitMoveAsync(ChessMove move)
        {
            _isSubmitting = true;
            try
            {
                var accepted = await gameController.PlayHumanMoveAsync(move);
                if (accepted)
                {
                    ClearSelection();
                }
            }
            finally
            {
                _isSubmitting = false;
            }
        }

        private PieceType InferPromotion(Square from, Square to)
        {
            var piece = gameController.Board.Get(from.File, from.Rank);
            if (piece.Type != PieceType.Pawn)
            {
                return PieceType.None;
            }

            if ((piece.Color == PieceColor.White && to.Rank == 7) || (piece.Color == PieceColor.Black && to.Rank == 0))
            {
                return PieceType.Queen;
            }

            return PieceType.None;
        }

        private void OnBoardChanged()
        {
            if (_hasSelection)
            {
                var piece = gameController.Board.Get(_selectedSquare.File, _selectedSquare.Rank);
                if (piece.IsEmpty || piece.Color != gameController.HumanColor)
                {
                    ClearSelection();
                }
            }
        }

        private void ClearSelection()
        {
            _hasSelection = false;
            boardRenderer.SetSelectedSquare(null);
        }

        private static bool TryGetPointerDownRay(Camera cameraRef, out Ray ray)
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    ray = cameraRef.ScreenPointToRay(touch.position);
                    return true;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                ray = cameraRef.ScreenPointToRay(Input.mousePosition);
                return true;
            }

            ray = default;
            return false;
        }
    }
}
