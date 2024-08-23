using System.Collections;
using RevolVR;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EvolveLobbyController : MonoBehaviour
{

    public GameObject simRunnerObject;
    public GameObject sceneLoaderObject;
    public Button startButton;
    private AppConfig config;
    private string configPath;

    public void Start()
    {
        startButton.interactable = false;
        configPath = "Assets/revolve2/vr/db/config.json";
        config = ConfigManager.LoadConfig(configPath);
        config.ROUNDS_INDEX -= 1;
        ConfigManager.SaveConfig(configPath, config);
        StartCoroutine(RunSimAndLoadScene());
    }

    private IEnumerator RunSimAndLoadScene()
    {
        SimRunner simRunner = simRunnerObject.GetComponent<SimRunner>();
        if (simRunner == null)
        {
            Debug.LogError("SimRunner component is missing from the specified GameObject.");
            yield break;
        }
        simRunner.FileName = "evolve_robots.py";
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
