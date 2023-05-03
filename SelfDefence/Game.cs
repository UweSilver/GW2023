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

        TileFieldObjectLayer<int> Land = new(); // 0 is none, 100 is full

        Player player = new();
        
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
                    int value = new Random().Next(0, 100);

                    var t = new RectangleNode();
                    t.RectangleSize = fieldUnitSize * 0.9f;
                    t.CenterPosition = t.RectangleSize / 2;
                    var worldPos = field.AddressToPosition(new Vector2I(i, j));
                    t.Position = !worldPos.Item2 ? worldPos.Item1 : new Vector2F(0, 0);
                    t.Color = new Color((byte)(100 * value / 100), 80, 10);

                    Land.LayerObjects.Add(new Vector2I(i, j), value);

                    scene.AddNode(t);
                }
            }
            {
                player.Position = new Vector2I(10, 10);
                var playerObj = new CircleNode();
                playerObj.Radius = 10;
                playerObj.VertNum = 25;
                var worldPos = field.AddressToPosition(player.Position);
                playerObj.Position = !worldPos.Item2 ? worldPos.Item1 : new Vector2F(0, 0);
                playerObj.Color = new Color(10, 10, 150);
                player.Node = playerObj;
                scene.AddNode(playerObj);
            }

        }

        public void Update()
        {
            UpdatePlayer();
            UpdateGameSystem();
        }

        void UpdatePlayer()
        {
            void Move(Vector2I vec)
            {
                if (Land.LayerObjects.TryGetValue(player.Position + vec, out int state))
                {
                    if (state >= player.WalkableLandState)
                    {
                        player.Position += vec;
                    }
                }

                player.Node.Position = field.AddressToPosition(player.Position).Item1;
            }
            if(Engine.Keyboard.GetKeyState(Key.W) == ButtonState.Push)
            {
                Move(new Vector2I(0, -1));
            }
            if(Engine.Keyboard.GetKeyState(Key.A) == ButtonState.Push)
            {
                Move(new Vector2I(-1, 0));
            }
            if(Engine.Keyboard.GetKeyState(Key.S) == ButtonState.Push)
            {
                Move(new Vector2I(0, 1));
            }
            if(Engine.Keyboard.GetKeyState(Key.D) == ButtonState.Push)
            {
                Move(new Vector2I(1, 0));
            }
        }

        void UpdateGameSystem()
        {

        }
    }
}
