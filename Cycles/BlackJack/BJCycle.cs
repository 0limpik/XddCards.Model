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

        public IUser[] Users => _Users.ToArray();
        private List<User> _Users = new List<User>();

        public IHandController HandController => _HandController;
        public HandController _HandController;

        public IBetController BetController => _BetController;
        private BetController _BetController;

        public IGameController GameController => _GameController;
        public GameController _GameController;

        private IEnumerable<IState> States
        {
            get
            {
                yield return _HandController;
                yield return _BetController;
                yield return _GameController;
            }
        }

        public void Init(Wallet[] wallets, int handCount)
        {
            _HandController = new HandController();
            _BetController = new BetController();
            _GameController = new GameController();

            _Users.Clear();

            for (int i = 0; i < wallets.Length; i++)
            {
                var user = new User(wallets[i]);
                user.Init(_HandController, _BetController, _GameController);
                _Users.Add(user);
            }

            _HandController.Init(_Users, handCount);
            _BetController.Init(_Users);
            _GameController.Init(_Users);

            Reset();
        }

        public void Start()
        {
            _HandController.IsExecute = true;
            OnStateChange?.Invoke(_HandController.State);
        }

        public bool CanSwitchState(out string message)
        {
            message = null;
            IState prevState = _GameController;
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
            IState prevState = _GameController;
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
            foreach (var state in States)
            {
                state.Reset();
            }
        }
    }

    public interface IBJCycle
    {
        event Action<BJCycleStates> OnStateChange;

        IUser[] Users { get; }

        IHandController HandController { get; }
        IBetController BetController { get; }
        IGameController GameController { get; }

        bool CanSwitchState(out string message);
        void Init(Wallet[] wallets, int handCount);
        void Reset();
        void Start();
        void SwitchState();
    }
}
