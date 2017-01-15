using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{

    [Serializable]
    public class HexAlphaGen : HexChunkProvider, HexValGen
    {
        public float[] ChunkForStorage(HexDataStorage storage, HexCoords chunkStart)
        {
            long chunkSize = 1L << (2 * storage.chunkDepth);
            float[] result = new float[chunkSize];
            for (long i = 0; i < chunkSize; i++)
            {
                result[i] = UnityEngine.Random.Range(-Mathf.PI, Mathf.PI);
            }
            return result;
        }

        public HexCoords[] PointsRequiredForEvaluating(HexCoords coords)
        {
            return new HexCoords[0];
        }

        public float Evaluate(HexCoords coords, HexDataStorage valueStorage, HexDataStorage alphaStorage)
        {
            return UnityEngine.Random.Range(-Mathf.PI, Mathf.PI);
        }
    }

}
