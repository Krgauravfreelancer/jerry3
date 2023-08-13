using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int totalRowsToBeAdded = 20;

            Console.WriteLine($"Total sample notes rows added - {totalRowsToBeAdded}");

            var message = $"Total words - {Sample.GetWordsCount(totalRowsToBeAdded)} and Total Characters - {Sample.GetCharactersCount(totalRowsToBeAdded)}";
            Console.WriteLine(message);
            Console.ReadLine();
        }
    }
}
