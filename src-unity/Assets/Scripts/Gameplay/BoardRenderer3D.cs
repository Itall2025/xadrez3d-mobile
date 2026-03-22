using System.Collections.Generic;
using UnityEngine;
using Xadrez3D.Core;

namespace Xadrez3D.Gameplay
{
    public sealed class BoardRenderer3D : MonoBehaviour
    {
        [SerializeField] private GameController gameController;
        [SerializeField] private float squareSize = 1.1f;
        [SerializeField] private float boardY = 0f;
        [SerializeField] private float moveAnimationTime = 0.24f;
        [SerializeField] private float spawnAnimationTime = 0.2f;
        [SerializeField] private Color darkSquareColor = new Color(0.13f, 0.18f, 0.24f);
        [SerializeField] private Color lightSquareColor = new Color(0.82f, 0.84f, 0.78f);
        [SerializeField] private Color selectedSquareColor = new Color(0.95f, 0.68f, 0.18f);
        [SerializeField] private Color whitePieceColor = new Color(0.96f, 0.95f, 0.9f);
        [SerializeField] private Color blackPieceColor = new Color(0.12f, 0.12f, 0.14f);

        private readonly Dictionary<int, Renderer> _squareRenderers = new Dictionary<int, Renderer>();
        private readonly Dictionary<int, Color> _baseSquareColors = new Dictionary<int, Color>();
        private readonly Dictionary<int, PieceView> _pieceViews = new Dictionary<int, PieceView>();
        private readonly List<PieceView> _tempRemoved = new List<PieceView>();

        private Transform _boardRoot;
        private Transform _pieceRoot;
        private Material _lightSquareMaterial;
        private Material _darkSquareMaterial;
        private Material _whitePieceMaterial;
        private Material _blackPieceMaterial;
        private int _selectedKey = -1;
        private float _selectionPulseTime;

        private sealed class PieceView
        {
            public GameObject GameObject;
            public Piece Piece;
            public Square Square;
        }

        public float SquareSize => squareSize;
        public Vector3 BoardCenter => transform.position + new Vector3(squareSize * 3.5f, boardY, squareSize * 3.5f);

        private void Awake()
        {
            if (gameController == null)
            {
                gameController = FindObjectOfType<GameController>();
            }
        }

        private void OnEnable()
        {
            BuildBoardVisuals();
            if (gameController != null)
            {
                gameController.BoardChanged += RenderPosition;
            }
            RenderPosition();
        }

        private void OnDisable()
        {
            if (gameController != null)
            {
                gameController.BoardChanged -= RenderPosition;
            }
        }

        private void Update()
        {
            if (_selectedKey < 0 || !_squareRenderers.TryGetValue(_selectedKey, out var renderer))
            {
                return;
            }

            _selectionPulseTime += Time.deltaTime * 4f;
            var baseColor = _baseSquareColors[_selectedKey];
            var pulse = (Mathf.Sin(_selectionPulseTime) + 1f) * 0.5f;
            renderer.material.color = Color.Lerp(baseColor, selectedSquareColor, 0.55f + 0.35f * pulse);
        }

        public bool TryGetSquareFromHit(RaycastHit hit, out Square square)
        {
            var squareView = hit.collider.GetComponent<BoardSquareView>();
            if (squareView == null)
            {
                square = default;
                return false;
            }

            square = new Square(squareView.File, squareView.Rank);
            return true;
        }

        public void SetSelectedSquare(Square? square)
        {
            if (_selectedKey >= 0 && _squareRenderers.TryGetValue(_selectedKey, out var previousRenderer))
            {
                previousRenderer.material.color = _baseSquareColors[_selectedKey];
            }

            if (!square.HasValue)
            {
                _selectedKey = -1;
                return;
            }

            _selectedKey = Key(square.Value.File, square.Value.Rank);
            if (_squareRenderers.TryGetValue(_selectedKey, out var renderer))
            {
                renderer.material.color = selectedSquareColor;
                _selectionPulseTime = 0f;
            }
        }

        public Vector3 SquareToWorld(Square square)
        {
            return transform.position + new Vector3(square.File * squareSize, boardY, square.Rank * squareSize);
        }

        private void BuildBoardVisuals()
        {
            if (_boardRoot != null)
            {
                return;
            }

            _boardRoot = new GameObject("BoardRoot").transform;
            _boardRoot.SetParent(transform, false);
            _pieceRoot = new GameObject("PiecesRoot").transform;
            _pieceRoot.SetParent(transform, false);

            var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "BoardFrame";
            frame.transform.SetParent(_boardRoot, false);
            frame.transform.localScale = new Vector3(squareSize * 8.55f, 0.28f, squareSize * 8.55f);
            frame.transform.localPosition = new Vector3(squareSize * 3.5f, boardY - 0.17f, squareSize * 3.5f);

            _lightSquareMaterial = CreateMaterial(lightSquareColor);
            _darkSquareMaterial = CreateMaterial(darkSquareColor);
            _whitePieceMaterial = CreateMaterial(whitePieceColor);
            _blackPieceMaterial = CreateMaterial(blackPieceColor);
            frame.GetComponent<Renderer>().material = CreateMaterial(new Color(0.08f, 0.11f, 0.15f), 0.22f, 0.84f);

            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    var square = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    square.transform.SetParent(_boardRoot, false);
                    square.transform.localScale = new Vector3(squareSize, 0.1f, squareSize);
                    square.transform.localPosition = new Vector3(file * squareSize, boardY - 0.06f, rank * squareSize);

                    var squareView = square.AddComponent<BoardSquareView>();
                    squareView.Initialize(file, rank);

                    var key = Key(file, rank);
                    var isLight = (file + rank) % 2 == 0;
                    var renderer = square.GetComponent<Renderer>();
                    renderer.material = isLight ? _lightSquareMaterial : _darkSquareMaterial;

                    _squareRenderers[key] = renderer;
                    _baseSquareColors[key] = isLight ? lightSquareColor : darkSquareColor;
                }
            }
        }

        private void RenderPosition()
        {
            if (gameController == null)
            {
                return;
            }

            var board = gameController.Board;
            _tempRemoved.Clear();

            foreach (var pair in _pieceViews)
            {
                var file = pair.Key & 7;
                var rank = (pair.Key >> 3) & 7;
                var expected = board.Get(file, rank);
                if (expected.IsEmpty || !SamePiece(expected, pair.Value.Piece))
                {
                    _tempRemoved.Add(pair.Value);
                }
            }

            for (int i = 0; i < _tempRemoved.Count; i++)
            {
                var removeKey = Key(_tempRemoved[i].Square.File, _tempRemoved[i].Square.Rank);
                _pieceViews.Remove(removeKey);
            }

            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    var piece = board.Get(file, rank);
                    if (piece.IsEmpty)
                    {
                        continue;
                    }

                    var targetSquare = new Square(file, rank);
                    var targetKey = Key(file, rank);
                    if (_pieceViews.ContainsKey(targetKey))
                    {
                        continue;
                    }

                    var reuseIndex = FindClosestReusable(piece, targetSquare);
                    if (reuseIndex >= 0)
                    {
                        var view = _tempRemoved[reuseIndex];
                        _tempRemoved.RemoveAt(reuseIndex);
                        AssignPieceToSquare(view, piece, targetSquare);
                        _pieceViews[targetKey] = view;
                        AnimateMove(view.GameObject.transform, GetLocalPosition(targetSquare, piece.Type), moveAnimationTime);
                        continue;
                    }

                    var created = CreatePieceView(piece, targetSquare);
                    _pieceViews[targetKey] = created;
                    AnimateSpawn(created.GameObject.transform);
                }
            }

            for (int i = 0; i < _tempRemoved.Count; i++)
            {
                var leftover = _tempRemoved[i];
                if (leftover.GameObject != null)
                {
                    Destroy(leftover.GameObject);
                }
            }
            _tempRemoved.Clear();

            if (_selectedKey >= 0)
            {
                var file = _selectedKey & 7;
                var rank = (_selectedKey >> 3) & 7;
                SetSelectedSquare(new Square(file, rank));
            }
        }

        private int FindClosestReusable(Piece piece, Square targetSquare)
        {
            var bestIndex = -1;
            var bestDistance = float.MaxValue;
            for (int i = 0; i < _tempRemoved.Count; i++)
            {
                var candidate = _tempRemoved[i];
                if (!SamePiece(piece, candidate.Piece))
                {
                    continue;
                }

                var dx = candidate.Square.File - targetSquare.File;
                var dy = candidate.Square.Rank - targetSquare.Rank;
                var dist = dx * dx + dy * dy;
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private static float PieceHeight(PieceType type)
        {
            return type switch
            {
                PieceType.Pawn => 0.25f,
                PieceType.Knight => 0.35f,
                PieceType.Bishop => 0.4f,
                PieceType.Rook => 0.35f,
                PieceType.Queen => 0.5f,
                PieceType.King => 0.55f,
                _ => 0.3f
            };
        }

        private PieceView CreatePieceView(Piece piece, Square square)
        {
            var go = CreatePieceObject(piece.Type);
            go.transform.SetParent(_pieceRoot, false);
            go.transform.localPosition = GetLocalPosition(square, piece.Type);
            go.GetComponent<Renderer>().material = piece.Color == PieceColor.White ? _whitePieceMaterial : _blackPieceMaterial;
            return new PieceView
            {
                GameObject = go,
                Piece = piece,
                Square = square
            };
        }

        private void AssignPieceToSquare(PieceView view, Piece piece, Square square)
        {
            view.Piece = piece;
            view.Square = square;
            view.GameObject.GetComponent<Renderer>().material = piece.Color == PieceColor.White ? _whitePieceMaterial : _blackPieceMaterial;
        }

        private GameObject CreatePieceObject(PieceType type)
        {
            var primitiveType = type switch
            {
                PieceType.Pawn => PrimitiveType.Capsule,
                PieceType.Knight => PrimitiveType.Sphere,
                PieceType.Bishop => PrimitiveType.Cylinder,
                PieceType.Rook => PrimitiveType.Cube,
                PieceType.Queen => PrimitiveType.Cylinder,
                PieceType.King => PrimitiveType.Capsule,
                _ => PrimitiveType.Cylinder
            };

            var go = GameObject.CreatePrimitive(primitiveType);
            go.name = $"Piece_{type}";
            go.transform.localScale = type switch
            {
                PieceType.Pawn => new Vector3(squareSize * 0.35f, 0.5f, squareSize * 0.35f),
                PieceType.Knight => new Vector3(squareSize * 0.45f, squareSize * 0.45f, squareSize * 0.45f),
                PieceType.Bishop => new Vector3(squareSize * 0.4f, 0.7f, squareSize * 0.4f),
                PieceType.Rook => new Vector3(squareSize * 0.45f, 0.65f, squareSize * 0.45f),
                PieceType.Queen => new Vector3(squareSize * 0.45f, 0.95f, squareSize * 0.45f),
                PieceType.King => new Vector3(squareSize * 0.48f, 1.1f, squareSize * 0.48f),
                _ => new Vector3(squareSize * 0.4f, 0.7f, squareSize * 0.4f)
            };
            go.GetComponent<Collider>().enabled = false;
            return go;
        }

        private Vector3 GetLocalPosition(Square square, PieceType type)
        {
            return new Vector3(square.File * squareSize, boardY + PieceHeight(type), square.Rank * squareSize);
        }

        private void AnimateSpawn(Transform pieceTransform)
        {
            StartCoroutine(SpawnRoutine(pieceTransform));
        }

        private System.Collections.IEnumerator SpawnRoutine(Transform pieceTransform)
        {
            var targetScale = pieceTransform.localScale;
            pieceTransform.localScale = Vector3.zero;
            var t = 0f;
            while (t < spawnAnimationTime)
            {
                t += Time.deltaTime;
                var blend = Mathf.SmoothStep(0f, 1f, t / spawnAnimationTime);
                pieceTransform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, blend);
                yield return null;
            }
            pieceTransform.localScale = targetScale;
        }

        private void AnimateMove(Transform pieceTransform, Vector3 destination, float duration)
        {
            StartCoroutine(MoveRoutine(pieceTransform, destination, duration));
        }

        private System.Collections.IEnumerator MoveRoutine(Transform pieceTransform, Vector3 destination, float duration)
        {
            var origin = pieceTransform.localPosition;
            var t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                var blend = Mathf.SmoothStep(0f, 1f, t / duration);
                var pos = Vector3.LerpUnclamped(origin, destination, blend);
                pos.y += Mathf.Sin(blend * Mathf.PI) * 0.22f;
                pieceTransform.localPosition = pos;
                yield return null;
            }
            pieceTransform.localPosition = destination;
        }

        private static bool SamePiece(Piece a, Piece b)
        {
            return a.Type == b.Type && a.Color == b.Color;
        }

        private static Material CreateMaterial(Color color, float metallic = 0.07f, float smoothness = 0.65f)
        {
            var material = new Material(Shader.Find("Standard"));
            material.color = color;
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Glossiness", smoothness);
            return material;
        }

        private static int Key(int file, int rank)
        {
            return (rank << 3) | file;
        }
    }
}
