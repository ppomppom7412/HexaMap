using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Hexa
{
    //헥사곤 맵의 좌표계 참고
    //https://catlikecoding.com/unity/tutorials/hex-map/part-1/

    public class HexaTile : MonoBehaviour
    {
        //좌표계
        public Hexa hexa;

        public int index;           //번호
        public int element =0;         //속성

    }

    [System.Serializable]
    public class Hexa
    {
        [SerializeField]
        private int x, z;

        public Vector3 POS { get { return new Vector3(x, -x - z, z); } }
        public int X { get { return x; } }
        public int Z { get { return z; } }
        public int Y {
            get {
                return -X - Z;
            }
        }

        public Hexa(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public static Hexa FromOffsetHexa(int x, int z) 
        {
            return new Hexa(x - z / 2, z);
        }

        public override string ToString()
        {
            return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
        }

        public static Vector3 TransPostionHexaToWorld(Vector3 pos) 
        {
            Vector3 result;
            result.x = (pos.x + pos.z * 0.5f - pos.z / 2) * (HexMetrics.innerRadius * 2f);
            result.y = 0f;
            result.z = pos.z * (HexMetrics.outerRadius * 1.5f);

            return result;
        }

        public static float Distance(Vector3 start, Vector3 end)
        {
            return Vector2.Distance(new Vector2(start.x, start.z), new Vector2(end.x, end.z));
        }
    }

    public static class HexMetrics
    {
        public const float outerRadius = 0.57f;
        public const float innerRadius = outerRadius * 0.866025404f;

        public static Vector3[] corners = {
            new Vector3(0f, 0f, outerRadius),
            new Vector3(innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(0f, 0f, -outerRadius),
            new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(-innerRadius, 0f, 0.5f * outerRadius)
        };
    }
}