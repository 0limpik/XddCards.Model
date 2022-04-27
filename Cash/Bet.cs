namespace Xdd.Model.Cash
{
    public class Bet
    {
        public decimal Amount { get; set; }

        public Bet(decimal bet)
        {
            Amount = bet;
        }
    }
}
