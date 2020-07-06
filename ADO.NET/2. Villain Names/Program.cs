using Microsoft.Data.SqlClient;
using System;

namespace _2._Villain_Names
{
    class Program
    {
        static void Main(string[] args)
        {
            using SqlConnection connection = new SqlConnection("Server=.;Database=MinionsDB;Integrated Security=true");
            connection.Open();

            using SqlCommand command = new SqlCommand(@"SELECT [Name], COUNT(MV.VillainId) AS [MinionsCount] FROM Villains AS V
                                                JOIN MinionsVillains AS MV ON MV.VillainId = V.Id
                                                GROUP BY[Name]
                                                HAVING COUNT(MV.VillainId) > 3
                                                ORDER BY COUNT(mv.VillainId)", connection);

            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string name = (string)reader["Name"];
                int count = (int)reader["MinionsCount"];
                Console.WriteLine($"{name} - {count}");
            }
        }
    }
}
