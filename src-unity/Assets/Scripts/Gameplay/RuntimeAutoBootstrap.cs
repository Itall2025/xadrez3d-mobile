using UnityEngine;

namespace Xadrez3D.Gameplay
{
    public static class RuntimeAutoBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrap()
        {
            if (Object.FindObjectOfType<DemoSceneBootstrap>() != null)
            {
                return;
            }

            var go = new GameObject("RuntimeBootstrap");
            go.AddComponent<DemoSceneBootstrap>();
            Debug.Log("RuntimeAutoBootstrap created DemoSceneBootstrap automatically.");
        }
    }
}
