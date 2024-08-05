using RevolVR;
using UnityEngine;

public class LobbyController : MonoBehaviour {

	public void StartSim(){
		SimRunner simRunner = GetComponent<SimRunner>();
		simRunner.RunRevolve();

		SceneLoader sceneLoader = GetComponent<SceneLoader>();
		sceneLoader.LoadScene();
	}
}
