using UnityEngine;
using Debug = Game.Utilities.DebugX;

namespace Game.Utilities
{
    /// <summary>
    /// A utility for drawing debug shapes like rays, boxes, spheres, and distances in the Scene view.
    /// Useful for visual debugging of physics, logic, and interactions.
    /// </summary>
    public static class DebugShapeVisualizer
    {
        #region Box

        /// <summary>
        /// Draws a wireframe box using Debug.DrawLine.
        /// </summary>
        public static void DrawBox(Vector3 center, Vector3 halfExtents, Quaternion rotation, Color? color = null, float duration = 0f)
        {
            Color col = color ?? Color.white;
            Vector3[] corners = new Vector3[8];
            int index = 0;

            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                        corners[index++] = center + rotation * Vector3.Scale(halfExtents, new Vector3(x, y, z));

            int[,] edges = {
            {0,1},{1,3},{3,2},{2,0},
            {4,5},{5,7},{7,6},{6,4},
            {0,4},{1,5},{2,6},{3,7}
        };

            for (int i = 0; i < edges.GetLength(0); i++)
                Debug.DrawLine(corners[edges[i, 0]], corners[edges[i, 1]], col, duration);
        }

        #endregion

        #region Ray

        /// <summary>
        /// Draws a ray using a normalized direction and optional endpoint sphere.
        /// </summary>
        public static void DrawRay(Vector3 origin, Vector3 direction, float distance = 1f, Color? color = null, float duration = 0f, bool showEndpoint = true)
        {
            Color col = color ?? Color.white;
            Vector3 end = origin + direction.normalized * distance;

            Debug.DrawLine(origin, end, col, duration);
            if (showEndpoint)
                DrawSphere(end, Mathf.Clamp(distance * 0.02f, 0.05f, 0.5f), col, duration);
        }

        #endregion

        #region Sphere

        /// <summary>
        /// Draws a wireframe sphere.
        /// </summary>
        public static void DrawSphere(Vector3 center, float radius, Color? color = null, float duration = 0f, int segments = 16)
        {
            Color col = color ?? Color.white;
            float angleStep = 360f / segments;

            DrawCircle(center, Vector3.up, Vector3.forward, radius, segments, col, duration);
            DrawCircle(center, Vector3.right, Vector3.forward, radius, segments, col, duration);
            DrawCircle(center, Vector3.up, Vector3.right, radius, segments, col, duration);
        }

        private static void DrawCircle(Vector3 center, Vector3 axis1, Vector3 axis2, float radius, int segments, Color color, float duration)
        {
            float angleStep = 360f / segments;
            Vector3 lastPoint = center + axis1 * radius;

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 nextPoint = center + (axis1 * Mathf.Cos(angle) + axis2 * Mathf.Sin(angle)) * radius;
                Debug.DrawLine(lastPoint, nextPoint, color, duration);
                lastPoint = nextPoint;
            }
        }

        #endregion

        #region Distance

        /// <summary>
        /// Draws a line between two points to visualize distance.
        /// </summary>
        public static void DrawDistance(Vector3 from, Vector3 to, Color? color = null, float duration = 0f)
        {
            Debug.DrawLine(from, to, color ?? Color.white, duration);
        }

        #endregion
    }

}