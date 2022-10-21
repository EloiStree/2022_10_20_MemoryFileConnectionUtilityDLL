using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Security.Principal;

namespace MemoryFileConnectionUtility
{
    [System.Serializable]
    public class TargetMemoryFileWithMutexInfo : IMemoryMapFileInitInfo
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
            fileName = m_fileName ;
        }

        public string GetFileName()
        {
            return m_fileName;
        }
    }
    [System.Serializable]
    public class TargetMemoryFileWithMutexInfoWithFormat : TargetMemoryFileWithMutexInfo
    {
        public string m_mutexFormatId = "Global\\{{{0}}}mutex";
    }



}
