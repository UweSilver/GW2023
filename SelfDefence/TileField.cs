using Altseed2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfDefence
{
    internal class TileField
    {
        public Vector2I Size { get; }
        public Vector2F UnitSize { get; }
        public TileField(Vector2I size, Vector2F unitSize)
        {
            Size = size;
            UnitSize = unitSize;
        }

        public (Vector2F, bool) AddressToPosition(Vector2I address)
        {
            bool isError = (address.X < 0 || address.X >= Size.X || address.Y < 0 || address.Y >= Size.Y);
            var position = address * UnitSize;

            return (position, isError);
        }
    }

    public delegate (Vector2F position, bool isError) Address2WorldPos(Vector2I address);

    internal class TileFieldObjectLayer<T> 
    {
        public Dictionary<Vector2I, T> LayerObjects = new();
    }

    interface FieldObject
    {
        public Vector2I Position { get; }
        public ShapeNode View { get; }
    }
}
