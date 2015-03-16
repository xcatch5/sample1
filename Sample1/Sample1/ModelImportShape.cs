using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Sample1._3d;
using System.IO;

namespace Sample1
{
    class ModelImportShape
    {
        public Vector3 shapeSize;
        public Vector3 shapePosition;
        private VertexPositionNormalTexture[] shapeVertices;
        private int shapeTriangles;
        public Texture2D shapeTexture;
        bool _isConstructed = false;
        ContentManager _content;
        Scene _scene = new Scene();

        public ModelImportShape(ContentManager content)
        {
            _content = content;
        }

        string VectorToString(Vector3 v)
        {
            return string.Format("({0},{1},{2})", v.X, v.Y, v.Z);
        }

        List<Vector3> Intersections = new List<Vector3>();
        List<Vector3> PointsInside = new List<Vector3>();

        private void BuildShape()
        {
            var myModel = _content.Load<Model>("Models\\primitives4");
           var tempScene = new Scene();
            VertexHelper.ExtractModelToScene(myModel, new Matrix(), tempScene);
            string fileName = "last.xml";
            if (File.Exists(fileName))
            {
                _scene = Scene.LoadFromFile(fileName);
                _scene.Objects.Add(tempScene.Objects[2]);
                
                var o = Object3d.CreateObjectAsIntersection(_scene.Objects);
                _scene.Objects.Clear();
                _scene.Objects.Add(o);
             
                
                //_scene.SaveToFile(fileName);
            }
            else
            {
                var o = Object3d.CreateObjectAsIntersection(tempScene.Objects);
                _scene.Objects.Clear();
                _scene.Objects.Add(o);
              //  _scene.SaveToFile(fileName);
            }

            shapeVertices = _scene.GetVertexPositionNormalTextures();
            shapeTriangles = shapeVertices.Length / 3;
            _isConstructed = true;
        }

        public void RenderShape(GraphicsDevice device)
        {
            // Build the cube, setting up the _vertices array
            if (_isConstructed == false)
                BuildShape();

            var rasterizerStateBefore = device.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            // убери, чтобы были сплошные
            rasterizerState.FillMode = FillMode.WireFrame;

            device.RasterizerState = rasterizerState;

            using (VertexBuffer shapeBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture),
                                   shapeVertices.Length, BufferUsage.WriteOnly | BufferUsage.None))
            {
                shapeBuffer.SetData(shapeVertices);


                device.SetVertexBuffer(shapeBuffer);

                device.SamplerStates[0] = SamplerState.LinearClamp;
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, shapeTriangles);
            }
            device.RasterizerState = rasterizerStateBefore;
            
            // Отрисовка дебажных объектов
            var dob = DebugObjects.Instance;
            for (int i = 0; i < dob.PointsToDraw1.Count; i++)
            {
                BasicShape cubeDebug;
                cubeDebug = new BasicShape(new Vector3(0.03f, 0.06f, 0.03f), dob.PointsToDraw1[i]);
                cubeDebug.RenderShape(device);
            }

            for (int i = 0; i < dob.PointsToDraw2.Count; i++)
            {
                BasicShape cubeDebug;
                cubeDebug = new BasicShape(new Vector3(0.03f, 0.03f, 0.06f), dob.PointsToDraw2[i]);
                cubeDebug.RenderShape(device);
            }

            if (DebugObjects.Instance.Triangles.Count > 0)
            {
                var dt = DebugObjects.Instance.GetVertexPositionNormalTextures();

                using (VertexBuffer shapeBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture),
                                  dt.Length, BufferUsage.WriteOnly | BufferUsage.None))
                {
                    shapeBuffer.SetData(dt);
                    device.SetVertexBuffer(shapeBuffer);
                    device.SamplerStates[0] = SamplerState.LinearClamp;
                    device.DrawPrimitives(PrimitiveType.TriangleList,
                     0, dt.Length / 3);
                }
            }
        }
    }
}
