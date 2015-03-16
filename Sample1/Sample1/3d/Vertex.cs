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

namespace Sample1._3d
{
    [Serializable]
    public class Vertex
    {
        public static readonly float Micron = 0.00001f;

        public Vector3 Vector;

        public static Vector3 VectorMax(){return new Vector3(10000,10000,0);}

        public static bool VectorsEqual(Vector3 v1, Vector3 v3) {
            if ((v3 - v1).Length() < Micron) return true;
            return false;
        }

        /// <summary>
        /// не проверено - расстояние от точки до отрезка
        /// </summary>
        /// <param name="point"></param>
        /// <param name="anchor"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        float DistanceFromPointToLineSegment(Vector3 point, Vector3 anchor, Vector3 end)
        {
            Vector3 d = end - anchor;
            float length = d.Length();
            if (length == 0) return (point - anchor).Length();
            d.Normalize();
            float intersect = Vector3.Dot((point - anchor), d);
            if (intersect < 0) return (point - anchor).Length();
            if (intersect > length) return (point - end).Length();
            return (point - (anchor + d * intersect)).Length();
        }
    }
}
