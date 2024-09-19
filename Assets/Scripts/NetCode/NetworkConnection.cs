using System.Threading.Tasks;
using Nakama;
using UnityEngine;

public class NetworkConnection : MonoBehaviour
{
    public IClient _client {get; private set;}
    public ISession _session {get; private set;}
    public string _deviceId {get; private set;}
    public ISocket _socket {get; private set;} 
    
    private const string ServerKey = "defaultkey"; 
    private const string Host = "localhost"; 
    private const int Port = 7350;

    public async Task ConnectAsync()
    {
        
        _client = new Client("http", Host, Port, ServerKey, UnityWebRequestAdapter.Instance);

        _deviceId = SystemInfo.deviceUniqueIdentifier;

        _session = await _client.AuthenticateDeviceAsync(_deviceId);
        _socket = _client.NewSocket(true);
        await _socket.ConnectAsync(_session, true);
    }
}
