using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

using Debug = UnityEngine.Debug;

namespace RevolVR {

	public class SceneLoader : MonoBehaviour
	{
		public string sceneName;

		public void LoadScene()
		{
			switch(sceneName)
			{
				case "Lobby":
					SimRunner revolveRunner = GetComponent<SimRunner>();

					revolveRunner.RunRevolve();
					break;
				default:
					Debug.Log("Special instructions for scene not found!");
					break;
			}

			StartCoroutine(LoadSceneAdditively());
		}

		IEnumerator LoadSceneAdditively()
		{
			AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

			asyncLoad.allowSceneActivation = false;

			while (!asyncLoad.isDone)
			{
				if (asyncLoad.progress >= 0.9f)
				{
					// spinning ball here???
					asyncLoad.allowSceneActivation = true;
				}

				yield return null;
			}
			// if i want to get really fancy i can do a scene fade-in here
		}
	}
}
