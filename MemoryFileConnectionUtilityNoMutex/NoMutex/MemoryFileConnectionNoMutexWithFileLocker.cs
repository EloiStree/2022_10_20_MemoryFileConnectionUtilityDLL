using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MemoryFileConnectionUtility
{
   public class MemoryFileConnectionNoMutexWithFileLocker : IMemoryFileConnectionSetGet
    {
        public const int LOCKSIZE= 1;
        public MemoryFileConnectionNoMutex m_memoryFile;
        public MemoryMappedFile m_locker;
        public string m_filenameLock;
        public string m_filenameValue;
        private TargetMemoryFileInitation m_paramsInit;

        public MemoryFileConnectionNoMutexWithFileLocker(string fileName, int fileSize = MemoryFileConnectionUtility._1MOSize )
        {
            SetWithNameAndSize( fileName,  fileSize);
        }

        public void SetWithNameAndSize(string fileName, int fileSize) {

            m_filenameLock = fileName + "_Locker";
            m_filenameValue = fileName + "_Value";
            if (m_locker != null) m_locker.Dispose();
            m_locker = MemoryMappedFile.CreateOrOpen(m_filenameLock, LOCKSIZE);
            if (m_memoryFile != null)
                m_memoryFile.Dispose();
            m_paramsInit = new TargetMemoryFileInitation() { m_fileName = fileName + "_Value", m_maxMemorySize = fileSize };
            m_memoryFile = new MemoryFileConnectionNoMutex(m_paramsInit);
        }

        public void SetName(string name)
        {
            SetWithNameAndSize(name, m_paramsInit.m_maxMemorySize);
        }

        public void SetMemorySize(int sizeInBit)
        {
            SetWithNameAndSize(m_paramsInit.m_fileName, sizeInBit);
        }
        public void SetMemorySizeTo1MO()
        {
                SetMemorySize(MemoryFileConnectionUtility._1MOSize);
        }

        public void SetMemorySizeTo1KO()
            {
                SetMemorySize(MemoryFileConnectionUtility._1KOSize);
            }


        public static string P_lockTextLock = "1";
        public static string P_lockTextUnlock = "";
        public static bool p_false = false;
        
        public void Lock() {
            using (MemoryMappedViewStream stream = m_locker.CreateViewStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                writer.BaseStream.Write(new byte[] {1}, 0, 1);
                writer.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                writer.Flush();
                writer.Close();
            }
        }
        public void Unlock() {
          
                using (MemoryMappedViewStream stream = m_locker.CreateViewStream())
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                    writer.BaseStream.Write(new byte[] { 0}, 0, 1);
                    writer.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                    writer.Flush();
                    writer.Close();
                }
      
        }
        public bool IsLock() {
            byte[] result = new byte[LOCKSIZE];
            using (MemoryMappedViewStream stream = m_locker.CreateViewStream())
            {
                BinaryReader reader = new BinaryReader(stream);
                using (var ms = new MemoryStream())
                {
                    byte[] buffer = new byte[1];
                    int count;
                    while ((count = reader.Read(buffer, 0, 1)) != 0)
                        ms.Write(buffer, 0, count);
                    result = ms.ToArray();
                    ms.Dispose();
                    ms.Close();
                }
                reader.Close();
            }
            return result[0] > 0;
        }


        public delegate void DoWhenFileNotUsed();
        public void WaitUntilLockerAllowIt(DoWhenFileNotUsed todo)
        {
            if (todo == null)
                return;
            bool executed = false;
            int antiLoop = 0;
            while (!executed)
            {
                if (IsLock())
                {
                    antiLoop++;
                    if (antiLoop > 5000)
                        return;
                    Thread.Sleep(1);
                }
                else
                {
                    Lock();
                    while (!executed)
                    {
                        //Should be done with Mutex but hey are not supported in IL2CPP
                        try
                        {
                            if (todo != null)
                                todo();
                            executed = true;
                            Unlock();
                        }
                        catch (Exception)
                        {
                            executed = false;
                            Thread.Sleep(1);
                            antiLoop++;
                            if (antiLoop > 5000)
                            {
                                Unlock();
                                return;
                            }
                        }
                    }
                }
            }
        }





        public void ResetToEmpty()
        {
            WaitUntilLockerAllowIt(MutexResetToEmpty);

        }
        private void MutexResetToEmpty()
        {
            m_memoryFile.ResetToEmpty();
        }



        public void AppendTextAtEnd( string textToAdd)
        {
            WaitUntilLockerAllowIt(() =>
            {
                MutexAppendText( textToAdd);
            });
        }
        private void MutexAppendText( string textToAdd)
        {
            m_memoryFile.AppendTextAtEnd( textToAdd);

        }
        public void AppendTextAtStart( string textToAdd)
        {
            WaitUntilLockerAllowIt(() =>
            {
                MutexAppendTextAtStart( textToAdd);
            });

        }
        private void MutexAppendTextAtStart( string textToAdd)
        {
            m_memoryFile.AppendTextAtStart( textToAdd);
        }


        public void SetAsText( string text)
        {
            WaitUntilLockerAllowIt(() =>
            {
                MutexSetAsText( text);
            });

        }
        private void MutexSetAsText( string text)
        {
            m_memoryFile.SetAsText( text);
        }

        public void GetAsText(out string readText,  bool removeContentAfter )
        {

            string textFound = "";
            WaitUntilLockerAllowIt(() =>
            {
                MutexGetAsText(out textFound,  removeContentAfter);
            });
            readText = textFound;

        }

        private void MutexGetAsText(out string readText,  bool removeContentAfter = false)
        {
            m_memoryFile.GetAsText(out readText,  removeContentAfter);
        }


        public void SetAsBytes( byte[] bytes)
        {
            WaitUntilLockerAllowIt(() =>
            {
                MutexSetAsBytes(bytes);
            });

        }
        private void MutexSetAsBytes( byte[] bytes)
        {

            m_memoryFile.SetAsBytes(bytes);
        }


        public void GetAsBytes(out byte[] bytes,  bool removeContentAfter )
        {
            byte[] b = new byte[0];
            WaitUntilLockerAllowIt(() =>
            {
                MutexGetAsBytes(out b,  removeContentAfter);
            });
            bytes = b;

        }

        private void MutexGetAsBytes(out byte[] bytes,  bool removeContentAfter = false)
        {
            m_memoryFile.GetAsBytes(out bytes,  removeContentAfter);
        }

        public void Dispose()
        {
            m_locker.Dispose();
            m_memoryFile.Dispose();
        }
    }
}
