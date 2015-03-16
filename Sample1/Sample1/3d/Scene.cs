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
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.IO;

namespace Sample1._3d
{
    [Serializable]
    public class Scene
    {
        public List<Object3d> Objects = new List<Object3d>();

        public int TotalIndicesCount
        {
            get
            {
                int res = 0;
                for (int i = 0; i < Objects.Count; i++)
                {
                    res += Objects[i].Triangles.Count * 3;
                }
                return res;
            }
        }

        public VertexPositionNormalTexture[] GetVertexPositionNormalTextures()
        {
            List<VertexPositionNormalTexture> points = new List<VertexPositionNormalTexture>();

            for (int i = 0; i < Objects.Count; i++)
            {
                for (int j = 0; j < Objects[i].Triangles.Count; j++)
                {
                    var triangle = Objects[i].Triangles[j];
                    points.Add(new VertexPositionNormalTexture() { Position = triangle.A, Normal = triangle.NormaleA });
                    points.Add(new VertexPositionNormalTexture() { Position = triangle.B, Normal = triangle.NormaleB });
                    points.Add(new VertexPositionNormalTexture() { Position = triangle.C, Normal = triangle.NormaleC });
                }
            }

            return points.ToArray();
        }

        public VertexPositionNormalTexture[] GetVertexPositionNormalTextures(int objectNumber)
        {
            List<VertexPositionNormalTexture> points = new List<VertexPositionNormalTexture>();


            for (int j = 0; j < Objects[objectNumber].Triangles.Count; j++)
            {
                var triangle = Objects[objectNumber].Triangles[j];
                points.Add(new VertexPositionNormalTexture() { Position = triangle.A, Normal = triangle.NormaleA });
                points.Add(new VertexPositionNormalTexture() { Position = triangle.B, Normal = triangle.NormaleB });
                points.Add(new VertexPositionNormalTexture() { Position = triangle.C, Normal = triangle.NormaleC });
            }


            return points.ToArray();
        }

        public VertexPositionColor[] GetVertexPositionColor(bool doubleSided = false)
        {
            List<VertexPositionColor> points = new List<VertexPositionColor>();

            for (int i = 0; i < Objects.Count; i++)
            {
                for (int j = 0; j < Objects[i].Triangles.Count; j++)
                {
                    var triangle = Objects[i].Triangles[j];
                    points.Add(new VertexPositionColor() { Position = triangle.A, Color = new Color(255, 255, 0, 100) });
                    points.Add(new VertexPositionColor() { Position = triangle.B, Color = new Color(255, 255, 0, 100) });
                    points.Add(new VertexPositionColor() { Position = triangle.C, Color = new Color(255, 255, 0, 100) });

                    if (doubleSided)
                    {
                        points.Add(new VertexPositionColor() { Position = triangle.A, Color = new Color(255, 255, 0, 100) });
                        points.Add(new VertexPositionColor() { Position = triangle.C, Color = new Color(255, 255, 0, 100) });
                        points.Add(new VertexPositionColor() { Position = triangle.B, Color = new Color(255, 255, 0, 100) });
                    }

                }
            }

            return points.ToArray();
        }

        public void SaveToFile(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Scene));
            using (TextWriter writer = new StreamWriter(fileName))
            {
                serializer.Serialize(writer, this);
            }
        }

        public static Scene LoadFromFile(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Scene));
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                return (Scene)serializer.Deserialize(fs);
            }
        }
        /*
        public void SaveToFile(string fileName)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(Scene));
            using (FileStream writer = new FileStream(fileName, FileMode.Create))
            {
                serializer.WriteObject(writer, this);
            }
        }

        public static Scene LoadFromFile(string fileName)
        {
            using (FileStream fs = new FileStream(fileName,
             FileMode.Open))
            {
                using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas()))
                {
                    DataContractSerializer ser = new DataContractSerializer(typeof(Scene));

                    // Deserialize the data and read it from the instance.
                    return (Scene)ser.ReadObject(reader, true);

                }
            }
        }*/
    }
}
