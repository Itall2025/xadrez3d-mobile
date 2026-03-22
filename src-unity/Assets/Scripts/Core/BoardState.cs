using System;

namespace Xadrez3D.Core
{
    public class BoardState
    {
        private readonly Piece[,] _board = new Piece[8, 8];

        public PieceColor SideToMove { get; private set; } = PieceColor.White;

        public Piece Get(int file, int rank)
        {
            return _board[file, rank];
        }

        public void Set(int file, int rank, Piece piece)
        {
            _board[file, rank] = piece;
        }

        public void ResetToInitialPosition()
        {
            Clear();
            SideToMove = PieceColor.White;

            for (int i = 0; i < 8; i++)
            {
                Set(i, 1, new Piece { Type = PieceType.Pawn, Color = PieceColor.White });
                Set(i, 6, new Piece { Type = PieceType.Pawn, Color = PieceColor.Black });
            }

            PlaceBackRank(PieceColor.White, 0);
            PlaceBackRank(PieceColor.Black, 7);
        }

        public bool TryApplyMove(ChessMove move)
        {
            if (!IsInside(move.From) || !IsInside(move.To))
            {
                return false;
            }

            var piece = Get(move.From.File, move.From.Rank);
            if (piece.IsEmpty || piece.Color != SideToMove)
            {
                return false;
            }

            // Placeholder: legal move validation will be done by MoveGenerator.
            Set(move.To.File, move.To.Rank, piece);
            Set(move.From.File, move.From.Rank, Piece.Empty);
            SideToMove = SideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
            return true;
        }

        private void Clear()
        {
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    _board[file, rank] = Piece.Empty;
                }
            }
        }

        private void PlaceBackRank(PieceColor color, int rank)
        {
            var order = new[]
            {
                PieceType.Rook,
                PieceType.Knight,
                PieceType.Bishop,
                PieceType.Queen,
                PieceType.King,
                PieceType.Bishop,
                PieceType.Knight,
                PieceType.Rook
            };

            for (int file = 0; file < 8; file++)
            {
                Set(file, rank, new Piece { Type = order[file], Color = color });
            }
        }

        private static bool IsInside(Square square)
        {
            return square.File >= 0 && square.File < 8 && square.Rank >= 0 && square.Rank < 8;
        }
    }
}
