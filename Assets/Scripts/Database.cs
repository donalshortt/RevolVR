using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class Database : MonoBehaviour
{
    private string dbPath = Application.dataPath + "/databa.sqlite";
    public void RetrieveGenerations()
    {

        if (string.IsNullOrEmpty(dbPath))
    {
        Debug.LogError("Database path is not set.");
        return;
    }
        // Open a connection to the database
        var connection = new SqliteConnection("URI=file:" + dbPath);
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            // Retrieve all users from the Users table
            command.CommandText = "SELECT * FROM Generation";
            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Debug.Log("Generation index: " + reader["generation_index"]);
                }
            }
        }

        connection.Close();
    }
}
