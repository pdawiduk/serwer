using System;

public class User
{

    private string userName = "";
	public User(string userName)
	{
        this.userName = userName;
	}

    public string getUserName()
    {
        return this.userName;
    }
}
