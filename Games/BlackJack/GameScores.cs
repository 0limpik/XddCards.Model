using System;
using System.Collections.Generic;
using System.Linq;
using Xdd.Model.Enums;

namespace Xdd.Model.Games.BlackJack
{
    public static class GameScores
    {
        public static int[] GetBlackJackScores(IEnumerable<ICard> cards)
        {
            var values = cards
                .Where(x => x != null)
                .Select(x => GetCardValue(x))
                .ToList();

            var scores = GetScore(values);

            return scores;
        }

        private static int[] GetScore(List<int[]> values)
        {
            if (values.Count < 1)
                return new int[0];

            var value = values[0];

            if (values.Count == 1)
                return value.Distinct().ToArray();

            values.Remove(value);

            var next = GetScore(values);

            int k = 0;

            var scores = new int[value.Length * next.Length];

            for (int i = 0; i < value.Length; i++)
            {
                for (int j = 0; j < next.Length; j++)
                {
                    scores[k] = value[i] + next[j];
                    k++;
                }
            }

            return scores.Distinct().ToArray();
        }

        private static int[] GetCardValue(ICard card)
        {
            if (card == null)
                return new int[0];

            switch (card.rank)
            {
                case Ranks.Two:
                case Ranks.Three:
                case Ranks.Four:
                case Ranks.Five:
                case Ranks.Six:
                case Ranks.Seven:
                case Ranks.Eight:
                case Ranks.Nine:
                    return new int[] { (int)card.rank };
                case Ranks.Ten:
                case Ranks.Jack:
                case Ranks.Queen:
                case Ranks.King:
                    return new int[] { 10 };
                case Ranks.Ace:
                    return new int[] { 1, 11 };
            }

            throw new ArgumentException();
        }
    }
}
