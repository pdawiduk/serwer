using System;

public class Server
{
    private string ip;
    private int port;
    private Boolean choosed = false;
    public string Ip
    {
        get
        {
            return ip;
        }
        set { ip = value; }
    }

    public Server(String ip, int port, bool choosed) {
        this.ip = ip;
        this.port = port;
        this.choosed = choosed;
    }
    public int Port
    {
        get
        {
            return port;
        }
        set
        {
            port = value;
        }
    }

    public Boolean Choosed{
    get { return choosed; }
    set { this.choosed = value; }
    }


}
