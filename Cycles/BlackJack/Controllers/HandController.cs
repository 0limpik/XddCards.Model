using System;
using System.Collections.Generic;
using System.Linq;

namespace Xdd.Model.Cycles.BlackJack.Controllers
{
    public interface IHandController : IState
    {
        IHand[] Hands { get; }
    }

    internal class HandController : AState, IHandController, IStateController
    {
        private const string c_handCount = "Hand need more 0";

        private List<User> users;

        public IHand[] Hands => _Hands.ToArray();
        private List<Hand> _Hands = new List<Hand>();

        private int HandCount => _Hands.Where(x => x.user != null).Count();

        public void Init(List<User> users, int handCount)
        {
            this.users = users;

            _Hands.Clear();

            for (int i = 0; i < handCount; i++)
            {
                _Hands.Add(new Hand());
            }
        }

        internal Hand Take(User user, IHand hand)
        {
            Check(user);

            var avalibleHand = _Hands.FirstOrDefault(x => x == hand && x.user == null)
                ?? throw new InvalidOperationException("already taken");

            avalibleHand.user = user;

            return avalibleHand;
        }

        internal Hand Release(User user, IHand hand)
        {
            Check(user);

            var userHand = _Hands.FirstOrDefault(x => x == hand && x.user == user)
                ?? throw new ArgumentException("can't release");

            userHand.user = null;

            return userHand;
        }

        private void Check(User user)
        {
            CheckExecute();

            if (!users.Contains(user))
                throw new ArgumentException();
        }

        protected override void Enter()
        {

        }

        protected override void Exit()
        {
            if (HandCount <= 0)
                throw new InvalidOperationException(c_handCount);
        }

        public override bool CanExit(out string message)
        {
            if (HandCount > 0)
            {
                message = null;
                return true;
            }
            else
            {
                message = c_handCount;
                return false;
            }
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}
