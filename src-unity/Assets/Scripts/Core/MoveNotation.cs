namespace Xadrez3D.Core
{
    public static class MoveNotation
    {
        public static string ToUci(ChessMove move)
        {
            var fromFile = (char)('a' + move.From.File);
            var fromRank = (char)('1' + move.From.Rank);
            var toFile = (char)('a' + move.To.File);
            var toRank = (char)('1' + move.To.Rank);

            if (move.Promotion == PieceType.None)
            {
                return $"{fromFile}{fromRank}{toFile}{toRank}";
            }

            var promotion = move.Promotion switch
            {
                PieceType.Queen => "q",
                PieceType.Rook => "r",
                PieceType.Bishop => "b",
                PieceType.Knight => "n",
                _ => "q"
            };

            return $"{fromFile}{fromRank}{toFile}{toRank}{promotion}";
        }
    }
}
