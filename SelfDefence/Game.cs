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

        Player player1;
        Player player2;
        RespawnPoint respawn1;
        RespawnPoint respawn2;

        float CrackTimer;
        float ItemTimer;

        Action<int> winnerNotify;

        SpriteNode CountDown;
        bool countdownActive = false;
        Texture2D[] CountDownTextures = new Texture2D[4];
                
        public Game(Action<int> winnerNotify)
        {
            this.winnerNotify = winnerNotify;

            scene = new(0b11, 0b01, 1);

            //init field
            var fieldSize = new Vector2I(35, 15);
            var fieldUnitSize = new Vector2F(30, 30);
            field = new(fieldSize, fieldUnitSize);

            var getPos = field.AddressToPosition(fieldSize / 2);
            scene.MainCamera.Scale *= 1.2f;
            scene.MainCamera.Position = (!getPos.Item2 ? getPos.Item1 : new Vector2F(0, 0)) - Engine.WindowSize.To2F() * 1.2f / 2.0f;


            List<Vector2I> rodPos = new();
            var rand = new Random();
            for(int i = 0; i < 1; i++)
            {
                rodPos.Add(new Vector2I(rand.Next(0, field.Size.X), rand.Next(0, field.Size.Y)));
            }

            for (int i = 0; i < fieldSize.X; i++)
            {
                for(int j = 0; j < fieldSize.Y; j++)
                {
                    if(!rodPos.Contains(new Vector2I(i, j))) //normal tile
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
                player1 = new Player(1, new Vector2I(10, 10), field.UnitSize, field.AddressToPosition, new Color(150, 10, 10));

                Entity.LayerObjects.Add(player1.Position, player1);
                scene.AddNode(player1.View);

                respawn1 = new RespawnPoint(1, field.UnitSize, new Color(150, 10, 10));
                DroppedItem spawn1 = new(field.AddressToPosition) { Position = new Vector2I(12, 12), Content = respawn1};
                spawn1.UpdateView();
                Entity.LayerObjects.Add(spawn1.Position, spawn1);
                scene.AddNode(spawn1.Content.View);

                player2 = new Player(2, new Vector2I(25, 10), field.UnitSize, field.AddressToPosition, new Color(10, 10, 150));

                Entity.LayerObjects.Add(player2.Position, player2);
                scene.AddNode(player2.View);

                respawn2 = new RespawnPoint(2, field.UnitSize, new Color(10, 10, 150));
                DroppedItem spawn2 = new(field.AddressToPosition) { Position = new Vector2I(27, 12), Content = respawn2 };
                spawn2.UpdateView();
                Entity.LayerObjects.Add(spawn2.Position, spawn2);
                scene.AddNode(spawn2.Content.View);
            }


            CountDown = new SpriteNode();
            for(int i = 3; i >= 0; i--)
            {
                CountDownTextures[i] = Texture2D.Load((i + 1) + @".png");
            }
            CountDown.Scale *= 0.6f;
            CountDown.Position = new Vector2I(Engine.WindowSize.X / 2, Engine.WindowSize.Y) - CountDownTextures[0].Size;
        }

        public void Update()
        {
            CrackTimer += (1 / Engine.CurrentFPS);
            ItemTimer += (1 / Engine.CurrentFPS);

            var idx = (int)MathF.Floor(5 - CrackTimer);

            if (0 <= idx && idx < 4)
            {
                if (!countdownActive)
                {
                    scene.AddNode(new IDrawn[] { CountDown });
                    countdownActive = true;
                }
                CountDown.Texture = CountDownTextures[idx];
            }
            else if (idx <= 0) { }
            else
            {
                if (countdownActive)
                {
                    scene.RemoveNode(new IDrawn[] { CountDown });
                    countdownActive = false;
                }
            }

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
            void Move(Vector2I vec, Player player)
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

                player1.UpdateView();
            }
            if(Engine.Keyboard.GetKeyState(Key.W) == ButtonState.Push)
            {
                Move(new Vector2I(0, -1), player1);
                player1.Direction = new Vector2I(0, -1);
            }
            if(Engine.Keyboard.GetKeyState(Key.A) == ButtonState.Push)
            {
                Move(new Vector2I(-1, 0), player1);
                player1.Direction = new Vector2I(-1, 0);
            }
            if(Engine.Keyboard.GetKeyState(Key.S) == ButtonState.Push)
            {
                Move(new Vector2I(0, 1), player1);
                player1.Direction = new Vector2I(0, 1);
            }
            if(Engine.Keyboard.GetKeyState(Key.D) == ButtonState.Push)
            {
                Move(new Vector2I(1, 0), player1);
                player1.Direction = new Vector2I(1, 0);
            }

            if (Engine.Keyboard.GetKeyState(Key.I) == ButtonState.Push)
            {
                Move(new Vector2I(0, -1), player2);
                player2.Direction = new Vector2I(0, -1);
            }
            if (Engine.Keyboard.GetKeyState(Key.J) == ButtonState.Push)
            {
                Move(new Vector2I(-1, 0), player2);
                player2.Direction = new Vector2I(-1, 0);
            }
            if (Engine.Keyboard.GetKeyState(Key.K) == ButtonState.Push)
            {
                Move(new Vector2I(0, 1), player2);
                player2.Direction = new Vector2I(0, 1);
            }
            if (Engine.Keyboard.GetKeyState(Key.L) == ButtonState.Push)
            {
                Move(new Vector2I(1, 0), player2);
                player2.Direction = new Vector2I(1, 0);
            }

            if (Engine.Keyboard.GetKeyState(Key.E) == ButtonState.Push)
            {
                if(player1.Inventory != null)
                {
                    //use/release
                    if(player1.Inventory is Footing footing)
                    {
                        var focusPos = player1.Position + player1.Direction;
                        if(Land.LayerObjects.TryGetValue(focusPos, out var land) && land is Tile tile && tile.State < 100)
                        {
                            tile.State = 100;
                            player1.Inventory = null;
                        }
                        else if(land is Rod)
                        {
                            CrackTimer -= 5;
                            player1.Inventory = null;
                        }
                    }
                    else if(player1.Inventory is Bomb bomb)
                    {
                        var focusPos = player1.Position + player1.Direction * 3;
                        var target = Enumerable.Range(0, 9).Select(i => new Vector2I(i / 3 - 1, i % 3 - 1) + focusPos);
                        foreach(var t in target)
                            if(Land.LayerObjects.TryGetValue(t, out var land) && land is Tile tile && tile.State > 0)
                            {
                                tile.State = 0;
                            }
                        player1.Inventory = null;
                    }
                    else if(player1.Inventory is RespawnPoint respawn)
                    {
                        var focusPos = player1.Position + player1.Direction;
                        if(Land.LayerObjects.TryGetValue(focusPos, out var land) && land is Tile tile && tile.State > player1.WalkableLandState && !Entity.LayerObjects.ContainsKey(focusPos))
                        {
                            DroppedItem item = new(field.AddressToPosition) { Position = focusPos, Content = respawn};
                            item.UpdateView();
                            Entity.LayerObjects.Add(item.Position, item);
                            scene.AddNode(item.Content.View);
                            player1.Inventory = null;
                        }
                    }
                }
                else
                {
                    //grub
                    var focusPos = player1.Position + player1.Direction;
                    if(Entity.LayerObjects.TryGetValue(focusPos, out var entity))
                    {
                        if(entity is DroppedItem item)
                        {
                            player1.Inventory = item.Content;
                            Entity.LayerObjects.Remove(item.Position);
                            scene.RemoveNode(item.View);
                        }
                    }
                }
            }

            if (Engine.Keyboard.GetKeyState(Key.O) == ButtonState.Push)
            {
                if (player2.Inventory != null)
                {
                    //use/release
                    if (player2.Inventory is Footing footing)
                    {
                        var focusPos = player2.Position + player2.Direction;
                        if (Land.LayerObjects.TryGetValue(focusPos, out var land) && land is Tile tile && tile.State < 100)
                        {
                            tile.State = 100;
                            player2.Inventory = null;
                        }
                        else if (land is Rod)
                        {
                            CrackTimer -= 5;
                            player2.Inventory = null;
                        }
                    }
                    else if (player2.Inventory is Bomb bomb)
                    {
                        var focusPos = player2.Position + player2.Direction * 3;
                        if (Land.LayerObjects.TryGetValue(focusPos, out var land) && land is Tile tile && tile.State > 0)
                        {
                            tile.State = 0;
                            player2.Inventory = null;
                        }
                    }
                    else if (player2.Inventory is RespawnPoint respawn)
                    {
                        var focusPos = player2.Position + player2.Direction;
                        if (Land.LayerObjects.TryGetValue(focusPos, out var land) && land is Tile tile && tile.State > player2.WalkableLandState && !Entity.LayerObjects.ContainsKey(focusPos))
                        {
                            DroppedItem item = new(field.AddressToPosition) { Position = focusPos, Content = respawn };
                            item.UpdateView();
                            Entity.LayerObjects.Add(item.Position, item);
                            scene.AddNode(item.Content.View);
                            player2.Inventory = null;
                        }
                    }
                }
                else
                {
                    //grub
                    var focusPos = player2.Position + player2.Direction;
                    if (Entity.LayerObjects.TryGetValue(focusPos, out var entity))
                    {
                        if (entity is DroppedItem item)
                        {
                            player2.Inventory = item.Content;
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
                        if (Entity.LayerObjects.TryGetValue(item.Key, out var entity) && entity is Player p && p.Inventory == null)
                        {
                            p.Inventory = footing;
                            scene.RemoveNode(footing.View);
                        }
                        else
                        {

                            var dropped = new DroppedItem(field.AddressToPosition) { Content = item.Value, Position = item.Key };
                            //scene.AddNode(dropped.View);
                            dropped.UpdateView();
                            Entity.LayerObjects.Add(item.Key, dropped);
                        }
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
                if (Entity.LayerObjects[v] is Player player)
                {
                    player.Position = Entity.LayerObjects.Where(obj => obj.Value is DroppedItem dropped && dropped.Content is RespawnPoint respawn && respawn.ID == player.ID).First().Key;
                    player.UpdateView();
                }
                else if (Entity.LayerObjects[v] is DroppedItem dropped && dropped.Content is RespawnPoint respawn)
                {
                    winnerNotify((int)(3 - respawn.ID));
                    break;
                }

                scene.RemoveNode(Entity.LayerObjects[v].View);
                Entity.LayerObjects.Remove(v);
            }
        }

        void UpdateLand()
        {
            foreach(Tile tile in Land.LayerObjects.Where(land => land.Value is Tile).Select(tile => tile.Value))
            {
                if(tile.State < 100)
                {
                    tile.State-= 5 * (1 / Engine.CurrentFPS);
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
                if (Land.LayerObjects.TryGetValue(address, out var obj) && obj is Tile tile && rand.Next(0, 100) > 20)
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

                    if (!(Land.LayerObjects[address] is Tile tile && tile.State > player1.WalkableLandState))
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
                        if((obj is Tile t && t.State > player1.WalkableLandState))
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
