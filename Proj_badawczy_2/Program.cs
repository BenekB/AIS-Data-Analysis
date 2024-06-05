using System;
using System.IO;
using System.Collections.Generic;
using System.Numerics;

/*  Ctrl + M + O    ->  hide all blocks of code 
 *  Ctrl + M + L    ->  show all blocks of code
 */

namespace Projekt_badawczy
{
    // wejście do programu
    class Program
    {
        public Dictionary<string, List<float>> abc = new();

        private static void Main(string[] args)
        {

            Console.WriteLine("START");
            DateTime start = DateTime.Now;
            
            Encoder Czytnik = new();
                // ^ utworzenie enkodera

            Czytnik.ask_directory = false;

            Czytnik.encode_all_files();
                // ^  rozkodowanie wszystkich wiadomości

            TimeSpan differ = DateTime.Now - start;
            Console.WriteLine("STOP - duration time:    {0} sekund", differ.TotalSeconds);

        }
    }
}
