using System;
using System.Collections.Generic;
using Xdd.Model.Cash;
using Xdd.Model.Cycles.BlackJack.Controllers;

namespace Xdd.Model.Cycles.BlackJack
{
    public class User
    {
        public event Action OnBet;

        internal Wallet wallet;

        public decimal Cash => wallet.Cash;
        public decimal Amount { get; internal set; }

        internal List<Hand> hands = new List<Hand>();
        public IReadOnlyList<Hand> Hands => hands;

        internal HandController handController;
        internal BetController betController;
        internal GameController gameController;

        public User(Wallet wallet)
        {
            this.wallet = wallet;
        }

        public void Take()
        {
            handController.Take(this);
        }

        public void Release()
        {
            handController.Release(this);
        }

        public bool CanBet(decimal amount)
        {
            return betController.CanBet(this, amount);
        }

        public void Bet(decimal amount)
        {
            betController.Bet(this, amount);
            OnBet?.Invoke();
        }
    }
}
