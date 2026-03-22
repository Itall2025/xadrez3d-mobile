namespace Xadrez3D.AI
{
    public readonly struct DifficultyProfile
    {
        public readonly string Name;
        public readonly int Elo;
        public readonly int MoveTimeMs;

        public DifficultyProfile(string name, int elo, int moveTimeMs)
        {
            Name = name;
            Elo = elo;
            MoveTimeMs = moveTimeMs;
        }
    }

    public static class DifficultyCatalog
    {
        public static readonly DifficultyProfile[] Levels =
        {
            new DifficultyProfile("Iniciante", 600, 80),
            new DifficultyProfile("Casual", 1000, 150),
            new DifficultyProfile("Intermediario", 1400, 300),
            new DifficultyProfile("Avancado", 1800, 600),
            new DifficultyProfile("Pro", 2200, 1200)
        };
    }
}
