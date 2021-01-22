using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Framework
{
    public static class GizmosMesh
    {
        private static readonly CircleMeshInfo _CircleMesh = new CircleMeshInfo();

        public static Mesh GetCircleMesh(float radius, int segment, Vector3 normal)
        {
            if (segment <= 2)
                throw new Exception($"边数不正确 {segment}");

            if (radius == _CircleMesh.Radius &&
                segment == _CircleMesh.Segment &&
                normal == _CircleMesh.Normal &&
                _CircleMesh.Mesh != null)
                return _CircleMesh.Mesh;

            var q = Quaternion.FromToRotation(Vector3.forward, normal);
            _CircleMesh.Radius = radius;
            _CircleMesh.Segment = segment;
            _CircleMesh.Normal = normal;

            var mesh = new Mesh();
            var vertices = new Vector3[segment + 1];
            for (int i = 0; i < segment; i++)
            {
                var angle = 360f * i / segment;
                var vertex = new Vector3(Tools.Sin(angle), Tools.Cos(angle)) * radius;
                vertex = q * vertex;
                vertices[i] = vertex;
            }
            vertices[segment] = Vector3.zero;
            mesh.vertices = vertices;

            var triangles = new int[segment * 3];
            // 三角形顺时针
            for (int i = 0; i < segment; i++)
            {
                triangles[i * 3] = i;
                triangles[i * 3 + 1] = segment;
                triangles[i * 3 + 2] = (i + 1) % segment;
            }
            mesh.triangles = triangles;

            mesh.RecalculateNormals();

            _CircleMesh.Mesh = mesh;
            return mesh;
        }

        private class CircleMeshInfo
        {
            public float Radius;
            public int Segment;
            public Vector3 Normal;
            public Mesh Mesh;
        }
    }
}
