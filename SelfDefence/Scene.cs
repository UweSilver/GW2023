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
        public ulong SceneCameraGroup { get; }
        ulong renderTargetCameraGroup;

        List<CameraNode> cameraNodes = new();
        List<RenderTexture> renderTargets = new();
        CameraNode masterCameraNode;

        public CameraNode MainCamera;
        Node rootNode;

        public Scene(ulong sceneCameraGroup, ulong renderTargetCameraGroup, uint cameraNum)
        {
            SceneCameraGroup = sceneCameraGroup;
            this.renderTargetCameraGroup = renderTargetCameraGroup;

            rootNode = new Node();
            Engine.AddNode(rootNode);

            MainCamera = new CameraNode();
            MainCamera.Group = sceneCameraGroup;
            MainCamera.IsColorCleared = true;
            MainCamera.ClearColor = new Color(100, 100, 100);
            Engine.AddNode(MainCamera);

            //SplitScreen(cameraNum);
        }

        public void AddNode(IEnumerable<IDrawn> nodes)
        {
            foreach(var node in nodes)
            {
                node.CameraGroup = SceneCameraGroup;
                rootNode.AddChildNode(node as Node);
            }
        }

        public void RemoveNode(IEnumerable<IDrawn> nodes)
        {
            foreach(var node in nodes)
                rootNode.RemoveChildNode(node as Node);
        }

        void SplitScreen(uint num)
        {
            var screenSize = new Vector2I(Engine.WindowSize.X / (int)num, Engine.WindowSize.Y);

            for (int i = 0; i < num; i++)
            {
                var renderTarget = RenderTexture.Create(screenSize * 2, TextureFormat.R8G8B8A8_UNORM);
                renderTargets.Add(renderTarget);

                var rectangleNode = new RectangleNode();
                rectangleNode.RectangleSize = screenSize;
                rectangleNode.Position = new Vector2F(screenSize.X * i, 0);
                rectangleNode.Texture = renderTarget;
                rectangleNode.CameraGroup = renderTargetCameraGroup;
                Engine.AddNode(rectangleNode);

                var camera = new CameraNode();
                camera.TargetTexture = renderTarget;
                camera.Group = SceneCameraGroup;
                camera.ClearColor = new Color(100, 100, 100);
                camera.IsColorCleared = true;
                cameraNodes.Add(camera);
                Engine.AddNode(camera);
            }

            masterCameraNode = new CameraNode();
            masterCameraNode.Group = renderTargetCameraGroup;
            masterCameraNode.ClearColor = new Color(100, 100, 100);
            masterCameraNode.IsColorCleared = true;
            Engine.AddNode(masterCameraNode);
        }
    }
}
