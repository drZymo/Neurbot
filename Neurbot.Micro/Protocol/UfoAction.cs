namespace MicroBot.Protocol
{
    public sealed class UfoAction
    {
        public int Id { get; set; }
        public Move Move { get; set; }
        public MoveTo MoveTo { get; set; }
        public Shoot Shoot { get; set; }
        public ShootAt ShootAt { get; set; }
    }
}