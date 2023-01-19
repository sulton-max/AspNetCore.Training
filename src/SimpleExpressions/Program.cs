using System.Linq.Expressions;
using Simple;

// Filtering resource
var users = new List<User>
{
    new User("Joah", "MacKenzie"),
    new User("Joab", "Maximilian"),
    new User("Joash", "Joe"),
    new User("Joel", "Bob"),
    new User("Michael", "Joey"),
    new User("John", "Doe")
}.AsQueryable();

var products = new List<Product>
{
    new Product("Joe's"),
    new Product("Johnsons")
}.AsQueryable();

var keyword1 = "Joe";
var keyword2 = "Joa";

// Task 1 - match users by given keyword
// Manual matching - can't use multiple keys
var match1 = users.Where(x =>
        x.FirstName.Contains(keyword1, StringComparison.OrdinalIgnoreCase) || x.LastName.Contains(keyword1, StringComparison.OrdinalIgnoreCase))
    .ToList();

Console.WriteLine("Manual matching results");
match1.ForEach(x => Console.WriteLine($"{x.FirstName} {x.LastName}"));
Console.WriteLine();

// Task 2 - match users by multiple keywords
// Creating predicates manually - need to update predicate when resource is updated
var predicateExpression = null as Expression<Func<User, string, bool>>;
predicateExpression = (user, keyword) => user.FirstName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                         user.LastName.Contains(keyword, StringComparison.OrdinalIgnoreCase);
var predicateFunction = predicateExpression.Compile();

var match2 = users.Where(x => predicateFunction.Invoke(x, keyword1) || predicateFunction.Invoke(x, keyword2)).ToList();
Console.WriteLine("Manual predicate results");
match2.ForEach(x => Console.WriteLine($"{x.FirstName} {x.LastName}"));
Console.WriteLine();

// Task 3 - match any resource properties
// Creating predicates via Expressions
var parameter = Expression.Parameter(typeof(User));
var properties = typeof(User).GetProperties().Where(x => x.PropertyType.Equals(typeof(string))).ToList();
var compareMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) }) ?? throw new InvalidOperationException();
var predicates = properties.Select(x =>
    {
        var memberExpression = Expression.PropertyOrField(parameter, x.Name);
        var argument = Expression.Convert(Expression.Constant(keyword1), x.PropertyType);
        var methodCallExpression = Expression.Call(memberExpression, compareMethod, argument);
        return Expression.Lambda<Func<User, bool>>(methodCallExpression, parameter);
    })
    .ToList();

// Joining predicates
var finalExpression = PredicateBuilder<User>.False;
predicates.ForEach(x => finalExpression = PredicateBuilder<User>.Or(finalExpression, x));

Console.WriteLine("Expression predicate results for users");
var match3 = users.Where(finalExpression).ToList();
match3.ForEach(x =>
{
    var user = x as User;
    Console.WriteLine($"{user?.FirstName} {user?.LastName}");
});
Console.WriteLine();

parameter = Expression.Parameter(typeof(Product));
properties = typeof(Product).GetProperties().Where(x => x.PropertyType.Equals(typeof(string))).ToList();
var productPredicates = properties.Select(x =>
    {
        var memberExpression = Expression.PropertyOrField(parameter, x.Name);
        var argument = Expression.Convert(Expression.Constant(keyword1), x.PropertyType);
        var methodCallExpression = Expression.Call(memberExpression, compareMethod, argument);
        return Expression.Lambda<Func<Product, bool>>(methodCallExpression, parameter);
    })
    .ToList();

var productFinalExpression = PredicateBuilder<Product>.False;
productPredicates.ForEach(x => productFinalExpression = PredicateBuilder<Product>.Or(productFinalExpression, x));

Console.WriteLine("Expression predicate results for products");
var match4 = products.Where(productFinalExpression).ToList();
match4.ForEach(x =>
{
    var product = x as Product;
    Console.WriteLine($"{product?.Name}");
});

// Task 4 - match any resource with one expression ?!

/// <summary>
/// Represents common properties for entity
/// </summary>
public interface IEntity
{
}

/// <summary>
/// Represents system user
/// </summary>
public class User : IEntity
{
    public User(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public string FirstName { get; set; }

    public string LastName { get; set; }
}

/// <summary>
/// Represents product that can be ordered
/// </summary>
public class Product : IEntity
{
    public Product(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}