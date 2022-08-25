using Sundry.Option;


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
        return OptionExtensions.Some("Some Value");
    }
}