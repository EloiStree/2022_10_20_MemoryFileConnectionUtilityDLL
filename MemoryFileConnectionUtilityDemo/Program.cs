
using MemoryFileConnectionUtility;
using System;
using System.IO;
using System.Reflection;

namespace MemoryFileConnectionUtilityDemo
{
    class Program
    {
        static void Main(string[] args)
        {


            string toPush="Hello World !";
           
            MemoryFileConnection connectionPush = new MemoryFileConnection();
            connectionPush.SetNameAndSizeThenReset("Test1", 100000);

            MemoryFileConnection connectionRecovert = new MemoryFileConnection();
            connectionRecovert.SetNameAndSizeThenReset("Test1", 100000);


            Console.WriteLine("Push: "+ toPush);
            connectionPush.SetText(toPush);

            connectionRecovert.GetAsText(out string t);
            Console.WriteLine("Recovert: " + t);

            while (true) {
                toPush = Console.ReadLine();

                Console.WriteLine("Push: " + toPush);
                connectionPush.SetText(toPush);
            }
        }
    }
}
