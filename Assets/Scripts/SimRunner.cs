using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Mujoco;
using TMPro;
using Debug = UnityEngine.Debug;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Unity.VisualScripting;
using UnityEditor;

public static class JsonHelper
{
	[Serializable]
	private class Wrapper<T>
	{
		public T[] Items;
	}

	public static T[] FromJson<T>(string json)
	{
		string newJson = "{\"Items\":" + json + "}";
		Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
		return wrapper.Items;
	}

	public static string ToJson<T>(T[] array)
	{
		Wrapper<T> wrapper = new Wrapper<T> { Items = array };
		return JsonUtility.ToJson(wrapper);
	}
}

[System.Serializable]
public class SimulationSceneState
{
	public float[] xpos;
	public float[] xquat;
	public float[] qpos;

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("SimulationSceneState:");

		sb.Append("xpos: ");
		if (qpos != null)
		{
			sb.Append(string.Join(", ", xpos));
		}
		else
		{
			sb.Append("null");
		}
		sb.AppendLine();

		sb.Append("xquat: ");
		if (qpos != null)
		{
			sb.Append(string.Join(", ", xquat));
		}
		else
		{
			sb.Append("null");
		}
		sb.AppendLine();

		sb.Append("qpos: ");
		if (qpos != null)
		{
			sb.Append(string.Join(", ", qpos));
		}
		else
		{
			sb.Append("null");
		}
		sb.AppendLine();

		return sb.ToString();
	}
}

[System.Serializable]
public class SimulationScene
{
	public SimulationSceneState[] scenes;
}

namespace RevolVR
{

	public class SimRunner : MonoBehaviour
	{
		public SimulationScene simulationScene;
		private int currentIndex = 0;
		private GameObject prefab;
		public string FileName { get; set; }
		public GameObject brainPrefab;

		public GameObject ImportMujocoScene()
		{
			string path = $"{Application.dataPath}/model.xml";
			var importer = new MjImporterWithAssets();
			GameObject importedScene = null;
			while (importedScene == null)
			{
				importedScene = importer.ImportFile(path);
			}
			importedScene.tag = "MuJoCoImport";
			return importedScene;
		}

		public void ApplyShaders()
		{
			Shader newShader = Shader.Find("Universal Render Pipeline/Lit");

			GameObject[] mujocoObjects = GameObject.FindGameObjectsWithTag("MuJoCoImport");

			foreach (GameObject obj in mujocoObjects)
			{
				Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

				foreach (Renderer r in renderers)
				{
					foreach (Material m in r.materials)
					{
						m.shader = newShader;
					}
				}
			}
		}

		public IEnumerator RunRevolveAsync()
		{
			if (string.IsNullOrWhiteSpace(FileName))
			{
				Debug.LogError("FileName cannot be null or empty.");
				yield break;
			}
			string command = $"wsl python3 Assets/revolve2/vr/db/{FileName}";
			// Initialize the ProcessStartInfo
			ProcessStartInfo processInfo = new ProcessStartInfo
			{
				FileName = "cmd.exe",
				Arguments = $"/c {command}",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using (Process process = new Process())
			{
				process.StartInfo = processInfo;
				process.Start();

				// Start reading output and error streams asynchronously
				Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
				Task<string> errorTask = process.StandardError.ReadToEndAsync();

				// Poll until the process exits without blocking the main thread
				while (!process.HasExited)
				{
					yield return null;  // Wait for the next frame
				}

				// Wait until both tasks are completed
				while (!outputTask.IsCompleted || !errorTask.IsCompleted)
				{
					yield return null;  // Wait for the next frame
				}

				// Now that both tasks are completed, get the results
				string output = outputTask.Result;
				string error = errorTask.Result;

				Debug.Log("Output: " + output);
				if (!string.IsNullOrEmpty(error))
				{
					Debug.Log("Error: " + error);
				}
			}
		}

		unsafe public void TrackMujocoData()
		{
			if (currentIndex < simulationScene.scenes.Length)
			{

				for (int i = 0; i < simulationScene.scenes[currentIndex].xpos.Length; i++)
				{
					MjScene.Instance.Data->xpos[i] = simulationScene.scenes[currentIndex].xpos[i];
				}

				for (int i = 0; i < simulationScene.scenes[currentIndex].xquat.Length; i++)
				{
					MjScene.Instance.Data->xquat[i] = simulationScene.scenes[currentIndex].xquat[i];
				}

				for (int i = 0; i < simulationScene.scenes[currentIndex].qpos.Length; i++)
				{
					MjScene.Instance.Data->qpos[i] = simulationScene.scenes[currentIndex].qpos[i];
				}
			}

			currentIndex++;
		}

		public void SetScenePosition(GameObject sceneRoot, Vector3 position)
		{
			if (sceneRoot != null)
			{
				sceneRoot.transform.position = position;
			}
			else
			{
				Debug.LogError("Imported scene root is null.");
			}
		}

		public void AddInfoObjects()
		{
			GameObject mujocoScene = GameObject.FindWithTag("MuJoCoImport");
			if (mujocoScene == null)
			{
				Debug.Log("Couldn't find imported MuJoCo scene");
				return;
			}

			Transform sceneTransform = mujocoScene.transform;
			int i = 0;
			List<IndividualData> latestIndividuals = DatabaseManager.GetIndividualsDataFromLatestGeneration();
			foreach (Transform child in sceneTransform)
			{
				GameObject childObject = child.gameObject;
				string childName = childObject.name;
				if (childName.StartsWith("mbs") && childName != "mbs0/")
				{
					GameObject instantiatedPrefab = Instantiate(brainPrefab);
					RobotInfo robotInfo = instantiatedPrefab.GetComponent<RobotInfo>();
					if (robotInfo != null)
					{
						if (i >= latestIndividuals.Count)
						{
							Debug.Log("There is a mismatch between the amount of users in the scene and the amount of individuals in the latest generation.");
						}
						else
						{
							IndividualData currentIndividual = latestIndividuals[i];
							i++;
							robotInfo.UpdateRobotInfo($"Robot {i}", currentIndividual.Fitness, currentIndividual.Id);
						}
					}
					else
					{
						Debug.LogError("RobotInfo script not found on the instantiated prefab.");
					}
					instantiatedPrefab.transform.SetParent(childObject.transform.GetChild(1), false);
					instantiatedPrefab.transform.position = childObject.transform.GetChild(1).position;
				}
			}
		}

	}
}
