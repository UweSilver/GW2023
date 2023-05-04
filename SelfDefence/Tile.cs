using Altseed2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfDefence
{
    internal class Tile
    {
        public int State
        {
            get { return state; }
            set
            {
                state = value;
                UpdateColor();
            }
        } //0 is none, 100 is full
        int state;

        public Vector2I Position;
        public RectangleNode Node;

        public bool isEdge;

        public Tile(Vector2I address, Vector2F unitSize, Address2WorldPos address2WorldPos, int initState = 100)
        {
            Node = new RectangleNode();
            Node.RectangleSize = unitSize * 0.9f;
            Node.CenterPosition = unitSize / 2;
            Node.Position = address2WorldPos(address);

            Position = address;
            State = initState;

            UpdateColor();
        }

        private void UpdateColor()
        {
            Node.Color = new Color((byte)(100 * State / 100), 80, 10);
        }

        public delegate Vector2F Address2WorldPos(Vector2I address);
    }
}
