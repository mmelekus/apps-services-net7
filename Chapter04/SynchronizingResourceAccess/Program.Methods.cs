partial class Program
{
    static void MethodA()
    {
        var complete = false;
        while (!complete)
        {
            //lock(SharedObjects.Conch) // will not prevent deadlocks
            if (Monitor.TryEnter(SharedObjects.Conch, TimeSpan.FromSeconds(15))) // will prevent deadlocks
            {
                try
                {
                    for (int i = 0;  i < 5; i++)
                    {
                        Thread.Sleep(Random.Shared.Next(10000));
                        SharedObjects.Message += "A";
                        Interlocked.Increment(ref SharedObjects.Counter);
                        Write(".");
                    }
                }
                finally
                {
                    Monitor.Exit(SharedObjects.Conch);
                    complete = true;
                }
            }
            else
            {
                WriteLine("Method A timed out when entering a monitor on conch.");
            }
        }
     }

    static void MethodB()
    {
        var complete = false;
        while (!complete)
        {
            // lock(SharedObjects.Conch)
            if (Monitor.TryEnter(SharedObjects.Conch, TimeSpan.FromSeconds(15))) // will prevent deadlocks
            {
                try
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Thread.Sleep(Random.Shared.Next(2000));
                        SharedObjects.Message += "B";
                        Interlocked.Increment(ref SharedObjects.Counter);
                        Write(".");
                    }
                }
                finally
                {
                    Monitor.Exit(SharedObjects.Conch);
                    complete = true;
                }
            }
            else
            {
                WriteLine("Method B timed out when entering a monitor on conch.");
            }
        }
    }
}