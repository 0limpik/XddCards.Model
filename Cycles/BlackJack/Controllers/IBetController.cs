using System;
using System.Linq;

namespace Xdd.Model.Cycles.BlackJack.Controllers
{
    public interface IBetController : IState
    {
        int HandCount { get; }
        decimal Amount { get; }
    }

    internal class BetController : AState, IBetController
    {
        public override BJCycleStates State => BJCycleStates.Bet;

        private const string c_userHasTBets = "At least one player must have a bet";

        private User[] users;

        public decimal Amount => users.First().Amount;
        public int HandCount => users.First().hands.Count;

        internal BetController(User[] users)
        {
            this.users = users;
        }

        internal bool CanBet(User user, decimal amount)
        {
            Check(user);

            return user.wallet.CanReserve(amount * user.hands.Count);
        }

        internal void Bet(User user, decimal amount)
        {
            Check(user);

            if (!user.wallet.CanReserve(amount * user.hands.Count))
                throw new ArgumentException("bet can't reserve");

            user.Amount = amount;
        }

        protected override void Enter()
        {

        }

        protected override void Exit()
        {
            foreach (var user in users)
            {
                if (user.Amount > 0)
                    foreach (var hand in user.hands)
                    {
                        hand.bet = user.wallet.Reserve(user.Amount);
                    }

                user.Amount = 0;
            }

            if (users.SelectMany(x => x.hands).All(x => !x.HasBet))
                throw new Exception(c_userHasTBets);

        }

        public override bool CanExit(out string message)
        {
            if (users.Any(x => x.Amount > 0))
            {
                message = null;
                return true;
            }
            else
            {
                message = c_userHasTBets;
                return false;
            }
        }

        private void Check(User user)
        {
            CheckExecute();

            if (!users.Contains(user))
                throw new ArgumentException();
        }
    }
}
