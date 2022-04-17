using System;
using System.Collections.Generic;
using System.Linq;
using Xdd.Model.Enums;

namespace Xdd.Model.Games
{
    internal class Deck
    {
        public List<ICard> cards;
        private List<ICard> removedCards = new List<ICard>();

        private Random Random = new Random();

        public Deck()
        {
            cards = Create().ToList();
            Shuffle();
        }

        public bool TryPeek(out ICard card)
        {
            if (cards.Count > 0)
            {
                card = cards[0];
                cards.Remove(card);
                removedCards.Add(card);

                return cards.Count > 0;
            }

            card = null;
            return false;
        }

        public void Reload()
        {
            var allCards = cards.ToList();
            allCards.AddRange(removedCards);

            cards = allCards;

            removedCards.Clear();

            Shuffle();
        }

        private void Shuffle()
        {
            for (int i = 0; i < cards.Count; i++)
            {
                var rand = Random.Next(0, cards.Count);

                (cards[i], cards[rand]) = (cards[rand], cards[i]);
            }
        }

        public static ICard[] Create()
        {
            var ranks = Enum.GetValues(typeof(Ranks)).Cast<Ranks>();
            var suits = Enum.GetValues(typeof(Suits)).Cast<Suits>();

            var cards = new Card[ranks.Count() * suits.Count()];

            var num = 0;

            foreach (var suit in suits)
            {
                foreach (var rank in ranks)
                {
                    cards[num++] = new Card { rank = rank, suit = suit };
                }
            }

            return cards;
        }
    }
}
