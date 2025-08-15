using System.Collections.Generic;
using CardWar.Enums;

namespace CardWar.Interfaces
{
    public interface IHaveVariant<T>
    {
        public Dictionary<ETerrain, T> VariantDict { get; }

        public void ChangeVariant(ETerrain key);
    }
}