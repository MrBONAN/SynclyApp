using Domain;
using Infrastructure.LocalServer;

namespace ClientTaskParsingTest;

public class FuncParseTests
{
    private EventArgs _resultParse;
    private readonly ClientDataParser _dataService;

    public FuncParseTests()
    {
        _dataService = new ClientDataParser();
        _dataService.AddHandler("OpenUserProfile", OnServerProfileRequest);
        _dataService.AddHandler("CreateProduct", OnServerProfileRequest);
        _dataService.AddHandler("ViewOrder", OnServerProfileRequest);
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void ReturnsExpectedResult(string clientData, bool shouldHaveResult, string expectedAction,
        Dictionary<string, string> expectedArgs, Type expectedType)
    {
        _resultParse = null;
        _dataService.HandleClientData(clientData);

        if (shouldHaveResult)
        {
            AssertParseResult(expectedAction, expectedArgs, expectedType);
        }
        else
        {
            Assert.Null(_resultParse);
        }
    }

    private void AssertParseResult(string expectedAction, Dictionary<string, string> expectedArgs, Type expectedType)
    {
        Assert.NotNull(_resultParse);

        Assert.IsType(expectedType, _resultParse);

        dynamic result = Convert.ChangeType(_resultParse, expectedType);

        Assert.Equal(expectedAction, result.Action);

        Assert.Equal(expectedArgs.Count, result.AdditionalData.Count);
        foreach (var kvp in expectedArgs)
        {
            Assert.True(result.AdditionalData.ContainsKey(kvp.Key),
                $"Key {kvp.Key} was not found in result.AdditionalData.");
            var arg = result.AdditionalData[kvp.Key];
            Assert.Equal(kvp.Value, arg);
        }
    }

    private void OnServerProfileRequest(object sender, EventArgs e)
    {
        if (e is ProfileEventArgs profileArgs)
        {
            _resultParse = profileArgs;
        }
        else if (e is OtherEventArgs otherArgs)
        {
            _resultParse = otherArgs;
        }
    }

    public static IEnumerable<object[]> TestData => new List<object[]>
    {
        new object[]
        {
            "[RUN]OpenUserProfile(id: 1, name: John)",
            true,
            "OpenUserProfile",
            new Dictionary<string, string>
            {
                { "id", "1" },
                { "name", "John" }
            },
            typeof(ProfileEventArgs)
        },
        new object[]
        {
            "[RUN]OpenUserProfile(id: 42)",
            true,
            "OpenUserProfile",
            new Dictionary<string, string>
            {
                { "id", "42" }
            },
            typeof(ProfileEventArgs)
        },
        new object[]
        {
            "AnotherFunction",
            false,
            null,
            null,
            null
        },
        new object[]
        {
            "AnotherFunction(id: 1)",
            false,
            null,
            null,
            null
        },
        new object[]
        {
            "[RUN]OpenUserProfile(id: 123, name: Alex, age: 25)",
            true,
            "OpenUserProfile",
            new Dictionary<string, string>
            {
                { "id", "123" },
                { "name", "Alex" },
                { "age", "25" }
            },
            typeof(ProfileEventArgs)
        },
        new object[]
        {
            "[RUN]OpenUserProfile()",
            true,
            "OpenUserProfile",
            new Dictionary<string, string>(),
            typeof(ProfileEventArgs)
        },
        new object[]
        {
            "[RUN]ViewOrder(orderId: 78)",
            true,
            "ViewOrder",
            new Dictionary<string, string>
            {
                { "orderId", "78" }
            },
            typeof(OtherEventArgs)
        },
        new object[]
        {
            "[RUN]ViewOrder()",
            true,
            "ViewOrder",
            new Dictionary<string, string>(),
            typeof(OtherEventArgs)
        },
        new object[]
        {
            "[RUN]",
            false,
            null,
            null,
            null
        },
        new object[]
        {
            "[RUN]InvalidFunction(param: value)",
            false,
            null,
            null,
            null
        },
        new object[]
        {
            "[RUN]OpenUserProfile(id)",
            false,
            null,
            null,
            null
        },
        new object[]
        {
            "",
            false,
            null,
            null,
            null
        },
        new object[]
        {
            "[RUN]CreateProduct(productId: 99, productName: 'Test Product', price: 10.99)",
            true,
            "CreateProduct",
            new Dictionary<string, string>
            {
                { "productId", "99" },
                { "productName", "'Test Product'" },
                { "price", "10.99" }
            },
            typeof(OtherEventArgs)
        },
        new object[]
        {
            "[RUN_]OpenUserProfile(id: 1)",
            false,
            null,
            null,
            null
        },
        new object[]
        {
            "[RUN]OpenUserProfile(id: 15 name: Jane)",
            false,
            null,
            null,
            null
        }
    };
}