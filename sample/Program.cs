using Sundry.Option;

Option<string> Email = Option.Some("me@sundryoss.fake");
Option<string> apple = Option.Some("apple");
Option<string> orange = Option.Some("orange");
string alsoOrange = "orange";
Option<string> noFruit = Option.None<string>();

Console.WriteLine(apple == orange); // false
Console.WriteLine(apple != orange); // true
Console.WriteLine(orange == alsoOrange); // true
Console.WriteLine(alsoOrange == noFruit); // false

int storeInventory = 1;

Option<string> fruit = storeInventory > 0
    ? Option.Some("apple")
    : Option.None<string>();
storeInventory = -1;
Option<string> Nonefruit = storeInventory > 0
    ? Option.Some("apple")
    : Option.None<string>();

Console.WriteLine(fruit); Console.WriteLine(Nonefruit);


ISample sample = new Sample();
var sampleValue = sample.GetSomeValue();
Console.WriteLine(sampleValue.ToString());
Console.ReadKey();
class Sample : ISample
{ }
interface ISample
{
    Option<string> GetSomeValue()
    {
        return Option.Some("Some Value");
    }
}