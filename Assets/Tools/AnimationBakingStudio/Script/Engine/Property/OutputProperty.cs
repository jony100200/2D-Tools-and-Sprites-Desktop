using System;
using UnityEngine;

namespace ABS
{
	[Serializable]
	public class OutputProperty : PropertyBase
	{
		public bool makeAnimationClip = true;
		public int frameRate = 20;
		public int frameInterval = 1;
		public bool makeAnimatorController = false;
		public bool makePrefab = false;
        public GameObject prefab;
        public PrefabBuilder prefabBuilder;
        public bool isCompactCollider = false;
		public bool makeLocationPrefab = false;
		public GameObject locationPrefab;
		public bool makeNormalMap = false;
		public bool makeMaterial = false;
		public Material material = null;
		public MaterialBuilder materialBuilder = null;

		public bool ShouldMakePrefab()
		{
            return makePrefab && prefab != null && prefabBuilder != null;
        }

		public bool ShouldMakeLocationPrefab()
        {
			return makeLocationPrefab && locationPrefab != null;
		}
	}
}
