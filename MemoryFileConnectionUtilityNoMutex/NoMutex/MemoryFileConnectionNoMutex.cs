using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace MemoryFileConnectionUtility
{
    [System.Serializable]
    public class TargetMemoryFileInitation : IMemoryMapFileInitInfo
    {
      
         public string m_fileName = "";
        public int m_maxMemorySize = 1000000;

        public void GetBitSize(out int bitSize)
        {
            bitSize = m_maxMemorySize;
        }

        public int GetBitSize()
        {
            return m_maxMemorySize;
        }

        public void GetFileName(out string fileName)
        {
            fileName = m_fileName;
        }

        public string GetFileName()
        {
            return m_fileName;
        }
    }
    public class MemoryFileConnectionNoMutex : IMemoryFileConnectionSetGet
    {

        public string m_fileName = "";
        public int m_maxMemorySize = 1000000;
        public bool m_created;
        public MemoryMappedFile m_memoryFile;

        public MemoryFileConnectionNoMutex(TargetMemoryFileInitation init) : this(init.m_fileName, init.m_maxMemorySize)
        { }


        public MemoryFileConnectionNoMutex(string fileName, int maxMemorySize = 1000000)
        {
            SetWithNameAndSize(fileName, maxMemorySize);
        }


        public void SetWithNameAndSize(string fileName, int maxMemorySize)
        {
            m_fileName = fileName;
            m_maxMemorySize = maxMemorySize;
            m_memoryFile = MemoryMappedFile.CreateOrOpen(fileName, maxMemorySize);
            m_created = true;
        }

        public void SetMemorySize(int sizeInBit)
        {
            m_maxMemorySize = sizeInBit;
            m_memoryFile = MemoryMappedFile.CreateOrOpen(m_fileName, sizeInBit);
        }

        public void SetMemorySizeTo1KO() => SetMemorySize(1000);

        public void Dispose()
        {
            m_memoryFile.Dispose();
        }

        public void SetMemorySizeTo1MO() => SetMemorySize(1000000);

        public void SetName(string name)
        {
            m_fileName = name;
            m_memoryFile = MemoryMappedFile.CreateOrOpen(m_fileName, m_maxMemorySize);
        }

        public delegate void DoWhenFileNotUsed();
        public void WaitUntilMutexAllowIt(DoWhenFileNotUsed todo)
        {
            if (todo == null)
                return;
            bool executed = false;
            int antiLoop=0;
            while (!executed)
            {
                //Should be done with Mutex but hey are not supported in IL2CPP
                try
                {
                    if (todo != null)
                        todo();
                    executed = true;
                }
                catch (Exception)
                {
                    executed = false;
                    Thread.Sleep(5);
                }
                antiLoop++;
                if (antiLoop > 50000)
                    break;
            }
        }


        public  void ResetToEmpty()
        {

            WaitUntilMutexAllowIt(MutexResetToEmpty);

        }

        private void MutexResetToEmpty()
        {


            using (MemoryMappedViewStream stream = m_memoryFile.CreateViewStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                writer.BaseStream.Write(new byte[m_maxMemorySize], 0, (int)m_maxMemorySize);
                writer.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

                //NOT TESTED
                writer.Flush();
                writer.Close();

                //                    Thread.Sleep(1000);
            }
        }



        public void AppendTextAtEnd(string textToAdd)
        {
            WaitUntilMutexAllowIt(() =>
            {
                MutexAppendText(textToAdd);
            });
        }


        private void MutexAppendText(string textToAdd)
        {

            string readText;
            using (MemoryMappedViewStream stream = m_memoryFile.CreateViewStream())
            {

                MutexGetAsText(out readText, false);

                BinaryWriter writer = new BinaryWriter(stream);
                string nexText = readText + textToAdd;
                if (nexText.Length > m_maxMemorySize)
                    nexText = nexText.Substring(0, m_maxMemorySize);

                writer.Write(nexText);

                //NotTested
                writer.Flush();
                writer.Close();

            }



        }
        public void AppendTextAtStart(string textToAdd)
        {
            WaitUntilMutexAllowIt(() =>
            {
                MutexAppendTextAtStart(textToAdd);
            });

        }
        private void MutexAppendTextAtStart(string textToAdd)
        {
            string readText;
            using (MemoryMappedViewStream stream = m_memoryFile.CreateViewStream())
            {
                MutexGetAsText(out readText, false);

                BinaryWriter writer = new BinaryWriter(stream);
                string nexText = textToAdd + readText;
                if (nexText.Length > m_maxMemorySize)
                    nexText = nexText.Substring(0, m_maxMemorySize);

                writer.Write(nexText);

                //NotTested
                writer.Flush();
                writer.Close();
            }
        }


        public void SetAsText(string text)
        {
            WaitUntilMutexAllowIt(() =>
            {
                MutexSetAsText(text);
            });

        }
        private void MutexSetAsText(string text)
        {

            using (MemoryMappedViewStream stream = m_memoryFile.CreateViewStream())
            {

                MutexResetToEmpty();

                BinaryWriter writer = new BinaryWriter(stream);
                string nexText = text.Trim();
                if (nexText.Length > m_maxMemorySize)
                    nexText = nexText.Substring(0, m_maxMemorySize);

                writer.Write(nexText);
                //NotTested
                writer.Flush();
                writer.Close();

            }
        }

        public void GetAsText(out string readText, bool removeContentAfter = false)
        {

            string textFound = "";
            WaitUntilMutexAllowIt(() =>
            {
                MutexGetAsText(out textFound, removeContentAfter);
            });
            readText = textFound;

        }

        private void MutexGetAsText(out string readText, bool removeContentAfter = false)
        {
            readText = "";

            using (MemoryMappedViewStream stream = m_memoryFile.CreateViewStream())
            {
                BinaryReader reader = new BinaryReader(stream);
                StringBuilder strb = new StringBuilder();
                string str;
                do
                {
                    str = reader.ReadString();
                    if ((!String.IsNullOrEmpty(str) && str[0] != 0))
                        strb.AppendLine(str);
                } while (!String.IsNullOrEmpty(str));

                readText = strb.ToString();

                if (removeContentAfter)
                {
                    MutexResetToEmpty();
                }

                //NotTested
                reader.Close();
            }
        }


        public void SetAsBytes(byte[] bytes)
        {
            WaitUntilMutexAllowIt(() =>
            {
                MutexSetAsBytes(bytes);
            });

        }
        private void MutexSetAsBytes(byte[] bytes)
        {

            MutexResetToEmpty();
            using (MemoryMappedViewStream stream = m_memoryFile.CreateViewStream())
            {
                // MutexResetMemory();
                BinaryWriter writer = new BinaryWriter(stream);
                if (bytes.Length > m_maxMemorySize)
                {
                    throw new Exception("Out of memory size");
                }
                writer.Write(bytes, 0, bytes.Length);

                //NotTested
                writer.Flush();
                writer.Close();
            }
        }


        public void GetAsBytes(out byte[] bytes, bool removeContentAfter = false)
        {
            byte[] b = new byte[0];
            WaitUntilMutexAllowIt(() =>
            {
                MutexGetAsBytes(out b, removeContentAfter);
            });
            bytes = b;

        }

        private void MutexGetAsBytes(out byte[] bytes, bool removeContentAfter = false)
        {
            bytes = null;
            using (MemoryMappedViewStream stream = m_memoryFile.CreateViewStream())
            {
                BinaryReader reader = new BinaryReader(stream);
                bytes = ReadAllBytes(reader);
                if (removeContentAfter)
                {
                    MutexResetToEmpty();
                }
                //NotTested
                reader.Close();
            }
        }

        public static byte[] ReadAllBytes(BinaryReader reader)
        {
            const int bufferSize = 4096;
            byte[] result = new byte[0];
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                result = ms.ToArray();

                ms.Dispose();
                ms.Close();
            }
            return result;
        }

        void IMemoryFileConnectionSetGet.Dispose()
        {
            m_memoryFile.Dispose();
            
        }
    }
}