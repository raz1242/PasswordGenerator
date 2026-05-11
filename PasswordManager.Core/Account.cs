namespace PasswordManager.Core; 
public class Account(string username, string password) {
    public int Id { get; set; } = -1;
    public string Title { get; set; } = "New Account";
    public string Site { get; set; }
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
    public string Notes { get; set; }


    public Account(string site, string username, string password)
        : this(username, password) {
        Site = site;
    }

    public Account(string title, string site, string username, string password)
        : this(site, username, password) {
        Title = title;
    }

    public override string ToString() {
        return Title;
    }
}