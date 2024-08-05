using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Mujoco;

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

namespace RevolVR {

	public class SimRunner : MonoBehaviour
	{
		public SimulationScene simulationScene;
		private int currentIndex = 0;

		public GameObject ImportMujocoScene()
		{
			string path = $"{Application.dataPath}/model.xml";
			var importer = new MjImporterWithAssets();
			GameObject importedScene = importer.ImportFile(path);

			return importedScene;
		}

		public void ApplyShaders()
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

		public void RunRevolve()
		{
			string command = "py -3.11 Assets/revolve2/vr/main.py";

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

			using (Process process = Process.Start(processInfo))
			{
				string output = process.StandardOutput.ReadToEnd();
				string error = process.StandardError.ReadToEnd();

				process.WaitForExit();

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
	}
}
