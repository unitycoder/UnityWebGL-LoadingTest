// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectSpawner.cs">
//   Copyright (c) 2020 Johannes Deml. All rights reserved.
// </copyright>
// <author>
//   Johannes Deml
//   public@deml.io
// </author>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Supyrb
{
	/// <summary>
	/// Spawns an object in regular intervals
	/// </summary>
	public class ObjectSpawner : MonoBehaviour
	{
		[SerializeField]
		private GameObject prefab = null;

		[SerializeField]
		private float spawnCoolDownSeconds = 0.5f;

		[SerializeField]
		private float spawnOffsetSeconds = 0f;

		[SerializeField]
		private int maxInstances = 200;

		[SerializeField]
		[Tooltip("If set, the object will be spawned in the scene with the given name")]
		private string spawnSceneName = string.Empty;

		public float SpawnCoolDownSeconds
		{
			get => spawnCoolDownSeconds;
			set
			{
				spawnTimeBase += (spawnCoolDownSeconds - value) * totalSpawnCount;
				spawnCoolDownSeconds = value;
			}
		}

		public float SpawnOffsetSeconds
		{
			get => spawnOffsetSeconds;
			set
			{
				spawnTimeBase += value - spawnOffsetSeconds;
				spawnOffsetSeconds = value;
			}
		}
		public int MaxInstances
		{
			get => maxInstances;
			set => maxInstances = value;
		}

		private Queue<GameObject> spawnedObjects = null;
		/// <summary>
		/// Time from which the spawn times are calculated
		/// will be set on Awake and updated when the spawner is paused, or spawning values are changed
		/// </summary>
		private float spawnTimeBase;
		private int totalSpawnCount = 0;
		private float pauseTime = 0f;

		private void Awake()
		{
			spawnedObjects = new Queue<GameObject>(maxInstances + 5);
			spawnTimeBase = Time.time + spawnOffsetSeconds;
		}

		private void Update()
		{
			float relativeSpawnTime = Time.time - spawnTimeBase;
			if (Mathf.FloorToInt(relativeSpawnTime / spawnCoolDownSeconds) > totalSpawnCount)
			{
				SpawnObject();
			}
		}

		public void PauseSpawning()
		{
			enabled = false;
			pauseTime = Time.time;
		}

		public void ResumeSpawning()
		{
			enabled = true;
			spawnTimeBase += Time.time - pauseTime;
		}

		private void SpawnObject()
		{
			if (spawnedObjects.Count >= maxInstances)
			{
				var recycleGo = spawnedObjects.Dequeue();
				recycleGo.transform.localPosition = transform.position;
				recycleGo.transform.localRotation = transform.localRotation;
				spawnedObjects.Enqueue(recycleGo);
				return;
			}

			var newGo = InstantiatePrefab();
			spawnedObjects.Enqueue(newGo);
			totalSpawnCount++;
		}

		private GameObject InstantiatePrefab()
		{
			var lastActiveScene = SceneManager.GetActiveScene();
			if(!string.IsNullOrEmpty(spawnSceneName))
			{
				var spawnScene = SceneManager.GetSceneByName(spawnSceneName);
				if(!spawnScene.IsValid())
				{
					spawnScene = SceneManager.CreateScene(spawnSceneName);
				}
				SceneManager.SetActiveScene(spawnScene);
			}

			var newGo = Instantiate(prefab, transform.position, transform.rotation);

			if(!string.IsNullOrEmpty(spawnSceneName))
			{
				SceneManager.SetActiveScene(lastActiveScene);
			}

			return newGo;
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Gizmos.DrawWireSphere(transform.position, 0.5f);
		}
		#endif
	}
}