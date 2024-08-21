using System;
using UnityEngine;
using Mujoco;
using System.IO;
using RevolVR;
using System.Data;
using Mono.Data.Sqlite;
using System.Collections.Generic;

public class ArenaController : MonoBehaviour
{

	public GameObject simRunnerObject;

	public class Individual
	{
		public int Id { get; set; }
		public int PopulationId { get; set; }
		public int PopulationIndex { get; set; }
		public int GenotypeId { get; set; }
		public float Fitness { get; set; }

	}

	void Start()
	{
		Application.targetFrameRate = 60;
		Time.timeScale = 1;

		SimRunner simRunner = simRunnerObject.GetComponent<SimRunner>();
		if (simRunner == null)
		{
			Debug.LogError("SimRunner component is missing from the specified GameObject.");
			return;
		}

		GameObject mujocoScene1 = simRunner.ImportMujocoScene();

		simRunner.ApplyShaders();
		MjScene.Instance.ctrlCallback += (_, _) => simRunner.TrackMujocoData();

		string filePath = Path.Combine(Application.dataPath, "animation_data.json");
		filePath = filePath.Replace("\\", "/");
		string jsonData = File.ReadAllText(filePath);

		SimulationSceneState[] sceneStates = JsonHelper.FromJson<SimulationSceneState>(jsonData);
		simRunner.simulationScene = new SimulationScene { scenes = sceneStates };
		//(int generationId, int populationId) = GetNewestPopulation();
		//List<Individual> individuals = GetIndividualsByPopulationId(populationId);
		//InsertParent(individuals[2].Id, individuals[4].Id, generationId, true);
		//GetParents();
	}

	public void GetParents()
	{
		using (IDbConnection dbConnection = GetDBConnection())
		{
			using (IDbCommand dbCmd = dbConnection.CreateCommand())
			{
				string sqlQuery = "SELECT * FROM parents LIMIT 1";
				dbCmd.CommandText = sqlQuery;

				using (IDataReader reader = dbCmd.ExecuteReader())
				{
					if (reader.Read()) 
					{
						
						int parent1 = reader.GetInt32(reader.GetOrdinal("parent1_id"));
						int parent2 = reader.GetInt32(reader.GetOrdinal("parent2_id"));
						Debug.Log($"Parent 1: {parent1}");
						Debug.Log($"Parent 2: {parent2}");
					}
				}
			}
		}
		Debug.Log("No parents found.");
	}

	public (int, int) GetNewestPopulation()
	{
		using (IDbConnection dbConnection = GetDBConnection())
		{
			using (IDbCommand dbCmd = dbConnection.CreateCommand())
			{
				string sqlQuery = "SELECT * FROM generation ORDER BY id DESC LIMIT 1";
				dbCmd.CommandText = sqlQuery;

				using (IDataReader reader = dbCmd.ExecuteReader())
				{
					if (reader.Read()) return (reader.GetInt32(reader.GetOrdinal("id")), reader.GetInt32(reader.GetOrdinal("population_id")));
				}
			}
		}
		Debug.Log("No generation found.");
		return (-1, -1);
	}

	public List<Individual> GetIndividualsByPopulationId(int populationId)
	{
		List<Individual> individuals = new List<Individual>();
		using (IDbConnection dbConnection = GetDBConnection())
		{
			using (IDbCommand dbCmd = dbConnection.CreateCommand())
			{
				string sqlQuery = "SELECT * FROM individual WHERE population_id = @PopulationId ORDER BY id ASC";
				dbCmd.CommandText = sqlQuery;

				IDbDataParameter param = dbCmd.CreateParameter();
				param.ParameterName = "@PopulationId";
				param.Value = populationId;
				dbCmd.Parameters.Add(param);

				using (IDataReader reader = dbCmd.ExecuteReader())
				{
					while (reader.Read())
					{
						individuals.Add(new Individual
						{
							Id = reader.GetInt32(reader.GetOrdinal("id")),
							PopulationId = reader.GetInt32(reader.GetOrdinal("population_id")),
							PopulationIndex = reader.GetInt32(reader.GetOrdinal("population_index")),
							GenotypeId = reader.GetInt32(reader.GetOrdinal("genotype_id")),
							Fitness = reader.GetFloat(reader.GetOrdinal("fitness")),
						});
					}
				}
			}
		}
		return individuals;
	}

	public void InsertParent(int parent1Id, int? parent2Id, int parentGenId, bool mutate)
	{
		using (IDbConnection dbConnection = GetDBConnection())
		{
			using (IDbCommand dbCmd = dbConnection.CreateCommand())
			{
				string sqlQuery = "INSERT INTO parents (parent1_id, parent2_id, parent_gen_id, mutate) VALUES (@Parent1Id, @Parent2Id, @ParentGenId, @Mutate)";
				dbCmd.CommandText = sqlQuery;

				// Adding parameters to prevent SQL Injection
				var param1 = dbCmd.CreateParameter();
				param1.ParameterName = "@Parent1Id";
				param1.Value = parent1Id;
				dbCmd.Parameters.Add(param1);

				var param2 = dbCmd.CreateParameter();
				param2.ParameterName = "@Parent2Id";
				param2.Value = (object)parent2Id ?? DBNull.Value; // Handle nullable
				dbCmd.Parameters.Add(param2);

				var param3 = dbCmd.CreateParameter();
				param3.ParameterName = "@ParentGenId";
				param3.Value = parentGenId;
				dbCmd.Parameters.Add(param3);

				var param4 = dbCmd.CreateParameter();
				param4.ParameterName = "@Mutate";
				param4.Value = mutate ? 1 : 0; // SQLite does not have a boolean type, using 0 or 1
				dbCmd.Parameters.Add(param4);

				dbCmd.ExecuteNonQuery();
			}
		}
	}
	public static IDbConnection GetDBConnection()
	{
		string conn = "URI=file:" + Application.dataPath + "/database.sqlite";
		IDbConnection dbconn;
		dbconn = (IDbConnection)new SqliteConnection(conn);
		dbconn.Open();
		return dbconn;
	}
}
