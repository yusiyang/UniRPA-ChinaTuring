using System;

namespace UniWorkforce.Exceptions
{
    public class RobotNotExistException : Exception
    {
        public RobotNotExistException(string message) : base(message)
        {
        }
    }
}
