using System;
using System.Collections.Generic;
using System.Linq;

namespace Xdd.Model.Cycles.BlackJack.Controllers
{
    public class HandController : AState
    {
        public override BJCycleStates State => BJCycleStates.Hand;

        private const string c_handCount = "Hand need more 0";

        private User[] users;
        private List<Hand> avalibleHands;

        private int HandCount => users.SelectMany(x => x.hands).Count();


        internal HandController(User[] users, List<Hand> hands)
        {
            this.users = users;
            this.avalibleHands = hands;
        }

        internal void Take(User user)
        {
            Check(user);

            var hand = avalibleHands.FirstOrDefault();

            if (hand == null)
                throw new InvalidOperationException("has't free hand");

            avalibleHands.Remove(hand);

            user.hands.Add(hand);
        }

        internal void Release(User user)
        {
            Check(user);

            var hand = user.hands.FirstOrDefault();

            if (hand == null)
                throw new ArgumentException("has't hands");

            user.hands.Remove(hand);

            avalibleHands.Add(hand);
        }

        void Check(User user)
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
    }
}
