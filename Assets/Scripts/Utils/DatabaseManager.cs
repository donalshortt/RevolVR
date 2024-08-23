using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using Mono.Data.Sqlite;

public class DatabaseManager : MonoBehaviour
{
    public static IDbConnection GetDBConnection()
    {
        string conn = "URI=file:" + Application.dataPath + "/database.sqlite";
        IDbConnection dbconn;
        dbconn = (IDbConnection)new SqliteConnection(conn);
        dbconn.Open();
        return dbconn;
    }

    public static bool InsertParents(int parent1Id, int parent2Id, int parentGenId, bool mutate)
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
                param2.Value = parent2Id != -1 ? parent2Id : DBNull.Value; // Handle nullable
                dbCmd.Parameters.Add(param2);

                var param3 = dbCmd.CreateParameter();
                param3.ParameterName = "@ParentGenId";
                param3.Value = parentGenId;
                dbCmd.Parameters.Add(param3);

                var param4 = dbCmd.CreateParameter();
                param4.ParameterName = "@Mutate";
                param4.Value = mutate ? 1 : 0; // SQLite does not have a boolean type, using 0 or 1
                dbCmd.Parameters.Add(param4);

                try
                {
                    int result = dbCmd.ExecuteNonQuery();  // Execute the command
                    return result > 0;  // Return true if one or more rows were affected
                }
                catch (Exception ex)
                {
                    Debug.LogError("Insert operation failed: " + ex.Message);  // Log the exception
                    return false;  // Return false if an exception occurred
                }
            }
        }
    }

    public static void GetParents()
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
                        int parent2 = reader.IsDBNull(reader.GetOrdinal("parent2_id")) ? -1 : reader.GetInt32(reader.GetOrdinal("parent2_id"));
                        Debug.Log($"Parent 1: {parent1}");
                        Debug.Log($"Parent 2: {parent2}");
                    }
                }
            }
        }
    }

    public static int GetLatestGenerationId()
    {
        using (IDbConnection dbConnection = GetDBConnection())
        {
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                dbCmd.CommandText = "SELECT id FROM generation ORDER BY id DESC LIMIT 1;";
                var result = dbCmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result); // Return the ID of the latest generation
                }
                else
                {
                    Debug.LogError("No generations found in the database.");
                    return -1; // Return an invalid ID if no generation is found
                }
            }
        }
    }

    public static List<int> GetIndividualIdsFromLatestPopulation()
    {
        List<int> individualIds = new List<int>();
        using (IDbConnection dbConnection = GetDBConnection())
        {
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                // Updated SQL query to include sorting by individual id
                dbCmd.CommandText = @"
                    SELECT i.id FROM individual i
                    JOIN population p ON i.population_id = p.id
                    JOIN generation g ON p.id = g.population_id
                    WHERE g.id = (
                        SELECT MAX(g.id) FROM generation g
                    )
                    ORDER BY i.id ASC;
                ";

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        individualIds.Add(reader.GetInt32(0)); // Assuming the first column is individual id
                    }
                }
            }
        }
        return individualIds;
    }
}
