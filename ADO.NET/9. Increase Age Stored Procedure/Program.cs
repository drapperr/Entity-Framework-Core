using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace _9._Increase_Age_Stored_Procedure
{
    class Program
    {
        static void Main(string[] args)
        {
            int id = int.Parse(Console.ReadLine());

            using SqlConnection connection = new SqlConnection("Server=.;Database=MinionsDB;Integrated Security=true");
            connection.Open();

            using SqlCommand increaseAgeCmd = new SqlCommand("usp_GetOlder", connection);

            increaseAgeCmd.CommandType = CommandType.StoredProcedure;
            increaseAgeCmd.Parameters.AddWithValue("@Id", id);

            increaseAgeCmd.ExecuteNonQuery();

            using SqlCommand getMinionInfoCmd = new SqlCommand($"SELECT Name, Age FROM Minions WHERE Id = {id}", connection);

            using SqlDataReader reader = getMinionInfoCmd.ExecuteReader();

            reader.Read();

            Console.WriteLine($"{reader["Name"]} – {reader["Age"]} years old");
        }
    }
}
