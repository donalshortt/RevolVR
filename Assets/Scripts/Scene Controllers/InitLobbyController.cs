using System.Collections;
using RevolVR;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InitLobbyController : MonoBehaviour
{
    public GameObject simRunnerObject;
    public GameObject sceneLoaderObject;
    public TMP_Text textMeshPro;
    public Button startButton;
    public GameObject startMenuObject;
    public GameObject settingsMenuObject;
    public Slider populationSlider;
    public Slider stepSizeSlider;
    public Slider roundsSlider;
    private AppConfig config;
    private string configPath;
    public GameObject confirmUpdatedObject;

    public void Start()
    {
        configPath = "Assets/revolve2/vr/db/config.json";
        config = ConfigManager.LoadConfig(configPath);
    }

    public void StartSim()
    {
        config.ROUNDS_INDEX = config.ROUNDS;
        ConfigManager.SaveConfig(configPath, config);
        StartCoroutine(RunSimAndLoadScene());
    }

    private IEnumerator RunSimAndLoadScene()
    {
        if (startButton == null)
        {
            Debug.LogError("Button component not found on the GameObject");
            yield break;
        }
        startButton.interactable = false;
        textMeshPro.text = "Simulating...";
        SimRunner simRunner = simRunnerObject.GetComponent<SimRunner>();
        if (simRunner == null)
        {
            Debug.LogError("SimRunner component is missing from the specified GameObject.");
            yield break;
        }
        simRunner.FileName = "gen_rand_robots.py";
        yield return StartCoroutine(simRunner.RunRevolveAsync());

        SceneLoader sceneLoader = sceneLoaderObject.GetComponent<SceneLoader>();
        if (sceneLoader == null)
        {
            Debug.LogError("SceneLoader component is missing from the specified GameObject.");
            yield break;
        }

        yield return sceneLoader.LoadSceneAsync();
    }

    public void ChangeMenus()
    {
        startMenuObject.SetActive(!startMenuObject.activeSelf);
        settingsMenuObject.SetActive(!settingsMenuObject.activeSelf);
    }

    public void ApplySettings()
    {
        config.POPULATION_SIZE = Mathf.RoundToInt(populationSlider.value);
        // In the future there may be other ways for survival selections
        // Right now there is only (mu, lambda) and we have the amount of
        // offspring equal to the population size. This can be changed later.
        config.OFFSPRING_SIZE = Mathf.RoundToInt(populationSlider.value);
        config.STEP_SIZE = Mathf.RoundToInt(stepSizeSlider.value);
        config.ROUNDS = Mathf.RoundToInt(roundsSlider.value);
        ConfigManager.SaveConfig(configPath, config);
        if (!confirmUpdatedObject.activeSelf) confirmUpdatedObject.SetActive(true);
    }
}
