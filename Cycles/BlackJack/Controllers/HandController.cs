using System;
using System.Collections.Generic;
using System.Linq;

namespace Xdd.Model.Cycles.BlackJack.Controllers
{
    public interface IHandController : IState
    {
        IHand[] AvalibleHands { get; }
    }

    internal class HandController : AState, IHandController
    {
        private const string c_handCount = "Hand need more 0";

        private List<User> users;

        public IHand[] AvalibleHands => _AvalibleHands.ToArray();
        private List<Hand> _AvalibleHands = new List<Hand>();

        private int HandCount => users.SelectMany(x => x._Hands).Count();

        public void Init(List<User> users, int handCount)
        {
            this.users = users;

            _AvalibleHands.Clear();

            for (int i = 0; i < handCount; i++)
            {
                _AvalibleHands.Add(new Hand());
            }
        }

        internal void Take(User user, IHand hand)
        {
            Check(user);

            var avalibleHand = _AvalibleHands.FirstOrDefault(x => x == hand) ?? throw new InvalidOperationException("has't free hand");

            _AvalibleHands.Remove(avalibleHand);

            avalibleHand.user = user;
            user._Hands.Add(avalibleHand);
        }

        internal void Release(User user, IHand hand)
        {
            Check(user);

            var userHand = user._Hands.FirstOrDefault(x => x == hand) ?? throw new ArgumentException("has't hands");

            userHand.user = null;
            user._Hands.Remove(userHand);

            _AvalibleHands.Add(userHand);
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
