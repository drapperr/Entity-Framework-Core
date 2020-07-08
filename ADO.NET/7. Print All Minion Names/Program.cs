using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace _7._Print_All_Minion_Names
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> minionNames = new List<string>();

            using SqlConnection connection = new SqlConnection("Server=.;Database=MinionsDB;Integrated Security=true");
            connection.Open();

            using SqlCommand command = new SqlCommand("SELECT Name FROM Minions", connection);

            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                minionNames.Add((string)reader["Name"]);
            }

            for (int i = 0; i < minionNames.Count/2 ; i++)
            {
                Console.WriteLine(minionNames[i]);
                Console.WriteLine(minionNames[minionNames.Count - 1 - i]);
            }
        }
    }
}
