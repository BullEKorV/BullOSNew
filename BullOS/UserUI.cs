using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
public class UserUI
{
    static string[] commands = { "help", "logout", "accounts", "text", "changepassword", "changeusername" };
    static string[] games = { "31" };
    public static void StartScreen(int currentUser)
    {
        bool loggedIn = true;
        var ThisUser = Program.accounts.Find(x => x.userID == currentUser);
        TextCode.LoadTextFiles(ThisUser.userID);
        Console.WriteLine($"Welcome {ThisUser.username} \nType help for all commands");
        while (loggedIn)
        {
            string command = Console.ReadLine();
            if (command == "logout") loggedIn = false;

            else if (command.Contains("accounts"))
            {
                command = command.Substring(8);
                command = Program.RemoveWhitespace(command);
                AllAccounts(command);
            }
            else if (command.ToLower().Contains("delete"))
            {
                command = command.Substring(6);
                command = Program.RemoveWhitespace(command);
                if (command.Length < 3)
                {
                    Console.WriteLine("Choose an account to delete");
                    AllAccounts(command);
                    command = Console.ReadLine();
                }
                Program.DeleteAccount(command);
            }
            else if (command.ToLower().Contains("changeusername"))
            {
                command = command.Substring(14);
                command = Program.RemoveWhitespace(command);
                if (command.Length < 2)
                {
                    Console.WriteLine("Choose a new username");
                    command = Console.ReadLine();
                }
                if (!command.Contains("§"))
                {
                    Program.ChangeUsername(command, ThisUser.userID);
                }
            }
            else if (command.ToLower().Contains("changepassword"))
            {
                command = command.Substring(14);
                command = Program.RemoveWhitespace(command);
                if (command.Length < 2)
                {
                    Console.WriteLine("Choose a new password");
                    command = Console.ReadLine();
                }
                if (!command.Contains("§"))
                {
                    Program.ChangePassword(command, ThisUser.userID);
                }
            }
            //text code
            else if (command.ToLower().Contains("text"))
            {
                TextCode.TextFileConditions(command, currentUser);
            }
            //game code
            else if (command.Contains("game"))
            {
                Console.Write("All available games: ");
                foreach (var game in games)
                {
                    Console.Write(game + ", ");
                }
                Console.WriteLine();
                command = Console.ReadLine();
                if (command.Contains("31"))
                {
                    Game31.Game();
                    Console.Clear();
                }
            }
            else if (command.Contains("help"))
            {
                Console.Write("All available commands: ");
                foreach (var com in commands)
                {
                    Console.Write(com + ", ");
                }
                Console.WriteLine();
            }
            else Console.WriteLine("Invalid command");
        }
    }
    static void AllAccounts(string modifier)
    {
        if (modifier == "id") Console.Write("All account IDs: ");
        else if (modifier == "admin") Console.Write("All admin accounts: ");
        else Console.Write("All accounts: ");
        foreach (var user in Program.accounts)
        {
            if (modifier == "id") Console.Write(user.userID + ", ");
            else if (modifier == "admin")
            {
                if (user.isAdmin) Console.Write(user.username + ", ");
            }
            else Console.Write(user.username + ", ");
        }
        Console.WriteLine();
    }
}