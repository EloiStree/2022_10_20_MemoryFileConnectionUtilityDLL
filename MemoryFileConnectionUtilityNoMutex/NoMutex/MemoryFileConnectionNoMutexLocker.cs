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
   public class MemoryFileConnectionNoMutexLocker
    {
        public const int LOCKSIZE= 1;
        public TargetMemoryFileWithNoMutex m_memoryFile;
        public MemoryMappedFile m_locker;
        public string m_filenameLock;
        public string m_filenameValue;

        public MemoryFileConnectionNoMutexLocker(string fileName, int fileSize = MemoryFileConnectionUtility._1MOSize )
        {
            m_filenameLock = fileName + "_Locker";
            m_filenameValue = fileName + "_Value";
            m_locker = MemoryMappedFile.CreateOrOpen(m_filenameLock, LOCKSIZE);
            m_memoryFile = new TargetMemoryFileWithNoMutex(new TargetMemoryFileInitation() { m_fileName = fileName + "_Value", m_maxMemorySize = fileSize });
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





        public void ResetMemory()
        {
            WaitUntilLockerAllowIt(MutexResetMemory);

        }
        private void MutexResetMemory()
        {
            m_memoryFile.ResetMemory();
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


        public void SetText( string text)
        {
            WaitUntilLockerAllowIt(() =>
            {
                MutexSetText( text);
            });

        }
        private void MutexSetText( string text)
        {
            m_memoryFile.SetText( text);
        }

        public void TextRecovering(out string readText,  bool removeContentAfter )
        {

            string textFound = "";
            WaitUntilLockerAllowIt(() =>
            {
                MutexTextRecovering(out textFound,  removeContentAfter);
            });
            readText = textFound;

        }

        private void MutexTextRecovering(out string readText,  bool directremove = true)
        {
            m_memoryFile.TextRecovering(out readText,  directremove);
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


        public void BytesRecovering(out byte[] bytes,  bool removeContentAfter )
        {
            byte[] b = new byte[0];
            WaitUntilLockerAllowIt(() =>
            {
                MutexBytesRecovering(out b,  removeContentAfter);
            });
            bytes = b;

        }

        private void MutexBytesRecovering(out byte[] bytes,  bool directremove = true)
        {
            m_memoryFile.BytesRecovering(out bytes,  directremove);
        }

    }
}
