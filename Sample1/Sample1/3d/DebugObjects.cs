using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Sample1._3d
{
    public class DebugObjects
    {
        public static DebugObjects Instance = new DebugObjects();

        public static int Iterator1 = 0;
        public static int Iterator2 = 0;
        public static bool Flag1 = false;

        public List<Vector3> PointsToDraw1 = new List<Vector3>();
        public List<Vector3> PointsToDraw2 = new List<Vector3>();

        public List<Triangle> Triangles = new List<Triangle>();

        public VertexPositionNormalTexture[] GetVertexPositionNormalTextures()
        {
            List<VertexPositionNormalTexture> points = new List<VertexPositionNormalTexture>();

            for (int j = 0; j < Triangles.Count; j++)
            {
                var triangle = Triangles[j];
                points.Add(new VertexPositionNormalTexture() { Position = triangle.A, Normal = triangle.NormaleA });
                points.Add(new VertexPositionNormalTexture() { Position = triangle.B, Normal = triangle.NormaleB });
                points.Add(new VertexPositionNormalTexture() { Position = triangle.C, Normal = triangle.NormaleC });
            }
            return points.ToArray();
        }


        /// <summary>
        /// для отладки времена
        /// </summary>
        /// <param name="s"></param>
        public static void PrintTiming(string s){
            Console.WriteLine(DateTime.Now.ToString("[dd.MM.yyyy HH:mm:ss] ") + s);
        }

    }
}
