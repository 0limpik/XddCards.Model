using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Xdd.Model.Games.BlackJack.Users;

[assembly: InternalsVisibleTo("XddCards.Tests")]
namespace Xdd.Model.Games.BlackJack
{
    public class Game : IBlackJack
    {
        public delegate int[] GetScores(IEnumerable<ICard> cards);

        public event Action OnGameEnd;
        public event Action<ICard> OnDillerUpHiddenCard;

        public IPlayer[] players => _players;
        public IPlayer dealer => _dealer;

        private Player[] _players;
        private Dealer _dealer;
        private ICard dillerHiddenCard;

        internal Deck deck = new Deck();

        public bool isGame { get; private set; }

        public Game()
        {
            _dealer = new Dealer(this, GameScores.GetBlackJackScores);
            _players = new Player[0];
        }

        public void Init(int playersCount)
        {
            if (playersCount <= 0)
                throw new ArgumentException("Players must be more 0");

            _players = new Player[playersCount];

            for (int i = 0; i < playersCount; i++)
            {
                _players[i] = new Player(this, GameScores.GetBlackJackScores, _dealer);
            }
        }

        public void Start()
        {
            if (isGame)
                throw new InvalidOperationException("game is start");

            isGame = true;

            _dealer.Reset();

            foreach (var player in _players)
            {
                player.Reset();
            }

            _dealer.AddCard(GetCard());
            dillerHiddenCard = GetCard();

            foreach (var player in _players)
            {
                player.AddCard(GetCard());
                player.AddCard(GetCard());
            }

            if (!_players.Any(x => x.CanTurn))
            {
                EndGame();
            }
        }

        internal bool Hit(IPlayer player)
        {
            GetPlayer(player).AddCard(GetCard());

            CheckGameIstEnd();

            return player.CanTurn;
        }

        internal void Stand(IPlayer player)
        {
            GetPlayer(player).CanTurn = false;

            CheckGameIstEnd();
        }

        private void EndGame()
        {
            UpDillerHiddenCard();

            if (!GetNotNotifiedPlayers().Any())
            {
                isGame = false;
                OnGameEnd?.Invoke();
                return;
            }

            while (_dealer.CanTurn)
            {
                _dealer.AddCard(GetCard());
            }

            if (_dealer.IsBust())
            {
                Notify(GameResult.Win);
                return;
            }

            if (_dealer.IsBlackJack())
            {
                Notify(GameResult.Lose);
                return;
            }

            foreach (var player in GetNotNotifiedPlayers())
            {
                if (player.IsMore())
                {
                    player.InvokeOnResult(GameResult.Win);
                    continue;
                }

                if (player.IsEquals())
                {
                    player.InvokeOnResult(GameResult.Push);
                    continue;
                }

                player.InvokeOnResult(GameResult.Lose);
            }

            isGame = false;
            OnGameEnd?.Invoke();

            void Notify(GameResult result)
            {
                NotifyPlayers(result);
                isGame = false;
                OnGameEnd?.Invoke();
            }
        }

        private void NotifyPlayers(GameResult result)
        {
            foreach (var player in GetNotNotifiedPlayers())
            {
                player.InvokeOnResult(result);
            }
        }

        private IEnumerable<Player> GetNotNotifiedPlayers()
            => _players.Where(x => !x.isNotifiedResult);

        private void UpDillerHiddenCard()
        {
            _dealer.AddCardHidden(dillerHiddenCard);
            OnDillerUpHiddenCard?.Invoke(dillerHiddenCard);
        }

        private ICard GetCard()
        {
            if (!deck.TryPeek(out ICard card))
            {
                deck.Reload();
            }
            return card;
        }

        private Player GetPlayer(IPlayer player)
        {
            if (!isGame) throw new Exception("Game is End");

            var ret = players.FirstOrDefault(x => x == player) as Player ?? throw new ArgumentException("Player not found");

            if (!player.CanTurn)
                throw new Exception("Can't turn");

            return ret;
        }

        private void CheckGameIstEnd()
        {
            if (!_players.Any(x => x.CanTurn))
            {
                EndGame();
            }
        }
    }

    public enum GameResult
    {
        Win = 1,
        Lose,
        Push,
    }
}
