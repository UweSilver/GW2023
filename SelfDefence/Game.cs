using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Altseed2;

namespace SelfDefence
{
    internal class Game
    {
        Scene scene;
        TileField field;
        
        public Game()
        {
            scene = new(0b1);

            var fieldSize = new Vector2I(55, 55);
            var fieldUnitSize = new Vector2F(30, 30);
            field = new(fieldSize, fieldUnitSize);

            for(int i = 0; i < fieldSize.X; i++)
            {
                for(int j = 0; j < fieldSize.Y; j++)
                {
                    var t = new RectangleNode();
                    t.RectangleSize = fieldUnitSize * 0.9f;
                    t.CenterPosition = t.RectangleSize / 2;
                    var worldPos = field.AddressToPosition(new Vector2I(i, j));
                    t.Position = !worldPos.Item2 ? worldPos.Item1 : new Vector2F(0, 0);
                    t.Color = new Color(100, 80, 10);

                    scene.AddNode(t);
                }
            }
        }
    }
}
