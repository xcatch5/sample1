using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Sample1._3d;
using Nuclex.Geometry.Lines.Collisions;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.IO;


namespace Sample1._3d
{
    [Serializable]
    public class Triangle : IXmlSerializable
    {
        #region ctors
        private Triangle() { }
        
        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            ResetValues(a, b, c);
        }
        #endregion ctors

        public Vector3 this[int key]
        {
            get
            {
                return _pointAsMassive[key];
            }
            set
            {
                switch (key)
                {
                    case 0:
                        ResetValues(value, _b, _c);
                        break;
                    case 1:
                        ResetValues(_a, value, _c);
                        break;
                    case 2:
                        ResetValues(_a, _b, value);
                        break;
                }
            }
        }

        public void ResetValues(Vector3 a, Vector3 b, Vector3 c)
        {
            _a = a;
            _b = b;
            _c = c;
          
            _triangleNormaleNormalized = GetNormale();
            NormaleA = TriangleNormaleNormalized;
            NormaleB = TriangleNormaleNormalized;
            NormaleC = TriangleNormaleNormalized;

            RecalculateMaxMin();
        }

        private void RecalculateMaxMin()
        {
            MaxX = Math.Max(Math.Max(A.X, B.X), C.X);
            MaxY = Math.Max(Math.Max(A.Y, B.Y), C.Y);
            MaxZ = Math.Max(Math.Max(A.Z, B.Z), C.Z);

            MinX = Math.Min(Math.Min(A.X, B.X), C.X);
            MinY = Math.Min(Math.Min(A.Y, B.Y), C.Y);
            MinZ = Math.Min(Math.Min(A.Z, B.Z), C.Z);

            _pointAsMassive = new Vector3[] { A, B, C };

        }

        Vector3[] _pointAsMassive;

        public Vector3[] PointAsMassive { get { return _pointAsMassive; } }

        private Vector3 GetNormale()
        {
            return Vector3.Normalize(Vector3.Cross(C - A, B - A));
        }

        private Vector3 _triangleNormaleNormalized;

        public Vector3 TriangleNormaleNormalized { get { return _triangleNormaleNormalized; } }

        public Vector3 A
        {
            get
            {
                return _a;
            }
        }
        public Vector3 B
        {
            get
            {
                return _b;
            }
        }
        public Vector3 C
        {
            get
            {
                return _c;
            }
        }

        private Vector3 _normaleA;
        private Vector3 _normaleB;
        private Vector3 _normaleC;



        public Vector3 NormaleA { get { return _normaleA; } private set { _normaleA = value; } }
        public Vector3 NormaleB { get { return _normaleB; } private set { _normaleB = value; } }
        public Vector3 NormaleC { get { return _normaleC; } private set { _normaleC = value; } }

        private Vector3 _a;
        private Vector3 _b;
        private Vector3 _c;

        public float MaxX { get { return _maxX; } private set { _maxX = value; } }
        public float MaxY { get { return _maxY; } private set { _maxY = value; } }
        public float MaxZ { get { return _maxZ; } private set { _maxZ = value; } }
        public float MinX { get { return _minX; } private set { _minX = value; } }
        public float MinY { get { return _minY; } private set { _minY = value; } }
        public float MinZ { get { return _minZ; } private set { _minZ = value; } }

        private float _minX;
        private float _minY;
        private float _minZ;
        private float _maxX;
        private float _maxY;
        private float _maxZ;

        /// <summary>
        /// пересечение с отрезком
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="noVertices"></param>
        /// <returns></returns>
        public Vector3? GetIntersectionWithSection(Vector3 p0, Vector3 p1, bool noVertices = false)
        {
            var intersect = SectionIntersectTriangle(p0, p1);
            if (intersect != null && noVertices && (
                Vertex.VectorsEqual(p0, (Vector3)intersect) ||
                Vertex.VectorsEqual(p1, (Vector3)intersect) ||
                Vertex.VectorsEqual(A, (Vector3)intersect) ||
                Vertex.VectorsEqual(B, (Vector3)intersect) ||
                Vertex.VectorsEqual(C, (Vector3)intersect)))
            {
                return null;
            }

            return intersect;
        }

        /// <summary>
        ///  это пустой треугольник (площадь=0)
        /// </summary>
        /// <returns></returns>
        public bool IsZeroTriangle()
        {

            var res = false;
            var l1 = (B - A).Length();
            var l2 = (C - A).Length();
            var l3 = (B - C).Length();

            if (l1 + l2 < l3 + Vertex.Micron ||
                l1 + l3 < l2 + Vertex.Micron ||
                l3 + l2 < l1 + Vertex.Micron)
            {
                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// два треугольника совпадают
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool Equals(Triangle t)
        {
            for (int i = 0; i < 3; i++)
            {
                var v = t.PointAsMassive[i];
                if (!Vertex.VectorsEqual(v, A) &&
                    !Vertex.VectorsEqual(v, B) &&
                    !Vertex.VectorsEqual(v, C))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// поворачивает нормали и порядок точек, чтобы были вдоль переданной
        /// </summary>
        /// <param name="normale"></param>
        public void SetNormaleAlong(Vector3 normale)
        {
            if (Vector3.Dot(TriangleNormaleNormalized, normale) < 0)
            {
                ResetValues(A, C, B);
            }

        }

        /// <summary>
        /// пересечение отрезка и треугольника
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        private Vector3? SectionIntersectTriangle(Vector3 p0, Vector3 p1)
        {
            if ((p0.X > MaxX && p1.X > MaxX) ||
                (p0.Y > MaxY && p1.Y > MaxY) ||
                (p0.Z > MaxZ && p1.Z > MaxZ) ||
                (p0.X < MinX && p1.X < MinX) ||
                (p0.Y < MinY && p1.Y < MinY) ||
                (p0.Z < MinZ && p1.Z < MinZ))
            {
                return null;
            }

            var lineDirection = p1 - p0;
            var planeNormal = TriangleNormaleNormalized;

            float dot = Vector3.Dot(planeNormal, lineDirection);
            Vector3 intersection;
            if (dot == 0.0)
            {
                return null;
            }
            else
            {
                var r = (-Vector3.Dot(planeNormal, p0 - A) / dot);
                if (r > 1 || r < 0) return null;
                intersection = p0 + (p1 - p0) * r;
            }

            var u = B - A;
            var v = C - A;
            var w = intersection - A;
            var uv = Vector3.Dot(u, v);
            var vv = Vector3.Dot(v, v);
            var wv = Vector3.Dot(w, v);
            var wu = Vector3.Dot(w, u);
            var uu = Vector3.Dot(u, u);

            var s = (uv * wv - vv * wu) / (uv * uv - uu * vv);
            var t = (uv * wu - uu * wv) / (uv * uv - uu * vv);

            if (s < -Vertex.Micron || t < 0 || s + t > 1f + Vertex.Micron) return null;


            return intersection;
        }

        /// <summary>
        /// проверка, что точка находится в положительном (по нормали) 
        /// направлении от плоскости
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool IsOnPositiveSideOrOnPlane(Vector3 v)
        {
            var cosAndLength = Vector3.Dot(v - A, TriangleNormaleNormalized);
            if (cosAndLength >= -Vertex.Micron)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// клон текущего объекта
        /// </summary>
        /// <returns></returns>
        public Triangle Clone()
        {
            Triangle res = new Triangle(A, B, C);
            res.NormaleA = NormaleA;
            res.NormaleB = NormaleB;
            res.NormaleC = NormaleC;
            res._triangleNormaleNormalized = _triangleNormaleNormalized;
            return res;
        }

        /// <summary>
        /// выводи в виде строки
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

            return String.Format("[{0},{1},{2}]", A, B, C);
        }

        /// <summary>
        /// возвращает тру, если два вектора параллельны
        /// </summary>
        /// <param name="t2"></param>
        /// <returns></returns>
        public bool TrianglesPlaneParallel(Triangle t2)
        {
            var minusLength=Vector3.Dot(_triangleNormaleNormalized, t2._triangleNormaleNormalized)-1;
            return Math.Abs(minusLength) <= Vertex.Micron;
        }

        /// <summary>
        /// объединяет два треугольника, если в сумме они дают один,т.е. имеют общую сторону
        /// и оставшиеся вершины лежат на одной прямой с одной из точек на общей стороне
        /// 
        /// если два треугольника нельзя слить в один, возвращает нулл
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static Triangle GetUnitedTriangle(Triangle t1, Triangle t2)
        {
            Triangle res = null;

            if (!t1.TrianglesPlaneParallel(t2)) return null;

            int t1p1 = -1;
            int t2p1 = -1;
            int t1p2 = -1;
            int t2p2 = -1;
            int t1p3 = -1;
            int t2p3 = -1;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (t1[i] == t2[j])
                    {
                        if (t1p1 == -1)
                        {
                            t1p1 = i;
                            t2p1 = j;
                        }
                        else
                        {
                            t1p2 = i;
                            t2p2 = j;
                        }
                    }
                }
            }
            if (t1p2 == -1) return null;

            t1p3 = (new int[] { 0, 1, 2 }).First(t => t != t1p1 && t != t1p2);
            t2p3 = (new int[] { 0, 1, 2 }).First(t => t != t2p1 && t != t2p2);

            if (SegmentHelper.IsSegmentParallel(t1[t1p1] - t1[t1p3], t2[t2p1] - t2[t2p3]))
            {
                res=new Triangle(t1[t1p3], t2[t2p3], t1[t1p2]);
                if (Vector3.Dot(res.TriangleNormaleNormalized, t1.TriangleNormaleNormalized) < 0)
                {
                    res.InvertTriangle();
                }
                return res;
            }

            if (SegmentHelper.IsSegmentParallel(t1[t1p2] - t1[t1p3], t2[t2p2] - t2[t2p3]))
            {
                res = new Triangle(t1[t1p3], t2[t2p3], t1[t1p1]);
                if (Vector3.Dot(res.TriangleNormaleNormalized, t1.TriangleNormaleNormalized) < 0)
                {
                    res.InvertTriangle();
                }
                return res;
            }
            return null;
        }

        /// <summary>
        /// перенаправлят нормаль в противоположную сторону
        /// </summary>
        public void InvertTriangle()
        {
            ResetValues(_a, _c, _b);
        }

        #region сериализация
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            WriteElement(_a, "_a", writer);
            WriteElement(_b, "_b", writer);
            WriteElement(_c, "_c", writer);
            WriteElement(_normaleA, "_normaleA", writer);
            WriteElement(_normaleB, "_normaleB", writer);
            WriteElement(_normaleC, "_normaleC", writer);
            WriteElement(_triangleNormaleNormalized, "_triangleNormaleNormalized", writer);
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            _a = DeserializeVector(Convert.FromBase64String(reader.ReadElementContentAsString()));
            _b = DeserializeVector(Convert.FromBase64String(reader.ReadElementContentAsString()));
            _c = DeserializeVector(Convert.FromBase64String(reader.ReadElementContentAsString()));
            _normaleA = DeserializeVector(Convert.FromBase64String(reader.ReadElementContentAsString()));
            _normaleB = DeserializeVector(Convert.FromBase64String(reader.ReadElementContentAsString()));
            _normaleC = DeserializeVector(Convert.FromBase64String(reader.ReadElementContentAsString()));
            _triangleNormaleNormalized = DeserializeVector(Convert.FromBase64String(reader.ReadElementContentAsString()));
            reader.ReadEndElement();
            RecalculateMaxMin();
        }

        private void WriteElement(Vector3 v, string name, XmlWriter writer)
        {

            writer.WriteStartElement(name);
            var bytes = SerializeVector(v);
            writer.WriteBase64(bytes, 0, bytes.Length);
            writer.WriteEndElement();

        }

        private byte[] SerializeVector(Vector3 v)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Vector3));
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, v);
                return ms.ToArray();
            }
        }




        private Vector3 DeserializeVector(byte[] b)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Vector3));
            using (MemoryStream ms = new MemoryStream(b))
            {
                return (Vector3)serializer.Deserialize(ms);
            }
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }
        #endregion сериализация
    }
}
