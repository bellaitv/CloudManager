using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudManagerCommons
{
    public class CloudManagerException : Exception
    {
        public CloudManagerException(String msg) : base(msg) {

        } 

    }
}
