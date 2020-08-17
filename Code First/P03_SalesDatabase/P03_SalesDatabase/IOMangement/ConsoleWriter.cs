using P03_SalesDatabase.IOMangement.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace P03_SalesDatabase.IOMangement
{
    public class ConsoleWriter : IWriter
    {
        public void Write(string text)
        {
            Console.Write(text);
        }

        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }
    }
}
