using System;

public class Server
{
    private string ip;
     public string Ip { get
        {
            return ip;
        }
        set { ip = value; } }

	public Server(String ip)
	{
        this.ip = ip;
    }
}
