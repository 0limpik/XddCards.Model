using System.Runtime.CompilerServices;
using Xdd.Model.Enums;

[assembly: InternalsVisibleTo("Tests")]
namespace Xdd.Model.Games
{
    internal class Card : ICard
    {
        public Suits suit { get; set; }
        public Ranks rank { get; set; }

        public override string ToString()
            => $"{suit} {rank}";
    }

    public interface ICard
    {
        Suits suit { get; }
        Ranks rank { get; }
    }
}
