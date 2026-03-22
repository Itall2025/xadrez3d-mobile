using System.Collections.Generic;
using System.Text;
using Xadrez3D.Core;

namespace Xadrez3D.Analysis
{
    public static class PgnExporter
    {
        public static string Export(IReadOnlyList<ChessMove> moves, string whiteName = "Usuario", string blackName = "Maquina")
        {
            var sb = new StringBuilder();
            sb.AppendLine("[Event \"Xadrez3D Mobile\"]");
            sb.AppendLine($"[White \"{whiteName}\"]");
            sb.AppendLine($"[Black \"{blackName}\"]");
            sb.AppendLine();

            for (int i = 0; i < moves.Count; i++)
            {
                if (i % 2 == 0)
                {
                    sb.Append((i / 2) + 1);
                    sb.Append(". ");
                }

                var move = moves[i];
                sb.Append(ToCoordinate(move));
                sb.Append(' ');
            }

            sb.Append("*");
            return sb.ToString();
        }

        private static string ToCoordinate(ChessMove move)
        {
            return $"{ToFile(move.From.File)}{move.From.Rank + 1}{ToFile(move.To.File)}{move.To.Rank + 1}";
        }

        private static char ToFile(int file)
        {
            return (char)('a' + file);
        }
    }
}
