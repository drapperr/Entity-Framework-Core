using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _8._Increase_Minion_Age
{
    class Program
    {
        static void Main(string[] args)
        {
            using SqlConnection connection = new SqlConnection("Server=.;Database=MinionsDB;Integrated Security=true");
            connection.Open();

            List<int> input = Console.ReadLine().Split(" ").Select(int.Parse).ToList();

            using SqlCommand updateMinionsCmd = new SqlCommand($@"UPDATE Minions
                                                                       SET Name = UPPER(LEFT(Name, 1)) + SUBSTRING(Name, 2, LEN(Name)), Age += 1
                                                                     WHERE Id IN ({string.Join(", ", input)})", connection);
            updateMinionsCmd.ExecuteNonQuery();

            using SqlCommand getMinionsCmd = new SqlCommand("SELECT Name, Age FROM Minions", connection);

            using SqlDataReader reader = getMinionsCmd.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine($"{reader["Name"]} {reader["Age"]}");
            }
        }
    }
}
