namespace StackAsyncAwaitComparison
{
    internal class Program
    {
static async Task Main(string[] args)
{
    try
    {
        await GetData();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}

static Task GetData()
{
            Console.WriteLine("GetData()....");

            return GetOtherData();
}

static async Task GetOtherData()
{
    await Task.Delay(1000);

    Console.WriteLine("GetOtherData()....");

    throw new InvalidOperationException("The exception");
}
    }
}
