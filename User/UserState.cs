
namespace ShelfSpace;

public class UserState {
    public User CurrentUser { get; private set; }
    public string AuthToken { get; private set; }

    public void SetUser(User user, string token)
    {
        CurrentUser = user;
        AuthToken = token;
    }

    public void ClearState()
    {
        CurrentUser = null;
        AuthToken = null;
    }
     
    
    }


