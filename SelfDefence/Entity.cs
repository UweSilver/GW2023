using Altseed2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SelfDefence
{
    interface Entity : FieldObject
    {
    }

    class Character : Entity
    {
        public Vector2I Position { get; set; }

        public int WalkableLandState = 20;

        public Vector2I Direction
        {
            get => direction;
            set
            {
                direction = value;
                UpdateView();
            }
        }
        protected Vector2I direction;

        public IEnumerable<IDrawn> View => new ShapeNode[] { Node };

        protected CircleNode Node;

        public ItemClass? Inventory = null;

        protected Address2WorldPos address2WorldPos;

        public Character(Vector2I address, Vector2F unitSize, Address2WorldPos address2WorldPos)
        {
            Position = address;
            this.address2WorldPos = address2WorldPos;

            Node = new CircleNode();
            Node.Radius = unitSize.X / 2f * 0.6f;
            Node.VertNum = 3;
            Node.Angle = -90;

            var getPosition = address2WorldPos(address);
            Node.Position = !getPosition.isError ? getPosition.position : new Vector2F(0, 0);
            Node.Color = new Color(10, 10, 150);
        }

        public void UpdateView()
        {
            var getPosition = address2WorldPos(Position);
            Node.Position = !getPosition.isError ? getPosition.position : new Vector2F(0, 0);

            Node.Angle = -90 + direction switch
            {
                Vector2I(0, -1) => 0,
                Vector2I(0, 1) => 180,
                Vector2I(1, 0) => 90,
                Vector2I(-1, 0) => -90,
                _ => 0
            };
        }
    }

    class Player : Character
    {
        public uint ID { get; }
        public Player(uint id, Vector2I address, Vector2F unitSize, Address2WorldPos address2WorldPos, Color color) : base(address, unitSize, address2WorldPos) 
        {
            Node.Color = color;
            this.ID = id;
        }
    }

    class NPC : Character
    {
        public NPC(Vector2I address, Vector2F unitSize, Address2WorldPos address2WorldPos) :base(address, unitSize, address2WorldPos)
        {
            Node.Color = new Color(150, 10, 10);
        }

        public void Update()
        {

        }

        public void SetObjective()
        {

        }
    }
}