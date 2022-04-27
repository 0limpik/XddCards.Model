using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xdd.Model.Cash;
using Xdd.Model.Games;
using Xdd.Model.Games.BlackJack;
using Xdd.Model.Games.BlackJack.Users;

namespace Xdd.Model.Cycles.BlackJack
{
    public interface IHand
    {
        event Action<ICard> OnCardAdd;
        event Action<GameResult> OnResult;

        bool IsPlaying { get; }

        bool CanTurn { get; }
        PlayerStatus? Status { get; }
        IEnumerable<int> Scores { get; }

        ValueTask<bool> Hit();
        ValueTask Stand();
        ValueTask DoubleUp();
    }

    public class Hand : IHand
    {
        internal IPlayer Player
        {
            get => _Player;
            set
            {
                if (_Player != null)
                {
                    _Player.OnCardAdd -= OnCardAddInvoke;
                    _Player.OnResult -= OnResultInvoke;
                }

                _Player = value;

                if (_Player != null)
                {
                    _Player.OnCardAdd += OnCardAddInvoke;
                    _Player.OnResult += OnResultInvoke;
                }
            }
        }
        private IPlayer _Player;

        public event Action<ICard> OnCardAdd;
        public event Action<GameResult> OnResult;

        public bool IsPlaying => Player != null;
        public bool CanTurn => Player.CanTurn;
        public PlayerStatus? Status => Player.GetStatus();
        public IEnumerable<int> Scores => Player.GetScores();

        public decimal Amount => bet == null ? 0 : doubleBet == null ? bet.Amount : bet.Amount + doubleBet.Amount;

        internal Bet bet;
        internal Bet doubleBet;
        internal bool HasBet => bet != null && bet.Amount > 0;

        internal User user;

        public ValueTask<bool> Hit()
        {
            return new ValueTask<bool>(user.Hit(this));
        }

        public ValueTask Stand()
        {
            user.Stand(this);

            return new ValueTask();
        }

        public ValueTask DoubleUp()
        {
            user.DoubleUp(this);

            return new ValueTask();
        }

        private void OnCardAddInvoke(ICard card)
        {
            OnCardAdd?.Invoke(card);
        }

        private void OnResultInvoke(GameResult result)
        {
            OnResult?.Invoke(result);
        }
    }
}
