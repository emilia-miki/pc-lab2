class Test
{
    private readonly string[] excludedMethods = 
        { "ToString", "Equals", "GetType", "GetHashCode", "SetUp", "TearDown" };

    private List<ITest> tests = new List<ITest>();

    public void Register(ITest test)
    {
        tests.Add(test);
    }

    public void Run()
    {
        var results = new Dictionary<string, Dictionary<string, bool>>();
        Debug.WriteLine("Running tests");
        foreach (var test in tests)
        {
            var testResult = new Dictionary<string, bool>();

            Debug.WriteLine($"Running {test.GetType().Name}");
            var methods = test.GetType().GetMethods().Where(
                method => !excludedMethods.Contains(method.Name));

            foreach (var method in methods)
            {
                var success = true;

                Debug.WriteLine($"    Running method {method.Name}");
                try
                {
                    test.SetUp();
                    method.Invoke(test, null);
                    test.TearDown();
                }
                catch
                {
                    success = false;
                }

                testResult.Add(method.Name, success);
            }

            results.Add(test.GetType().Name, testResult);
        }

        Console.WriteLine("Test results:");
        foreach (var result in results)
        {
            var passed = result.Value.Values.Where(success => success == true).Count();
            var total = result.Value.Values.Count();

            Console.WriteLine($"{result.Key}: {passed} tests passed out of {total} total");
            foreach (var pair in result.Value)
            {
                var successStr = pair.Value ? "Passed" : "Failed";
                Console.WriteLine($"    {pair.Key} - {successStr}");
            }
        }
    }
}
