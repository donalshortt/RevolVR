using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using Mono.Data.Sqlite;
using TMPro;

public class SelectParentsWristMenuController : MonoBehaviour
{
    public Dictionary<string, int> robotNameToId = new Dictionary<string, int>();
    public TMP_Dropdown parent1Dropdown;
    public TMP_Dropdown parent2Dropdown;
    public TMP_Dropdown mutateDropdown;

    public int SelectedParent1Id { get; private set; }
    public int SelectedParent2Id { get; private set; }
    public bool ShouldMutate { get; private set; }

    void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            robotNameToId.Add($"Robot {i + 1}", i);
        }
    }

    public void CaptureSelection()
    {
        SelectedParent1Id = robotNameToId[parent1Dropdown.options[parent1Dropdown.value].text];
        string parent2Value = parent2Dropdown.options[parent2Dropdown.value].text;
        Debug.Log($"parent2Value: {parent2Value}");
        SelectedParent2Id = parent2Value == "None" ? -1 : robotNameToId[parent2Value];
        Debug.Log($"Selected parent 2: {SelectedParent2Id}");
        ShouldMutate = (mutateDropdown.options[mutateDropdown.value].text == "Mutate");
        InsertParents(SelectedParent1Id, SelectedParent2Id, 1, ShouldMutate);
        Debug.Log("Parents updated");
        GetParents();
    }

    public static IDbConnection GetDBConnection()
    {
        string conn = "URI=file:" + Application.dataPath + "/database.sqlite";
        IDbConnection dbconn;
        dbconn = (IDbConnection)new SqliteConnection(conn);
        dbconn.Open();
        return dbconn;
    }

    public void InsertParents(int parent1Id, int parent2Id, int parentGenId, bool mutate)
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

                dbCmd.ExecuteNonQuery();
            }
        }
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
                        int parent2 = reader.IsDBNull(reader.GetOrdinal("parent2_id")) ? -1 : reader.GetInt32(reader.GetOrdinal("parent2_id"));
                        Debug.Log($"Parent 1: {parent1}");
                        Debug.Log($"Parent 2: {parent2}");
                    }
                }
            }
        }
    }
}
