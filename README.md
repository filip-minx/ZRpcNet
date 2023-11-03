# ZRpcNet

A minimal RPC implementation for C# projects.

Key characteristics:
- Full duplex, brokerless, 0MQ communication.
- Dynamic generation of client-to-service bindings from C# interfaces.
- Emphasis on simplicity over performance. Zero setup needed.

# Example usage:
## Server implementation

Create an interface and implementation for your RPC service.
```
public interface IWeatherService
{
    void SetTemperature(int degrees);
  
    event EventHandler<int> TemperatureChanged;
}

public class WeatherService : IWeatherService
{
    public event EventHandler<int> TemperatureChanged;

    public void SetTemperature(int degrees)
    {
        TemperatureChanged?.Invoke(this, degrees);
    }
}
```

Start the server and register a service.
```
using (var server = new ZRpcServer("localhost"))
{
    server.RegisterService<IWeatherService, WeatherService>(new WeatherService());

    Console.WriteLine("Service is running... Press any key to exit.");
    Console.ReadKey();
}
```

## Client implementation

```
using (var client = new ZRpcClient("localhost"))
{
    var weatherService = client.GetService<IWeatherService>();

    weatherService.TemperatureChanged += (s, degrees) =>
    {
        Console.WriteLine($"Temperature changed to {degrees}°C.");
    };

    weatherService.SetTemperature(38);
}
```

**That is it!**
--- 
