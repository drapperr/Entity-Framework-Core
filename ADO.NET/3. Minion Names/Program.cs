using Microsoft.Data.SqlClient;
using System;
using System.Text;

namespace _3._Minion_Names
{
    class Program
    {
        static void Main(string[] args)
        {
            using SqlConnection connection = new SqlConnection("Server=.;Database=MinionsDB;Integrated Security=true");
            connection.Open();

            int id = int.Parse(Console.ReadLine());

            using SqlCommand commandForName = new SqlCommand($"SELECT Name FROM Villains WHERE Id = {id}", connection);
            string villainName = (string)commandForName.ExecuteScalar();

            if(villainName == null)
            {
                Console.WriteLine($"No villain with ID {id} exists in the database.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Villain: {villainName}");

            using SqlCommand commandForMinnions = new SqlCommand($@"SELECT ROW_NUMBER() OVER (ORDER BY m.Name) as RowNum,m.Name, m.Age
                                                                    FROM MinionsVillains AS mv
                                                                    JOIN Minions As m ON mv.MinionId = m.Id
                                                                   WHERE mv.VillainId = {id}
                                                                ORDER BY m.Name",connection);

            using SqlDataReader reader = commandForMinnions.ExecuteReader();

            if (!reader.HasRows)
            {
                sb.AppendLine("(no minions)");
            }

            while(reader.Read())
            {
                long rowNum = (long)reader["RowNum"];
                string name = (string)reader["Name"];
                int age = (int)reader["Age"];
                sb.AppendLine($"{rowNum}. {name} {age}");
            }

            Console.WriteLine(sb.ToString().TrimEnd());
        }
    }
}
