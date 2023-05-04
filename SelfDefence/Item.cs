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
        public IEnumerable<ShapeNode> View => Content.View;
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
                var getPos = address2worldPos(Position);
                v.Position = !getPos.isError ? getPos.position : new Vector2I(0, 0);
            }
        }
    }

    interface ItemClass 
    {
        IEnumerable<ShapeNode> View { get; }
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
        public IEnumerable<ShapeNode> View => node;
    }

    class Bomb : ItemClass
    {
        public void Use(Vector2I targetAddress, TileFieldObjectLayer<Land> field)
        {
            if (field.LayerObjects.TryGetValue(targetAddress, out var land))
            {
                if (land is Tile tile && tile.State > 0)
                {
                    tile.State = 0;
                }
            }
        }

        public IEnumerable<ShapeNode> View
        {
            get { return new ShapeNode[] { new CircleNode() }; }
        }
    }
}
