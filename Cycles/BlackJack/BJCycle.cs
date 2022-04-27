using System;
using System.Collections.Generic;
using System.Linq;
using Xdd.Model.Cash;
using Xdd.Model.Cycles.BlackJack.Controllers;

namespace Xdd.Model.Cycles.BlackJack
{
    public interface IBJCycle
    {
        IHandController HandController { get; }
        IBetController BetController { get; }
        IGameController GameController { get; }
    }
    public interface IBJCycleController : IBJCycle
    {
        void Init(int handCount);
        void Start();

        bool CanSwitchState(out string message);
        void SwitchState();

        void Reset();
        IUser AddUser(Wallet wallet);
        void RemoveUser(IUser user);
    }

    public static class BJCycleFabric
    {
        public static IBJCycle Create() => new BJCycle();
    }

    internal class BJCycle : IBJCycle
    {
        public event Action<IState> OnStateChange;

        public IUser[] Users => _Users.ToArray();
        private List<User> _Users = new List<User>();

        public IHandController HandController => _HandController;
        public HandController _HandController;

        public IBetController BetController => _BetController;
        private BetController _BetController;

        public IGameController GameController => _GameController;
        public GameController _GameController;

        private IEnumerable<AState> States
        {
            get
            {
                yield return _HandController;
                yield return _BetController;
                yield return _GameController;
            }
        }

        public BJCycle()
        {
            _HandController = new HandController();
            _BetController = new BetController();
            _GameController = new GameController();
        }

        public IUser AddUser(Wallet wallet)
        {
            var user = new User(wallet);
            user.Init(_HandController, _BetController, _GameController);
            _Users.Add(user);

            return user;
        }

        public void RemoveUser(IUser user)
        {
            _Users.Remove(_Users.First(x => x == user));
        }

        public void Init(int handCount)
        {
            _HandController.Init(_Users, handCount);
            _BetController.Init(_Users);
            _GameController.Init(_Users);

            Reset();
        }

        public void Start()
        {
            _HandController.IsExecute = true;
            OnStateChange?.Invoke(_HandController);
        }

        public bool CanSwitchState(out string message)
        {
            message = null;
            AState prevState = _GameController;
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
            AState prevState = _GameController;
            foreach (var state in States)
            {
                if (prevState.IsExecute)
                {
                    prevState.IsExecute = false;
                    state.IsExecute = true;
                    OnStateChange?.Invoke(state);
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
}
