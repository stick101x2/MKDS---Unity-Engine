using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
   [System.Serializable]
   public class Pool
	{
		public string tag;
		public GameObject prefab;
		public int Count;
		public bool expandable;
	}

	public static ObjectPooler instance;

	private void Awake()
	{
		instance = this;
	}

	public List<Pool> pools;
	public Dictionary<string, Queue<GameObject>> poolDictionary;

	private void Start()
	{
		poolDictionary = new Dictionary<string, Queue<GameObject>>();

		foreach (Pool pool in pools)
		{
			Queue<GameObject> objectPool = new Queue<GameObject>();

			for (int i = 0; i < pool.Count; i++)
			{
				GameObject obj = Instantiate(pool.prefab, Vector3.zero, Quaternion.identity, transform);
				obj.SetActive(false);
				objectPool.Enqueue(obj);
			}

			poolDictionary.Add(pool.tag, objectPool);
		}
	}

	public GameObject SpawnPoolObject(string tag, Vector3 pos, Quaternion rot)
	{
		if (!poolDictionary.ContainsKey(tag))
		{
			Dev.LogError("Pool of tag (" + tag + ") can't be found");
			return null;
		}

		GameObject objToSpawn = poolDictionary[tag].Dequeue();

		if(objToSpawn.activeSelf)
		{
			
			foreach (Pool pool in pools)
			{
				if(pool.tag == tag && pool.expandable)
				{
					GameObject obj2 = null;
					poolDictionary[tag].Enqueue(objToSpawn);

					pool.Count++;
					obj2 = Instantiate(pool.prefab, pos, rot, null);
					obj2.SetActive(true);

					//IPooledObject ipooledObj2 = obj2.GetComponent<IPooledObject>();

				//	if (ipooledObj2 != null)
				//	{
				//		ipooledObj2.OnObjectSpawn();
					//}

					poolDictionary[tag].Enqueue(obj2);

					return obj2;
				}
			}
			
		}

		objToSpawn.SetActive(true);
		objToSpawn.transform.parent = null;
		objToSpawn.transform.position = pos;
		objToSpawn.transform.rotation = rot;


	//	IPooledObject ipooledObj = objToSpawn.GetComponent<IPooledObject>();

	//	if(ipooledObj != null)
//		{
//			ipooledObj.OnObjectSpawn();
	//	}

		poolDictionary[tag].Enqueue(objToSpawn);

		return objToSpawn;
	}
}