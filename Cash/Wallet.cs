using System;
using System.Collections.Generic;
using System.Linq;

namespace Xdd.Model.Cash
{
    [Serializable]
    public class Wallet
    {
        public event Action OnChange;

        public decimal Cash { get; private set; }

        private List<Bet> bets = new List<Bet>();

        public decimal All => Cash + BetsAmount;
        public decimal BetsAmount => bets.Select(x => x.Amount).Sum();

        public Wallet(decimal cache)
        {
            this.Cash = cache;
        }

        public bool CanReserve(decimal amount)
            => Cash - amount >= 0;

        public Bet Reserve(decimal amount)
        {
            if (!CanReserve(amount))
                throw new ArgumentException("bet greater cash");

            Cash -= amount;

            var bet = new Bet(amount);
            bets.Add(bet);

            OnChange?.Invoke();

            return bet;
        }

        public void Take(Bet bet)
        {
            if (!bets.Remove(bet))
                throw new ArgumentException("bet not found");

            OnChange?.Invoke();
        }

        public void Give(Bet bet)
        {
            if (!bets.Remove(bet))
                throw new ArgumentException("bet not found");

            Cash += bet.Amount * 2;

            OnChange?.Invoke();
        }

        public void Cancel(Bet bet)
        {
            Cash += bet.Amount;

            if (!bets.Remove(bet))
                throw new ArgumentException();

            OnChange?.Invoke();
        }
    }
}
