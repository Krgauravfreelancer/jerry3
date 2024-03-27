using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timeline.UserControls.Exceptions
{
    internal class UnauthorizedAppActionException : Exception
    {
        public UnauthorizedAppActionException(string mediaName) : base($"App is not allowed to do this action for media \"{mediaName}\"") { }
    }
}
