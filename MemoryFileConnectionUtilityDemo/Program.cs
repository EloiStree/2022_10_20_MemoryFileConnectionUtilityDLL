
using MemoryFileConnectionUtility;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;


namespace MemoryFileConnectionUtilityDemo
{
    class Program
    {


        static void Main(string[] args)
        {
            int mode = 1;

            if (mode == 0)
            {

                string toPush = "Hello World !";

                MemoryFileConnectionWithMutex connectionPush = new MemoryFileConnectionWithMutex();
                connectionPush.SetNameAndSizeThenReset("Test1", 100000);

                MemoryFileConnectionWithMutex connectionRecovert = new MemoryFileConnectionWithMutex();
                connectionRecovert.SetNameAndSizeThenReset("Test1", 100000);


                Console.WriteLine("Push: " + toPush);
                connectionPush.SetText(toPush);

                connectionRecovert.GetAsText(out string t);
                Console.WriteLine("Recovert: " + t);

                while (true)
                {
                    toPush = Console.ReadLine();

                    Console.WriteLine("Push: " + toPush);
                    connectionPush.SetText(toPush);

                    connectionRecovert.GetAsText(out string tt);
                    Console.WriteLine("Recovert: " + tt);
                }
            }
            else if (mode == 1)
            {

                (new Thread(new ThreadStart(() => PushRandomText(1000)))).Start();
                //(new Thread(new ThreadStart(() => PushRandomText(1)))).Start();
                //(new Thread(new ThreadStart(() => PushRandomText(50)))).Start();
                (new Thread(new ThreadStart(() => ReadRandomText(1)))).Start();
            }
            else if (mode == 2)
            {

                (new Thread(new ThreadStart(() => PushRandomTextMutex(1000)))).Start();
                //(new Thread(new ThreadStart(() => PushRandomText(1)))).Start();
                //(new Thread(new ThreadStart(() => PushRandomText(50)))).Start();
                (new Thread(new ThreadStart(() => ReadRandomTextMutex(1)))).Start();
            }
        }


        public static long m_index=0;
        private static void PushRandomText( int millisecond = 1)
        {
            int max = int.MaxValue;
            MemoryFileConnectionNoMutexLocker connectionPush = new MemoryFileConnectionNoMutexLocker("Test1", 100000);
            while (max>0)
            {

                Stopwatch watch = new Stopwatch();
                watch.Start();
                connectionPush.SetText(DateTime.Now.ToString());
                watch.Stop();
                Console.WriteLine("Write: " + watch.ElapsedMilliseconds) ;
                Thread.Sleep(millisecond);
                max--;
            }
        }
        private static void ReadRandomText( int millisecond = 1)
        {

            int max = int.MaxValue;
            MemoryFileConnectionNoMutexLocker connectionRecovert = new MemoryFileConnectionNoMutexLocker("Test1", 100000);
           
            while (max > 0)
            {
                m_index++;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                connectionRecovert.TextRecovering(out string t,false);
              

                watch.Stop(); 
                if (t.Trim().Length == 0)
                {
                    //Console.WriteLine("------------------Empty------------------ " );
                }
                else
                    Console.WriteLine("Recovert (" + m_index + "): " + t);
                Console.WriteLine("Read: " + watch.ElapsedMilliseconds);
                Thread.Sleep(millisecond); 
                max--;
            }
        }

        private static void PushRandomTextMutex(int millisecond = 1)
        {
            int max = int.MaxValue;
            MemoryFileConnectionWithMutex connectionPush = new MemoryFileConnectionWithMutex("Test1", 100000);
            while (max > 0)
            {

                Stopwatch watch = new Stopwatch();
                watch.Start();
                connectionPush.SetText(DateTime.Now.ToString());
                watch.Stop();
                Console.WriteLine("Write: " + watch.ElapsedMilliseconds);
                Thread.Sleep(millisecond);
                max--;
            }
        }
        private static void ReadRandomTextMutex(int millisecond = 1)
        {

            int max = int.MaxValue;
            MemoryFileConnectionWithMutex connectionRecovert = new MemoryFileConnectionWithMutex("Test1", 100000);

            while (max > 0)
            {
                m_index++;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                connectionRecovert.GetAsTextAndFlush(out string t);
                watch.Stop(); 
                if (t.Trim().Length == 0)
                {
                    //Console.WriteLine("------------------Empty------------------ " );
                }
                else
                    Console.WriteLine("Recovert (" + m_index + "): " + t);
                Console.WriteLine("Read: " + watch.ElapsedMilliseconds);
                Thread.Sleep(millisecond);
                max--;
            }
        }
    }
}
