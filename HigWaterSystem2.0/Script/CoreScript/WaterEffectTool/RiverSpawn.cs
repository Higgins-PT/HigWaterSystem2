using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class RiverCurve
    {
        public List<RiverPoint> riverPoints;
        public List<Vector3> riverWidthPoint1;
        public List<Vector3> riverWidthPoint2;
        public RiverCurve(List<RiverPoint> riverPoints)
        {
            this.riverPoints = riverPoints;
            RiverWidthPoint();

        }
        public void RiverWidthPoint()
        {
            riverWidthPoint1 = new List<Vector3>();
            riverWidthPoint2 = new List<Vector3>();
            for (int i = 0; i < riverPoints.Count; i++)
            {
                RiverPoint riverPoint0;
                RiverPoint riverPoint1;
                float halfWidth;
                Vector3 dir;
                Vector3 widthDir;
                Vector3 point0;
                Vector3 point1;
                if (i < riverPoints.Count - 1)
                {
                    riverPoint0 = riverPoints[i];
                    riverPoint1 = riverPoints[i + 1];
                    halfWidth = riverPoint0.width / 2;
                    dir = (riverPoint1.worldPos - riverPoint0.worldPos).normalized;
                    widthDir = Vector3.Cross(dir, new Vector3(0, 1, 0)).normalized;
                    point0 = riverPoint0.worldPos + widthDir * halfWidth;
                    point1 = riverPoint0.worldPos - widthDir * halfWidth;
                }
                else
                {
                    riverPoint0 = riverPoints[i - 1];
                    riverPoint1 = riverPoints[i];
                    halfWidth = riverPoint1.width / 2;
                    dir = (riverPoint1.worldPos - riverPoint0.worldPos).normalized;
                    widthDir = Vector3.Cross(dir, new Vector3(0, 1, 0)).normalized;
                    point0 = riverPoint1.worldPos + widthDir * halfWidth;
                    point1 = riverPoint1.worldPos - widthDir * halfWidth;
                }

                riverWidthPoint1.Add(point0);
                riverWidthPoint2.Add(point1);

            }


        }

    }
    public class RiverPoint
    {
        public Vector3 worldPos;
        public float width;


    }
    public struct SplineCurve
    {
        public List<Vector3> controlPoints;
        public List<Vector3> splinePoints;


        public SplineCurve(List<Vector3> points)
        {
            controlPoints = points;
            splinePoints = new List<Vector3>();
        }
    }

    public class RiverSpawn
    {

        public static Mesh GenerateMesh(SplineCurve spline1, SplineCurve spline2, Vector3 centerPos)
        {
            if (spline1.splinePoints.Count != spline2.splinePoints.Count)
            {
                Debug.LogError("The two splines must have the same number of points.");
                return null;
            }

            Mesh mesh = new Mesh();

            int pointCount = spline1.splinePoints.Count;
            Vector3[] vertices = new Vector3[pointCount * 2];

            int[] triangles = new int[(pointCount - 1) * 6];
            Vector3[] normals = new Vector3[pointCount * 2];
            Vector2[] uvs = new Vector2[pointCount * 2];
            for (int i = 0; i < pointCount; i++)
            {
                vertices[i] = spline1.splinePoints[i] - centerPos;
                normals[i] = i < pointCount - 1 ? (spline1.splinePoints[i + 1] - spline1.splinePoints[i]).normalized : (spline1.splinePoints[i] - spline1.splinePoints[i - 1]).normalized;
                vertices[i + pointCount] = spline2.splinePoints[i] - centerPos;
                normals[i + pointCount] = i < pointCount - 1 ? (spline2.splinePoints[i + 1] - spline2.splinePoints[i]).normalized : (spline2.splinePoints[i] - spline2.splinePoints[i - 1]).normalized;
                uvs[i] = new Vector2((float)i / (pointCount - 1), 0);
                uvs[i + pointCount] = new Vector2((float)i / (pointCount - 1), 1);
            }

            // Generate triangle indices
            int triIndex = 0;
            for (int i = 0; i < pointCount - 1; i++)
            {
                // First triangle
                triangles[triIndex] = i;
                triangles[triIndex + 1] = i + pointCount;
                triangles[triIndex + 2] = i + 1;

                // Second triangle
                triangles[triIndex + 3] = i + 1;
                triangles[triIndex + 4] = i + pointCount;
                triangles[triIndex + 5] = i + pointCount + 1;

                triIndex += 6;
            }


            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.RecalculateBounds();

            return mesh;
        }
        public static SplineCurve GenerateSpline(List<Vector3> controlPoints, int resolution = 10)
        {
            SplineCurve spline = new SplineCurve(controlPoints);
            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                Vector3 p0 = i > 0 ? controlPoints[i - 1] : controlPoints[i];
                Vector3 p1 = controlPoints[i];
                Vector3 p2 = controlPoints[i + 1];
                Vector3 p3 = i != controlPoints.Count - 2 ? controlPoints[i + 2] : controlPoints[i + 1];


                for (int j = 0; j <= resolution; j++)
                {
                    float t = j / ((float)resolution + 1);
                    Vector3 point = CalculateCatmullRom(t, p0, p1, p2, p3);
                    spline.splinePoints.Add(point);
                }
            }
            return spline;
        }


        private static Vector3 CalculateCatmullRom(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            Vector3 result = 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );

            return result;
        }

    }
}