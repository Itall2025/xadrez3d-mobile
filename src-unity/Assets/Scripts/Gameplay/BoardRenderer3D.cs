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
        [SerializeField] private Color darkSquareColor = new Color(0.2f, 0.25f, 0.3f);
        [SerializeField] private Color lightSquareColor = new Color(0.88f, 0.89f, 0.84f);
        [SerializeField] private Color selectedSquareColor = new Color(0.96f, 0.8f, 0.25f);
        [SerializeField] private Color whitePieceColor = new Color(0.95f, 0.95f, 0.95f);
        [SerializeField] private Color blackPieceColor = new Color(0.16f, 0.16f, 0.18f);

        private readonly Dictionary<int, Renderer> _squareRenderers = new Dictionary<int, Renderer>();
        private readonly Dictionary<int, Color> _baseSquareColors = new Dictionary<int, Color>();
        private readonly List<GameObject> _pieceObjects = new List<GameObject>();

        private Transform _boardRoot;
        private Transform _pieceRoot;
        private Material _lightSquareMaterial;
        private Material _darkSquareMaterial;
        private Material _whitePieceMaterial;
        private Material _blackPieceMaterial;
        private int _selectedKey = -1;

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

            _lightSquareMaterial = CreateMaterial(lightSquareColor);
            _darkSquareMaterial = CreateMaterial(darkSquareColor);
            _whitePieceMaterial = CreateMaterial(whitePieceColor);
            _blackPieceMaterial = CreateMaterial(blackPieceColor);

            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    var square = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    square.transform.SetParent(_boardRoot, false);
                    square.transform.localScale = new Vector3(squareSize, 0.12f, squareSize);
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

            ClearPieces();

            var board = gameController.Board;
            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    var piece = board.Get(file, rank);
                    if (piece.IsEmpty)
                    {
                        continue;
                    }

                    var pieceObj = CreatePieceObject(piece.Type);
                    pieceObj.transform.SetParent(_pieceRoot, false);
                    pieceObj.transform.localPosition = new Vector3(file * squareSize, boardY + PieceHeight(piece.Type), rank * squareSize);
                    pieceObj.GetComponent<Renderer>().material = piece.Color == PieceColor.White ? _whitePieceMaterial : _blackPieceMaterial;
                    _pieceObjects.Add(pieceObj);
                }
            }

            if (_selectedKey >= 0)
            {
                var file = _selectedKey & 7;
                var rank = (_selectedKey >> 3) & 7;
                SetSelectedSquare(new Square(file, rank));
            }
        }

        private void ClearPieces()
        {
            for (int i = 0; i < _pieceObjects.Count; i++)
            {
                if (_pieceObjects[i] != null)
                {
                    Destroy(_pieceObjects[i]);
                }
            }

            _pieceObjects.Clear();
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
            return go;
        }

        private static Material CreateMaterial(Color color)
        {
            var material = new Material(Shader.Find("Standard"));
            material.color = color;
            return material;
        }

        private static int Key(int file, int rank)
        {
            return (rank << 3) | file;
        }
    }
}
