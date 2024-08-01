using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEngine;
using UnityEditor.Scripting.Python;
using Mujoco;
using System.Text;

using Debug = UnityEngine.Debug;

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

public class RevolveRunner : MonoBehaviour
{
	private SimulationScene simulationScene;
	private int currentIndex = 0;

	GameObject ImportScene()
	{
		string path = $"{Application.dataPath}/model.xml";
		var importer = new MjImporterWithAssets();
		GameObject importedScene = importer.ImportFile(path);

		return importedScene;
	}

	void ApplyShaders()
	{
		Shader newShader = Shader.Find("Universal Render Pipeline/Lit");

		foreach (Renderer r in FindObjectsOfType<Renderer>())
		{
			foreach (Material m in r.materials)
			{
				m.shader = newShader;
			}
		}
	}

	unsafe void TrackMujocoData()
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

	// Example method to run the Python command
	public void RunRevolve()
	{
		string command = "py -3.11 Assets/revolve2/vr/main.py";  // Your command

		// Initialize the ProcessStartInfo
		ProcessStartInfo processInfo = new ProcessStartInfo
		{
			FileName = "cmd.exe",   // Use cmd.exe for Windows
			Arguments = $"/c {command}",
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		// Start the process
		using (Process process = Process.Start(processInfo))
		{
			// Read the output
			string output = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();

			process.WaitForExit();

			// Log the output (or do something with it)
			Debug.Log("Output: " + output);
			if (!string.IsNullOrEmpty(error))
			{
				Debug.Log("Error: " + error);
			}
		}
	}

	void SetScenePosition(GameObject sceneRoot, Vector3 position)
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

	void Start()
	{
		Application.targetFrameRate = 60;
		Time.timeScale = 1;
		Debug.Log("Running python script");

		//PythonRunner.RunFile($"{Application.dataPath}/revolve2/vr/main.py");
		RunRevolve();

		GameObject mujocoScene1 = ImportScene();
		GameObject mujocoScene2 = ImportScene();

		SetScenePosition(mujocoScene1, new Vector3(10f, 0f, 5f));
		SetScenePosition(mujocoScene2, new Vector3(-10f, 0f, -5f));

		ApplyShaders();
		MjScene.Instance.ctrlCallback += (_, _) => TrackMujocoData();

		string filePath = Path.Combine(Application.dataPath, "animation_data.json");
		filePath = filePath.Replace("\\", "/");
		string jsonData = File.ReadAllText(filePath);

		SimulationSceneState[] sceneStates = JsonHelper.FromJson<SimulationSceneState>(jsonData);
		simulationScene = new SimulationScene { scenes = sceneStates };
	}

	void Update()
	{
		//TrackMujocoData();
	}
}
