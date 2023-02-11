using UnityEngine;
using TerrainGeneration;

namespace RayTracing
{
    public static class RayTracer
    {
        private const int mapSize = MapGenerator.mapSize;
        private const float sunOrbitRadius = 1.52f * mapSize;
        private const float sunHeight = 5;

        private const float brightnessCoef = 0.4f;
        private const int distToCheckOnRay = 40;

        public static Vector3 sunPosition = new Vector3(sunOrbitRadius, 0, 0);
        private static Vector2 sunPosition2D = new Vector2(sunOrbitRadius, 0);

        /// <summary>
        /// Set la position du soleil sur le demi cercle de centre le centre de la carte et de rayon sunOrbitRadius
        /// </summary>
        /// <param name="theta"> Angle entre l'axe x et le rayon du centre vers le soleil</param>
        public static void SetSunPosition(float theta)
        {
            float r = sunOrbitRadius;
            float cos_theta = Mathf.Cos(theta), sin_theta = Mathf.Sin(theta);

            sunPosition = new Vector3(r * cos_theta + mapSize/2, sunHeight, r * sin_theta + mapSize/2);
            sunPosition2D = new Vector2(sunPosition.x, sunPosition.z);

            Debug.Log("New sun position : " + sunPosition.ToString());
        }

        /// <summary>
        /// Simule les ombres sur la map pour le soleil placé en sunPosition
        /// </summary>
        /// <param name="mapdata"></param>
        public static void RayTrace(MapData mapData)
        {
            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    CalculateLightOnPoint(new Vector3(x, mapData.heightMap[x, y], y), mapData);
                }
            }
        }

        private static void CalculateLightOnPoint(Vector3 point, MapData mapData)
        {
            //Si le point sur lequel on est est dans l'eau la hauteur qui compte est celle de la surface de l'eau
            point.y = point.y > 0.3f ? point.y : 0.3f;

            // On parcourt le segment en regardant si il y a un obstacle entre le soleil et le point
            //Line rayTrajectoryXZ = GetLineFromSunToPoint(point);
            Line rayHeight = GetRayFromSunToPointHeight(point);

            Vector2Int pointPos2D = new Vector2Int((int)point.x, (int)point.z);
            float d = Distance(pointPos2D, sunPosition2D);

            // On va parcourir le segment du point vers le soleil
            // Pour cela on calcule la direction vers laquelle on doit aller
            Vector2 direction = new Vector2(1 / d * (sunPosition2D.x - pointPos2D.x), 1 / d * (sunPosition2D.y - pointPos2D.y));
            Debug.Assert(direction != Vector2Int.zero);

            int maxXPossible = Mathf.Min(mapSize, pointPos2D.x + distToCheckOnRay);
            int minXPossible = Mathf.Max(0, pointPos2D.x - distToCheckOnRay);

            int maxYPossible = Mathf.Min(mapSize, pointPos2D.y + distToCheckOnRay);
            int minYPossible = Mathf.Max(0, pointPos2D.y - distToCheckOnRay);

            Vector2Int curPos = pointPos2D;
            Vector2Int lastPos = pointPos2D;

            for (int i = 0; curPos.x >= minXPossible && curPos.x < maxXPossible && curPos.y >= minYPossible && curPos.y < maxYPossible; i++)
            {
                if(curPos != lastPos)
                {
                    // On vérifie que ce nouveau point soit entre le point et le soleil
                    float distFromPoint = Distance(curPos, pointPos2D);
                    if(distFromPoint > d)
                    {
                        break;
                    }

                    // On regarde si la hauteur du rayon est inférieure à celle du point (x,z) i.e. la lumière ne passe pas
                    // Si le point de coordonnée (x,z) est dans l'eau, il ne peut pas faire d'ombre
                    // on vérifie que height[x,z] soit supérieur à la hauteur de l'eau
                    if(mapData.heightMap[curPos.x, curPos.y] > 0.3f && rayHeight.Evaluate(distFromPoint) < mapData.heightMap[curPos.x, curPos.y])
                    {
                        // Si c'est le cas il n'y a pas de lumère qui arrive au point, on diminue la brightness
                        mapData.colourMap[pointPos2D.y * mapSize + pointPos2D.x] = DecreaseColorBrightness(mapData.colourMap[pointPos2D.y * mapSize + pointPos2D.x]);

                        // La lumière n'arrive pas, on peut break
                        break;
                    }
                    
                }
                // Test

                lastPos = curPos;
                curPos = new Vector2Int((int)(pointPos2D.x + (i+1) * direction.x), (int)(pointPos2D.y + (i+1) * direction.y));
            }
        }

        /// <summary>
        /// Calcule les coefficients de la droite dans le plan 0xz passant par le point et par le soleil.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Line GetLineFromSunToPoint(Vector3 point)
        {
            Vector2 point1 = new Vector2(point.x, point.z);

            return GetLineFromPoints(point1, sunPosition2D);
        }

        /// <summary>
        /// Retourne la droite h(t) qui donne la hauteur du rayon allant du projeté du soleil dans le plan 0xz 
        /// vers le point en fonction de la distance au point t
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Line GetRayFromSunToPointHeight(Vector3 point)
        {
            // On cherche la hauteur du rayon allant du soleil vers le point
            // En fonction de la hauteur
            Vector2 point1 = new Vector2(0, point.y);
            // Distance entre le soleil et le point
            float d = Distance(new Vector2(point.x, point.z), sunPosition2D);
            Vector2 point2 = new Vector2(d, sunPosition.y);
            
            return GetLineFromPoints(point1, point2);
        }

        public static Line GetLineFromPoints(Vector2 point1, Vector2 point2)
        {
            float a = (point1.y - point2.y) / (point1.x - point2.x);
            float b = point2.y - a * point2.x;

            return new Line(a,b);
        }

        private static float Distance(Vector2 p1, Vector2 p2) => Mathf.Sqrt(Mathf.Pow(p1.x - p2.x, 2) + Mathf.Pow(p1.y - p2.y, 2));
        //private static float Distance(Vector3 p1, Vector3 p2) => Mathf.Sqrt(Mathf.Pow(p1.x - p2.x, 2) + Mathf.Pow(p1.y - p2.y, 2) + Mathf.Pow(p1.z - p2.z, 2));

        /// <summary>
        /// Simule la lumière d'intensité brightness sur color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="britghtness"></param>
        /// <returns> Retourne la couleur sur laquelle on ajoute la brightness </returns>
        private static Color DecreaseColorBrightness(Color color)
        {
            float h, s, b;
            Color.RGBToHSV(color, out h, out s, out b);
            return Color.HSVToRGB(h, s, brightnessCoef * b);
        }

        public struct Line
        {
            float a;
            float b;
            public Line(float a, float b)
            {
                this.a = a;
                this.b = b;
            }

            public new void ToString()
            {
                Debug.Log("a = " + a + ", b = " + b);
            }

            public float Evaluate(float t) => a * t + b;

            public bool IsNull() => a == 0 && b == 0;
        }
    }
}