using Microsoft.Data.SqlClient;
using System;
using System.Text;

namespace _4._Add_Minion
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] minionInfo = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string[] villainInfo = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string minionName = minionInfo[1];
            int minionAge = int.Parse(minionInfo[2]);
            string minionTown = minionInfo[3];
            string villainName = villainInfo[1];

            StringBuilder sb = new StringBuilder();

            using SqlConnection connection = new SqlConnection("Server=.;Database=MinionsDB;Integrated Security=true");
            connection.Open();

            using SqlCommand getTownIdCmd = new SqlCommand($"SELECT Id FROM Towns WHERE Name = '{minionTown}'", connection);
            string townId = (string)getTownIdCmd.ExecuteScalar()?.ToString();

            if (townId == null)
            {
                using SqlCommand addTown = new SqlCommand($"INSERT INTO Towns (Name) VALUES ('{minionTown}')", connection);
                addTown.ExecuteNonQuery();
                sb.AppendLine($"Town {minionTown} was added to the database.");
                townId = (string)getTownIdCmd.ExecuteScalar()?.ToString();
            }

            using SqlCommand getVillainCmd = new SqlCommand($"SELECT Id FROM Villains WHERE Name = '{villainName}'", connection);
            string villainId = (string)getVillainCmd.ExecuteScalar()?.ToString();

            if (villainId == null)
            {
                using SqlCommand addVillainCmd = new SqlCommand($"INSERT INTO Villains (Name, EvilnessFactorId)  VALUES ('{villainName}', 4)", connection);
                addVillainCmd.ExecuteNonQuery();
                villainId = (string)getVillainCmd.ExecuteScalar()?.ToString();
                sb.AppendLine($"Villain {villainName} was added to the database.");
            }

            using SqlCommand getMinionIdCmd = new SqlCommand($"SELECT Id FROM Minions WHERE Name = '{minionName}' AND Age = {minionAge} AND TownId={townId}", connection);
            string minionId = (string)getMinionIdCmd.ExecuteScalar()?.ToString();

            if (minionId == null)
            {
                using SqlCommand addMinionCmd = new SqlCommand($"INSERT INTO Minions (Name, Age, TownId) VALUES ('{minionName}', {minionAge}, {townId})", connection);
                addMinionCmd.ExecuteNonQuery();
                minionId = (string)getMinionIdCmd.ExecuteScalar()?.ToString();
            }

            using SqlCommand AddCmd = new SqlCommand($"INSERT INTO MinionsVillains (MinionId, VillainId) VALUES ({villainId}, {minionId})", connection);
            sb.AppendLine($"Successfully added {minionName} to be minion of {villainName}.");

            Console.WriteLine(sb.ToString().TrimEnd());
        }
    }
}
