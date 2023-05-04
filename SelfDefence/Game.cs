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
        TileFieldObjectLayer<ItemClass> FlyingEntity = new();

        Player player;

        float CrackTimer;
        float ItemTimer;
                
        public Game()
        {
            scene = new(0b11, 0b01, 1);

            //init field
            var fieldSize = new Vector2I(45, 25);
            var fieldUnitSize = new Vector2F(30, 30);
            field = new(fieldSize, fieldUnitSize);

            var getPos = field.AddressToPosition(fieldSize / 2);
            scene.MainCamera.Scale *= 1.2f;
            scene.MainCamera.Position = (!getPos.Item2 ? getPos.Item1 : new Vector2F(0, 0)) - Engine.WindowSize.To2F() * 1.2f / 2.0f;
            //scene.MainCamera.Position = new Vector2F(0, 0);

            for (int i = 0; i < fieldSize.X; i++)
            {
                for(int j = 0; j < fieldSize.Y; j++)
                {
                    var typeIdx = new Random().Next(1, 1000);
                    if(typeIdx < 990) //normal tile
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
            CrackTimer += (1 / Engine.CurrentFPS);
            ItemTimer += (1 / Engine.CurrentFPS);

            UpdatePlayer();
            UpdateFlyingItems();
            UpdateLand();

            if(CrackTimer > 5)
            {
                RandomCrack();
                CrackTimer = 0;
            }
            if(ItemTimer > 2.5)
            {
                var rand = new Random().Next(0, 100);
                int h = 5;

                var pos = new Vector2I(new Random().Next(0, field.Size.X), new Random().Next(0, field.Size.Y));
                while(FlyingEntity.LayerObjects.ContainsKey(pos) || Entity.LayerObjects.ContainsKey(pos) || (Land.LayerObjects.TryGetValue(pos, out var land) && land is Rod))
                {
                    pos = new Vector2I(new Random().Next(0, field.Size.X), new Random().Next(0, field.Size.Y));
                }

                if(rand <= 70)
                {
                    var item = new Footing(field.UnitSize);
                    item.Height = h;
                    FlyingEntity.LayerObjects.Add(pos, item);
                    var targetPos = field.AddressToPosition(pos);
                    foreach(var v in item.View)
                    {
                        if(v is ShapeNode t)
                            t.Position = targetPos.Item1 + new Vector2F(0, -1) * item.Height * field.UnitSize.Y;
                    }
                    scene.AddNode(item.View);
                }
                else if(rand  > 70)
                {
                    var item = new Bomb(field.UnitSize);
                    item.Height = h;
                    FlyingEntity.LayerObjects.Add(pos, item);
                    var targetPos = field.AddressToPosition(pos);
                    foreach (var v in item.View)
                    {
                        if (v is ShapeNode t)
                            t.Position = targetPos.Item1 + new Vector2F(0, -1) * item.Height * field.UnitSize.Y;
                    }
                    scene.AddNode(item.View);
                }

                ItemTimer = 0;
            }

            CheckLand();
            CheckEntity();

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

                player.UpdateView();
            }
            if(Engine.Keyboard.GetKeyState(Key.W) == ButtonState.Push)
            {
                Move(new Vector2I(0, -1));
                player.Direction = new Vector2I(0, -1);
            }
            if(Engine.Keyboard.GetKeyState(Key.A) == ButtonState.Push)
            {
                Move(new Vector2I(-1, 0));
                player.Direction = new Vector2I(-1, 0);
            }
            if(Engine.Keyboard.GetKeyState(Key.S) == ButtonState.Push)
            {
                Move(new Vector2I(0, 1));
                player.Direction = new Vector2I(0, 1);
            }
            if(Engine.Keyboard.GetKeyState(Key.D) == ButtonState.Push)
            {
                Move(new Vector2I(1, 0));
                player.Direction = new Vector2I(1, 0);
            }
            if(Engine.Keyboard.GetKeyState(Key.E) == ButtonState.Push)
            {
                if(player.Inventory != null)
                {
                    //use/release
                    if(player.Inventory is Footing footing)
                    {
                        var focusPos = player.Position + player.Direction;
                        if(Land.LayerObjects.TryGetValue(focusPos, out var land) && land is Tile tile && tile.State < 100)
                        {
                            tile.State = 100;
                            player.Inventory = null;
                        }
                        else if(land is Rod)
                        {
                            CrackTimer -= 5;
                            player.Inventory = null;
                        }
                    }
                    else if(player.Inventory is Bomb bomb)
                    {
                        var focusPos = player.Position + player.Direction * 3;
                        if(Land.LayerObjects.TryGetValue(focusPos, out var land) && land is Tile tile && tile.State > 0)
                        {
                            tile.State = 0;
                            player.Inventory = null;
                        }
                    }
                }
                else
                {
                    //grub
                    var focusPos = player.Position + player.Direction;
                    if(Entity.LayerObjects.TryGetValue(focusPos, out var entity))
                    {
                        if(entity is DroppedItem item)
                        {
                            player.Inventory = item.Content;
                            Entity.LayerObjects.Remove(item.Position);
                            scene.RemoveNode(item.View);
                        }
                    }
                }
            }
        }

        void UpdateFlyingItems()
        {
            Queue<Vector2I> removelist = new();
            foreach(var item in FlyingEntity.LayerObjects)
            {
                if(item.Value.Height > 0)
                {
                    item.Value.Height -= (1 / Engine.CurrentFPS);
                    
                    foreach(var v in item.Value.View)
                    {
                        if(v is ShapeNode t)
                        {
                            var targetPos = field.AddressToPosition(item.Key);
                            t.Position = targetPos.Item1 + new Vector2F(0, -1) * item.Value.Height * field.UnitSize.Y;
                        }
                    }
                }

                if(item.Value.Height <= 0)
                {
                    removelist.Enqueue(item.Key);

                    if(item.Value is Footing footing)
                    {
                        var dropped = new DroppedItem(field.AddressToPosition) { Content = item.Value, Position = item.Key };
                        //scene.AddNode(dropped.View);
                        dropped.UpdateView();
                        Entity.LayerObjects.Add(item.Key, dropped);
                        removelist.Enqueue(item.Key);
                    }
                    else if(item.Value is Bomb bomb)
                    {
                        if(Entity.LayerObjects.TryGetValue(item.Key, out var entity) && entity is Player p && p.Inventory == null)
                        {
                            p.Inventory = bomb;
                            scene.RemoveNode(bomb.View);
                        }
                        else if(Land.LayerObjects.TryGetValue(item.Key, out var land) && land is Tile tile)
                        {
                            tile.State = 0;
                            scene.RemoveNode(bomb.View);
                        }
                        removelist.Enqueue(item.Key);
                    }
                }
            }

            foreach (var v in removelist)
                FlyingEntity.LayerObjects.Remove(v);
        }

        void CheckEntity()
        {
            Queue<Vector2I> remove = new(); 
            foreach(var e in Entity.LayerObjects)
            {
                var pos = e.Key;
                if (Land.LayerObjects.TryGetValue(pos, out var land) && land is Tile tile && tile.State <= 0)
                {
                    remove.Enqueue(pos);
                }
            }

            foreach(var v in remove)
            {
                Entity.LayerObjects.Remove(v);
            }
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

            var rand = new Random();
            for (Vector2I address = target * (new Vector2I(1, 1) - direction); address.X < field.Size.X && address.Y < field.Size.Y; address += direction)
            {
                if (Land.LayerObjects.TryGetValue(address, out var obj) && obj is Tile tile && rand.Next(0, 100) > 40)
                {
                    tile.State -= 5;
                }
            }
        }

        void CheckLand()
        {
            bool isReachable(Vector2I startAddress, Vector2I fieldSize ,Action<Vector2I> connectedAdd)
            {
                bool[][] field = new bool[fieldSize.X][];
                for (int i = 0; i < fieldSize.X; i++)
                {
                    field[i] = new bool[fieldSize.Y];
                }

                Queue<Vector2I> queue = new();
                queue.Enqueue(startAddress);
                field[startAddress.X][startAddress.Y] = true;

                while (queue.TryDequeue(out var address))
                {

                    if ((Land.LayerObjects[address]) is Rod)
                    {
                        connectedAdd(address);
                        return true;
                    }

                    if (!(Land.LayerObjects[address] is Tile tile && tile.State > player.WalkableLandState))
                    {
                        continue;
                    }

                    var neighbour = Enumerable.Range(0, 9).Select(i => new Vector2I(i / 3, i % 3) - new Vector2I(1, 1)).Where(v => v.X != v.Y && (v.X == 0 || v.Y == 0))
                        .Select(v => v + address)
                        .Where(v => 0 <= v.X && v.X < fieldSize.X && 0 <= v.Y && v.Y < fieldSize.Y)
                        .Where(v => !field[v.X][v.Y])
                        .Where(key => Land.LayerObjects.ContainsKey(key));
                    connectedAdd(address);

                    foreach (var n in neighbour)
                    {
                        queue.Enqueue(n);
                        field[n.X][n.Y] = true;
                    }
                }

                return false;
            }
            bool[][] check = new bool[field.Size.X][];
            bool[][] connectedToRod = new bool[field.Size.X][];

            for(int i = 0; i < field.Size.X; i++)
            {
                check[i] = new bool[field.Size.Y];
                connectedToRod[i] = new bool[field.Size.Y];
            }

            int count = 0;
            for(int i = 0; i < field.Size.X; i++)
                for(int j = 0; j < field.Size.Y; j++)
                {
                    if (check[i][j]) continue;
                    if(Land.LayerObjects.TryGetValue(new Vector2I(i, j), out var obj))
                    {
                        if((obj is Tile t && t.State > player.WalkableLandState))
                        {
                            var queue = new Queue<Vector2I>();
                            connectedToRod[i][j] = isReachable(new Vector2I(i, j), field.Size, (_) => { check[_.X][_.Y] = true; queue.Enqueue(_); });

                            foreach(var p in queue)
                            {
                                check[p.X][p.Y] = true;
                                connectedToRod[p.X][p.Y] = connectedToRod[i][j];
                            }
                            count += queue.Count();
                        }
                    }
                }

            for(int i = 0; i < field.Size.X; i++)
                for(int j = 0; j < field.Size.Y; j++)
                {
                    if (check[i][j] && (!connectedToRod[i][j]))
                        if(Land.LayerObjects.TryGetValue(new Vector2I(i, j), out var land ))
                            if(land is Tile tile)
                            {
                                tile.State = 0;
                            }
                }
        }
    }
}
