using System.Linq.Expressions;

namespace Simple;

/// <summary>
/// Provides extensions to build predicate expressions
/// </summary>
/// <typeparam name="TSource">Expression source</typeparam>
public static class PredicateBuilder<TSource> where TSource : class
{
    /// <summary>
    /// Initial true expression
    /// </summary>
    public static Expression<Func<TSource, bool>> True = entity => true;

    /// <summary>
    /// Initial false expression
    /// </summary>
    public static Expression<Func<TSource, bool>> False = entity => false;

    /// <summary>
    /// Joins to expression with OR logic
    /// </summary>
    /// <param name="left">Left expression</param>
    /// <param name="right">Right expression</param>
    /// <returns>Joined expression</returns>
    public static Expression<Func<TSource, bool>> Or(Expression<Func<TSource, bool>> left, Expression<Func<TSource, bool>> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var invokeExpr = Expression.Invoke(right, left.Parameters.Cast<Expression>());
        return Expression.Lambda<Func<TSource, bool>>(Expression.OrElse(left.Body, invokeExpr), left.Parameters);
    }

    /// <summary>
    /// Joins to expression with AND logic
    /// </summary>
    /// <param name="left">Left expression</param>
    /// <param name="right">Right expression</param>
    /// <returns>Joined expression</returns>
    public static Expression<Func<TSource, bool>> And(Expression<Func<TSource, bool>> left, Expression<Func<TSource, bool>> right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var invokeExpr = Expression.Invoke(right, left.Parameters.Cast<Expression>());
        return Expression.Lambda<Func<TSource, bool>>(Expression.AndAlso(left.Body, invokeExpr), left.Parameters);
    }
}