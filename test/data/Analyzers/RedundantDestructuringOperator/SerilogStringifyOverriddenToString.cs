using Serilog;

namespace ConsoleApp
{
    public class User
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public static class Program
    {
        public static void Main(User user)
        {
            Log.Logger.Information("User {$User} logged in", user);
        }
    }
}
