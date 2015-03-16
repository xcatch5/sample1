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
    public class Object3d
    {
        public string Name;

        public List<Triangle> Triangles = new List<Triangle>();

        public List<Vector3> GetAllIntersections(Object3d o2)
        {
            var intersections = new List<Vector3>();
            for (int i = 0; i < Triangles.Count; i++)
            {
                for (int j = 0; j < o2.Triangles.Count; j++)
                {
                    var t2 = o2.Triangles[j];
                    var intersection = Triangles[i].GetIntersectionWithSection(t2.A, t2.B);
                    if (intersection != null)
                    {
                        intersections.Add((Vector3)intersection);
                    }
                    intersection = Triangles[i].GetIntersectionWithSection(t2.B, t2.C);
                    if (intersection != null)
                    {
                        intersections.Add((Vector3)intersection);
                    }
                    intersection = Triangles[i].GetIntersectionWithSection(t2.A, t2.C);
                    if (intersection != null)
                    {
                        intersections.Add((Vector3)intersection);
                    }
                }
            }
            return intersections;
        }

        /// <summary>
        /// работает для замкнутых фигур
        /// Проверяет, что точка находится внутри объекта
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool PointIsInsideObject(Vector3 v)
        {


            // Находим все треугольники в плоскости z=v.z  
            List<Triangle> triangles = new List<Triangle>();
            List<Triangle> innerTriangles = new List<Triangle>();
            List<Triangle> outerTriangles = new List<Triangle>();

            for (int i = 0; i < Triangles.Count; i++)
            {
                var tr = Triangles[i];
                if (Vertex.VectorsEqual(tr.A, v) || Vertex.VectorsEqual(tr.B, v) || Vertex.VectorsEqual(tr.C, v) ||
                    Vertex.VectorsEqual((tr.A + tr.B) / 2, v) || Vertex.VectorsEqual((tr.B + tr.C) / 2, v) || Vertex.VectorsEqual((tr.C + tr.A) / 2, v)

                    ) return false;

                if (v.Z >= tr.MinZ - Vertex.Micron && v.Z <= tr.MaxZ + Vertex.Micron)
                {
                    // треугольник в плоскости
                    triangles.Add(tr);
                    if (!tr.IsOnPositiveSideOrOnPlane(v))
                    {
                        innerTriangles.Add(tr);
                    }
                    else
                    {
                        outerTriangles.Add(tr);
                    }
                }
            }
            if (triangles.Count == 0)
            {
                return false;
            }
            if (outerTriangles.Count == 0)
            {
                return true;
            }

            // для каждого треугольника наружу ищем треугольник 
            // ближе "внутрь"

            for (int i = 0; i < outerTriangles.Count; i++)
            {
                var outt = outerTriangles[i];
                Vector3 vOut = new Vector3((outt.A.X + outt.B.X + outt.C.X) / 3, (outt.A.Y + outt.B.Y + outt.C.Y) / 3, v.Z);
                bool hasInner = false;
                for (int j = 0; j < innerTriangles.Count; j++)
                {
                    if (innerTriangles[j].GetIntersectionWithSection(v, vOut) != null)
                    {
                        hasInner = true;
                        break;
                    }
                    if (!hasInner)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool PointIsInsideObjectOld(Vector3 v)
        {
            for (int i = 0; i < Triangles.Count; i++)
            {
                if (Triangles[i].IsOnPositiveSideOrOnPlane(v))
                {
                    return false;
                }
            }
            return true;
        }

        public List<Vector3> GetPointsInsideObject(Object3d o2)
        {

            var pointsInside = new List<Vector3>();

            for (int i = 0; i < Triangles.Count; i++)
            {
                for (int pointN = 0; pointN < 3; pointN++)
                {
                    var point = Triangles[i].PointAsMassive[pointN];
                    if (o2.PointIsInsideObject(point))
                    {
                        pointsInside.Add(point);
                        Console.WriteLine("point inside: " + point);
                    }
                }
            }

            return pointsInside;
        }

        public void RemoveAllTrianglesContainingPoints(List<Vector3> points)
        {
            for (int p = 0; p < points.Count; p++)
            {
                for (int i = 0; i < Triangles.Count; i++)
                {
                    for (int pointN = 0; pointN < 3; pointN++)
                    {
                        if (Triangles[i].PointAsMassive[pointN] == points[p])
                        {
                            Triangles.RemoveAt(i--);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// клонирование объекта
        /// </summary>
        /// <returns></returns>
        public Object3d Clone()
        {
            Object3d res = new Object3d();
            for (int i = 0; i < Triangles.Count; i++)
            {
                res.Triangles.Add(Triangles[i].Clone());
            }
            return res;
        }

        /// <summary>
        /// создает объект, как объединение переданных
        /// </summary>
        /// <param name="ob"></param>
        /// <returns></returns>
        public static Object3d CreateObjectAsIntersection(List<Object3d> ob)
        {
            Object3d res = ob[0];

            for (int i = 1; i < ob.Count; i++)
            {
                DebugObjects.PrintTiming("CreateObjectAsIntersection: " + i + " of " + ob.Count);
                DebugObjects.Flag1 = i == 1;
                res = CreateObjectAsIntersection(res, ob[i]);
                // break;
            }


            DebugObjects.PrintTiming("CreateObjectAsIntersection:simplify");
            res.SimplifyObjectPrecise();
            DebugObjects.PrintTiming("CreateObjectAsIntersection:finish");
            return res;
        }

        public static Object3d CreateObjectAsIntersection(Object3d po1, Object3d po2)
        {
            var o1 = po1.Clone();
            var o2 = po2.Clone();

            o1.SplitByIntersections(o2);
            o1.RemoveTrianglesInsideObject(po2);


            o2.RemoveTrianglesInsideObject(po1);
            Object3d res = new Object3d();
            res.Triangles.AddRange(o1.Triangles);
            res.Triangles.AddRange(o2.Triangles);
            return res;

        }

        /// <summary>
        /// удяляем треугольники внутри другой фигуры
        /// </summary>
        /// <param name="container"></param>
        private void RemoveTrianglesInsideObject(Object3d container)
        {
            for (int i = 0; i < Triangles.Count; i++)
            {
                var t = Triangles[i];
                if (container.PointIsInsideObject(t.A) ||
                    container.PointIsInsideObject(t.B) ||
                    container.PointIsInsideObject(t.C) ||
                    container.PointIsInsideObject((t.A + t.B) / 2) ||
                    container.PointIsInsideObject((t.B + t.C) / 2) ||
                    container.PointIsInsideObject((t.A + t.C) / 2))
                {
                    if (DebugObjects.Flag1 && i >= 0 && i <= 30)
                    {
                        //DebugObjects.Instance.PointsToDraw2.Add(Triangles[i].A);
                        //DebugObjects.Instance.PointsToDraw2.Add(Triangles[i].B);
                        //DebugObjects.Instance.PointsToDraw2.Add(Triangles[i].C);
                    }
                    if (!DebugObjects.Flag1)
                    {
                        //      DebugObjects.Instance.Triangles.Add(Triangles[i]);
                    }
                    Triangles.RemoveAt(i--);
                }
            }
        }

        #region intersection
        /// <summary>
        /// для двух фигур сливаем две точки, если расстояние между
        /// ними мало
        /// и удаляем пустые треугольники
        /// </summary>
        /// <param name="o2"></param>
        private void RemoveMicronDistance(Object3d o2)
        {
            for (int i = 0; i < Triangles.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var v = Triangles[i].PointAsMassive[j];

                    for (int i2 = i + 1; i2 < Triangles.Count; i2++)
                    {
                        var tr2 = Triangles[i2];
                        if (Vertex.VectorsEqual(v, tr2.A))
                        {
                            tr2.ResetValues(v, tr2.B, tr2.C);
                        }
                        if (Vertex.VectorsEqual(v, tr2.B))
                        {
                            tr2.ResetValues(tr2.A, v, tr2.C);
                        }
                        if (Vertex.VectorsEqual(v, tr2.C))
                        {
                            tr2.ResetValues(tr2.A, tr2.B, v);
                        }
                    }

                    for (int i2 = 0; i2 < o2.Triangles.Count; i2++)
                    {
                        var tr2 = o2.Triangles[i2];
                        if (Vertex.VectorsEqual(v, tr2.A))
                        {
                            tr2.ResetValues(v, tr2.B, tr2.C);
                        }
                        if (Vertex.VectorsEqual(v, tr2.B))
                        {
                            tr2.ResetValues(tr2.A, v, tr2.C);
                        }
                        if (Vertex.VectorsEqual(v, tr2.C))
                        {
                            tr2.ResetValues(tr2.A, tr2.B, v);
                        }
                    }
                }
            }

            for (int i = 0; i < Triangles.Count; i++)
            {
                if (Triangles[i].IsZeroTriangle()) Triangles.RemoveAt(i--);
            }

            for (int i = 0; i < o2.Triangles.Count; i++)
            {
                if (o2.Triangles[i].IsZeroTriangle()) o2.Triangles.RemoveAt(i--);
            }
        }

        /// <summary>
        /// на всех точках пересечения текущего объекта и из параметров
        /// создает вершины
        /// </summary>
        /// <param name="o2"></param+>
        /// <param name="intersections"></param>
        /// <returns></returns>
        public List<Vector3> SplitByIntersections(Object3d o2, List<Vector3> intersections = null)
        {

            DebugObjects.PrintTiming("SplitByIntersections:start");

            int iteration = 0;
            while (true)
            {
                intersections = intersections ?? new List<Vector3>();
                if (intersections.Count > 1000)
                {

                    Console.WriteLine("WARNING: 200 intersections");
                    break;
                }


                DebugObjects.PrintTiming("SplitByIntersections: iteration " + iteration++);
                //     RemoveMicronDistance(o2);


                int o1MaxIndex = Triangles.Count;
                int o2MaxIndex = o2.Triangles.Count;

                bool hasIntersections = false;

                for (int i = 0; i < o1MaxIndex; i++)
                {
                    var t1 = Triangles[i];
                    for (int j = 0; j < o2MaxIndex; j++)
                    {
                        var t2 = o2.Triangles[j];
                        List<Triangle> outT1;
                        List<Triangle> outT2;
                        List<Vector3> intersectionPoints_temp;


                        DebugObjects.Iterator2++;

                        if (DebugObjects.Iterator2 >= 535 && DebugObjects.Iterator2 < 536)
                        {
                            //DebugObjects.Instance.Triangles.Add(t1);
                            // DebugObjects.Instance.Triangles.Add(t2);
                        }

                        if (IntersectTriangles(t1, t2, out outT1, out  outT2, out intersectionPoints_temp))
                        {

                            hasIntersections = true;

                            o1MaxIndex--;
                            o2MaxIndex--;

                            Triangles.RemoveAt(i);
                            Triangles.AddRange(outT1);
                            o2.Triangles.RemoveAt(j);
                            o2.Triangles.AddRange(outT2);
                            intersections.AddRange(intersectionPoints_temp);
                            i--;
                            break;
                        }
                    }
                }

                DebugObjects.PrintTiming("SplitByIntersections:iteration completed");
                if (!hasIntersections)
                {
                    break;
                }
                // if (iteration > 1) break;
            }
            DebugObjects.PrintTiming("SplitByIntersections:finish");
            return intersections;
        }

        /// <summary>
        /// пересечаение двух треугольников - на выходе как надо распилить 2 треугольника
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="outT1"></param>
        /// <param name="outT2"></param>
        /// <param name="intersectionPoints"></param>
        /// <returns></returns>
        private bool IntersectTriangles(Triangle t1, Triangle t2, out List<Triangle> outT1, out List<Triangle> outT2, out List<Vector3> intersectionPoints)
        {
            outT1 = new List<Triangle>();
            outT2 = new List<Triangle>();
            intersectionPoints = new List<Vector3>();

            // если у треугольников 2 вершины совпадают - уходим
            int sameVerices = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (Vertex.VectorsEqual(t1.PointAsMassive[i], t2.PointAsMassive[j])) sameVerices++;
                }
            }
            if (sameVerices >= 2) return false;



            bool hasIntersection = false;
            Vector3 p1 = Vertex.VectorMax(), p2 = Vertex.VectorMax();

            var t1points = new Vector3[] { t1.A, t1.B, t1.C, t1.A, t1.B, t1.C };
            var t2points = new Vector3[] { t2.A, t2.B, t2.C, t2.A, t2.B, t2.C };



            #region поиск пересечений
            for (int i = 0; i < 3; i++)
            {
                var intersection = t1.GetIntersectionWithSection(t2points[i], t2points[i + 1], false);
                if (intersection != null)
                {

                    if (hasIntersection)
                    {
                        if (!Vertex.VectorsEqual((Vector3)intersection, p1))
                        {
                            p2 = (Vector3)intersection;
                        }
                    }
                    else
                    {
                        p1 = (Vector3)intersection;
                    }
                    hasIntersection = true;
                }
                intersection = t2.GetIntersectionWithSection(t1points[i], t1points[i + 1], false);
                if (intersection != null)
                {
                    if (hasIntersection)
                    {
                        if (!Vertex.VectorsEqual((Vector3)intersection, p1))
                        {
                            p2 = (Vector3)intersection;
                        }
                    }
                    else
                    {

                        p1 = (Vector3)intersection;
                    }
                    hasIntersection = true;
                }
            }
            #endregion поиск пересечений

            if (!hasIntersection || p1 == Vertex.VectorMax() || p2 == Vertex.VectorMax() || Vertex.VectorsEqual(p1, p2)) return false;
            intersectionPoints.Add(p1);
            intersectionPoints.Add(p2);

            Vector3 p1t = p1, p2t = p2;

            if ((p1 - t1.B).Length() > (p2 - t1.B).Length())
            {
                p1t = p2; p2t = p1;
            }

            outT1.Add(new Triangle(p1t, p2t, t1.A));
            outT1.Add(new Triangle(p1t, p2t, t1.C));
            outT1.Add(new Triangle(p1t, t1.C, t1.B));
            outT1.Add(new Triangle(p1t, t1.A, t1.B));
            outT1.Add(new Triangle(p2t, t1.A, t1.C));
            if (Vertex.VectorsEqual(p1t, t1.A))
            {
                outT1.Add(new Triangle(p2t, p1t, t1.B));
                outT1.Add(new Triangle(p2t, t1.C, t1.B));
            }

            for (int n = 0; n < outT1.Count; n++)
            {
                if (outT1[n].IsZeroTriangle()) { outT1.RemoveAt(n--); continue; }
                outT1[n].SetNormaleAlong(t1.TriangleNormaleNormalized);
            }

            for (int n = 0; n < outT1.Count && outT1.Count > 1; n++)
            {
                if (outT1[n].Equals(t1)) { outT1.RemoveAt(n--); continue; }
                outT1[n].SetNormaleAlong(t1.TriangleNormaleNormalized);
            }


            p1t = p1; p2t = p2;
            if ((p1 - t2.B).Length() > (p2 - t2.B).Length())
            {
                p1t = p2; p2t = p1;
            }

            outT2.Add(new Triangle(p1t, p2t, t2.A));
            outT2.Add(new Triangle(p1t, p2t, t2.C));
            outT2.Add(new Triangle(p1t, t2.C, t2.B));
            outT2.Add(new Triangle(p1t, t2.A, t2.B));
            outT2.Add(new Triangle(p2t, t2.A, t2.C));
            if (Vertex.VectorsEqual(p1t, t2.A))
            {
                outT2.Add(new Triangle(p2t, p1t, t2.B));
                outT2.Add(new Triangle(p2t, t2.C, t2.B));
            }


            for (int n = 0; n < outT2.Count; n++)
            {
                if (outT2[n].IsZeroTriangle()) { outT2.RemoveAt(n--); continue; }
                outT2[n].SetNormaleAlong(t2.TriangleNormaleNormalized);
            }

            for (int n = 0; n < outT2.Count && outT2.Count > 1; n++)
            {
                if (outT2[n].Equals(t2)) { outT2.RemoveAt(n--); continue; }
                outT2[n].SetNormaleAlong(t2.TriangleNormaleNormalized);
            }

            return hasIntersection;
        }
        #endregion intersection

        #region
        /// <summary>
        /// убирает из объекта лишние точки:
        /// когда 2 треугольника в сумме составляют один
        /// </summary>
        void SimplifyObjectPrecise()
        {
            for (int i = 0; i < Triangles.Count; i++)
            {
                for (int j = i + 1; j < Triangles.Count; j++)
                {
                    var t1 = Triangles[i];
                    var t2 = Triangles[j];

                    Triangle sumTriangle = Triangle.GetUnitedTriangle(t1,t2);
                    if (sumTriangle != null)
                    {
                        Triangles[i] = sumTriangle;
                        Triangles.RemoveAt(j);
                        i--;
                        j--;
                    }
                }
            }
        }
        #endregion
    }
}
