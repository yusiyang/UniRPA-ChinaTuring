using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniWorkforce.Models
{
    public class LoginResponseModel
    {
        /// <summary>
        /// 响应代码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 响应消息内容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 返回的响应数据
        /// </summary>
        public string Data { get; set; }
    }
}
