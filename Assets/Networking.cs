using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Networking : MonoBehaviour
{
    public static Networking instance = null;
    public bool isServer = false;
    private bool hostSetup = false;
    private int hostID;
    private int connectionID;
    private int channelID;
    public string name = "#";
    private Dictionary<int, UnityEngine.UI.Slider> map = new Dictionary<int, UnityEngine.UI.Slider>();
    private bool send = true;
    private int serverHostID;
    public UnityEngine.UI.Slider clientSlider;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }

    public static string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    public void Init()
    {
        setupHost(10, 1993);
    }

    void FixedUpdate()
    {
        if (hostSetup)
        {
            int recHostId;
            int connectionId;
            int channelId;
            byte[] recBuffer = new byte[1];
            int bufferSize = 1;
            int dataSize;
            byte error;
            bool endLoop = false;

            do
            {
                endLoop = false;

                NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);

                NetworkError theError = (NetworkError)error;

                if (theError != NetworkError.Ok)
                {
                    Debug.Log("Error: " + theError.ToString());
                }

                switch (recData)
                {
                    case NetworkEventType.Nothing:
                        endLoop = true;
                        break;
                    case NetworkEventType.ConnectEvent:
                        if (isServer)
                        {
                            View.instance.onServerConnected();
                        }
                        else
                        {
                            View.instance.onClientConnected();
                        }

                        Debug.Log(name + ": Connect Event");
                        break;
                    case NetworkEventType.DataEvent:
                        Debug.Log(name + ": Data Event");
                        if (!map.ContainsKey(connectionId))
                        {
                            map.Add(connectionId, View.instance.createSlider());
                        }

                        map[connectionId].value = (recBuffer[0] / 255.0f);
                        break;
                    case NetworkEventType.DisconnectEvent:
                        Debug.Log(name + ": Disconnect Event");
                        break;
                    case NetworkEventType.BroadcastEvent:
                        Debug.Log(name + ": Broadcast Event");
                        break;
                }
            } while (!endLoop);
        }
    }

    public IEnumerator sendCoroutine()
    {
        while (send)
        {
            byte[] dataToSend = new byte[1];
            byte error;

            dataToSend[0] = (byte)(255.0f * clientSlider.value / clientSlider.maxValue);

            NetworkTransport.Send(hostID, connectionID, channelID, dataToSend, 1, out error); 

            yield return new WaitForSeconds(0.01f);
        }
    }

    void setupHost(int maxConnections, int port)
    {
        NetworkTransport.Init();

        ConnectionConfig config = new ConnectionConfig();
        channelID = config.AddChannel(QosType.Unreliable);

        HostTopology topology = new HostTopology(config, maxConnections);

        if (isServer)
        {
            hostID = NetworkTransport.AddHost(topology, port);
        }
        else
        {
            hostID = NetworkTransport.AddHost(topology, 0);
        }

        hostSetup = true;
    }

    public void connectTo(string ip, int port)
    {
        byte error;

        connectionID = NetworkTransport.Connect(hostID, ip, port, 0, out error);

    }

    public void startSending()
    {
        StopAllCoroutines();
        StartCoroutine(Networking.instance.sendCoroutine());
    }
}
