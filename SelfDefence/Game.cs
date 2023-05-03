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

        TileFieldObjectLayer<(int, RectangleNode)> LandStatus = new(); // 0 is none, 100 is full
        TileFieldObjectLayer<CircleNode> Lod = new();
        TileFieldObjectLayer<Entity> Entity = new();

        Player player = new();
        Lod lod = new();
        
        public Game()
        {
            scene = new(0b1);

            //init field
            var fieldSize = new Vector2I(55, 55);
            var fieldUnitSize = new Vector2F(30, 30);
            field = new(fieldSize, fieldUnitSize);

            for(int i = 0; i < fieldSize.X; i++)
            {
                for(int j = 0; j < fieldSize.Y; j++)
                {
                    int value = new Random().Next(100, 100);

                    var t = new RectangleNode();
                    t.RectangleSize = fieldUnitSize * 0.9f;
                    t.CenterPosition = t.RectangleSize / 2;
                    var worldPos = field.AddressToPosition(new Vector2I(i, j));
                    t.Position = !worldPos.Item2 ? worldPos.Item1 : new Vector2F(0, 0);
                    t.Color = new Color((byte)(100 * value / 100), 80, 10);

                    LandStatus.LayerObjects.Add(new Vector2I(i, j), (value, t));

                    scene.AddNode(t);
                }
            }

            //init player
            {
                player.Position = new Vector2I(10, 10);
                var playerObj = new CircleNode();
                playerObj.Radius = 10;
                playerObj.VertNum = 25;
                var worldPos = field.AddressToPosition(player.Position);
                playerObj.Position = !worldPos.Item2 ? worldPos.Item1 : new Vector2F(0, 0);
                playerObj.Color = new Color(10, 10, 150);
                player.Node = playerObj;

                Entity.LayerObjects.Add(player.Position, player);
                scene.AddNode(playerObj);
            }

            //init lod
            {
                var lodObj = new CircleNode();
                lodObj.Radius = 20;
                lodObj.VertNum = 5;
                var worldPos = field.AddressToPosition(new Vector2I(20, 20));
                lodObj.Position = !worldPos.Item2 ? worldPos.Item1 : new Vector2F(0, 0);
                lodObj.Color = new Color(250, 250, 250);

                lod.Node = lodObj;
                lod.Position = new Vector2I(20, 20);
                Entity.LayerObjects.Add(new Vector2I(20, 20), lod);
                scene.AddNode(lodObj);
            }
        }

        public void Update()
        {
            UpdatePlayer();
            UpdateLand();
            UpdateGameSystem();
        }

        void UpdatePlayer()
        {
            void Move(Vector2I vec)
            {
                if (LandStatus.LayerObjects.TryGetValue(player.Position + vec, out var state) && !Entity.LayerObjects.ContainsKey(player.Position + vec))
                {
                    if (state.Item1 >= player.WalkableLandState)
                    {
                        Entity.LayerObjects.Remove(player.Position);
                        player.Position += vec;
                        Entity.LayerObjects.Add(player.Position, player);
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

        void UpdateLand()
        {
            var lodPos = lod.Position;

            var target = new Vector2I(new Random().Next(0, field.Size.X), new Random().Next(0, field.Size.Y));

            LandStatus.LayerObjects[target] = (LandStatus.LayerObjects[target].Item1 - 50, LandStatus.LayerObjects[target].Item2);
            LandStatus.LayerObjects[target].Item2.Color = new Color((byte)(100 * LandStatus.LayerObjects[target].Item1 / 100), 80, 10);
        }
    }
}
