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
    public class VertexHelper
    {
        public VertexHelper()
        {
        }

        private struct TriangleVertexIndices
        {
            public int A;
            public int B;
            public int C;
        }

        /// <summary>
        /// Extract the vertices and indices from the specified model
        /// </summary>
        /// <param name="vertices">Output the list of vertices</param>
        /// <param name="indices">Output the list of indices</param>
        /// <param name="worldPosition">The models world position or use Matrix.Identity for object space</param>
        public static void ExtractModelToScene(Model modelToUse, Matrix worldPosition, Scene scene)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<TriangleVertexIndices> indices = new List<TriangleVertexIndices>();
            Matrix transform = Matrix.Identity;
            int lastVertex = 0;
            int lastIndex = 0;
            foreach (ModelMesh mesh in modelToUse.Meshes)
            {
                Object3d currentObject = new Object3d();
                currentObject.Name = mesh.Name;

                // If the model has bones the vertices have to be transformed by the bone position
                transform = mesh.ParentBone.Transform;// Matrix.Multiply(GetAbsoluteTransform(mesh.ParentBone), Matrix.Identity);
                ExtractModelMeshData(mesh, ref transform, vertices, indices);

                List<Vector3> newVertices = new List<Vector3>();
                List<TriangleVertexIndices> newTriangles = new List<TriangleVertexIndices>();
                Dictionary<int, int> globalToLocalIndex = new Dictionary<int, int>();

                for (int i = lastIndex; i < indices.Count; i++)
                {                    
                    currentObject.Triangles.Add(new Triangle(vertices[indices[i].A], 
                        vertices[indices[i].B],vertices[indices[i].C] ));
                }
                scene.Objects.Add(currentObject);
                
                lastIndex = indices.Count;
                lastVertex = vertices.Count;
            }
        }

        /// <summary>
        /// Transform by a bone position or Identity if no bone is supplied
        /// </summary>
        private static Matrix GetAbsoluteTransform(ModelBone bone)
        {
            if (bone == null)
            {
                return Matrix.Identity;
            }
            return bone.Transform * GetAbsoluteTransform(bone.Parent);
        }

        /// <summary>
        /// Get all the triangles from all mesh parts
        /// </summary>
        private static void ExtractModelMeshData(ModelMesh mesh, ref Matrix transform,
            List<Vector3> vertices, List<TriangleVertexIndices> indices)
        {
            foreach (ModelMeshPart meshPart in mesh.MeshParts)
            {
                ExtractModelMeshPartData(meshPart, ref transform, vertices, indices);
            }
        }

        /// <summary>
        /// Get all the triangles from each mesh part (Changed for XNA 4)
        /// </summary>
        private static void ExtractModelMeshPartData(ModelMeshPart meshPart, ref Matrix transform,
            List<Vector3> vertices, List<TriangleVertexIndices> indices)
        {
            // Before we add any more where are we starting from
            int offset = vertices.Count;

            // == Vertices (Changed for XNA 4.0)

            // Read the format of the vertex buffer
            VertexDeclaration declaration = meshPart.VertexBuffer.VertexDeclaration;
            VertexElement[] vertexElements = declaration.GetVertexElements();
            // Find the element that holds the position
            VertexElement vertexPosition = new VertexElement();
            foreach (VertexElement vert in vertexElements)
            {
                if (vert.VertexElementUsage == VertexElementUsage.Position &&
                    vert.VertexElementFormat == VertexElementFormat.Vector3)
                {
                    vertexPosition = vert;
                    // There should only be one
                    break;
                }
            }
            // Check the position element found is valid
            if (vertexPosition == null ||
                vertexPosition.VertexElementUsage != VertexElementUsage.Position ||
                vertexPosition.VertexElementFormat != VertexElementFormat.Vector3)
            {
                throw new Exception("Model uses unsupported vertex format!");
            }
            // This where we store the vertices until transformed
            Vector3[] allVertex = new Vector3[meshPart.NumVertices];
            // Read the vertices from the buffer in to the array
            meshPart.VertexBuffer.GetData<Vector3>(
                meshPart.VertexOffset * declaration.VertexStride + vertexPosition.Offset,
                allVertex,
                0,
                meshPart.NumVertices,
                declaration.VertexStride);
            // Transform them based on the relative bone location and the world if provided
                for (int i = 0; i != allVertex.Length; ++i)
                {
                    Vector3.Transform(ref allVertex[i], ref transform, out allVertex[i]);
                }
            // Store the transformed vertices with those from all the other meshes in this model
            vertices.AddRange(allVertex);

            // == Indices (Changed for XNA 4)

            // Find out which vertices make up which triangles
            if (meshPart.IndexBuffer.IndexElementSize != IndexElementSize.SixteenBits)
            {
                // This could probably be handled by using int in place of short but is unnecessary
                throw new Exception("Model uses 32-bit indices, which are not supported.");
            }
            // Each primitive is a triangle
            short[] indexElements = new short[meshPart.PrimitiveCount * 3];
            meshPart.IndexBuffer.GetData<short>(
                meshPart.StartIndex * 2,
                indexElements,
                0,
                meshPart.PrimitiveCount * 3);
            // Each TriangleVertexIndices holds the three indexes to each vertex that makes up a triangle
            TriangleVertexIndices[] tvi = new TriangleVertexIndices[meshPart.PrimitiveCount];
            for (int i = 0; i != tvi.Length; ++i)
            {
                // The offset is because we are storing them all in the one array and the 
                // vertices were added to the end of the array.
                tvi[i].A = indexElements[i * 3 + 0] + offset;
                tvi[i].B = indexElements[i * 3 + 1] + offset;
                tvi[i].C = indexElements[i * 3 + 2] + offset;
            }
            // Store our triangles
            indices.AddRange(tvi);
        }


    }
}
