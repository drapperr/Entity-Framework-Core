﻿using P03_SalesDatabase.Data;
using P03_SalesDatabase.Data.Seeding;
using P03_SalesDatabase.Data.Seeding.Contracts;
using P03_SalesDatabase.IOMangement;
using P03_SalesDatabase.IOMangement.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;

namespace P03_SalesDatabase
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            //SalesContext dbContext = new SalesContext();
            //Random random = new Random();
            //IWriter consoleWriter = new ConsoleWriter();

            //ICollection<ISeeder> seeders = new List<ISeeder>();
            //seeders.Add(new ProductSeeder(dbContext, random,consoleWriter));
            //seeders.Add(new StoreSeeder(dbContext,consoleWriter));

            //foreach (ISeeder seeder in seeders)
            //{
            //    seeder.Seed();
            //}
        }
    }
}
