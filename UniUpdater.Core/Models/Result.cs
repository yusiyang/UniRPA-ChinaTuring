using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniUpdater.Models
{
    public class Result<T>:Result
    {
        public T Data { get; set; }

        public Result(T data=default(T), bool isSucess = true, string message = null) 
        {
            Data = data;
            IsSucess = isSucess;
            Message = message;
        }

    }

    public class Result
    {
        public bool IsSucess { get; set; }

        public string Message { get; set; }

        public Result()
        { }

        public Result(bool isSucess,string message=null)
        {
            IsSucess = isSucess;
            Message = message;
        }

        public static Result Success()
        {
            return new Result(true);
        }

        public static Result<T> Success<T>(T data)
        {
            return new Result<T>(data);
        }

        public static Result Fail(string message)
        {
            return new Result(false, message);
        }

        public static Result<T> Fail<T>(string message)
        {
            return new Result<T>(default(T),false,message);
        }
    }
}
