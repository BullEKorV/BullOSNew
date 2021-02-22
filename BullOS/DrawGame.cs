using System;
using System.IO;
using System.Linq;
using Raylib_cs;
using System.Collections.Generic;

public class DrawGame
{
    static Rectangle p1;
    static int currentUser;
    static List<Obstacle> obstacles = new List<Obstacle>();
    static List<string> levels = new List<string>();
    static string currentLevel;
    static bool touchingGround;
    static bool jumping;
    static bool drawing = false;
    static float startingX;
    static float startingY;
    static int timer;
    static float x;
    static int yOffset = 0;
    static float xVelocity;
    static float yVelocity;
    static State gameState;
    static Type buildType;
    static bool allowBuild;
    static int sinceLastClick;
    public static void DrawGameInit(int tempCurrentUser)
    {
        Raylib.SetTargetFPS(120);
        Raylib.InitWindow(1900, 1000, "DrawGame");
        bool gameActive = true;

        currentUser = tempCurrentUser;
        try
        {
            System.IO.Directory.CreateDirectory(@"dataStorage/" + currentUser + "/" + "DrawGameStorage");
        }
        catch (SystemException)
        {
            throw;
        }
        FindLevels();
        gameState = State.Menu;
        p1 = new Rectangle(Raylib.GetScreenWidth() / 4, (Raylib.GetScreenHeight() / 4) * 3, Raylib.GetScreenHeight() / 29, Raylib.GetScreenHeight() / 18);
        while (gameActive)
        {
            Raylib.SetExitKey(0);
            if (gameState == State.Game || gameState == State.Create)
            {
                CheckKeyPresses();
                UpdatePos();
                if (gameState == State.Game)
                {
                    CheckCollision();
                    timer++;
                }
            }
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.WHITE);
            if (gameState == State.Game || gameState == State.Create || gameState == State.GamePause || gameState == State.CreatePause || gameState == State.Goal)
            {
                RenderScene();
                StatusBar();
            }
            if (gameState != State.Game && gameState != State.Create && gameState != State.Goal) Menu(ref gameActive);
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }
    static void MenuInstructions()
    {
        Raylib.DrawRectangle(Raylib.GetScreenWidth() / 8 - 30, Raylib.GetScreenHeight() - 170, 1580, 220, Color.GRAY);
        Raylib.DrawText("Welcome to draw thingy", Raylib.GetScreenWidth() / 3, 20, 60, Color.BLACK);
        Raylib.DrawText("You can create a new level or open an existing one by clicking a button", Raylib.GetScreenWidth() / 8, Raylib.GetScreenHeight() - 155, 40, Color.BLACK);
        Raylib.DrawText("Green obstacles can be placed down in both create mode and play mode", Raylib.GetScreenWidth() / 8, Raylib.GetScreenHeight() - 120, 40, Color.BLACK);
        Raylib.DrawText("Red obstacles kills the player while in play mode, Orange is the goal", Raylib.GetScreenWidth() / 8, Raylib.GetScreenHeight() - 85, 40, Color.BLACK);
        Raylib.DrawText("Once you're done with your level you can play it by saving it (IMPORTANT) and then accesing it from the levels menu", Raylib.GetScreenWidth() / 8, Raylib.GetScreenHeight() - 42, 26, Color.BLACK);
    }
    static void Menu(ref bool gameActive)
    {
        List<Obstacle> buttons = new List<Obstacle>();
        buttons.AddRange(MenuButtons(buttons));
        //Draw background stripes
        for (int i = 0; i < Raylib.GetScreenHeight() / 29; i++)
        {
            Raylib.DrawRectangle(0, i * 30, Raylib.GetScreenWidth(), 15, Color.LIGHTGRAY);
        }

        sinceLastClick++;
        if (gameState == State.LevelSelect)
        {
            Raylib.DrawText("Use the down and up arrows to navigate", Raylib.GetScreenWidth() / 3 - 100, 5, 40, Color.DARKGRAY);
            if (Raylib.IsKeyDown(KeyboardKey.KEY_UP)) yOffset -= 4;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN)) yOffset += 4;
            if (yOffset < 0) yOffset = 0;
        }
        else yOffset = 0;

        drawing = false;

        if (gameState == State.Menu) MenuInstructions();
        for (int i = 0; i < buttons.Count; i++)
        {
            Obstacle button = buttons[i];
            Raylib.DrawRectangle((int)button.x, (int)button.y - yOffset, (int)button.width, (int)button.height, Color.DARKGRAY);
            Raylib.DrawRectangle((int)button.x + 10, (int)button.y + 10 - yOffset, (int)button.width - 20, (int)button.height - 20, Color.GRAY);

            if (button.type != Type.Remove) Raylib.DrawText(button.data, (int)(button.x + button.width / 10), (int)(button.y + button.height / 4) - yOffset, 80, Color.DARKGRAY);
            else Raylib.DrawText(button.type.ToString(), (int)(button.x + button.width / 10), (int)(button.y + button.height / 4) - yOffset, 80, Color.DARKGRAY);

            //button logic
            if (Raylib.GetMouseX() > button.x && Raylib.GetMouseX() < button.x + button.width && Raylib.GetMouseY() > button.y - yOffset && Raylib.GetMouseY() < button.y + button.height - yOffset)
            {
                Raylib.DrawRectangle((int)button.x + 10, (int)button.y + 10 - yOffset, (int)button.width - 20, (int)button.height - 20, Color.LIGHTGRAY);

                if (button.type != Type.Remove) Raylib.DrawText(button.data, (int)(button.x + button.width / 10), (int)(button.y + button.height / 4) - yOffset, 80, Color.DARKGRAY);
                else Raylib.DrawText(button.type.ToString(), (int)(button.x + button.width / 10), (int)(button.y + button.height / 4) - yOffset, 80, Color.DARKGRAY);

                if (!levels.Contains(currentLevel) && button.type == Type.Save)
                {
                    Raylib.DrawText("Open console application and enter name", (int)(button.x + 20), (int)button.y + ((int)button.height / 8) * 6 - yOffset, 22, Color.DARKGRAY);
                }
                if (!levels.Any() && button.type == Type.Levels)
                {
                    Raylib.DrawText("You have no saved levels", (int)(button.x + 20), (int)button.y + ((int)button.height / 8) * 6 - yOffset, 22, Color.DARKGRAY);
                }

                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) && sinceLastClick > 3)
                {
                    sinceLastClick = 0;
                    if (button.type == Type.New)
                    {
                        allowBuild = true;
                        currentLevel = "";
                        obstacles.Clear();
                        x = 0;
                        gameState = State.Create;
                        buildType = Type.Static;
                    }
                    else if (button.type == Type.Levels && levels.Any())
                    {
                        gameState = State.LevelSelect;
                    }
                    else if (button.type == Type.Exit) gameActive = false;
                    else if (button.type == Type.Resume)
                    {
                        if (gameState == State.CreatePause) gameState = State.Create;
                        else if (gameState == State.GamePause) gameState = State.Game;
                    }
                    else if (button.type == Type.Save) SaveStage();
                    else if (button.type == Type.Restart)
                    {
                        gameState = State.Game;
                        LoadStage();
                    }
                    else if (button.type == Type.Menu)
                    {
                        gameState = State.Menu;
                    }
                    else if (button.type == Type.Play)
                    {
                        gameState = State.Game;
                        buildType = Type.Editable;
                        LoadStage();
                    }
                    else if (button.type == Type.Edit)
                    {
                        gameState = State.Create;
                        buildType = Type.Static;
                        LoadStage();
                    }
                    else if (button.type == Type.Back)
                    {
                        if (gameState == State.LevelSelect) gameState = State.Menu;
                        else if (gameState == State.GamemodeChoose) gameState = State.LevelSelect;
                    }
                    else if (button.type == Type.LevelName)
                    {
                        currentLevel = button.data;
                        gameState = State.GamemodeChoose;
                    }
                    else if (button.type == Type.Remove)
                    {
                        string targetFile = button.data + ".txt";
                        try
                        {
                            File.Delete(@"dataStorage/" + currentUser + "/" + "DrawGameStorage/" + targetFile);
                        }
                        catch (SystemException)
                        {
                            throw;
                        }
                        FindLevels();
                    }
                }
            }
            if (Raylib.WindowShouldClose()) break;
        }
    }
    static List<Obstacle> MenuButtons(List<Obstacle> buttons)
    {
        buttons.Clear();
        if (gameState == State.Menu)
        {
            buttons.Add(new Obstacle(Raylib.GetScreenWidth() / 2 - 250, 130, 500, 140, Type.New, "New level"));
            buttons.Add(new Obstacle(Raylib.GetScreenWidth() / 2 - 250, 350, 500, 140, Type.Levels, "All Levels"));
            buttons.Add(new Obstacle(Raylib.GetScreenWidth() / 2 - 250, 570, 500, 140, Type.Exit, "Exit Game"));
        }
        else if (gameState == State.GamePause || gameState == State.CreatePause)
        {
            buttons.Add(new Obstacle(Raylib.GetScreenWidth() / 2 - 250, 130, 500, 140, Type.Resume, "Resume"));
            buttons.Add(new Obstacle(Raylib.GetScreenWidth() / 2 - 250, 570, 500, 140, Type.Menu, "Menu"));
        }
        else if (gameState == State.LevelSelect)
        {
            string[] files = Directory.GetFiles(@"dataStorage/" + currentUser + "/" + "DrawGameStorage/");
            for (int i = 0; i < files.Length; i++)
            {
                string buttonName = ConvertName(files[i]);
                buttons.Add(new Obstacle(Raylib.GetScreenWidth() / 2 - 250, 50 + i * 180, 500, 125, Type.LevelName, buttonName));
                buttons.Add(new Obstacle(Raylib.GetScreenWidth() / 2 - 250 + 60 + 500, 50 + i * 180, 500, 125, Type.Remove, buttonName));
            }
        }
        else if (gameState == State.GamemodeChoose)
        {
            buttons.Add(new Obstacle(Raylib.GetScreenWidth() / 2 - 250, 130, 500, 140, Type.Play, "Play Level"));
            buttons.Add(new Obstacle(Raylib.GetScreenWidth() / 2 - 250, 350, 500, 140, Type.Edit, "Edit Level"));
        }
        if (gameState == State.LevelSelect || gameState == State.GamemodeChoose)
        {
            buttons.Add(new Obstacle(20, 60, 270, 140, Type.Back, "Back"));

        }
        if (gameState == State.CreatePause)
        {
            buttons.Add(new Obstacle(Raylib.GetScreenWidth() / 2 - 250, 350, 500, 140, Type.Save, "Save"));
        }
        else if (gameState == State.GamePause)
        {
            buttons.Add(new Obstacle(Raylib.GetScreenWidth() / 2 - 250, 350, 500, 140, Type.Restart, "Restart"));
        }
        return buttons;
    }
    static void StatusBar()
    {
        if (allowBuild) Raylib.DrawText("Building on", 10, 10, 50, Color.DARKGRAY);
        else if (!allowBuild) Raylib.DrawText("Building off", 10, 10, 50, Color.DARKGRAY);
        if (gameState == State.Create)
        {
            Raylib.DrawText("Press (b) to toggle", 10, 60, 20, Color.DARKGRAY);
            Raylib.DrawText("Block type: " + buildType + " press (g) to change", Raylib.GetScreenWidth() / 3 - 20, 10, 40, Color.DARKGRAY);
        }
        else if (gameState == State.Game || gameState == State.Goal)
        {
            Raylib.DrawText("Time: " + timer / 120, Raylib.GetScreenWidth() / 2 - 40, 10, 60, Color.DARKGRAY);
        }
        if (gameState == State.Goal)
        {
            Raylib.DrawRectangle(200, Raylib.GetScreenHeight() / 2 - 140, Raylib.GetScreenWidth() - 400, 240, Color.DARKGRAY);
            Raylib.DrawRectangle(210, Raylib.GetScreenHeight() / 2 - 130, Raylib.GetScreenWidth() - 420, 220, Color.GRAY);
            Raylib.DrawText("You beat the course with a time of " + timer / 120 + " seconds", 235, Raylib.GetScreenHeight() / 2 - 80, 60, Color.LIGHTGRAY);
            Raylib.DrawText("Press \"Enter\" to continue...", 235, Raylib.GetScreenHeight() / 2, 50, Color.LIGHTGRAY);
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
            {
                gameState = State.Menu;
            }
        }
    }
    static void RenderScene()
    {
        for (int i = 0; i < obstacles.Count; i++)
        {
            Obstacle obstacle = obstacles[i];
            var color = Color.GRAY;
            if (obstacle.type == Type.Static) color = Color.GRAY;
            else if (obstacle.type == Type.Editable) color = Color.GREEN;
            else if (obstacle.type == Type.Goal) color = Color.ORANGE;
            else if (obstacle.type == Type.Danger) color = Color.RED;
            Raylib.DrawRectangle((int)-x + (int)obstacle.x, (int)obstacle.y, (int)obstacle.width, (int)obstacle.height, color);
        }
        Raylib.DrawRectangle((int)p1.x, (int)p1.y, (int)p1.width, (int)p1.height, Color.GOLD);
        if (drawing) DrawObstacle();
    }
    static void CheckKeyPresses()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE) && (gameState == State.Game || gameState == State.Create))
        {
            if (gameState == State.Create) gameState = State.CreatePause;
            if (gameState == State.Game) gameState = State.GamePause;
        }
        if (Raylib.IsKeyDown(KeyboardKey.KEY_W) && !Raylib.IsKeyDown(KeyboardKey.KEY_S))
        {
            if (gameState == State.Game)
            { //Jump
                if (touchingGround)
                {
                    yVelocity = 9;
                    touchingGround = false;
                    jumping = true;
                }
            }
            else if (gameState == State.Create) yVelocity += (float)0.6; //Float
        }
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_B) && gameState == State.Create) allowBuild = !allowBuild;
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_G) && gameState == State.Create)
        {
            if (buildType == Type.Editable) buildType = Type.Static;
            else if (buildType == Type.Static) buildType = Type.Danger;
            else if (buildType == Type.Danger) buildType = Type.Goal;
            else if (buildType == Type.Goal) buildType = Type.Editable;
        }
        if (Raylib.IsKeyDown(KeyboardKey.KEY_S) && !Raylib.IsKeyDown(KeyboardKey.KEY_W) && gameState == State.Create) yVelocity -= (float)0.6;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_A) && !Raylib.IsKeyDown(KeyboardKey.KEY_D)) xVelocity -= (float)0.6;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_D) && !Raylib.IsKeyDown(KeyboardKey.KEY_A)) xVelocity += (float)0.6;
        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON) && (gameState == State.Create || allowBuild))
        {
            startingX = Raylib.GetMouseX() + x;
            startingY = Raylib.GetMouseY();
            drawing = true;
        }
        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON)) DeleteObstacle();
    }
    static void DrawObstacle()
    {
        float xP1 = startingX;
        float yP1 = startingY;
        float xP2 = Raylib.GetMouseX() - startingX + x;
        float yP2 = Raylib.GetMouseY() - startingY;
        //keep width and height positive
        if (Raylib.GetMouseX() + x < startingX)
        {
            xP1 = Raylib.GetMouseX() + x;
            xP2 = startingX - xP1;
        }
        if (Raylib.GetMouseY() < startingY)
        {
            yP1 = Raylib.GetMouseY();
            yP2 = startingY - yP1;
        }
        // check if overlapping with player
        bool isOverlapping = xP1 - x < p1.x + p1.width && xP1 - x + xP2 > p1.x && yP1 + yP2 > p1.y && !(yP1 > p1.y + p1.height);
        if (isOverlapping && gameState == State.Game)
        {
            Raylib.DrawRectangle((int)xP1 - (int)x, (int)yP1, (int)xP2, (int)yP2, Color.RED);
        }
        else Raylib.DrawRectangle((int)xP1 - (int)x, (int)yP1, (int)xP2, (int)yP2, Color.BLUE);
        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
        {
            drawing = false;
            if ((!isOverlapping || gameState == State.Create) && !(yP2 < 5 || xP2 < 5))
            {
                obstacles.Add(new Obstacle((int)xP1, (int)yP1, (int)xP2, (int)yP2, buildType, ""));
            }
        }
    }
    static void DeleteObstacle()
    {
        for (int i = 0; i < obstacles.Count; i++)
        {
            Obstacle obstacle = obstacles[i];
            Rectangle r = new Rectangle(-x + obstacle.x, obstacle.y, obstacle.width, obstacle.height);
            if (Raylib.GetMouseX() > r.x && Raylib.GetMouseY() > r.y && Raylib.GetMouseX() < r.x + r.width && Raylib.GetMouseY() < r.y + r.height)
            {
                if (gameState == State.Game && obstacle.type == Type.Editable) obstacles.RemoveAt(i);
                else if (gameState == State.Create) obstacles.RemoveAt(i);
            }
        }
    }
    static void UpdatePos()
    {
        if (jumping)
        {
            if (yVelocity < 12) yVelocity += (float)0.5;
            else jumping = false;
        }
        if (!touchingGround && !jumping && gameState == State.Game) yVelocity -= (float)0.5;
        p1.y -= yVelocity;
        x += xVelocity;
        xVelocity *= (float)0.9;
        if (gameState == State.Create) yVelocity *= (float)0.9;
    }
    static void CheckCollision()
    {
        if (p1.y > Raylib.GetScreenHeight())
        {
            LoadStage();
        }
        int nonTouchingObstacles = obstacles.Count;
        for (int i = 0; i < obstacles.Count; i++)
        {
            Obstacle obstacle = obstacles[i];
            Rectangle r2 = new Rectangle(-x + obstacle.x, obstacle.y, obstacle.width, obstacle.height);
            bool isOverlapping = Raylib.CheckCollisionRecs(p1, r2);
            if (isOverlapping && obstacle.type != Type.Goal && obstacle.type != Type.Danger)
            {
                if (x + p1.x <= x + r2.x && p1.y + p1.height + yVelocity > r2.y && p1.y < r2.y + r2.height - yVelocity)
                {
                    x = r2.x + x - p1.x - p1.width;
                    xVelocity = 0;
                }
                if (p1.x + p1.width >= r2.x + r2.width && p1.y + p1.height + yVelocity > r2.y && p1.y < r2.y + r2.height - yVelocity)
                {
                    x = r2.x + x + r2.width - p1.x;
                    xVelocity = 0;
                }
                if (p1.y + p1.height + yVelocity <= r2.y && p1.x + p1.width > r2.x && p1.x < r2.x + r2.width)
                {
                    touchingGround = true;
                    jumping = false;
                    p1.y = r2.y - p1.height;
                    yVelocity = 0;
                }
                if (p1.y + yVelocity >= r2.y + r2.height && p1.x + p1.width > r2.x && p1.x < r2.x + r2.width)
                {
                    jumping = false;
                    p1.y = r2.y + r2.height;
                    yVelocity = 0;
                }
            }
            if (isOverlapping && obstacle.type == Type.Goal)
            {
                gameState = State.Goal;
            }
            if (isOverlapping && obstacle.type == Type.Danger)
            {
                LoadStage();
            }
            if (!(p1.y + p1.height == r2.y && p1.x + p1.width > r2.x && p1.x < r2.x + r2.width) && !jumping)
            {
                nonTouchingObstacles--;
            }
        }
        if (nonTouchingObstacles == 0) touchingGround = false;
    }
    static void SaveStage()
    {
        string[] files = Directory.GetFiles(@"dataStorage/" + currentUser + "/" + "DrawGameStorage");
        FindLevels();
        if (!(levels.Contains(currentLevel)))
        {
            Console.Write("\nPlease enter the name for your level here: ");
            string response = Console.ReadLine();
            while (response == "" || response.Length > 10 || levels.Contains(response.ToLower()))
            {
                Console.WriteLine("\nName cannot be longer than 10 characters or be an already existing name");
                Console.Write("Please enter the name for your level here:");
                response = Console.ReadLine();
            }
            currentLevel = response.ToLower();
            levels.Add(currentLevel);
            Console.WriteLine("Thank you! You may now return to the game");
        }
        if (!(files.Contains(currentLevel + ".txt")))
        {
            File.Create(@"dataStorage/" + currentUser + "/" + "DrawGameStorage/" + currentLevel + ".txt").Dispose();
        }
        using (var stream = File.Open(@"dataStorage/" + currentUser + "/" + "DrawGameStorage/" + currentLevel + ".txt", FileMode.Open))
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(stream))
            {
                file.WriteLine("?" + allowBuild);
                file.WriteLine("pX" + x);
                file.WriteLine("pY" + p1.y);
                for (int i = 0; i < obstacles.Count; i++)
                {
                    Obstacle obstacle = obstacles[i];
                    file.WriteLine("x" + obstacle.x);
                    file.WriteLine("y" + obstacle.y);
                    file.WriteLine("w" + obstacle.width);
                    file.WriteLine("h" + obstacle.height);
                    file.WriteLine("t" + obstacle.type);
                }
            }
        }
    }
    static void FindLevels()
    {
        levels.Clear();
        string[] files = Directory.GetFiles(@"dataStorage/" + currentUser + "/" + "DrawGameStorage");
        foreach (string line in files)
        {
            string name = line;
            name = ConvertName(name);
            levels.Add(name.ToLower());
        }
    }
    static void LoadStage()
    {
        obstacles.Clear();
        int tempX = 0;
        int tempY = 0;
        int tempWidth = 0;
        int tempHeight = 0;
        Type tempType;
        string[] lines;
        try
        {
            lines = System.IO.File.ReadAllLines(@"dataStorage/" + currentUser + "/" + "DrawGameStorage/" + currentLevel + ".txt");
        }
        catch (SystemException)
        {
            throw;
        }
        foreach (string line in lines)
        {
            if (line.StartsWith("?")) allowBuild = bool.Parse(line.Substring(1));
            else if (line.StartsWith("pX")) x = float.Parse(line.Substring(2));
            else if (line.StartsWith("pY")) p1.y = float.Parse(line.Substring(2));
            else if (line.StartsWith("x")) tempX = int.Parse(line.Substring(1));
            else if (line.StartsWith("y")) tempY = int.Parse(line.Substring(1));
            else if (line.StartsWith("w")) tempWidth = int.Parse(line.Substring(1));
            else if (line.StartsWith("h")) tempHeight = int.Parse(line.Substring(1));
            else if (line.StartsWith("t"))
            {
                Enum.TryParse(line.Substring(1), out tempType);
                obstacles.Add(new Obstacle((int)tempX, (int)tempY, (int)tempWidth, (int)tempHeight, tempType, ""));
            }
        }
        yVelocity = 0;
        xVelocity = 0;
        drawing = false;
        timer = 0;
    }
    static string ConvertName(string text)
    {
        text = text.Remove(text.Length - 4).Substring(37);
        return text;
    }
}
public class Obstacle
{
    public float x;
    public float y;
    public float width;
    public float height;
    public Type type;
    public string data;
    public Obstacle(float x, float y, float width, float height, Type type, string data)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.type = type;
        this.data = data;
    }
}
public enum State
{
    Game,
    Create,
    Menu,
    Goal,
    CreatePause,
    GamePause,
    LevelSelect,
    GamemodeChoose
}
public enum Type
{
    Editable,
    Static,
    Goal,
    Danger,
    New,
    Levels,
    LevelName,
    Resume,
    Back,
    Edit,
    Exit,
    Play,
    Restart,
    Menu,
    Save,
    Remove
}