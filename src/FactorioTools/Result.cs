using System;

namespace Knapcode.FactorioTools;

public class Result<T> : Result
{
    public Result(T? data, Exception? exception) : base(exception)
    {
        Data = data;
    }

    public T? Data { get; }

    public void Deconstruct(out T? data, out Exception? exception)
    {
        data = Data;
        exception = Exception;
    }
}

public class Result
{
    public Result(Exception? exception)
    {
        Exception = exception;
    }

    public Exception? Exception { get; }

    public static Result<T> NewData<T>(T data)
    {
        return new Result<T>(data, exception: null);
    }

    public static Result<T> NewException<T>(Exception exception)
    {
        return new Result<T>(data: default, exception);
    }

    public static Result NewException(Exception exception)
    {
        return new Result(exception);
    }
}
