using System;
using System.Text;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace MemoryFileConnectionUtility
{
    public class MemoryFileConnectionWithMutexLocker : IMemoryFileConnectionSetGet
    {
        public string m_fileName = "";
        public int m_maxMemorySize = MemoryFileConnectionUtility._1MOSize;
        public bool m_created;
        public MemoryMappedFile m_memoryFile;
        public Mutex m_memoryFileMutex;
        public string m_mutexFormatId = "Global\\{{{0}}}mutex";

        public MemoryFileConnectionWithMutexLocker(TargetMemoryFileWithMutexInfo init) : this(init.m_fileName, init.m_maxMemorySize)
        { }
        public MemoryFileConnectionWithMutexLocker(TargetMemoryFileWithMutexInfoWithFormat init) : this(init.m_fileName, init.m_mutexFormatId, init.m_maxMemorySize)
        { }

        public MemoryFileConnectionWithMutexLocker(string fileName, int maxMemorySize = MemoryFileConnectionUtility._1MOSize)
        {

            SetWithNameAndSize(fileName, maxMemorySize);
        }
        public MemoryFileConnectionWithMutexLocker(string fileName, string fileNameSpecificFormat, int maxMemorySize = MemoryFileConnectionUtility._1MOSize)
        {
            SetWithNameAndSize(fileName, maxMemorySize);
             m_mutexFormatId = fileNameSpecificFormat;
        }


        public void SetWithNameAndSize(string name, int size)
        {
            m_fileName = name;
            m_maxMemorySize = size;
            string mutexId = string.Format(m_mutexFormatId, name);
            //m_memoryFileMutex = new Mutex(false, mutexId, out createdNew, securitySettings);
            m_memoryFile = MemoryMappedFile.CreateOrOpen(name, size);
            m_memoryFileMutex = new Mutex(false, mutexId, out m_created);
        }

        public void SetName(string name)
        {
            SetWithNameAndSize(name, m_maxMemorySize);
        }

        public void SetMemorySize(int sizeInBit)
        {
            SetWithNameAndSize(m_fileName, sizeInBit);
        }

        public void SetMemorySizeTo1MO()
        {
            SetMemorySize(MemoryFileConnectionUtility._1MOSize);
        }

        public void SetMemorySizeTo1KO()
        {
            SetMemorySize(MemoryFileConnectionUtility._1KOSize);
        }


        public delegate void DoWhenFileNotUsed();
        public void WaitUntilMutexAllowIt(DoWhenFileNotUsed todo)
        {

            var hasHandle = false;
            try
            {
                try
                {

                    // mutex.WaitOne(Timeout.Infinite, false);
                    hasHandle = m_memoryFileMutex.WaitOne(5000, false);
                    if (hasHandle == false)
                        throw new TimeoutException("Timeout waiting for exclusive access");
                }
                catch (AbandonedMutexException)
                {
                    hasHandle = true;
                }
                todo();
            }
            finally
            {
                if (hasHandle)
                    m_memoryFileMutex.ReleaseMutex();
            }

        }

        public void ResetToEmpty()
        {

            WaitUntilMutexAllowIt(MutexResetMemory);

        }

        private void MutexResetMemory()
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

                MutexResetMemory();

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
            WaitUntilMutexAllowIt(() => {
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
                    MutexResetMemory();
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

            MutexResetMemory();
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
            WaitUntilMutexAllowIt(() => {
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
                    MutexResetMemory();
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

       

        public void Dispose()
        {
            m_memoryFile.Dispose();
            m_memoryFileMutex.Dispose();
        }
    }



}
