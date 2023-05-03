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
        CameraNode cameraNode;
        Node rootNode;

        public Scene(ulong cameraGroup)
        {
            rootNode = new Node();
            Engine.AddNode(rootNode);

            cameraNode = new CameraNode();
            cameraNode.Group = cameraGroup;
            rootNode.AddChildNode(cameraNode);
        }

        public void AddNode(SpriteNode node)
        {
            node.CameraGroup = cameraNode.Group;
            rootNode.AddChildNode(node);
        }

        public void  AddNode(RectangleNode node)
        {
            node.CameraGroup = cameraNode.Group;
            rootNode.AddChildNode(node);
        }
    }
}
