using CoinReleaseMonitor;

if (args.Length == 0)
{
    Console.WriteLine("Must Specify Discord Webhook Url");
}
else
{
    while (true)
    {
        Processor.Process(args[0]);
        Thread.Sleep(TimeSpan.FromMinutes(15));
    }
}
Console.ReadLine();
