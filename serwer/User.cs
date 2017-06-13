using System;

public class User
{

    private string username;
    public string userName
    {
        get
        {
            return username;
        }
        set { username = value; }
    }

    public User(string username)
    {
        this.username = username;

    }

 

}
