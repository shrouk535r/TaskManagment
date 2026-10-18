using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Application.Results
{
    public class Result<TValue>
    {
        public bool IsSuccess { get; }
        public TValue? Value { get; }
        public Error Error { get; }
        public Result(TValue value) 
        {
            IsSuccess = true;
            Value = value;
            Error = Error.None;
        }
        public Result(Error error)
        {
            IsSuccess = false;
            Error = error;
            Value = default;
        }
        public static Result<TValue> Success(TValue value) => new Result<TValue>(value);
        public static Result<TValue> Failure(Error error) => new Result<TValue>(error);

        public static implicit operator Result<TValue>(TValue value) => new Result<TValue>(value);
        public static implicit operator Result<TValue>(Error error) => new Result<TValue>(error);
    }
}
