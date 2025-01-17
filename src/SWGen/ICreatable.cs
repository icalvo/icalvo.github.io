namespace SWGen;

/// <summary>
/// Object that can be created without parameters.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// This looks a lot like "objects with default constructor", so why use this interface instead?
///
/// This is used for document metadata. We want to be able to create default metadata values.
/// Why not simply use a new() constraint? The problem is that when you have a default constructor then
/// you cannot have a `required` member, and at the same time build a new instance with T() because
/// `required` means you must use the object initializer.
///
/// Since we want to promote the object initializer syntax for metadata we used this interface. 
/// </remarks>
public interface ICreatable<out T>
{
    static abstract T Create();
}
