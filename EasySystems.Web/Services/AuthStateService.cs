namespace EasySystems.Web.Services;

public class AuthStateService
{
    public bool IsLoggedIn { get; private set; }
    public bool IsAdmin { get; private set; }

    public event Action? OnChange;

    public void SetUser(bool loggedIn, string role)
    {
        IsLoggedIn = loggedIn;
        IsAdmin = role == "Admin" || role == "SuperAdmin";

        NotifyStateChanged();
    }

    public void Logout()
    {
        IsLoggedIn = false;
        IsAdmin = false;

        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}