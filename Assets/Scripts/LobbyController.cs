using System.Collections;
using RevolVR;
using UnityEngine;
using TMPro;

public class LobbyController : MonoBehaviour {

    public GameObject simRunnerObject;
 	public GameObject sceneLoaderObject;
	public TMP_Text textMeshPro;

    public void StartSim()
    {
        StartCoroutine(RunSimAndLoadScene());
    }

    private IEnumerator RunSimAndLoadScene()
    {
		textMeshPro.text = "Simulating...";

        SimRunner simRunner = simRunnerObject.GetComponent<SimRunner>();
        if (simRunner == null)
        {
            Debug.LogError("SimRunner component is missing from the specified GameObject.");
            yield break;
        }

        yield return StartCoroutine(simRunner.RunRevolveAsync());

        SceneLoader sceneLoader = sceneLoaderObject.GetComponent<SceneLoader>();
        if (sceneLoader == null)
        {
            Debug.LogError("SceneLoader component is missing from the specified GameObject.");
            yield break;
        }

        yield return sceneLoader.LoadSceneAsync();
    }
}
