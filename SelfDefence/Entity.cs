using Altseed2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfDefence
{
    interface Entity : FieldObject
    {
    }

    internal class Player : Entity
    {
        public Vector2I Position { get; set; }

        public int WalkableLandState = 20;

        public ShapeNode View => Node;

        private CircleNode Node;

        Address2WorldPos address2WorldPos;

        public Player(Vector2I address, Vector2F unitSize, Address2WorldPos address2WorldPos)
        {
            Position = address;
            this.address2WorldPos = address2WorldPos;

            Node = new CircleNode();
            Node.Radius = unitSize.X / 2f * 0.6f;
            Node.VertNum = 25;

            var getPosition = address2WorldPos(address);
            Node.Position = !getPosition.isError ? getPosition.position : new Vector2F(0, 0);
            Node.Color = new Color(10, 10, 150);
        }
    }
}