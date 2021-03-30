using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetWorkActivity.Enums
{
    public enum AcceptTypeEnum
    {
        [Description("*/*")]
        Any,

        [Description("application/xml")]
        Xml,

        [Description("application/json")]
        Json,
    }
}
