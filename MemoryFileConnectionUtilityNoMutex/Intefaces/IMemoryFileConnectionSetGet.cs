using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryFileConnectionUtility
{
    public interface IMemoryFileConnectionSetGet
    {
         void SetWithNameAndSize(string name, int size);
         void SetName(string name);
         void SetMemorySize(int sizeInBit);
         void SetMemorySizeTo1MO();
         void SetMemorySizeTo1KO();
         void ResetToEmpty();
         void AppendTextAtEnd(string textToAdd);
         void AppendTextAtStart(string textToAdd);
         void SetAsText(string text);
         void GetAsText(out string readText, bool removeContentAfter = false);
         void SetAsBytes(byte[] bytes);
         void GetAsBytes(out byte[] bytes, bool removeContentAfter = false);
         void Dispose();
    }
}
