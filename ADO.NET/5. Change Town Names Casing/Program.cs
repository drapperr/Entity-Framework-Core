using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace _5._Change_Town_Names_Casing
{
    class Program
    {
        static void Main(string[] args)
        {
            string countryName = Console.ReadLine();

            using SqlConnection connection = new SqlConnection("Server=.;Database=MinionsDB;Integrated Security=true");
            connection.Open();

            using SqlCommand updateCmd = new SqlCommand(@$"UPDATE Towns
                                                           SET Name = UPPER(Name)
                                                         WHERE CountryCode = (SELECT c.Id FROM Countries AS c WHERE c.Name = '{countryName}')", connection);
            int count = updateCmd.ExecuteNonQuery();

            if (count == 0)
            {
                Console.WriteLine("No town names were affected.");
                return;
            }

            List<string> towns = new List<string>();

            using SqlCommand getTownsCmd = new SqlCommand($@"SELECT t.Name 
                                                               FROM Towns as t
                                                               JOIN Countries AS c ON c.Id = t.CountryCode
                                                              WHERE c.Name = '{countryName}'", connection);

            using SqlDataReader reader = getTownsCmd.ExecuteReader();

            while (reader.Read())
            {
                towns.Add((string)reader["Name"]);
            }

            Console.WriteLine($"{count} town names were affected.{Environment.NewLine}[{string.Join(", ", towns)}]");
        }
    }
}
