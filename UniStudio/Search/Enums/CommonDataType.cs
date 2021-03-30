using System;

namespace UniStudio.Search.Enums
{
    [Flags]
    public enum CommonDataType
    {
        Unknown=0x00,
        Variable= 0x01,
        Property = 0x02,
        InArgument = 0x04,
        OutArgument = 0x08,
        InOutArgument = 0x10,
        PropertyArgument = 0x40,
        DesignerActivity=0x80,
        Activity = 0x0100,
        Import = 0x0200,
        ProjectFile = 0x0400,
        Dependency = 0x0800,
        Snippet = 0x1000
    }
}
