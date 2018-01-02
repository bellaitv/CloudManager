using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudManagerCommons
{
    public class CloudManagerException : Exception
    {
        public Exception BasException { get; set; }

        public CloudManagerException() { }

        public CloudManagerException(String msg, ExceptionType type) : base(msg) {
        }

        public CloudManagerException(String msg, ExceptionType type, Exception baseException) : base(msg)
        {
            BasException = baseException;
        }
    }
}
