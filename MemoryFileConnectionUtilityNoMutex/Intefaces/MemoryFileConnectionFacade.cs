using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryFileConnectionUtility
{
    public enum MemoryFileConnectionType { WithMutex, MemoryFileLocker, MemoryFileRawWrapper }
    public class MemoryFileConnectionFacade
    {


        public static void CreateConnection(MemoryFileConnectionType connectionType, IMemoryMapFileInitInfo buildInfo,
          out IMemoryFileConnectionSetGet connection)
        {
            CreateConnection(connectionType, buildInfo.GetFileName(), out connection, buildInfo.GetBitSize() );
        }
        public static void CreateConnection(MemoryFileConnectionType connectionType, string fileName ,
            out  IMemoryFileConnectionSetGet connection,  int fileSize=MemoryFileConnectionUtility._1MOSize) 
        {
            connection = null;
            if (connectionType == MemoryFileConnectionType.WithMutex)
                connection = new MemoryFileConnectionWithMutexLocker(fileName, fileSize);
            if (connectionType == MemoryFileConnectionType.MemoryFileLocker)
                connection = new MemoryFileConnectionNoMutexWithFileLocker(fileName, fileSize);
            if (connectionType == MemoryFileConnectionType.MemoryFileRawWrapper)
                connection = new MemoryFileConnectionNoMutex(fileName, fileSize);
        }

    }
}
