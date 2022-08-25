namespace Sundry.Option;

/// <summary>
/// Encapsulates optional value.
/// It is recommended to use provided extension methods and not to use properties of the <see cref="Option{T}"/> directly.
/// <para />The Option can contain a value of a type <typeparamref name="T"/> and it's called Some in such case.
/// <para />If no value is encapsulated it's called None.
/// </summary>
/// <typeparam name="T">Type of encapsulated value.</typeparam>
public readonly struct Option<T> : IEquatable<Option<T>>, IEquatable<T>
{
    /// <summary>
    /// Encapsulated value. Will be default(T) in None case.
    /// <para /> It is not recommended to use it directly.
    /// <para /> Use extension methods to access the value instead.
    /// </summary>
    public T Value { get; }

    internal Option(T value)
    {
        Value = value;
        IsSome = true;
    }
    
    /// <summary>
    /// Returns true if the option has value (is Some case).
    /// </summary>
    public bool IsSome { get; }

    /// <summary>
    /// Returns true if the option has no value (is None case).
    /// </summary>
    public bool IsNone => !IsSome;
    
    /// <summary>
    /// Creates a string representation of the <see cref="Option{T}"/>.
    /// <para/>Returns "{Value}" if the option has Some, otherwise "string.Empty".
    /// </summary>
    public override string ToString() =>
        IsSome
            ? $"{Value}"
            : string.Empty;
    
    /// <summary>
    /// Computes the hashcode for the <see cref="Option{T}"/> instance.
    /// </summary>
    public override int GetHashCode()
    {
        unchecked
        {
            return IsNone
                ? base.GetHashCode() ^ 13
                : Value == null
                    ? base.GetHashCode() ^ 29
                    : Value.GetHashCode() ^ 31;
        }
    }
    
    /// <summary>
    /// Compares the <see cref="Option{T}"/> with other object.
    /// Option is only equal to other option given the conditions described in <see cref="Option{T}.Equals(Option{T})"/>.
    /// </summary>
    /// <param name="obj">Object to compare with</param>
    public override bool Equals(object? obj) =>
        obj is Option<T> option && Equals(option);
    
    /// <summary>
    /// Compares two instances of the <see cref="Option{T}"/>.
    /// Two options are equal if both are of the same type, the same case
    /// and the underlying values are equal.
    /// </summary>
    /// <param name="other">Option to compare with</param>
    public bool Equals(Option<T> other) =>
        (IsNone && other.IsNone) || (IsSome && other.IsSome && EqualityComparer<T>.Default.Equals(Value, other.Value));
    
    /// <summary>
    /// Compares <see cref="Option{T}"/> with the value of type <typeparamref name="T"/>.
    /// Option is equal to the value of the underlying type if it's Some case
    /// and encapsulated value is equal to <paramref name="other"/> value.
    /// </summary>
    /// <param name="other">Option to compare with</param>
    public bool Equals(T? other) =>
        IsSome && EqualityComparer<T>.Default.Equals(Value, other);
    
    public static bool operator ==(in Option<T> a, in Option<T> b) => a.Equals(b);

    public static bool operator !=(in Option<T> a, in Option<T> b) => !a.Equals(b);

    public static bool operator ==(in Option<T> a, in T b) => a.Equals(b);

    public static bool operator !=(in Option<T> a, in T b) => !a.Equals(b);

    public static bool operator ==(in T a, in Option<T> b) => b.Equals(a);

    public static bool operator !=(in T a, in Option<T> b) => !b.Equals(a);
    
    public static implicit operator T(in Option<T> option) => option.IsSome ? option.Value : default!;
    
}