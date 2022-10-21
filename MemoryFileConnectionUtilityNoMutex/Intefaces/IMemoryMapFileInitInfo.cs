using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryFileConnectionUtility
{
    public interface IMemoryMapFileInitInfo
    {
        void GetFileName(out string fileName);
        void GetBitSize(out int bitSize);
        string GetFileName();
        int GetBitSize();

    }
}
