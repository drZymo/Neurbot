namespace Neurbot.Learner
{
    internal sealed class GameResult
    {
        internal sealed class Player
        {
            public int id { get; set; }
            public string name { get; set; }
            public string bot { get; set; }
            public int[] ufos { get; set; }
            public int[] survivors { get; set; }
            public int[] casualties { get; set; }
            public string color { get; set; }
            public double hue { get; set; }
        }

        public Player[] players { get; set; }
        public int gameId { get; set; }
        public int winner { get; set; }
    }
}
