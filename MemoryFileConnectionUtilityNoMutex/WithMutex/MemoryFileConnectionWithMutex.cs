using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryFileConnectionUtility
{
    [System.Serializable]
    public class MemoryFileConnectionWithMutex
    {
        public TargetMemoryFileWithMutexInfoWithFormat m_setupInfo= new TargetMemoryFileWithMutexInfoWithFormat();
        private TargetMemoryFileWithMutex m_connection;

        public MemoryFileConnectionWithMutex() { m_connection = new TargetMemoryFileWithMutex(m_setupInfo); }
        public MemoryFileConnectionWithMutex(string fileName, int maxSize)
        {
            m_setupInfo.m_fileName = fileName;
            m_setupInfo.m_maxMemorySize = maxSize;
            m_connection = new TargetMemoryFileWithMutex(m_setupInfo);
        }

        public void CheckThatConnectionExist()
        {
            m_connection = new TargetMemoryFileWithMutex(m_setupInfo);
        }
        public void SetNameThenReset(string fileName)
        {
            m_setupInfo.m_fileName = fileName;
            m_connection = new TargetMemoryFileWithMutex(m_setupInfo);
        }
        public void SetMaxSizeThenReset(string fileName)
        {
            m_setupInfo.m_fileName = fileName;
            m_connection = new TargetMemoryFileWithMutex(m_setupInfo);
        }
        public void SetNameAndSizeThenReset(string fileName, int maxSize)
        {
            m_setupInfo.m_fileName = fileName;
            m_setupInfo.m_maxMemorySize = maxSize;
            m_connection = new TargetMemoryFileWithMutex(m_setupInfo);
        }



        public TargetMemoryFileWithMutex Connection()
        {
            CheckThatConnectionExist();
            return m_connection;
        }
        public void SetAsBytes(byte[] bytes)
        {
            Connection().SetAsBytes(bytes);
        }
        public void SetText(string text)
        {
            Connection().SetText(text);
        }
        public void AppendTextAtEnd(string text)
        {
            Connection().AppendTextAtEnd(text);
        }
        public void AppendTextAtStart(string text)
        {
            Connection().AppendTextAtStart(text);
        }
        public void GetAsText(out string text)
        {
            Connection().TextRecovering(out text, false);
        }
        public void GetAsTextAndFlush(out string text)
        {
            Connection().TextRecovering(out text, true);
        }
        //public void SetAsTexture2D_Heavy(RenderTexture renderTexture)
        //{
        //    //THIS CODE CREATE MEMORY LEAK I TINK SO CHECK IF I IT RENDER TEXTURE TO TEXTURE OR SET BYTES
        //    //IS IT POSSIBLE THAT IAM SET BYTES WITH TO HEAVY IMAGE AND SO PUSH DATA OUT OF MY ZONE OF MEMORY AND LEAK ?
        //    Eloi.E_Texture2DUtility.RenderTextureToTexture2D(in renderTexture, out Texture2D texture);
        //    byte[] t = texture.EncodeToPNG();
        //    Connection().SetAsBytes(t);
        //}
        //public void SetAsTexture2D_Heavy(Texture2D texture)
        //{
        //    byte[] t = texture.EncodeToPNG();
        //    Connection().SetAsBytes(t);
        //}

        public void GetAsBytes(out byte[] bytes)
        {
            Connection().BytesRecovering(out bytes, false);
        }

        public void GetAsBytesAndFlush(out byte[] bytes)
        {
            Connection().BytesRecovering(out bytes, true);
        }

        //public void GetAsTexture2D(out Texture2D texture)
        //{
        //    Connection().BytesRecovering(out byte[] bytes, false);
        //    texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        //    texture.LoadImage(bytes);
        //    texture.Apply();
        //}
        //[ContextMenu("Read Bytes And Flush")]
        //public void GetAsTexture2DAndFlush(out Texture2D texture)
        //{
        //    Connection().BytesRecovering(out byte[] bytes, true);
        //    texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        //    texture.LoadImage(bytes);
        //    texture.Apply();
        //}


        //public void SetAsOjectInJsonFromat<T>(T target)
        //{
        //    string json = JsonSerializer.Serialize(target);
        //    SetText(json);
        //}
        //public T GetInObjectFromJsonFormat<T>()
        //{
        //    GetAsText(out string json);
        //    return JsonSerializer.Deserialize<T>(json);
        //}
        //public void GetInObjectFromJsonFormat<T>(out T recovered)
        //{
        //    GetAsText(out string json);
        //    recovered = JsonSerializer.Deserialize<T>(json);
        //}

        public void Flush()
        {
            m_connection.ResetMemory();
        }
    }



}
