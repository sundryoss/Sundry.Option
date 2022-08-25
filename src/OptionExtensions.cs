namespace Sundry.Option;

/// <summary>
/// Contains the set of extensions to work with the <see cref="Option{T}"/> type.
/// </summary>
public static class OptionExtensions
{
    /// <summary>
    /// Creates the Some case instance of the <see cref="Option{T}"/> type, encapsulating provided value.
    /// </summary>
    /// <typeparam name="T">Type of the encapsulated value.</typeparam>
    /// <param name="value">The value to encapsulate.</param>
    public static Option<T> Some<T>(T value) => new(value);

    /// <summary>
    /// Creates the None case instance of the <see cref="Option{T}"/> type, containing no value.
    /// </summary>
    /// <typeparam name="T">Desired type parameter for <see cref="Option{T}"/> type.</typeparam>
    public static Option<T> None<T>() => default;

    /// <summary>
    /// Converts the value of class T to the <see cref="Option{T}"/> type.
    /// <para/>If the value is null, the None case is yielded.
    /// <para/>Otherwise Some case with provided value is returned.
    /// </summary>
    /// <typeparam name="T">Type of the encapsulated value.</typeparam>
    /// <param name="value">The value to convert to <see cref="Option{T}"/>.</param>
    public static Option<T> OfObject<T>(T value) where T : class =>
        value != null ? Some(value) : None<T>();

    /// <summary>
    /// Converts the value of class T to the <see cref="Option{T}"/> type.
    /// <para/>If the value is null, the <b>None case</b> is yielded.
    /// <para/>Otherwise <b>Some case</b> with provided value is returned.
    /// </summary>
    /// <remarks>Extension method variant of <see cref="OfObject{T}"/></remarks>
    /// <typeparam name="T">Type of the encapsulated value.</typeparam>
    /// <param name="value">The value to convert to <see cref="Option{T}"/>.</param>
    public static Option<T> ToOption<T>(this T value) where T : class => OfObject(value);


    /// <summary>
    /// Converts the value of <see cref="Nullable{T}"/> to the <see cref="Option{T}"/> type.
    /// <para/>If the value is null, the <b>None case</b> is yielded.
    /// <para/>Otherwise <b>Some case</b> with provided value is returned.
    /// </summary>
    /// <typeparam name="T">Type of the encapsulated value.</typeparam>
    /// <param name="value">The value to convert to <see cref="Option{T}"/>.</param>
    public static Option<T> OfNullable<T>(T? value) where T : struct =>
        value.HasValue ? Some(value.Value) : None<T>();

    /// <summary>
    /// Converts the value of <see cref="Nullable{T}"/> to the <see cref="Option{T}"/> type.
    /// <para/>If the value is null, the <b>None case</b> is yielded.
    /// <para/>Otherwise <b>Some case</b> with provided value is returned.
    /// </summary>
    /// <remarks>Extension method variant of <see cref="OfNullable{T}"/></remarks>
    /// <typeparam name="T">Type of the encapsulated value.</typeparam>
    /// <param name="value">The value to convert to <see cref="Option{T}"/>.</param>
    public static Option<T> ToOption<T>(this T? value) where T : struct => OfNullable(value);


    /// <summary>
    /// Converts the string value to the <see cref="Option{T}"/> type.
    /// If the value is null or empty string, the <b>None case</b> is yielded.
    /// Otherwise <b>Some case</b> with provided value is returned.
    /// </summary>
    /// <param name="value">The value to convert to <see cref="Option{T}"/>.</param>
    public static Option<string> OfString(string value) =>
        string.IsNullOrEmpty(value) ? None<string>() : Some(value);

    /// <summary>
    /// Converts the string value to the <see cref="T:Option{string}"/> type.
    /// <para/>If the value is null or empty string, the <b>None case</b> is yielded.
    /// <para/>Otherwise <b>Some case</b> with provided value is returned.
    /// </summary>
    /// <remarks>Extension method variant of <see cref="OfString(string)"/></remarks>
    /// <param name="value">The value to convert to <see cref="T:Option{string}"/>.</param>
    public static Option<string> ToOption(this string value) => OfString(value);


    /// <summary>
    /// Does the pattern matching on the <see cref="Option{T}"/> type.
    /// <para/>If the <paramref name="option"/> is <b>Some</b>, calls <paramref name="some"/> function
    /// with the value from the option as a parameter and returns its result.
    /// <para/>Otherwise calls <paramref name="none"/> function and returns its result.
    /// </summary>
    /// <typeparam name="TIn">Type of the value in the option.</typeparam>
    /// <typeparam name="TOut">Type of the returned value.</typeparam>
    /// <param name="option">The option to match on.</param>
    /// <param name="some">Function called for the Some case.</param>
    /// <param name="none">Function called for the None case.</param>
    public static TOut Match<TIn, TOut>(this Option<TIn> option, Func<TIn, TOut> some, Func<TOut> none) =>
        option.IsSome ? some(option.Value) : none();


    /// <summary>
    /// Maps the value of the <paramref name="option"/> into another <see cref="Option{T}"/> using the <paramref name="mapper"/> function.
    /// <para/>If the input option is <b>Some</b>, returns the Some case with the value of the mapper call (which is <typeparamref name="TOut"/>).
    /// <para/>Otherwise returns None case of the <typeparamref name="TOut"/> option.
    /// </summary>
    /// <typeparam name="TIn">Type of the value in the input option.</typeparam>
    /// <typeparam name="TOut">Type of the value in the returned option.</typeparam>
    /// <param name="option">The option to map on.</param>
    /// <param name="mapper">Function called with the input option value if it's Some case.</param>
    public static Option<TOut> Map<TIn, TOut>(this Option<TIn> option, Func<TIn, TOut> mapper) =>
        option.IsSome ? Some(mapper(option.Value)) : None<TOut>();


    /// <summary>
    /// Transforms the <paramref name="option"/> into another <see cref="Option{T}"/> using the <paramref name="binder"/> function.
    /// <para/>If the input option is <b>Some</b>, returns the value of the binder call (which is <typeparamref name="TOut"/> option).
    /// <para/>Otherwise returns None case of the <typeparamref name="TOut"/> option.
    /// </summary>
    /// <typeparam name="TIn">Type of the value in the input option.</typeparam>
    /// <typeparam name="TOut">Type of the value in the returned option.</typeparam>
    /// <param name="option">The option to bind with.</param>
    /// <param name="binder">Function called with the input option value if it's Some case.</param>
    public static Option<TOut> Bind<TIn, TOut>(this Option<TIn> option, Func<TIn, Option<TOut>> binder) =>
        option.IsSome ? binder(option.Value) : None<TOut>();


    /// <summary>
    /// Gets the value of the <paramref name="option"/> if it's <b>Some case</b>.
    /// <para/>If the option is <b>None case</b> returns value specified by the <paramref name="whenNone"/> parameter;
    /// <para/>if the parameter is not set returns the default value of the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of the value in the option.</typeparam>
    /// <param name="option">The option to get a value from.</param>
    /// <param name="whenNone">Value to return if the option is the None case.</param>
    public static T GetOrDefault<T>(this Option<T> option, T whenNone = default(T)) =>
        option.IsSome
            ? option.Value
            : whenNone;

    /// <summary>
    /// Gets the value from the <paramref name="option"/> using the <paramref name="getter"/> function if it's Some case.
    /// <para/>If the option is <b>None case</b> returns value specified by the <paramref name="whenNone"/> parameter;
    /// <para/>if the parameter is not set returns the default value of the type <typeparamref name="TOut"/>.
    /// </summary>
    /// <remarks>Effectively the combination of <see cref="Map{TIn,TOut}"/> and <see cref="GetOrDefault{T}"/> calls.</remarks>
    /// <typeparam name="TIn">Type of the value in the option.</typeparam>
    /// <typeparam name="TOut">Type of the return value.</typeparam>
    /// <param name="option">The option to get a value from.</param>
    /// <param name="getter">Function used to get the value if the option is the Some case.</param>
    /// <param name="whenNone">Value to return if the option is the None case.</param>
    public static TOut GetOrDefault<TIn, TOut>(this Option<TIn> option, Func<TIn, TOut> getter,
        TOut whenNone = default(TOut)) =>
        option.IsSome ? getter(option.Value) : whenNone;


    /// <summary>
    /// Performs the <paramref name="action"/> with the value of the <paramref name="option"/> if it's <b>Some case</b>.
    /// <para/> If the option is <b>None case</b> nothing happens.
    /// <para/> In both cases unmodified option is returned.
    /// </summary>
    /// <typeparam name="T">Type of the value in the option.</typeparam>
    /// <param name="option">The option to check for a value.</param>
    /// <param name="action">Function executed if the option is Some case.</param>
    public static Option<T> WhenSome<T>(this Option<T> option, Action<T> action)
    {
        if (option.IsSome)
            action(option.Value);

        return option;
    }

    /// <summary>
    /// Performs the <paramref name="action"/> if the <paramref name="option"/> is <b>None case</b>.
    /// <para/>If the option is <b>Some case</b> nothing happens.
    /// <para/>In both cases unmodified option is returned.
    /// </summary>
    /// <typeparam name="T">Type of the value in the option.</typeparam>
    /// <param name="option">The option to check for a value.</param>
    /// <param name="action">Function executed if the option is None case.</param>
    public static Option<T> WhenNone<T>(this Option<T> option, Action action)
    {
        if (option.IsNone)
            action();
        return option;
    }
}