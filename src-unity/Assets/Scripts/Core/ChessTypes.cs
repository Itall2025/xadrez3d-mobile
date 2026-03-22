namespace Xadrez3D.Core
{
    public enum PieceType
    {
        None,
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King
    }

    public enum PieceColor
    {
        White,
        Black
    }

    public struct Piece
    {
        public PieceType Type;
        public PieceColor Color;

        public bool IsEmpty => Type == PieceType.None;

        public static Piece Empty => new Piece { Type = PieceType.None, Color = PieceColor.White };
    }

    public struct Square
    {
        public int File;
        public int Rank;

        public Square(int file, int rank)
        {
            File = file;
            Rank = rank;
        }
    }

    public struct ChessMove
    {
        public Square From;
        public Square To;
        public PieceType Promotion;

        public ChessMove(Square from, Square to, PieceType promotion = PieceType.None)
        {
            From = from;
            To = to;
            Promotion = promotion;
        }
    }
}
