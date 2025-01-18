using System;

namespace SWGen.Razor.Atom;

public interface IDated
{
    DateTime Published { get; }
}