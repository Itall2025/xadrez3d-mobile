using UnityEngine;
using Xadrez3D.AI;

namespace Xadrez3D.Gameplay
{
    public sealed class GameHudOverlay : MonoBehaviour
    {
        [SerializeField] private GameController gameController;
        [SerializeField] private int margin = 16;
        [SerializeField] private int panelWidth = 310;

        private GUIStyle _panelStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;
        private Texture2D _panelTexture;
        private Texture2D _buttonTexture;
        private bool _stylesReady;

        private void Awake()
        {
            if (gameController == null)
            {
                gameController = FindObjectOfType<GameController>();
            }
        }

        private void OnGUI()
        {
            if (gameController == null)
            {
                return;
            }

            EnsureStyles();

            var panelHeight = 232;
            var area = new Rect(margin, margin, panelWidth, panelHeight);
            GUI.Box(area, GUIContent.none, _panelStyle);

            GUILayout.BeginArea(new Rect(area.x + 12, area.y + 10, area.width - 24, area.height - 20));
            GUILayout.Label("Xadrez3D Mobile", _titleStyle);
            GUILayout.Space(6);
            GUILayout.Label($"Turno: {gameController.Board.SideToMove}", _labelStyle);
            GUILayout.Label($"Dificuldade: {DifficultyCatalog.Levels[gameController.DifficultyIndex].Name}", _labelStyle);
            GUILayout.Label($"Engine: {(gameController.IsEngineThinking ? "pensando..." : "pronta")}", _labelStyle);
            GUILayout.Label($"FEN: {gameController.Board.ToFen()}", _labelStyle);
            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            for (int i = 0; i < DifficultyCatalog.Levels.Length; i++)
            {
                if (GUILayout.Button((i + 1).ToString(), _buttonStyle, GUILayout.Width(42), GUILayout.Height(28)))
                {
                    gameController.NewGame(i);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Toque/clique: selecione origem e destino.", _labelStyle);
            GUILayout.Label("Botao direito: orbitar camera. Scroll/pinca: zoom.", _labelStyle);
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (_stylesReady)
            {
                return;
            }

            _panelTexture = MakeTexture(new Color(0.03f, 0.05f, 0.07f, 0.86f));
            _buttonTexture = MakeTexture(new Color(0.95f, 0.68f, 0.18f, 0.96f));

            _panelStyle = new GUIStyle(GUI.skin.box);
            _panelStyle.normal.background = _panelTexture;
            _panelStyle.border = new RectOffset(8, 8, 8, 8);

            _titleStyle = new GUIStyle(GUI.skin.label);
            _titleStyle.fontSize = 19;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.normal.textColor = new Color(0.98f, 0.9f, 0.72f);

            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = 13;
            _labelStyle.normal.textColor = new Color(0.86f, 0.9f, 0.94f);
            _labelStyle.wordWrap = true;

            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.normal.background = _buttonTexture;
            _buttonStyle.normal.textColor = new Color(0.15f, 0.11f, 0.04f);
            _buttonStyle.fontStyle = FontStyle.Bold;

            _stylesReady = true;
        }

        private static Texture2D MakeTexture(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
