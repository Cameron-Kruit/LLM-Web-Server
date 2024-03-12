namespace Yriclium.LlmApi.Services;

public class ConnectionStoreService {
    private int connections;

    public int  Connections  () => connections;
    public void AddConnection() => connections++;
    public void RemoveConnection() => connections++;
}