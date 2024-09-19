using CoinReleaseMonitor;

//var args = Environment.GetCommandLineArgs();
if (args.Length == 0)
{
    Console.WriteLine("Must Specify Discord Webhook Url");
}
else
{
    Console.WriteLine("Using Discord Webhook Url: " + args[0]);
    var startTimeSpan = TimeSpan.Zero;
    var periodTimeSpan = TimeSpan.FromMinutes(15);
    Processor.Process(args[0]);
    var timer = new System.Threading.Timer((e) =>
    {
        Processor.Process(args[0]);
    }, null, startTimeSpan, periodTimeSpan);
}
Console.ReadLine();
