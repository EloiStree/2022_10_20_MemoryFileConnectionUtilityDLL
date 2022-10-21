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
    public class TargetMemoryFileInitation
    {
        public string m_fileName = "";
        public int m_maxMemorySize = 1000000;
    }


    public class TargetMemoryFileWithNoMutex
    {

        public string m_fileName = "";
        public int m_maxMemorySize = 1000000;
        public bool m_created;
        public MemoryMappedFile m_memoryFile;

        public TargetMemoryFileWithNoMutex(TargetMemoryFileInitation init) : this(init.m_fileName, init.m_maxMemorySize)
        { }


        public TargetMemoryFileWithNoMutex(string fileName, int maxMemorySize = 1000000)
        {
            m_fileName = fileName;
            m_maxMemorySize = maxMemorySize;
            m_memoryFile = MemoryMappedFile.CreateOrOpen(fileName, maxMemorySize);
            m_created = true;
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





        public void ResetMemory()
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

                MutexTextRecovering(out readText, false);

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
                MutexTextRecovering(out readText, false);

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


        public void SetText(string text)
        {
            WaitUntilMutexAllowIt(() =>
            {
                MutexSetText(text);
            });

        }
        private void MutexSetText(string text)
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

        public void TextRecovering(out string readText, bool removeContentAfter = true)
        {

            string textFound = "";
            WaitUntilMutexAllowIt(() =>
            {
                MutexTextRecovering(out textFound, removeContentAfter);
            });
            readText = textFound;

        }

        private void MutexTextRecovering(out string readText, bool directremove = true)
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

                if (directremove)
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


        public void BytesRecovering(out byte[] bytes, bool removeContentAfter = true)
        {
            byte[] b = new byte[0];
            WaitUntilMutexAllowIt(() =>
            {
                MutexBytesRecovering(out b, removeContentAfter);
            });
            bytes = b;

        }

        private void MutexBytesRecovering(out byte[] bytes, bool directremove = true)
        {
            bytes = null;
            using (MemoryMappedViewStream stream = m_memoryFile.CreateViewStream())
            {
                BinaryReader reader = new BinaryReader(stream);
                bytes = ReadAllBytes(reader);
                if (directremove)
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

    }
}