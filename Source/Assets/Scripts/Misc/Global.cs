using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Global 
{
	static bool called;

	
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void OnBeforeSceneLoadRuntimeMethod()
	{
		if (called)
			return;
		if (Object.FindObjectOfType<GlobalManager>())
			return;

		GameObject test = new GameObject("GlobalManager");
		test.AddComponent<GlobalManager>();

		Object.DontDestroyOnLoad(test);

		called = true;
	}
}
