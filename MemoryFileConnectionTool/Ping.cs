using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryFileConnectionUtility
{
    public class HelloPing
    {
        public static MemoryFileConnection c;
        public string PingMessage(string variable) { return "Ping " + variable; }
        public int PingMessage(int variable) { return 1 + variable; }
        public string ReadMemory(string memoryName) {

            if (c == null) { 
                c = new MemoryFileConnection();
                c.SetNameAndSizeThenReset(memoryName, 100000);
            }
            c.GetAsText(out string t);
            return t;
        
        }
    }
}
