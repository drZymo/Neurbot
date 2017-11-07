namespace MacroBot.Protocol
{
    public sealed class Planet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OrbitDistance { get; set; }
        public double OrbitRotation { get; set; }
        public int OwnedBy { get; set; }
    }
}