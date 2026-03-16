namespace PasswordManager.Core; 
public class Account(string username, string password) {
    private int _id = -1;
    private string _title = "New Account";
    private string _site;
    private string _username = username;
    private string _password = password;
    private string _notes;

    public Account(string site, string username, string password)
        : this(username, password) {
        _site = site;
    }

    public Account(string title, string site, string username, string password)
        : this(site, username, password) {
        _title = title;
    }

    public int Id {
        get => _id;
        set => _id = value;
    }
    public string Site {
        get => _site;
        set => _site = value;
    }
    public string Username {
        get => _username;
        set => _username = value;
    }
    public string Password {
        get => _password;
        set => _password = value;
    }
    public string Notes {
        get => _notes;
        set => _notes = value;
    }
    public string Title {
        get => _title;
        set => _title = value;
    }

    public override string ToString() {
        return Title;
    }
}