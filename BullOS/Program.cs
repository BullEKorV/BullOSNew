using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class UserInfo
{
    public string username;
    public string password;
    public int userID;
    public bool isAdmin;
    public UserInfo(string username, string password, int userID, bool isAdmin)
    {
        this.username = username;
        this.password = password;
        this.userID = userID;
        this.isAdmin = isAdmin;
    }
}
public class Program
{
    static public List<UserInfo> accounts = new List<UserInfo>();
    static string[] games = { "31" };
    static void Main(string[] args)
    {
        //Console.WriteLine(Console.WindowWidth);
        try
        {
            System.IO.Directory.CreateDirectory(@"dataStorage");
        }
        catch (SystemException)
        {
            throw;
        }
        LoadAccounts();
        while (true)
        {
            Console.WriteLine("Please enter a command. Use \"§\" to go back");
            string input = Console.ReadLine();
            if (input == "create")
            {
                CreateUsername();
            }
            else if (input == "login")
            {
                LoginAccount();
            }
            else if (input == "delete")
            {
                input = Console.ReadLine();
                if (!input.Contains("§"))
                {
                    DeleteAccount(input);
                }
            }
            else if (input == "game")
            {
                DrawGame.DrawGameInit(accounts[0].userID);
            }
        }
    }
    static void CreateUsername()
    {
        Console.WriteLine("Input username");
        string username = Console.ReadLine();

        //conditions for choosing username
        var usernameCheck = accounts.Find(x => x.username.ToLower() == username.ToLower());
        while (usernameCheck != null || username.Contains(" ") || username.Length > 12 || username.Length < 3 && !username.Contains("§"))
        {
            if (usernameCheck != null) Console.WriteLine("There's already a username with that name");
            else Console.WriteLine("Username can't contain a space or be longer than 12 characters and shorter than 3");
            username = Console.ReadLine();
            usernameCheck = accounts.Find(x => x.username.ToLower() == username.ToLower());
        }
        if (!username.Contains("§"))
        {
            CreatePassword(username);
        }
    }
    static void CreatePassword(string username)
    {
        Console.WriteLine("Input password");
        string password = Console.ReadLine();
        //conditions for choosing password
        while (password.Contains(" ") || password.Length > 24 || password.Length < 3 && !password.Contains("§"))
        {
            Console.WriteLine("Password can't contain a space or be longer than 24 characters and shorter than 3");
            password = Console.ReadLine();
        }
        if (!password.Contains("§"))
        {
            CreateAccount(username, password);
        }
    }
    static void CreateAccount(string username, string password)
    {
        Random rnd = new Random();
        int userID = rnd.Next(11111111, 99999999);
        var userIDCheck = accounts.Find(x => x.userID == userID);
        while (userIDCheck != null)
        {
            userID = rnd.Next(11111111, 99999999);
            userIDCheck = accounts.Find(x => x.userID == userID);
        }

        bool isAdmin;
        if (accounts.Count == 0)
        {
            isAdmin = true;
        }
        else isAdmin = false;

        accounts.Add(new UserInfo(username, password, userID, isAdmin));

        //add textfile and directory
        string[] lines = { username, password, userID.ToString(), isAdmin.ToString() };
        string fileName = $@"dataStorage/" + userID + ".txt";
        File.Create(fileName).Dispose();
        System.IO.File.WriteAllLines(fileName, lines);
        System.IO.Directory.CreateDirectory($@"dataStorage/" + userID);
    }
    static void LoginAccount()
    {
        Console.WriteLine("All accounts: ");
        foreach (var account in accounts)
        {
            Console.Write(account.username + ", ");
        }

        string input = Console.ReadLine();
        var usernameFind = accounts.Find(x => x.username == input);
        while (usernameFind == null && !input.Contains("§"))
        {
            Console.WriteLine("No account exists with that username");
            input = Console.ReadLine();
            usernameFind = accounts.Find(x => x.username == input);
        }
        if (usernameFind != null)
        {
            Console.WriteLine("Enter password");
            input = Console.ReadLine();
            while (usernameFind.password != input && !input.Contains("§"))
            {
                Console.WriteLine("Incorrect password");
                input = Console.ReadLine();
            }
            if (usernameFind.password == input)
            {
                UserUI.StartScreen(usernameFind.userID);
            }
        }
    }
    static public void ChangeUsername(string newUsername, int currentUser)
    {
        var usernameCheck = Program.accounts.Find(x => x.username.ToLower() == newUsername.ToLower());
        if (usernameCheck == null && newUsername.Length >= 3 && !newUsername.Contains("§"))
        {
            foreach (var user in Program.accounts)
            {
                if (user.userID == currentUser)
                {
                    user.username = newUsername;
                    LineChanger(newUsername, "dataStorage/" + currentUser + ".txt", 1);
                    Console.WriteLine("Username succesfully changed");
                }
            }
        }
        else if (usernameCheck != null)
        {
            Console.WriteLine("There's already an account with that name");
        }
        else Console.WriteLine("Username must be longer than 3 characters and can't include \"§\"");
    }
    static public void ChangePassword(string newPassword, int currentUser)
    {
        if (newPassword.Length >= 3 && !newPassword.Contains("§"))
        {
            foreach (var user in Program.accounts)
            {
                if (user.userID == currentUser)
                {
                    user.password = newPassword;
                    LineChanger(newPassword, "dataStorage/" + currentUser + ".txt", 2);
                    Console.WriteLine("Password succesfully changed");
                }
            }
        }
        else
        {
            Console.WriteLine("Has to be longer than 3 characters and can't include \"§\"");
        }
    }
    static public void DeleteAccount(string userToDelete)
    {
        var userFind = Program.accounts.Find(x => x.username.ToLower() == userToDelete.ToLower());
        try
        {
            userFind = Program.accounts.Find(x => x.userID == int.Parse(userToDelete));
        }
        catch (System.Exception)
        {
        }
        if (userFind != null)
        {
            //delete textfile and directory
            string fileName = $@"dataStorage/" + userFind.userID;
            File.Delete(fileName + ".txt");
            System.IO.DirectoryInfo di = new DirectoryInfo(fileName);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            Directory.Delete(fileName);

            //delete user in class
            accounts.Remove(accounts.Find(x => x.userID == userFind.userID));
            Console.WriteLine(userToDelete + " was succesfully deleted");
        }
        else Console.WriteLine("Account does not exist");

    }
    static void LoadAccounts()
    {
        System.IO.DirectoryInfo di = new DirectoryInfo(@"dataStorage/");
        foreach (FileInfo file in di.GetFiles())
        {
            string[] lines = System.IO.File.ReadAllLines(@"" + file);
            accounts.Add(new UserInfo(lines[0], lines[1], int.Parse(lines[2]), bool.Parse(lines[3])));
        }
    }
    public static string RemoveWhitespace(string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !Char.IsWhiteSpace(c))
            .ToArray());
    }
    public static string RemoveCharacters(string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !Char.IsLetter(c))
            .ToArray());
    }
    public static void LineChanger(string newText, string fileName, int lineToEdit)
    {
        string[] arrLine = File.ReadAllLines(fileName);
        arrLine[lineToEdit - 1] = newText;
        File.WriteAllLines(fileName, arrLine);
    }
}