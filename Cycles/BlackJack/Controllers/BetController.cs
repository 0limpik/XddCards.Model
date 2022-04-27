using System;
using System.Collections.Generic;
using System.Linq;

namespace Xdd.Model.Cycles.BlackJack.Controllers
{
    public interface IBetController : IState
    {

    }

    internal class BetController : AState, IBetController, IStateController
    {
        private const string c_userHasTBets = "At least one player must have a bet";

        internal List<User> users;

        internal void Init(List<User> users)
        {
            this.users = users;
        }

        internal bool CanBet(User user, decimal amount)
        {
            Check(user);

            return user.wallet.CanReserve(amount * user.Hands.Length);
        }

        internal void Bet(User user, decimal amount)
        {
            Check(user);

            if (!user.wallet.CanReserve(amount * user.Hands.Length))
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
                    foreach (var hand in user._Hands)
                    {
                        hand.bet = user.wallet.Reserve(user.Amount);
                    }

                user.Amount = 0;
            }

            if (users.SelectMany(x => x._Hands).All(x => !x.HasBet))
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

        public override void Reset()
        {
            foreach (var user in users)
            {
                foreach (var hand in user._Hands)
                {
                    user.wallet.Cancel(hand.bet);
                }

                user.Amount = 0;
            }
            base.Reset();
        }
    }
}
