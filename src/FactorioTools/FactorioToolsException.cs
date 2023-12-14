using System;

namespace Knapcode.FactorioTools;

public class FactorioToolsException : Exception
{
    public FactorioToolsException(string? message) : this(message, badInput: false)
    {
        BadInput = false;
    }

    public FactorioToolsException(string? message, bool badInput) : base(message)
    {
        BadInput = badInput;
    }

    public FactorioToolsException(string? message, Exception innerException, bool badInput) : base(message, innerException)
    {
        BadInput = badInput;
    }

    public bool BadInput { get; }
}
