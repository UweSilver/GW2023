using Altseed2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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

        public delegate bool isTarget(T cnadidate);
        public bool isReachable(Vector2I startAddress, Vector2I fieldSize, isTarget canPass, isTarget isTarget)
        {
            bool[][] field = new bool[fieldSize.X][];
            for(int i = 0; i < fieldSize.X; i++)
            {
                field[i] = new bool[fieldSize.Y];
            }

            Queue<Vector2I> queue = new();
            queue.Enqueue(startAddress);
            field[startAddress.X][startAddress.Y] = true;

            while(queue.TryDequeue(out var address)){

                if (isTarget(LayerObjects[address]))
                {
                    return true;
                }

                if (!canPass(LayerObjects[address]))
                {
                    continue;
                }

                var neighbour = Enumerable.Range(0, 9).Select(i => new Vector2I(i / 3, i % 3) - new Vector2I(1, 1)).Where(v => v.X != v.Y && (v.X == 0 || v.Y == 0))
                    .Select(v => v + address)
                    .Where(v => 0 <= v.X && v.X < fieldSize.X && 0 <= v.Y && v.Y < fieldSize.Y)
                    .Where(v => !field[v.X][v.Y])
                    .Where(key => LayerObjects.ContainsKey(key));


                foreach (var n in neighbour)
                {
                    queue.Enqueue(n);
                    field[n.X][n.Y] = true;
                }
            }

            return false;
        }
    }

    interface FieldObject
    {
        public Vector2I Position { get; }
        public ShapeNode View { get; }
    }
}
