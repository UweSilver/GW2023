using Altseed2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfDefence
{

    interface Land : FieldObject
    {
    }

    class Tile : Land
    {
        public Vector2I Position { get; }
        public ShapeNode View => Node;
        
        public float State
        {
            get { return state; }
            set
            {
                state = Math.Max(value, 0);
                UpdateColor();
            }
        } //0 is none, 100 is full
        float state;

        private RectangleNode Node;

        public bool isEdge;

        public Tile(Vector2I address, Vector2F unitSize, Address2WorldPos address2WorldPos, int initState = 100)
        {
            Node = new RectangleNode();
            Node.RectangleSize = unitSize * 0.9f;
            Node.CenterPosition = unitSize / 2;
            var getPos = address2WorldPos(address);
            Node.Position = !getPos.isError ? getPos.position : new Vector2F(0, 0);

            Position = address;
            State = initState;

            UpdateColor();
        }

        private void UpdateColor()
        {
            Node.Color = new Color((byte)(100 * State / 100), 80, 10);
        }
    }

    class Rod : Land
    {
        public Vector2I Position { get; }
        public ShapeNode View => Node;

        private CircleNode Node;

        public Rod(Vector2I address, Vector2F unitSize, Address2WorldPos address2WorldPos)
        {
            Position = address;

            Node = new CircleNode();
            Node.Radius = unitSize.X;
            Node.VertNum = 5;
            var getPos = address2WorldPos(Position);
            Node.Position = !getPos.isError ? getPos.position : new Vector2F(0, 0);
            Node.Color = new Color(250, 250, 250);
        }
    }

}