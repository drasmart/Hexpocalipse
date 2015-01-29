using System;
using System.Collections;
using System.Collections.Generic;

namespace World {
	
	using HexChunk = Dictionary<HexCoords, HexData>;

	[Serializable()]
	internal class HexGrid {

		// Stored Properties

		private Dictionary<HexCoords, HexChunk> heightMap;

		private int _chunkDepth = 16;

		// Computed Properties

		public int chunkDepth { get { return _chunkDepth; } }

		// Lifecycle

		public HexGrid(int chunkDepth) {
			_chunkDepth = chunkDepth > 0 ? chunkDepth : _chunkDepth;
			heightMap = new Dictionary<HexCoords, HexChunk> (9);
		}

		// Indexator

		public HexData this[HexCoords index] {
			get {
				HexCoords key = index.ChunkCoords(_chunkDepth);
				if (!heightMap.ContainsKey(key)) {
					return null;
				}
				HexChunk chunk = heightMap[key];
				return chunk.ContainsKey(index) ? chunk[index] : null;
			}
			set {
				HexCoords key = index.ChunkCoords(_chunkDepth);
				if (heightMap.ContainsKey(key)) {
					HexChunk chunk = heightMap[index.ChunkCoords(_chunkDepth)];
					chunk[index] = value;
				} else {
					HexChunk chunk = new HexChunk();
					chunk[index] = value;
					heightMap[index.ChunkCoords(_chunkDepth)] = chunk;
				}
			}
		}

	}

}