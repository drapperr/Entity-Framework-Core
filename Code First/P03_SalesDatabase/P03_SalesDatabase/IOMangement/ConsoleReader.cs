using P03_SalesDatabase.IOMangement.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace P03_SalesDatabase.IOMangement
{
    public class ConsoleReader : IReader
    {
        public string ReadLine()
        {
            return Console.ReadLine();
        }
    }
}
