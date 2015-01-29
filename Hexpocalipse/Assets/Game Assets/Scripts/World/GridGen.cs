using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace World {

	[Serializable()]
	public class GridGen {

		// Stored Properties

		private int _fractalDepth;

		private HexGrid heightMap;

		private float _delta0;
		private float _lambda;

		// Lifecycle

		public GridGen(float delta0, float lambda, int fractalDepth) {
			_delta0 = delta0;
			_lambda = lambda;
			_fractalDepth = fractalDepth;
			heightMap = new HexGrid (fractalDepth);
		}

		// public indexators

		public HexData this[HexCoords index] {
			get {
				HexData result = heightMap[index];
				if (result != null) {
					return result;
				} else {
					return GenHex(index, true);
				}
			}
		}

		public HexData this[long u, long v] {
			get {
				return this[new HexCoords(u, v)];
			}
		}

		// private methods

		private float Delta(int n) {
			return _delta0 * Mathf.Pow(_lambda, n);
		}

		private HexData GenHex(HexCoords coords, bool save) {
			float result = 0;
			float rnd = UnityEngine.Random.value - 0.5f;
			if(coords.d >= _fractalDepth) {
				result = _delta0 * rnd;
			} else {
				result += Delta(_fractalDepth - coords.d) * rnd;
				HexData d0 = this[coords.dep0];
				HexData d1 = this[coords.dep1];
				result += (d0.height + d1.height) / 2;
			}
			HexData data = new HexData (result);
			if (save) {
				heightMap[coords] = data;
			}
			return data;
		}

	}

}