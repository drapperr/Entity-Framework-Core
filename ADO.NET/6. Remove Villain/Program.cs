using Microsoft.Data.SqlClient;
using System;

namespace _6._Remove_Villain
{
    class Program
    {
        static void Main(string[] args)
        {
            int villainId = int.Parse(Console.ReadLine());

            using SqlConnection connection = new SqlConnection("Server=.;Database=MinionsDB;Integrated Security=true");
            connection.Open();

            SqlTransaction sqlTransaction = connection.BeginTransaction();

            using SqlCommand getNameCmd = new SqlCommand($"SELECT Name FROM Villains WHERE Id = {villainId}", connection);
            getNameCmd.Transaction = sqlTransaction;
            string villainName = (string)getNameCmd.ExecuteScalar();

            if (villainName == null)
            {
                Console.WriteLine("No such villain was found.");
                return;
            }

            try
            {
                SqlCommand deleteMinionsCmd = new SqlCommand($"DELETE FROM MinionsVillains WHERE VillainId = {villainId}", connection);
                deleteMinionsCmd.Transaction = sqlTransaction;
                int minionsCount = deleteMinionsCmd.ExecuteNonQuery();

                SqlCommand deleteVillainCmd = new SqlCommand($"DELETE FROM Villains WHERE Id = {villainId}", connection);
                deleteVillainCmd.Transaction = sqlTransaction;
                deleteVillainCmd.ExecuteNonQuery();

                sqlTransaction.Commit();

                Console.WriteLine($"{villainName} was deleted.{Environment.NewLine}{minionsCount} minions were released.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                sqlTransaction.Rollback();
            }
        }
    }
}
