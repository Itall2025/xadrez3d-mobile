using UnityEngine;

namespace Xadrez3D.Gameplay
{
    public sealed class BoardSquareView : MonoBehaviour
    {
        public int File { get; private set; }
        public int Rank { get; private set; }

        public void Initialize(int file, int rank)
        {
            File = file;
            Rank = rank;
            name = $"Square_{file}_{rank}";
        }
    }
}
