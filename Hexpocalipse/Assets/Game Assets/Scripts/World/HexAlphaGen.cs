using System;
using System.Collections;
using System.Collections.Generic;

namespace World
{

    [Serializable]
    public class HexAlphaGen : HexChunkProvider, HexValGen
    {
        private System.Random randGen = new System.Random();

        public float[] ChunkForStorage(HexDataStorage storage, HexCoords chunkStart)
        {
            long chunkSize = 1L << (2 * storage.chunkDepth);
            float[] result = new float[chunkSize];
            for (long i = 0; i < chunkSize; i++)
            {
                result[i] = ((float)randGen.NextDouble() - 0.5f) * 2 * (float)Math.PI;
            }
            return result;
        }

        public HexCoords[] PointsRequiredForEvaluating(HexCoords coords)
        {
            return new HexCoords[0];
        }

        public float Evaluate(HexCoords coords, HexDataStorage valueStorage, HexDataStorage alphaStorage)
        {
            return ((float)randGen.NextDouble() - 0.5f) * 2 * Mathf.PI;
        }
    }

}
