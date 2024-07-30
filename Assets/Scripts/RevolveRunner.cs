using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor.Scripting.Python;
using Mujoco;
using System.Text;

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

    void ImportScene()
    {
        string path = $"{Application.dataPath}/model.xml";
        var importer = new MjImporterWithAssets();
        importer.ImportFile(path);
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

	void Start()
	{
		Application.targetFrameRate = 60;
		Time.timeScale = 0.5f;
		Debug.Log("Running python script");

		//PythonRunner.RunFile($"{Application.dataPath}/Scripts/RevolveEntry.py");

		ImportScene();
		ApplyShaders();
		MjScene.Instance.ctrlCallback += (_, _) => TrackMujocoData();

		string filePath = Path.Combine(Application.dataPath, "animation_data.json");
		filePath = filePath.Replace("\\", "/");
		string jsonData = File.ReadAllText(filePath);

		Debug.Log("Filepath: " + filePath);

		SimulationSceneState[] sceneStates = JsonHelper.FromJson<SimulationSceneState>(jsonData);
		simulationScene = new SimulationScene { scenes = sceneStates };

		Debug.Log("SIMULATION DATA");
		Debug.Log(simulationScene.scenes[0]);
	}

	void Update()
	{
		//TrackMujocoData();
	}
}
