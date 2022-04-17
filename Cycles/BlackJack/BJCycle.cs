using System;
using System.Collections.Generic;
using System.Linq;
using Xdd.Model.Cash;
using Xdd.Model.Cycles.BlackJack.Controllers;

namespace Xdd.Model.Cycles.BlackJack
{
    public enum BJCycleStates
    {
        Hand,
        Bet,
        Game
    }

    public static class BJCycleFabric
    {
        public static IBJCycle Create() => new BJCycle();
    }

    internal class BJCycle : IBJCycle
    {
        public event Action<BJCycleStates> OnStateChange;

        private User[] users;
        private List<Hand> hands;

        public User[] Users => users.ToArray();
        public Hand[] Hands => hands.ToArray();

        public HandController handController { get; private set; }
        public IBetController BetController => _BetController;
        private BetController _BetController;
        public GameController gameController { get; private set; }


        private IEnumerable<IState> States
        {
            get
            {
                yield return handController;
                yield return _BetController;
                yield return gameController;
            }
        }

        public void Init(Wallet[] wallets, int handCount)
        {
            users = new User[wallets.Length];
            hands = new List<Hand>(handCount);

            for (int i = 0; i < wallets.Length; i++)
            {
                users[i] = new User(wallets[i]);
            }

            foreach (var i in Enumerable.Range(0, handCount))
            {
                hands.Add(new Hand());
            }

            Reset();
        }

        public void Start()
        {
            handController.IsExecute = true;
            OnStateChange?.Invoke(handController.State);
        }

        public bool CanSwitchState(out string message)
        {
            message = null;
            IState prevState = gameController;
            foreach (var state in States)
            {
                if (prevState.IsExecute)
                {
                    if (!prevState.CanExit(out message))
                        return false;
                    if (!state.CanEnter(out message))
                        return false;

                    return true;
                }
                prevState = state;
            }
            throw new Exception("active state not found");
        }

        public void SwitchState()
        {
            IState prevState = gameController;
            foreach (var state in States)
            {
                if (prevState.IsExecute)
                {
                    prevState.IsExecute = false;
                    state.IsExecute = true;
                    OnStateChange?.Invoke(state.State);
                    return;
                }
                prevState = state;
            }
            throw new Exception("active state not found");
        }

        public void Reset()
        {
            handController = new HandController(users, hands);
            _BetController = new BetController(users);
            gameController = new GameController(users);

            foreach (var user in users)
            {
                user.handController = handController;
                user.betController = _BetController;
                user.gameController = gameController;

                foreach (var hand in user.hands)
                {
                    hand.gameController = gameController;
                }
            }

            foreach (var hand in hands)
            {
                hand.gameController = gameController;
            }

            foreach (var state in States)
            {
                //state.OnIncorectState += (state) =>
                    //Debug.LogAssertion($"Incorect {state} when active {States.FirstOrDefault(x => x.IsExecute)?.ToString() ?? "null"}");
            }
        }
    }

    public interface IBJCycle
    {
        event Action<BJCycleStates> OnStateChange;

        User[] Users { get; }

        HandController handController { get; }
        IBetController BetController { get; }
        GameController gameController { get; }

        bool CanSwitchState(out string message);
        void Init(Wallet[] wallets, int handCount);
        void Reset();
        void Start();
        void SwitchState();
    }
}
