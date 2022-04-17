using System;
using System.Collections.Generic;
using Xdd.Model.Cash;
using Xdd.Model.Cycles.BlackJack.Controllers;
using Xdd.Model.Games;
using Xdd.Model.Games.BlackJack;
using Xdd.Model.Games.BlackJack.Users;

namespace Xdd.Model.Cycles.BlackJack
{
    public class Hand
    {
        internal IPlayer player;

        public event Action<ICard> OnCardAdd
        {
            add => player.OnCardAdd += value;
            remove => player.OnCardAdd -= value;
        }
        public event Action<GameResult> OnResult
        {
            add => player.OnResult += value;
            remove => player.OnResult -= value;
        }

        public bool CanTurn => player.CanTurn;
        public PlayerStatus? Status => player.GetStatus();
        public IEnumerable<int> Scores => player.GetScores();

        public decimal Amount => bet == null ? 0 : doubleBet == null ? bet.Amount : bet.Amount + doubleBet.Amount;

        internal Bet bet;
        internal Bet doubleBet;
        internal bool HasBet => bet != null && bet.Amount > 0;

        internal GameController gameController;

        public void Hit()
        {
            gameController.Hit(player);
        }

        public void Stand()
        {
            gameController.Stand(player);
        }

        public void DoubleUp()
        {
            gameController.DoubleUp(player);
        }
    }
}
