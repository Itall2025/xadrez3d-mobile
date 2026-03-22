using UnityEngine;
using UnityEngine.Rendering;

namespace Xadrez3D.Gameplay
{
    public sealed class DemoSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private bool autoCreateIfMissing = true;

        private void Start()
        {
            if (!autoCreateIfMissing)
            {
                return;
            }

            var controller = FindObjectOfType<GameController>();
            if (controller == null)
            {
                var go = new GameObject("GameController");
                controller = go.AddComponent<GameController>();
            }

            var board = FindObjectOfType<BoardRenderer3D>();
            if (board == null)
            {
                var boardGo = new GameObject("BoardRenderer3D");
                boardGo.transform.position = new Vector3(-4f, 0f, -4f);
                board = boardGo.AddComponent<BoardRenderer3D>();
            }

            var camera = Camera.main;
            if (camera == null)
            {
                var cameraGo = new GameObject("Main Camera");
                camera = cameraGo.AddComponent<Camera>();
                cameraGo.tag = "MainCamera";
            }
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.06f, 0.09f);
            camera.fieldOfView = 52f;
            camera.nearClipPlane = 0.02f;
            camera.farClipPlane = 120f;

            var orbit = camera.GetComponent<OrbitCameraController>();
            if (orbit == null)
            {
                orbit = camera.gameObject.AddComponent<OrbitCameraController>();
            }
            orbit.SetTarget(board.transform);

            if (FindObjectOfType<ChessBoardInput>() == null)
            {
                var inputGo = new GameObject("ChessBoardInput");
                inputGo.AddComponent<ChessBoardInput>();
            }

            if (FindObjectOfType<GameHudOverlay>() == null)
            {
                var hudGo = new GameObject("GameHudOverlay");
                hudGo.AddComponent<GameHudOverlay>();
            }

            EnsureLighting(board.transform.position + new Vector3(3.5f, 0f, 3.5f));
            EnsureGround(board.transform.position + new Vector3(3.5f, -0.13f, 3.5f));
            EnsureAtmosphere();
        }

        private static void EnsureLighting(Vector3 center)
        {
            if (FindObjectOfType<Light>() != null)
            {
                return;
            }

            var keyLightObj = new GameObject("Key Light");
            var keyLight = keyLightObj.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.2f;
            keyLight.color = new Color(1f, 0.96f, 0.9f);
            keyLightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var fillLightObj = new GameObject("Fill Light");
            var fillLight = fillLightObj.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.intensity = 1.6f;
            fillLight.range = 40f;
            fillLight.color = new Color(0.5f, 0.67f, 1f);
            fillLightObj.transform.position = center + new Vector3(-2f, 8f, 8f);

            var rimLightObj = new GameObject("Rim Light");
            var rimLight = rimLightObj.AddComponent<Light>();
            rimLight.type = LightType.Point;
            rimLight.intensity = 1.3f;
            rimLight.range = 28f;
            rimLight.color = new Color(1f, 0.5f, 0.2f);
            rimLightObj.transform.position = center + new Vector3(8f, 3.5f, -5f);
        }

        private static void EnsureGround(Vector3 center)
        {
            var ground = GameObject.Find("GroundPlane");
            if (ground != null)
            {
                return;
            }

            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "GroundPlane";
            ground.transform.position = center;
            ground.transform.localScale = new Vector3(1.8f, 1f, 1.8f);
            var renderer = ground.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Standard"))
            {
                color = new Color(0.11f, 0.13f, 0.16f)
            };
            mat.SetFloat("_Glossiness", 0.35f);
            mat.SetFloat("_Metallic", 0.2f);
            renderer.material = mat;
        }

        private static void EnsureAtmosphere()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.16f, 0.18f, 0.22f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.018f;
            RenderSettings.fogColor = new Color(0.04f, 0.06f, 0.09f);
        }
    }
}
