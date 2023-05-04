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

        TileFieldObjectLayer<Tile> Land = new();
        TileFieldObjectLayer<Entity> Entity = new();

        Player player = new();
        Lod lod = new();
        
        public Game()
        {
            scene = new(0b1);

            scene.CameraNode.Scale *= 1.5f;

            //init field
            var fieldSize = new Vector2I(55, 55);
            var fieldUnitSize = new Vector2F(30, 30);
            field = new(fieldSize, fieldUnitSize);

            for(int i = 0; i < fieldSize.X; i++)
            {
                for(int j = 0; j < fieldSize.Y; j++)
                {
                    int value = new Random().Next(100, 100);

                    var tile = new Tile(new Vector2I(i, j), field.UnitSize, (_) => field.AddressToPosition(_).Item1, value);
                    if(i == 0 || i == fieldSize.X - 1 || j == 0 || j == fieldSize.Y - 1)
                    {
                        tile.isEdge = true;
                    }

                    Land.LayerObjects.Add(tile.Position, tile);
                    scene.AddNode(tile.Node);
                }
            }

            //init player
            {
                player.Position = new Vector2I(10, 10);
                var playerObj = new CircleNode();
                playerObj.Radius = field.UnitSize.X / 2f * 0.6f;
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
            if(Engine.Keyboard.GetKeyState(Key.T) == ButtonState.Push)
                UpdateLand();
            UpdateGameSystem();

            //scene.CameraNode.Scale *= 0.999f;
        }

        void UpdatePlayer()
        {
            void Move(Vector2I vec)
            {
                if (Land.LayerObjects.TryGetValue(player.Position + vec, out var tile) && !Entity.LayerObjects.ContainsKey(player.Position + vec))
                {
                    if (tile.State >= player.WalkableLandState)
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
            var candidate = Land.LayerObjects.Where(land => land.Value.isEdge).ToArray();

            var target = new Random().Next(0, candidate.Length);

            var cutSize = new Vector2I(5, 5);

            var direction = new Random().Next(0, 4);

            cutSize *= direction switch
            {
                0 => new Vector2I(1, 1),
                1 => new Vector2I(-1, 1),
                2 => new Vector2I(-1, -1),
                _ => new Vector2I(1, -1)
            };

            var targetPoint = candidate[target].Key;
            var diagramPoint = targetPoint + cutSize;

            for (int i = targetPoint.X; i != diagramPoint.X; i += cutSize.X / Math.Abs(cutSize.X))
                for (int j = targetPoint.Y; j != diagramPoint.Y; j += cutSize.Y / Math.Abs(cutSize.Y))
                {
                    if(i == targetPoint.X || i == diagramPoint.X - cutSize.X / Math.Abs(cutSize.X) || j == targetPoint.Y || j == diagramPoint.Y - cutSize.Y / Math.Abs(cutSize.Y))
                    {
                        var pos = new Vector2I(i, j);
                        if(Land.LayerObjects.TryGetValue(pos, out var obj))
                        {
                            obj.State -= 100;
                        }
                    }
                }
        }
    }
}
