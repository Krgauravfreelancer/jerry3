using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timeline.UserControls.Exceptions
{
    public class UnknownMediaTypeException : Exception
    {
        public UnknownMediaTypeException(int mediaId) : base($"MediaId={mediaId} is not supported.") { }
    }
}
