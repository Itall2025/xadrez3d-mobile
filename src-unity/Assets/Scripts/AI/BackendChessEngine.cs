using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Xadrez3D.Core;

namespace Xadrez3D.AI
{
    public sealed class BackendChessEngine : IChessEngine
    {
        private readonly string _baseUrl;
        private readonly IChessEngine _fallbackEngine;

        [Serializable]
        private sealed class AnalysisRequest
        {
            public string fen;
            public int depth;
            public string[] moves;
        }

        [Serializable]
        private sealed class AnalysisResponse
        {
            public string bestMove;
        }

        public BackendChessEngine(string baseUrl, IChessEngine fallbackEngine = null)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _fallbackEngine = fallbackEngine;
        }

        public async Task<ChessMove> GetBestMoveAsync(string fen, DifficultyProfile profile, CancellationToken ct)
        {
            try
            {
                var bestMove = await GetBestMoveUciAsync(fen, profile.MoveTimeMs, ct);
                return ParseUciMove(bestMove);
            }
            catch
            {
                if (_fallbackEngine != null)
                {
                    return await _fallbackEngine.GetBestMoveAsync(fen, profile, ct);
                }

                throw;
            }
        }

        private async Task<string> GetBestMoveUciAsync(string fen, int moveTimeMs, CancellationToken ct)
        {
            var depth = MoveTimeMsToDepth(moveTimeMs);
            var payload = new AnalysisRequest
            {
                fen = string.IsNullOrWhiteSpace(fen) ? "startpos" : fen,
                depth = depth,
                moves = Array.Empty<string>()
            };

            var json = JsonUtility.ToJson(payload);
            var endpoint = $"{_baseUrl}/api/analysis";

            using (var request = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST))
            {
                var bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                using (ct.Register(() => request.Abort()))
                {
                    var operation = request.SendWebRequest();
                    while (!operation.isDone)
                    {
                        ct.ThrowIfCancellationRequested();
                        await Task.Yield();
                    }
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"Backend request failed: {request.error}");
                }

                var response = JsonUtility.FromJson<AnalysisResponse>(request.downloadHandler.text);
                if (response == null || string.IsNullOrWhiteSpace(response.bestMove))
                {
                    throw new Exception("Backend returned no bestMove.");
                }

                return response.bestMove;
            }
        }

        private static int MoveTimeMsToDepth(int moveTimeMs)
        {
            if (moveTimeMs <= 120) return 8;
            if (moveTimeMs <= 250) return 10;
            if (moveTimeMs <= 450) return 12;
            if (moveTimeMs <= 800) return 14;
            return 16;
        }

        private static ChessMove ParseUciMove(string uci)
        {
            if (uci.Length < 4)
            {
                throw new ArgumentException("Invalid UCI move.", nameof(uci));
            }

            var from = new Square(FileToIndex(uci[0]), RankToIndex(uci[1]));
            var to = new Square(FileToIndex(uci[2]), RankToIndex(uci[3]));
            var promotion = uci.Length >= 5 ? PromotionFromChar(uci[4]) : PieceType.None;

            return new ChessMove(from, to, promotion);
        }

        private static int FileToIndex(char file)
        {
            return char.ToLowerInvariant(file) - 'a';
        }

        private static int RankToIndex(char rank)
        {
            return rank - '1';
        }

        private static PieceType PromotionFromChar(char c)
        {
            switch (char.ToLowerInvariant(c))
            {
                case 'q': return PieceType.Queen;
                case 'r': return PieceType.Rook;
                case 'b': return PieceType.Bishop;
                case 'n': return PieceType.Knight;
                default: return PieceType.None;
            }
        }
    }
}
