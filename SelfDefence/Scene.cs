using Altseed2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfDefence
{
    internal class Scene
    {
        public CameraNode CameraNode { get; }
        Node rootNode;

        public Scene(ulong cameraGroup)
        {
            rootNode = new Node();
            Engine.AddNode(rootNode);

            CameraNode = new CameraNode();
            CameraNode.Group = cameraGroup;
            rootNode.AddChildNode(CameraNode);
        }

        public void AddNode(ShapeNode node)
        {
            node.CameraGroup = CameraNode.Group;
            rootNode.AddChildNode(node);
        }

        public void RemoveNode(ShapeNode node)
        {
            rootNode.RemoveChildNode(node);
        }
    }
}
