# zRPC.NET 

A minimal RPC implementation for C# projects.

Key characteristics:
- 0MQ based communication.
- JSON based data serialization
- Using dynamic proxies to generate client-to-service bindings.
- Emphasis on simplicity over performance.

# Example usage:
## Server implementation

Create interface and implementation for your RPC service.
```
public interface IMathService
{
    int AddNumbers(int a, int b);
}

public class MathService : IMathService
{
    public int AddNumbers(int a, int b)
    {
        return a + b;
    }
}
```

Start the server and register a service.
```
using (var server = new ZRpcServer("localhost", 5556))
{
    server.RegisterService<IMathService, MathService>(new MathService());

    Console.WriteLine("Service is running... Press any key to exit.");
    Console.ReadKey();
}
```

## Client implementation

```
using (var client = new ZRpcClient("localhost", 5556))
{
    var service = client.GetService<IMathService>();

    var added = service.AddNumbers(5, 10);

    Console.WriteLine(added); // 15
}
```

**That is it!**
--- 