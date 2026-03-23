using System;
using System.Text;

namespace Xadrez3D.Core
{
    public class BoardState
    {
        private readonly Piece[,] _board = new Piece[8, 8];
        private bool _whiteKingMoved;
        private bool _blackKingMoved;
        private bool _whiteQueenRookMoved;
        private bool _whiteKingRookMoved;
        private bool _blackQueenRookMoved;
        private bool _blackKingRookMoved;
        private Square? _enPassantTarget;

        public PieceColor SideToMove { get; private set; } = PieceColor.White;
        public int HalfmoveClock { get; private set; }
        public int FullmoveNumber { get; private set; } = 1;

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
            HalfmoveClock = 0;
            FullmoveNumber = 1;
            _enPassantTarget = null;
            _whiteKingMoved = false;
            _blackKingMoved = false;
            _whiteQueenRookMoved = false;
            _whiteKingRookMoved = false;
            _blackQueenRookMoved = false;
            _blackKingRookMoved = false;

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

            var target = Get(move.To.File, move.To.Rank);
            if (!target.IsEmpty && target.Color == piece.Color)
            {
                return false;
            }

            if (!IsPseudoLegalMove(piece, move, target))
            {
                return false;
            }

            var isCapture = !target.IsEmpty;
            var isPawnMove = piece.Type == PieceType.Pawn;
            var isEnPassantCapture = isPawnMove && target.IsEmpty && move.From.File != move.To.File;
            var isCastling = piece.Type == PieceType.King && Math.Abs(move.To.File - move.From.File) == 2;

            if (isEnPassantCapture)
            {
                var captureRank = piece.Color == PieceColor.White ? move.To.Rank - 1 : move.To.Rank + 1;
                if (captureRank >= 0 && captureRank < 8)
                {
                    Set(move.To.File, captureRank, Piece.Empty);
                    isCapture = true;
                }
            }

            UpdateCastlingStateForMove(piece, move);
            UpdateCastlingStateForCapture(target, move.To);

            Set(move.To.File, move.To.Rank, piece);
            Set(move.From.File, move.From.Rank, Piece.Empty);

            if (isPawnMove && (move.To.Rank == 7 || move.To.Rank == 0) && move.Promotion != PieceType.None)
            {
                Set(move.To.File, move.To.Rank, new Piece
                {
                    Type = move.Promotion,
                    Color = piece.Color
                });
            }

            if (isCastling)
            {
                MoveRookForCastling(piece.Color, move);
            }

            if (isPawnMove && Math.Abs(move.To.Rank - move.From.Rank) == 2)
            {
                var targetRank = (move.To.Rank + move.From.Rank) / 2;
                _enPassantTarget = new Square(move.From.File, targetRank);
            }
            else
            {
                _enPassantTarget = null;
            }

            HalfmoveClock = (isPawnMove || isCapture) ? 0 : HalfmoveClock + 1;
            if (SideToMove == PieceColor.Black)
            {
                FullmoveNumber++;
            }

            SideToMove = SideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
            return true;
        }

        public string ToFen()
        {
            var boardBuilder = new StringBuilder();

            for (int rank = 7; rank >= 0; rank--)
            {
                int emptyCount = 0;
                for (int file = 0; file < 8; file++)
                {
                    var piece = Get(file, rank);
                    if (piece.IsEmpty)
                    {
                        emptyCount++;
                        continue;
                    }

                    if (emptyCount > 0)
                    {
                        boardBuilder.Append(emptyCount);
                        emptyCount = 0;
                    }

                    boardBuilder.Append(ToFenChar(piece));
                }

                if (emptyCount > 0)
                {
                    boardBuilder.Append(emptyCount);
                }

                if (rank > 0)
                {
                    boardBuilder.Append('/');
                }
            }

            var castling = BuildCastlingRights();
            var enPassant = _enPassantTarget.HasValue ? ToSquareName(_enPassantTarget.Value) : "-";
            var side = SideToMove == PieceColor.White ? "w" : "b";
            return $"{boardBuilder} {side} {castling} {enPassant} {HalfmoveClock} {FullmoveNumber}";
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

        private bool IsPseudoLegalMove(Piece piece, ChessMove move, Piece target)
        {
            return piece.Type switch
            {
                PieceType.Pawn => IsPseudoLegalPawnMove(piece, move, target),
                PieceType.Knight => IsPseudoLegalKnightMove(move),
                PieceType.Bishop => IsPseudoLegalBishopMove(move),
                PieceType.Rook => IsPseudoLegalRookMove(move),
                PieceType.Queen => IsPseudoLegalQueenMove(move),
                PieceType.King => IsPseudoLegalKingMove(piece, move),
                _ => false
            };
        }

        private bool IsPseudoLegalPawnMove(Piece piece, ChessMove move, Piece target)
        {
            var direction = piece.Color == PieceColor.White ? 1 : -1;
            var startRank = piece.Color == PieceColor.White ? 1 : 6;
            var fileDelta = move.To.File - move.From.File;
            var rankDelta = move.To.Rank - move.From.Rank;

            if (fileDelta == 0)
            {
                if (!target.IsEmpty)
                {
                    return false;
                }

                if (rankDelta == direction)
                {
                    return true;
                }

                if (move.From.Rank == startRank && rankDelta == 2 * direction)
                {
                    var middleRank = move.From.Rank + direction;
                    return Get(move.From.File, middleRank).IsEmpty;
                }

                return false;
            }

            if (Math.Abs(fileDelta) == 1 && rankDelta == direction)
            {
                if (!target.IsEmpty)
                {
                    return true;
                }

                return _enPassantTarget.HasValue &&
                       _enPassantTarget.Value.File == move.To.File &&
                       _enPassantTarget.Value.Rank == move.To.Rank;
            }

            return false;
        }

        private static bool IsPseudoLegalKnightMove(ChessMove move)
        {
            var fileDelta = Math.Abs(move.To.File - move.From.File);
            var rankDelta = Math.Abs(move.To.Rank - move.From.Rank);
            return (fileDelta == 1 && rankDelta == 2) || (fileDelta == 2 && rankDelta == 1);
        }

        private bool IsPseudoLegalBishopMove(ChessMove move)
        {
            var fileDelta = Math.Abs(move.To.File - move.From.File);
            var rankDelta = Math.Abs(move.To.Rank - move.From.Rank);
            if (fileDelta != rankDelta || fileDelta == 0)
            {
                return false;
            }

            return IsPathClear(move.From, move.To);
        }

        private bool IsPseudoLegalRookMove(ChessMove move)
        {
            var sameFile = move.To.File == move.From.File;
            var sameRank = move.To.Rank == move.From.Rank;
            if (sameFile == sameRank)
            {
                return false;
            }

            return IsPathClear(move.From, move.To);
        }

        private bool IsPseudoLegalQueenMove(ChessMove move)
        {
            return IsPseudoLegalBishopMove(move) || IsPseudoLegalRookMove(move);
        }

        private bool IsPseudoLegalKingMove(Piece piece, ChessMove move)
        {
            var fileDelta = Math.Abs(move.To.File - move.From.File);
            var rankDelta = Math.Abs(move.To.Rank - move.From.Rank);

            if (fileDelta <= 1 && rankDelta <= 1 && (fileDelta + rankDelta) > 0)
            {
                return true;
            }

            if (rankDelta != 0 || fileDelta != 2)
            {
                return false;
            }

            if (piece.Color == PieceColor.White)
            {
                if (_whiteKingMoved || move.From.Rank != 0 || move.From.File != 4)
                {
                    return false;
                }

                if (move.To.File == 6)
                {
                    return !_whiteKingRookMoved && Get(7, 0).Type == PieceType.Rook && IsPathClear(new Square(4, 0), new Square(7, 0));
                }

                if (move.To.File == 2)
                {
                    return !_whiteQueenRookMoved && Get(0, 0).Type == PieceType.Rook && IsPathClear(new Square(0, 0), new Square(4, 0));
                }

                return false;
            }

            if (_blackKingMoved || move.From.Rank != 7 || move.From.File != 4)
            {
                return false;
            }

            if (move.To.File == 6)
            {
                return !_blackKingRookMoved && Get(7, 7).Type == PieceType.Rook && IsPathClear(new Square(4, 7), new Square(7, 7));
            }

            if (move.To.File == 2)
            {
                return !_blackQueenRookMoved && Get(0, 7).Type == PieceType.Rook && IsPathClear(new Square(0, 7), new Square(4, 7));
            }

            return false;
        }

        private bool IsPathClear(Square from, Square to)
        {
            var fileStep = Math.Sign(to.File - from.File);
            var rankStep = Math.Sign(to.Rank - from.Rank);

            var file = from.File + fileStep;
            var rank = from.Rank + rankStep;
            while (file != to.File || rank != to.Rank)
            {
                if (!Get(file, rank).IsEmpty)
                {
                    return false;
                }

                file += fileStep;
                rank += rankStep;
            }

            return true;
        }

        private void UpdateCastlingStateForMove(Piece piece, ChessMove move)
        {
            if (piece.Type == PieceType.King)
            {
                if (piece.Color == PieceColor.White)
                {
                    _whiteKingMoved = true;
                }
                else
                {
                    _blackKingMoved = true;
                }
            }

            if (piece.Type != PieceType.Rook)
            {
                return;
            }

            if (piece.Color == PieceColor.White && move.From.Rank == 0)
            {
                if (move.From.File == 0) _whiteQueenRookMoved = true;
                if (move.From.File == 7) _whiteKingRookMoved = true;
            }

            if (piece.Color == PieceColor.Black && move.From.Rank == 7)
            {
                if (move.From.File == 0) _blackQueenRookMoved = true;
                if (move.From.File == 7) _blackKingRookMoved = true;
            }
        }

        private void UpdateCastlingStateForCapture(Piece capturedPiece, Square targetSquare)
        {
            if (capturedPiece.Type != PieceType.Rook)
            {
                return;
            }

            if (capturedPiece.Color == PieceColor.White && targetSquare.Rank == 0)
            {
                if (targetSquare.File == 0) _whiteQueenRookMoved = true;
                if (targetSquare.File == 7) _whiteKingRookMoved = true;
            }

            if (capturedPiece.Color == PieceColor.Black && targetSquare.Rank == 7)
            {
                if (targetSquare.File == 0) _blackQueenRookMoved = true;
                if (targetSquare.File == 7) _blackKingRookMoved = true;
            }
        }

        private void MoveRookForCastling(PieceColor color, ChessMove kingMove)
        {
            if (color == PieceColor.White)
            {
                if (kingMove.To.File == 6)
                {
                    var rook = Get(7, 0);
                    Set(5, 0, rook);
                    Set(7, 0, Piece.Empty);
                    _whiteKingRookMoved = true;
                }
                else if (kingMove.To.File == 2)
                {
                    var rook = Get(0, 0);
                    Set(3, 0, rook);
                    Set(0, 0, Piece.Empty);
                    _whiteQueenRookMoved = true;
                }
            }
            else
            {
                if (kingMove.To.File == 6)
                {
                    var rook = Get(7, 7);
                    Set(5, 7, rook);
                    Set(7, 7, Piece.Empty);
                    _blackKingRookMoved = true;
                }
                else if (kingMove.To.File == 2)
                {
                    var rook = Get(0, 7);
                    Set(3, 7, rook);
                    Set(0, 7, Piece.Empty);
                    _blackQueenRookMoved = true;
                }
            }
        }

        private string BuildCastlingRights()
        {
            var sb = new StringBuilder();

            var whiteKing = Get(4, 0);
            var whiteQueenRook = Get(0, 0);
            var whiteKingRook = Get(7, 0);
            var blackKing = Get(4, 7);
            var blackQueenRook = Get(0, 7);
            var blackKingRook = Get(7, 7);

            if (!_whiteKingMoved && !_whiteKingRookMoved &&
                whiteKing.Type == PieceType.King && whiteKing.Color == PieceColor.White &&
                whiteKingRook.Type == PieceType.Rook && whiteKingRook.Color == PieceColor.White)
            {
                sb.Append('K');
            }

            if (!_whiteKingMoved && !_whiteQueenRookMoved &&
                whiteKing.Type == PieceType.King && whiteKing.Color == PieceColor.White &&
                whiteQueenRook.Type == PieceType.Rook && whiteQueenRook.Color == PieceColor.White)
            {
                sb.Append('Q');
            }

            if (!_blackKingMoved && !_blackKingRookMoved &&
                blackKing.Type == PieceType.King && blackKing.Color == PieceColor.Black &&
                blackKingRook.Type == PieceType.Rook && blackKingRook.Color == PieceColor.Black)
            {
                sb.Append('k');
            }

            if (!_blackKingMoved && !_blackQueenRookMoved &&
                blackKing.Type == PieceType.King && blackKing.Color == PieceColor.Black &&
                blackQueenRook.Type == PieceType.Rook && blackQueenRook.Color == PieceColor.Black)
            {
                sb.Append('q');
            }

            return sb.Length == 0 ? "-" : sb.ToString();
        }

        private static string ToSquareName(Square square)
        {
            var file = (char)('a' + square.File);
            var rank = (char)('1' + square.Rank);
            return $"{file}{rank}";
        }

        private static char ToFenChar(Piece piece)
        {
            char c = piece.Type switch
            {
                PieceType.Pawn => 'p',
                PieceType.Knight => 'n',
                PieceType.Bishop => 'b',
                PieceType.Rook => 'r',
                PieceType.Queen => 'q',
                PieceType.King => 'k',
                _ => ' '
            };

            return piece.Color == PieceColor.White ? char.ToUpperInvariant(c) : c;
        }
    }
}
