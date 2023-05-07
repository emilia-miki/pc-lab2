class Test
{
    // TODO: maybe don't use reflection somehow
    private readonly string[] excludedMethods = 
        { "ToString", "Equals", "GetType", "GetHashCode", "SetUp", "TearDown" };

    private List<ITest> tests = new List<ITest>();

    public void Register(ITest test)
    {
        tests.Add(test);
        Console.WriteLine($"Test {test.GetType().Name} registered");
    }

    // TODO: Collect success rate data and display as a table in the end
    public void Run()
    {
        Console.WriteLine("Running tests");
        foreach (var test in tests)
        {
            Console.WriteLine($"Running {test.GetType().Name}");
            var methods = test.GetType().GetMethods().Where(
                method => !excludedMethods.Contains(method.Name));

            foreach (var method in methods)
            {
                Console.WriteLine($"    Running method {method.Name}");
                test.SetUp();
                method.Invoke(test, null);
                test.TearDown();
            }
        }
    }
}
