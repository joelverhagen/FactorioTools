using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cathei.LinqGen.Hidden;

public interface IInternalStub<out T> : IInternalStub, IStub<IEnumerable<T>, Compiled>
{
}

public interface IInternalStub
{
}

public interface IStub<out T, TSignature> where T : IEnumerable
{
}

public abstract class Compiled : IStubSignature
{
}

public interface IStubSignature
{
}

public class PooledListManaged<T> : IDisposable
{
    private List<T>? _list;

    public PooledListManaged(int capacity)
    {
    }

    public void Add(T item)
    {
        if (_list is null)
        {
            _list = new List<T>();
        }

        _list.Add(item);
    }

    public List<T> ToList()
    {
        return _list ?? new List<T>();
    }

    public void Dispose()
    {
        _list = null;
    }
}