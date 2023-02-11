using UnityEngine;

namespace RayTracing
{
    public class RayTracerTest : MonoBehaviour
    {
        public GameObject cube;
        Material mat;

        public void Awake()
        {
            mat = cube.GetComponent<MeshRenderer>().material;
        }

        void TestGetLine()
        {
            for (int i = 0; i < 4; i++)
            {
                RayTracer.SetSunPosition(0);
                RayTracer.GetLineFromSunToPoint(new Vector3(1, 0, 1)).ToString();
            }
        }

        void TestGetRay()
        {
            for (int i = 0; i < 4; i++)
            {
                RayTracer.SetSunPosition(i * Mathf.PI / 4);
                RayTracer.GetRayFromSunToPointHeight(new Vector3(1, 0, 1)).ToString();
            }
        }
    }
}