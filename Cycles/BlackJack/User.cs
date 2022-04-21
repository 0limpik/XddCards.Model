using System;
using System.Collections.Generic;
using System.Linq;
using Xdd.Model.Cash;
using Xdd.Model.Cycles.BlackJack.Controllers;

namespace Xdd.Model.Cycles.BlackJack
{
    public interface IUser
    {
        decimal Cash { get; }
        IHand[] Hands { get; }
        decimal Amount { get; }

        void Take(IHand hand);
        void Release(IHand hand);

        bool CanBet(decimal amount);
        void Bet(decimal amount);
    }

    internal class User : IUser
    {
        internal Wallet wallet;

        public decimal Cash => wallet.Cash;
        public decimal Amount { get; internal set; }

        public IHand[] Hands => _Hands.ToArray();
        internal List<Hand> _Hands = new List<Hand>();

        private HandController handController;
        private BetController betController;
        private GameController gameController;

        public User(Wallet wallet)
        {
            this.wallet = wallet;
        }

        public void Init(HandController handController, BetController betController, GameController gameController)
        {
            this.handController = handController;
            this.betController = betController;
            this.gameController = gameController;
        }

        public void Take(IHand hand)
        {
            handController.Take(this, hand);
        }

        public void Release(IHand hand)
        {
            handController.Release(this, CheckAndGetHand(hand));
        }

        public bool CanBet(decimal amount)
        {
            return betController.CanBet(this, amount);
        }

        public void Bet(decimal amount)
        {
            betController.Bet(this, amount);
        }

        internal bool Hit(Hand hand)
        {
            return gameController.Hit(CheckAndGetHand(hand).Player);
        }

        internal void Stand(IHand hand)
        {
            gameController.Stand(CheckAndGetHand(hand).Player);
        }

        internal void DoubleUp(IHand hand)
        {
            gameController.DoubleUp(CheckAndGetHand(hand).Player);
        }

        private Hand CheckAndGetHand(IHand hand)
{
           return _Hands.FirstOrDefault(x => x == hand) ?? throw new ArgumentException("hand has'n own user");
        }
    }
}
