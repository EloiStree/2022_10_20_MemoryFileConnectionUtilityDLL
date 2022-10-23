
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
            int mode = 3;

            if (mode == 0)
            {
                string toPush = "Hello World !";
                IMemoryFileConnectionSetGet connectionPush = new MemoryFileConnectionWithMutexLocker("Test1", 100000);
                IMemoryFileConnectionSetGet connectionRecovert = new MemoryFileConnectionWithMutexLocker("Test1", 100000);


                Console.WriteLine("Push: " + toPush);
                connectionPush.SetAsText(toPush);

                connectionRecovert.GetAsText(out string t, false);
                Console.WriteLine("Recovert: " + t);

                while (true)
                {
                    toPush = Console.ReadLine();

                    Console.WriteLine("Push: " + toPush);
                    connectionPush.SetAsText(toPush);

                    connectionRecovert.GetAsText(out string tt, false);
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
            else if (mode == 3)
            {
                MemoryFileConnectionFacade.CreateConnection(MemoryFileConnectionType.MemoryFileLocker,
                    "OMICommandLines", out IMemoryFileConnectionSetGet connect, 1000000);
                while (true)
                {

                    string toPush = Console.ReadLine();
                    connect.AppendTextAtEnd(toPush);
                    Console.WriteLine("PUSH|",toPush);
                }

            }
            else if (mode == 4)
            {
                MemoryFileConnectionFacade.CreateConnection(MemoryFileConnectionType.MemoryFileLocker,
                    "6c9a793d-b160-4228-aa63-87e6b6c18e0a", out IMemoryFileConnectionSetGet connect, 1000000);
                while (true) {

                    connect.GetAsText(out string text, false);
                    Console.WriteLine(text);
                    Thread.Sleep(1000);
                }

            }

        }


        public static long m_index=0;
        private static void PushRandomText( int millisecond = 1)
        {
            int max = int.MaxValue;
            IMemoryFileConnectionSetGet connectionPush = new MemoryFileConnectionNoMutexWithFileLocker("Test1", 100000);
            while (max>0)
            {

                Stopwatch watch = new Stopwatch();
                watch.Start();
                connectionPush.SetAsText(DateTime.Now.ToString());
                watch.Stop();
                Console.WriteLine("Write: " + watch.ElapsedMilliseconds) ;
                Thread.Sleep(millisecond);
                max--;
            }
        }
        private static void ReadRandomText( int millisecond = 1)
        {

            int max = int.MaxValue;
            IMemoryFileConnectionSetGet connectionRecovert = new MemoryFileConnectionNoMutexWithFileLocker("Test1", 100000);
           
            while (max > 0)
            {
                m_index++;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                connectionRecovert.GetAsText(out string t,false);
              

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
            IMemoryFileConnectionSetGet connectionPush = new MemoryFileConnectionWithMutexLocker("Test1", 100000);
            while (max > 0)
            {

                Stopwatch watch = new Stopwatch();
                watch.Start();
                connectionPush.SetAsText(DateTime.Now.ToString());
                watch.Stop();
                Console.WriteLine("Write: " + watch.ElapsedMilliseconds);
                Thread.Sleep(millisecond);
                max--;
            }
        }
        private static void ReadRandomTextMutex(int millisecond = 1)
        {

            int max = int.MaxValue;
            IMemoryFileConnectionSetGet connectionRecovert = new MemoryFileConnectionWithMutexLocker("Test1", 100000);

            while (max > 0)
            {
                m_index++;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                connectionRecovert.GetAsText(out string t, false);
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
