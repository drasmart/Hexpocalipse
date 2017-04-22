using System;
using UnityEngine;

namespace World
{
    [Serializable]
    public class HexGridGen: HexValGen
    {
        [SerializeField]
        public int   depth  { get; private set; }
        [SerializeField]
        public float lambda { get; private set; }
        [SerializeField]
        public float delta0 { get; private set; }
        [SerializeField]
        private System.Random randGen = new System.Random();

        public HexGridGen(int depth, float delta0, float lambda)
        {
            this.depth = depth;
            this.delta0 = delta0;
            this.lambda = lambda;
        }

        public HexCoords[] PointsRequiredForEvaluating(HexCoords coords)
        {
            if(coords.Depth >= depth)
            {
                return new HexCoords[0];
            }
            return coords.Parents;
        }

        public float Evaluate(HexCoords coords, HexDataStorage valueStorage, HexDataStorage alphaStorage)
        {
            if(coords.Depth >= depth)
            {
                //Logger.Log("req = " + coords.ToString() + " -> Random");
                return (float)randGen.NextDouble() * delta0;
            }
            HexCoords[] parents = coords.Parents;
            //if (coords.Depth == depth - 1)
            //{
            //    Logger.Log("req = " + coords.ToString() + " : Parents : " + parents[0].ToString() + " , " + parents[1].ToString());
            //}
            float g = GradientAt(parents, valueStorage, alphaStorage);
            float v0 = valueStorage.ValueAt(parents[0]);
            float v1 = valueStorage.ValueAt(parents[1]);
            //Logger.Log("req = " + coords.ToString() + " : Parents : " + parents[0].ToString() + " ( " + v0.ToString() + " ) , " + parents[1].ToString() + " ( " + v1.ToString() + " ) ");
            float cd = DeltaAt(depth - coords.Depth - 1);
            float rv = (float)randGen.NextDouble() - 0.5f;
            float dv = cd * (rv + g / 2.0f);
            float mv = (v0 + v1) / 2.0f;
            float result = mv + dv;
            //Logger.Log("req = " + coords.ToString() + " ; mv = " + mv.ToString() + " ; cd = " + cd.ToString() + " ; dv = " + dv.ToString() + " ; result = " + result.ToString());
            return result;
        }

        private float GradientAt(HexCoords[] section, HexDataStorage valueStorage, HexDataStorage alphaStorage)
        {
            float g1;
            float g2;
            HexCoords direction = section[0].ClosestAxialDirectionTo(section[1]);
            if (section[0].Depth >= depth - 1)
            {
                float a1 = alphaStorage.ValueAt(section[0].ChunkCoords(depth));
                Vector2 av = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1));
                g1 = Mathf.Cos(Mathf.Deg2Rad * Vector2.Angle(av, direction.toVector2));
            } else
            {
                HexCoords[] p0 = section[0].Parents;
                float vA = valueStorage.ValueAt(p0[0]);
                float vB = valueStorage.ValueAt(p0[1]);
                g1 = (vB - vA) / DeltaAt(depth - section[0].Depth - 2);
            }
            if (section[1].Depth >= depth - 1)
            {
                float a2 = alphaStorage.ValueAt(section[1].ChunkCoords(depth));
                Vector2 av = new Vector2(Mathf.Cos(a2), Mathf.Sin(a2));
                g2 = Mathf.Cos(Mathf.Deg2Rad * Vector2.Angle(av, -direction.toVector2));
            }
            else
            {
                HexCoords[] p1 = section[1].Parents;
                float vA = valueStorage.ValueAt(p1[0]);
                float vB = valueStorage.ValueAt(p1[1]);
                g2 = (vB - vA) / DeltaAt(depth - section[1].Depth - 2);
            }
            return (g1 + g2) / 2.0f;
        }

        private float DeltaAt(int depth)
        {
            return delta0 * Mathf.Pow(lambda, depth);
        }
    }

}
