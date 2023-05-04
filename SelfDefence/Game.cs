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

        TileFieldObjectLayer<Land> Land = new();
        TileFieldObjectLayer<Entity> Entity = new();

        Player player;
        
        public Game()
        {
            scene = new(0b1, 0b01, 1);

            //scene.CameraNode.Scale *= 1.5f;

            //init field
            var fieldSize = new Vector2I(55, 55);
            var fieldUnitSize = new Vector2F(30, 30);
            field = new(fieldSize, fieldUnitSize);

            for(int i = 0; i < fieldSize.X; i++)
            {
                for(int j = 0; j < fieldSize.Y; j++)
                {
                    var typeIdx = new Random().Next(1, 100);
                    if(typeIdx < 99) //normal tile
                    {
                        int value = new Random().Next(100, 100);

                        var tile = new Tile(new Vector2I(i, j), field.UnitSize, field.AddressToPosition, value);
                        if(i == 0 || i == fieldSize.X - 1 || j == 0 || j == fieldSize.Y - 1)
                        {
                            tile.isEdge = true;
                        }

                        Land.LayerObjects.Add(tile.Position, tile);
                        scene.AddNode(tile.View);
                    }
                    else //rod
                    {
                        var rod = new Rod(new Vector2I(i, j), field.UnitSize, field.AddressToPosition);
                        Land.LayerObjects.Add(rod.Position, rod);
                        scene.AddNode(rod.View);
                    }
                    
                }
            }

            //init player
            {
                player = new Player(new Vector2I(10, 10), field.UnitSize, field.AddressToPosition);

                Entity.LayerObjects.Add(player.Position, player);
                scene.AddNode(player.View);
            }
        }

        public void Update()
        {
            UpdatePlayer();
            UpdateGameSystem();
            UpdateLand();

            if(Engine.Keyboard.GetKeyState(Key.T) == ButtonState.Push)
            {
                RandomCrack();

            }
            if (Engine.Keyboard.GetKeyState(Key.G) == ButtonState.Push)
                Console.WriteLine(Land.isReachable(player.Position, field.Size, (_) => _ is Tile t && t.State > player.WalkableLandState, (_) => _ is Rod));
            //scene.CameraNode.Scale *= 0.999f;
        }

        void UpdatePlayer()
        {
            void Move(Vector2I vec)
            {
                if (Land.LayerObjects.TryGetValue(player.Position + vec, out var land) && !Entity.LayerObjects.ContainsKey(player.Position + vec))
                {
                    if (land is Tile tile && tile.State >= player.WalkableLandState)
                    {
                        Entity.LayerObjects.Remove(player.Position);
                        player.Position += vec;
                        Entity.LayerObjects.Add(player.Position, player);
                    }
                }

                player.View.Position = field.AddressToPosition(player.Position).Item1;
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
            foreach(Tile tile in Land.LayerObjects.Where(land => land.Value is Tile).Select(tile => tile.Value))
            {
                if(tile.State < 100)
                {
                    tile.State-= 100 * (1 / Engine.CurrentFPS);
                }
            }
        }
        void RandomNotch()
            {
                var candidate = Land.LayerObjects.Where(land => land.Value is Tile tile && tile.isEdge).ToArray();

                var target = new Random().Next(0, candidate.Length);

                var cutSize = new Vector2I(10, 10);

                var direction = new Random().Next(0, 4);

                var cutDirection = direction switch
                {
                    0 => new Vector2I(1, 1),
                    1 => new Vector2I(-1, 1),
                    2 => new Vector2I(-1, -1),
                    _ => new Vector2I(1, -1)
                };

                cutSize *= cutDirection;

                var targetPoint = candidate[target].Key;
                var diagramPoint = targetPoint + cutSize;

                for (int i = targetPoint.X; i != diagramPoint.X; i += cutSize.X / Math.Abs(cutSize.X))
                    for (int j = targetPoint.Y; j != diagramPoint.Y; j += cutSize.Y / Math.Abs(cutSize.Y))
                    {
                        if (i == targetPoint.X || j == targetPoint.Y)
                        {
                            var pos = new Vector2I(i, j);
                            if (Land.LayerObjects.TryGetValue(pos, out var obj) && obj is Tile tile)
                            {
                                tile.State -= 100;
                            }
                        }
                    }
            }
        void RandomCrack()
            {
                var candidates = Land.LayerObjects.Where(land => land.Value is Tile);

                var targetIdx = new Random().Next(0, candidates.Count());
                var target = candidates.Skip(targetIdx).First().Key;

                var directionIdx = new Random().Next(0, 2);
                var direction = directionIdx switch
                {
                    0 => new Vector2I(1, 0),
                    _ => new Vector2I(0, 1),
                };

                for (Vector2I address = target * (new Vector2I(1, 1) - direction); address.X < field.Size.X && address.Y < field.Size.Y; address += direction)
                {
                    if (Land.LayerObjects.TryGetValue(address, out var obj) && obj is Tile tile)
                    {
                        tile.State -= 5;
                    }
                }
            }
    }
}
