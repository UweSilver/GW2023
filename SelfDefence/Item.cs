using Altseed2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfDefence
{
    class DroppedItem : Entity
    {
        public Vector2I Position { get; set; }
        public IEnumerable<IDrawn> View => Content.View;
        public ItemClass Content;

        Address2WorldPos address2worldPos;

        public DroppedItem(Address2WorldPos address2WorldPos)
        {
            this.address2worldPos = address2WorldPos;
        }
        public void UpdateView()
        {
            foreach(var v in View)
            {
                if(v is TransformNode t)
                {
                    var getPos = address2worldPos(Position);
                    t.Position = !getPos.isError ? getPos.position : new Vector2I(0, 0);
                }
            }
        }
    }

    interface ItemClass 
    {
        IEnumerable<IDrawn> View { get; }
        float Height { get; set; }
    }

    class Footing : ItemClass
    {
        public Footing(Vector2F unitSize)
        {
            var back = new CircleNode();
            back.Color = new Color(255, 255, 255);
            back.Radius = unitSize.X / 2;
            back.VertNum = 25;

            var item = new RectangleNode();
            var l = back.Radius * 0.85f * 2;
            item.RectangleSize = new Vector2F(l, l);
            item.CenterPosition = item.RectangleSize / 2;
            item.ZOrder = back.ZOrder + 1;
            item.Angle = 10;
            item.Color = new Color(0, 0, 0);

            node.Add(back);
            node.Add(item);
        }

        public float Height { get; set; }

        public void Use(Vector2I targetAddress, TileFieldObjectLayer<Land> field) 
        {
            if(field.LayerObjects.TryGetValue(targetAddress, out var land))
            {
                if(land is Tile tile && tile.State < 100)
                {
                    tile.State = 100;
                }
            }
        }

        List<ShapeNode> node = new();
        public IEnumerable<IDrawn> View => node;
    }

    class Bomb : ItemClass
    {
        public float Height { get; set; }

        public Bomb(Vector2F unitSize)
        {
            var body = new CircleNode();
            body.Color = new Color(10, 10, 10);
            body.Radius = unitSize.X * 0.9f / 2;
            body.VertNum = 25;

            node.Add(body);
        }

        public IEnumerable<IDrawn> View
        {
            get => node;
        }

        List<ShapeNode> node = new();
    }

    class RespawnPoint : ItemClass
    {
        public RespawnPoint(uint id,Vector2F unitSize, Color color)
        {
            var star = new PolygonNode();
            var vertices = new List<Vertex>();
            for(int i = 0; i < 7; i++)
            {
                var Rad = unitSize.X * 0.9f * 0.5f;
                var rad = unitSize.X * 0.3f * 0.5f;

                var angle0 = i * 2.0f * MathF.PI / 5.0f;
                var angle1 = (i + 0.5f) * 2.0f * MathF.PI / 5.0f;
                vertices.Add(new Vertex( new Vector3F(Rad * MathF.Cos(angle0), Rad * MathF.Sin(angle0), 0), color, new Vector2F(0, 0), new Vector2F(0, 0)));
                vertices.Add(new Vertex(new Vector3F(rad * MathF.Cos(angle1), rad * MathF.Sin(angle1), 0), color, new Vector2F(0, 0), new Vector2F(0, 0)));
            }
            star.Vertexes = vertices;

            node.Add(star);

            ID = id;
        }

        public uint ID { get; }

        List<PolygonNode> node = new();
        public IEnumerable<IDrawn> View => node;

        public float Height { get; set; }
    }
}
