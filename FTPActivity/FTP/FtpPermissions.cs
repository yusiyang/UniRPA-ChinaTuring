using System;

namespace FTPActivity
{
    [Flags]
    public enum FtpPermissions : uint
    {
        None = 0,
        Execute = 1,
        Write = 2,
        Read = 4
    }
}