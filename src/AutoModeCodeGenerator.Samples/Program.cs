using Samples.Entities;
using bbb;

Console.WriteLine("AutoModeCodeGenerator Samples");
Console.WriteLine("-----------------------------");

var recordPoint = new Point1ccc(3.14);
Console.WriteLine($"Record  Point1ccc.Xxxxx = {recordPoint.Xxxxx}");

var pocoPoint = new Point2ccc { Y = 1.5, Name = "origin", ProjectId = 42, Name111 = "from base" };
Console.WriteLine($"Poco    Point2ccc: Y={pocoPoint.Y}, Name={pocoPoint.Name}, ProjectId={pocoPoint.ProjectId}");

var inpcPoint = new ccc.Point1 { Xxxxx = 2.71 };
Console.WriteLine($"INPC    Point1.Xxxxx = {inpcPoint.Xxxxx}");

var entityModel = new TestModel { Aa = "hello", Bb = "world" };
Console.WriteLine($"Entity  TestModel: Aa={entityModel.Aa}, Bb={entityModel.Bb}");
