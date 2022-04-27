using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xdd.Model.Cash;
using Xdd.Model.Games;
using Xdd.Model.Games.BlackJack;
using Xdd.Model.Games.BlackJack.Users;

namespace Xdd.Model.Cycles.BlackJack.Controllers
{
    public interface IGameController : IState
    {
        event Action<ICard> OnDillerUpHiddenCard;
        event Action OnGameEnd;

        IHand DealerHand { get; }
        IHand[] PlayerHands { get; }

        void Start();
        Task StartAsync();
    }

    internal class GameController : AState, IGameController
    {
        private const string c_usersCount = "Players must be more 0";

        private IBlackJack game;

        public event Action OnGameEnd;

        public event Action<ICard> OnDillerUpHiddenCard;

        public IHand DealerHand => _DealerHand;
        public Hand _DealerHand;

        private List<User> users;

        public IHand[] PlayerHands => users.SelectMany(x => x._Hands).ToArray();

        internal GameController()
        {
            _DealerHand = new Hand();
            InitGame();
        }

        internal void Init(List<User> users)
        {
            this.users = users;
        }

        public void Start()
        {
            CheckExecute();
            game.Start();
        }

        internal bool Hit(IPlayer player)
        {
            CheckExecute();
            return player.Hit();
        }

        internal void Stand(IPlayer player)
        {
            CheckExecute();
            player.Stand();
        }

        internal void DoubleUp(IPlayer player)
        {
            CheckExecute();
            foreach (var user in users)
            {
                foreach (var hand in user._Hands)
                {
                    if (hand.Player == player)
                    {
                        if (user.wallet.CanReserve(hand.bet.Amount))
                        {
                            hand.doubleBet = user.wallet.Reserve(hand.bet.Amount);
                        }
                        else
                        {
                            throw new InvalidOperationException("bet greater cash");
                        }
                        player.Hit();
                        if (player.CanTurn)
                            player.Stand();
                        return;
                    }
                }
            }
            throw new Exception("hand not found");
        }

        protected override void Enter()
        {
            _DealerHand.Player = game.dealer;

            var hands = users
             .SelectMany(x => x._Hands)
             .Where(x => x.HasBet);

            game.Init(hands.Count());

            var playerCount = 0;
            foreach (var hand in hands)
            {
                var player = game.players[playerCount++];
                hand.Player = player;

                player.OnResult += (result) => OnResult(result, hand);
            }
            game.Start();
        }

        protected override void Exit()
        {

        }

        private void OnResult(GameResult result, Hand hand)
        {
            var bet = hand.bet;
            var doubleBet = hand.doubleBet;
            hand.bet = hand.doubleBet = null;

            Handle(bet);

            if (doubleBet != null)
                Handle(doubleBet);

            void Handle(Bet handleBet)
            {
                if (result == GameResult.Win)
                {
                    hand.user.wallet.Give(handleBet);
                    return;
                }
                if (result == GameResult.Lose)
                {
                    hand.user.wallet.Take(handleBet);
                    return;
                }
                if (result == GameResult.Push)
                {
                    hand.user.wallet.Cancel(handleBet);
                    return;
                }
                throw new Exception($"uninspected {nameof(GameResult)}");
            }
        }

        public Task StartAsync()
        {
            throw new NotImplementedException();
        }

        public override void Reset()
        {
            InitGame();

            base.Reset();
        }

        private void InitGame()
        {
            game = new Game();
            game.OnGameEnd += () => OnGameEnd?.Invoke();
            game.OnDillerUpHiddenCard += (card) => OnDillerUpHiddenCard?.Invoke(card);
        }
    }
}
