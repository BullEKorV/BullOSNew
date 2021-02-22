using System;
using System.Collections.Generic;
using System.IO;
public class TextFile
{
    public string name;
    public string content;
    public TextFile(string name, string content)
    {
        this.name = name;
        this.content = content;
    }
}
public class TextCode
{
    static public List<TextFile> textFiles = new List<TextFile>();
    public static void TextFileConditions(string command, int currentUser)
    {
        command = command.Substring(4);
        command = Program.RemoveWhitespace(command);
        var textFileCheck = textFiles.Find(x => x.name.ToLower() == command.ToLower());
        if (textFileCheck != null)
        {
            string newTextContent = EditTextFile(textFileCheck.name, textFileCheck.content);
            if (newTextContent != textFileCheck.content)
            {
                SaveTextFile(newTextContent, textFileCheck.name, currentUser);
            }
        }
        else
        {
            if (textFiles.Count > 0)
            {
                Console.Write("All textfiles: ");
                foreach (var textFile in textFiles)
                {
                    Console.Write(textFile.name + ", ");
                }
                Console.WriteLine();
                command = Console.ReadLine();
                textFileCheck = textFiles.Find(x => x.name.ToLower() == command.ToLower());
                while (textFileCheck == null && command.ToLower() != "new" && command.ToLower() != "delete" && !command.Contains("ยง"))
                {
                    Console.WriteLine("There's no textfile with that name. You can create a new one with \"new\"");
                    command = Console.ReadLine();
                    textFileCheck = textFiles.Find(x => x.name.ToLower() == command.ToLower());
                }
                if (textFileCheck != null)
                {
                    string newTextContent = EditTextFile(textFileCheck.name, textFileCheck.content);
                    if (newTextContent != textFileCheck.content)
                    {
                        SaveTextFile(newTextContent, textFileCheck.name, currentUser);
                    }
                }
                else if (command.ToLower() == "new")
                {
                    CreateTextFile(currentUser);
                }
                else if (command.ToLower() == "delete")
                {
                    Console.WriteLine("Enter file to delete");
                    command = Console.ReadLine();
                    DeleteTextFile(command, currentUser);
                }
            }
            else CreateTextFile(currentUser);
        }
    }
    static void CreateTextFile(int currentUser)
    {
        Console.WriteLine("Choose a name");
        string name = Console.ReadLine();
        var textFileCheck = textFiles.Find(x => x.name.ToLower() == name.ToLower());
        Console.WriteLine(textFileCheck);
        while (!name.Contains("§") && name.Length < 3 || name.Length > 20 || textFileCheck != null)
        {
            if (textFileCheck != null) Console.WriteLine("There's already a file with that name");
            else Console.WriteLine("Name must be longer than 3 and shorter than 20 characters");

            name = Console.ReadLine();
            textFileCheck = textFiles.Find(x => x.name.ToLower() == name.ToLower());
        }
        if (!name.Contains("§"))
        {
            //add textfile and directory
            textFiles.Add(new TextFile(name, ""));
            string[] lines = { name, "" };
            string fileName = $@"dataStorage/" + currentUser + "/" + name + ".txt";
            File.Create(fileName).Dispose();
            System.IO.File.WriteAllLines(fileName, lines);
            Console.WriteLine("File succesfully created");
        }
    }
    static string EditTextFile(string textFileName, string textFileContent)
    {
        Console.Clear();

        ConsoleKeyInfo info;
        List<char> chars = new List<char>();
        if (string.IsNullOrEmpty(textFileContent) == false)
        {
            chars.AddRange(textFileContent.ToCharArray());
        }
        int pos = chars.Count;

        TextFileRender(textFileName, chars);
        PlaceCursor(pos);
        while (true)
        {
            info = Console.ReadKey(true);
            if (info.Key == ConsoleKey.Backspace && pos > 0)
            {
                chars.RemoveAt(pos - 1);
                pos--;
                TextFileRender(textFileName, chars);
            }
            else if (info.Key == ConsoleKey.LeftArrow && pos > 0)
            {
                pos -= 1;
            }
            else if (info.Key == ConsoleKey.RightArrow && pos < chars.Count)
            {
                pos += 1;
            }
            else if (info.Key == ConsoleKey.UpArrow)
            {
                if (pos - Console.WindowWidth > 0) pos -= Console.WindowWidth;
                else pos = 0;
            }
            else if (info.Key == ConsoleKey.DownArrow)
            {
                if (pos + Console.WindowWidth < chars.Count + 1) pos += Console.WindowWidth;
                else pos = chars.Count;
            }
            else if (char.IsLetterOrDigit(info.KeyChar) || char.IsWhiteSpace(info.KeyChar) || char.IsSymbol(info.KeyChar) || char.IsPunctuation(info.KeyChar))
            {
                //Save or discard changes
                if (info.Key == ConsoleKey.Enter)
                {
                    PlaceCursor(chars.Count + Console.WindowWidth);
                    Console.CursorLeft = 0;
                    Console.WriteLine("Do you want to save your document?");
                    string input = Console.ReadLine().ToLower();
                    while (input != "yes" && input != "no")
                    {
                        Console.WriteLine("Yes or no?");
                        input = Console.ReadLine().ToLower();
                    }
                    if (input == "yes")
                    {
                        textFileContent = null;
                        for (int i = 0; i < chars.Count; i++)
                        {
                            textFileContent = textFileContent + chars[i];
                        }
                    }
                    return textFileContent;
                }
                else
                {
                    chars.Insert(pos, info.KeyChar);
                    pos++;
                }
                TextFileRender(textFileName, chars);
            }
            PlaceCursor(pos);
        }
    }
    static void PlaceCursor(int pos)
    {
        int posY = Convert.ToInt32(Math.Floor((double)pos / Console.WindowWidth));
        int posX = pos - posY * Console.WindowWidth;

        Console.CursorLeft = posX;
        Console.CursorTop = posY + 2;
    }
    static void TextFileRender(string textFileName, List<char> chars)
    {
        Console.CursorLeft = 0;
        Console.CursorTop = 0;
        int xOffset = (Console.WindowWidth / 2) - (textFileName.Length / 2);
        for (int i = 0; i < xOffset; i++)
        {
            Console.Write(" ");
        }
        Console.WriteLine(textFileName + ":\n");
        for (int i = 0; i < chars.Count; i++)
        {
            Console.Write(chars[i]);
        }
        Console.Write(" ");
    }
    static void SaveTextFile(string content, string name, int currentUser)
    {
        Program.LineChanger(content, "dataStorage/" + currentUser + "/" + name + ".txt", 2);
        var textFileFind = textFiles.Find(x => x.name == name);
        textFileFind.content = content;
        Console.WriteLine("Textfile succesfully changed");
    }
    static void DeleteTextFile(string name, int currentUser)
    {
        var textFileFind = textFiles.Find(x => x.name.ToLower() == name.ToLower());
        if (textFileFind != null)
        {
            textFiles.Remove(textFileFind);
            File.Delete($@"dataStorage/" + currentUser + "/" + textFileFind.name + ".txt");
            Console.WriteLine("Textfile succesfully deleted");
        }
        else Console.WriteLine("There's no textfile with that name");
    }
    public static void LoadTextFiles(int currentUser)
    {
        textFiles.Clear();
        string fileName = $@"dataStorage/" + currentUser;
        System.IO.DirectoryInfo di = new DirectoryInfo(fileName);
        foreach (FileInfo file in di.GetFiles())
        {
            string[] lines = System.IO.File.ReadAllLines(@"" + file);
            textFiles.Add(new TextFile(lines[0], lines[1]));
        }
    }
}