using UnityEngine;
using Mujoco;
using System.IO;
using RevolVR;

public class ArenaController : MonoBehaviour {
	void Start()
	{
		Application.targetFrameRate = 60;
		Time.timeScale = 1;

		SimRunner simRunner = GetComponent<SimRunner>();

		GameObject mujocoScene1 = simRunner.ImportMujocoScene();
		GameObject mujocoScene2 = simRunner.ImportMujocoScene();

		simRunner.SetScenePosition(mujocoScene1, new Vector3(10f, 0f, 5f));
		simRunner.SetScenePosition(mujocoScene2, new Vector3(-10f, 0f, -5f));

		simRunner.ApplyShaders();
		MjScene.Instance.ctrlCallback += (_, _) => simRunner.TrackMujocoData();

		string filePath = Path.Combine(Application.dataPath, "animation_data.json");
		filePath = filePath.Replace("\\", "/");
		string jsonData = File.ReadAllText(filePath);

		SimulationSceneState[] sceneStates = JsonHelper.FromJson<SimulationSceneState>(jsonData);
		simRunner.simulationScene = new SimulationScene { scenes = sceneStates };
	}
}
