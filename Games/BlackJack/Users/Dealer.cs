using System.Linq;

namespace Xdd.Model.Games.BlackJack.Users
{
    internal class Dealer : User
    {
        public Dealer(Game game, Game.GetScores GetScores)
            : base(game, GetScores)
        {
        }

        public override void AddCard(ICard card)
        {
            base.AddCard(card);

            CheckTurn();

            InvokeOnCardAdd(card);
        }

        public void AddCardHidden(ICard card)
        {
            base.AddCard(card);

            CheckTurn();
        }

        private void CheckTurn()
        {
            if (GetScores().Where(x => x <= 21).Any(x => x >= 17))
                CanTurn = false;
        }

        public override PlayerStatus? GetStatus()
        {
            if (IsBust())
                return PlayerStatus.Bust;

            return null;
        }
    }
}
