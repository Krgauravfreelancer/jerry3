using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timeline.UserControls.Exceptions
{
    public class UnknownAppAccessException : Exception
    {
        public UnknownAppAccessException(int appId) : base($"AppId={appId} is not supported.") { }
    }
}
