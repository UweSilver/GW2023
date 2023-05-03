using Altseed2;
using SelfDefence;
using System.Collections.Generic;
using System.Linq;

Vector2I WindowSize = new Vector2I(720 / 9 * 16, 720);

Engine.Initialize("Game", WindowSize.X, WindowSize.Y, new Configuration{ DeviceType = GraphicsDevice.DirectX12, ConsoleLoggingEnabled = true, });

var game = new Game();

while (Engine.DoEvents())
{
    Engine.Update();

    if (Engine.Keyboard.GetKeyState(Key.Escape) == ButtonState.Push)
    {
        break;
    }
}

Engine.Terminate();