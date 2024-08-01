using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RevolveViewerLoader : MonoBehaviour
{
	public string sceneName;

	public void LoadScene()
	{
		StartCoroutine(LoadSceneAdditively());
	}

	IEnumerator LoadSceneAdditively()
	{
		// Start loading the new scene additively
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

		// Disable scene activation until fully loaded
		asyncLoad.allowSceneActivation = false;

		// Wait until the scene is fully loaded
		while (!asyncLoad.isDone)
		{
			// Check if the scene has been loaded to 90% (which means loading is essentially done)
			if (asyncLoad.progress >= 0.9f)
			{
				// Optionally, display a loading screen or perform some task

				// Allow the scene to activate
				asyncLoad.allowSceneActivation = true;
			}

			yield return null;
		}

		// At this point, the new scene is loaded and active

		// Optionally, unload the previous scene (if not needed anymore)
		// SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);

		// Optionally, you can trigger some event, e.g., fade in the new scene, etc.
	}
}
