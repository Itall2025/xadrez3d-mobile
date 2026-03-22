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

            var panelHeight = 218;
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
                if (GUILayout.Button((i + 1).ToString(), GUILayout.Width(42)))
                {
                    gameController.NewGame(i);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Toque/clique: selecione origem e destino.", _labelStyle);
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (_stylesReady)
            {
                return;
            }

            _panelStyle = new GUIStyle(GUI.skin.box);
            _panelStyle.normal.background = Texture2D.whiteTexture;
            _panelStyle.normal.textColor = new Color(0.16f, 0.16f, 0.16f);

            _titleStyle = new GUIStyle(GUI.skin.label);
            _titleStyle.fontSize = 19;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.normal.textColor = Color.black;

            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = 13;
            _labelStyle.normal.textColor = new Color(0.1f, 0.1f, 0.1f);
            _labelStyle.wordWrap = true;

            _stylesReady = true;
        }
    }
}
