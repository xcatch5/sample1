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

namespace Sample1
{
    public class SegmentHelper
    {
        public SegmentHelper()
        {
        }


        public static bool IsSegmentParallel(Vector3 s1, Vector3 s2)
        {
            var minusLength = Vector3.Dot(s1, s2)/s1.Length()/s2.Length() - 1;
            return Math.Abs(minusLength) <= Vertex.Micron;
        }

    }
}
