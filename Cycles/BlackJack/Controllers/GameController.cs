using System;
using System.Linq;
using Xdd.Model.Cash;
using Xdd.Model.Games;
using Xdd.Model.Games.BlackJack;
using Xdd.Model.Games.BlackJack.Users;

namespace Xdd.Model.Cycles.BlackJack.Controllers
{
    public class GameController : AState
    {
        public override BJCycleStates State => BJCycleStates.Game;

        private const string c_usersCount = "Players must be more 0";

        private IBlackJack game = new Game();

        public event Action OnGameEnd
        {
            add => game.OnGameEnd += value;
            remove => game.OnGameEnd -= value;
        }

        public event Action<ICard> OnDillerUpHiddenCard
        {
            add => game.OnDillerUpHiddenCard += value;
            remove => game.OnDillerUpHiddenCard -= value;
        }

        public Hand dealerHand { get; private set; }

        private User[] users;

        private IPlayer[] Players => game.players;

        internal GameController(User[] users)
        {
            this.users = users;

            dealerHand = new Hand { player = game.dealer };
        }

        public void Start()
        {
            CheckExecute();
            game.Start();
        }

        public void Hit(IPlayer player)
        {
            CheckExecute();
            player.Hit();
        }

        public void Stand(IPlayer player)
        {
            CheckExecute();
            player.Stand();
        }

        public void DoubleUp(IPlayer player)
        {
            CheckExecute();
            foreach (var user in users)
            {
                foreach (var hand in user.hands)
                {
                    if (hand.player == player)
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
            game.Init(users
            .SelectMany(x => x.hands)
            .Where(x => x.HasBet)
            .Count());

            var playerCount = 0;

            foreach (var player in users)
            {
                foreach (var hand in player.hands)
                {
                    var user = game.players[playerCount++];
                    hand.player = user;

                    user.OnResult += (result) => OnResult(result, player, hand);
                }
            }
        }

        protected override void Exit()
        {

        }

        private void OnResult(GameResult result, User player, Hand hand)
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
                    player.wallet.Give(handleBet);
                    return;
                }
                if (result == GameResult.Lose)
                {
                    player.wallet.Take(handleBet);
                    return;
                }
                if (result == GameResult.Push)
                {
                    player.wallet.Cancel(handleBet);
                    return;
                }
                throw new Exception($"uninspected {nameof(GameResult)}");
            }
        }
    }
}
