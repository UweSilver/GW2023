using Altseed2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfDefence
{
    internal class Player
    {
        public Vector2I Position;

        public int WalkableLandState = 20;

        public CircleNode Node;
    }
}