using Altseed2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfDefence
{
    interface Entity
    {
        public Vector2I Position { get; }
        public TransformNode Node { get; }
    }

    internal class Player : Entity
    {
        public Vector2I Position { get; set; }

        public int WalkableLandState = 20;

        public TransformNode Node { get; set; }
    }

    class Lod : Entity
    {
        public Vector2I Position { get; set; }
        public TransformNode Node { get; set; }
    }
}