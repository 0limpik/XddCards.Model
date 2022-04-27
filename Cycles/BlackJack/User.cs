using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xdd.Model.Cash;
using Xdd.Model.Cycles.BlackJack.Controllers;

namespace Xdd.Model.Cycles.BlackJack
{
    public interface IUser
    {
        decimal Cash { get; }
        IHand[] Hands { get; }
        decimal Amount { get; }

        ValueTask Take(IHand hand);
        ValueTask Release(IHand hand);

        bool CanBet(decimal amount);
        ValueTask Bet(decimal amount);
    }

    internal class User : IUser
    {
        internal Wallet wallet;

        public decimal Cash => wallet.Cash;
        public decimal Amount { get; internal set; }

        public IHand[] Hands => _Hands.ToArray();
        public List<Hand> _Hands = new();

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

        public ValueTask Take(IHand hand)
        {
            var takedHand = handController.Take(this, hand);

            _Hands.Add(takedHand);

            return new ValueTask();
        }

        public ValueTask Release(IHand hand)
        {
            var releasedHand = handController.Release(this, CheckAndGetHand(hand));

            _Hands.Remove(releasedHand);

            return new ValueTask();
        }

        public bool CanBet(decimal amount)
        {
            return betController.CanBet(this, amount);
        }

        public ValueTask Bet(decimal amount)
        {
            betController.Bet(this, amount);

            return new ValueTask();
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
