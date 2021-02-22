using System;
using Raylib_cs;
using System.Collections.Generic;
using System.IO;
public class Game31
{
    static List<string> cardsLeft = new List<string>();
    static List<Button> buttons = new List<Button>();
    static string[] playerCards = new string[3];
    static string[] botCards = new string[3];
    static bool[] hasKnocked = { false, false };
    static string lastThrownCard;
    static string cardPickedUp;
    public static void Game()
    {
        cardsLeft.AddRange(CreateAllCards());
        Raylib.SetTargetFPS(120);
        Raylib.InitWindow(1200, 800, "31");
        bool gameActive = true;

        CreateButtons();
        ResetGame();
        while (gameActive)
        {
            Raylib.SetExitKey(0);

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.WHITE);
            RenderButtons();
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_R))
            {
                ResetGame();
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.KEY_Q))
            {
                gameActive = false;
            }
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }
    static void Knock()
    {
        hasKnocked[0] = true;
        BotPickUp();
    }
    static void TrashPickUp()
    {
        cardPickedUp = lastThrownCard;
        lastThrownCard = null;
        var buttonTypeFind = buttons.Find(x => x.type == "trash");
        buttonTypeFind.text = "Trash:\n ";
    }
    static void NewCard()
    {
        cardPickedUp = ChooseRandomCard();
    }
    static void ChangeCard(int card)
    {
        card -= 1;
        lastThrownCard = playerCards[card];
        var buttonTypeFind = buttons.Find(x => x.type == "trash");
        buttonTypeFind.text = "Trash:\n " + lastThrownCard;

        playerCards[card] = cardPickedUp;
        buttonTypeFind = buttons.Find(x => x.type == "playerCard" + (card + 1));
        buttonTypeFind.text = playerCards[card] + "\n= " + (CardValue(playerCards[card]) + "p");
        cardPickedUp = null;
        BotPickUp();
    }
    static void Throw()
    {
        lastThrownCard = cardPickedUp;
        var buttonTypeFind = buttons.Find(x => x.type == "trash");
        buttonTypeFind.text = "Trash:\n " + lastThrownCard;
        cardPickedUp = null;
        BotPickUp();
    }
    static void ActionListener(string buttonType)
    {
        if (buttonType == "new" && cardPickedUp == null) NewCard();
        if (buttonType == "throw")
        {
            if (cardPickedUp != null) Throw();
        }

        if (buttonType == "trash" && lastThrownCard != null) TrashPickUp();
        if (buttonType.Contains("playerCard") && cardPickedUp != null)
        {
            ChangeCard(int.Parse(buttonType.Substring(10)));
        }
        if (buttonType == "knock") Knock();


    }
    static void BotPickUp()
    {
        if (hasKnocked[1])
        {
            WinCondition();
        }
        else
        {
            Console.WriteLine("bot be playing");
            // Print bot cards
            int totalValue = 0;
            for (int i = 0; i < botCards.Length; i++)
            {
                totalValue += CardValue(botCards[i]);
            }
            // Knock or pick up card
            if (totalValue >= 27 && totalValue <= 31)
            {
                // Do the knocking
                hasKnocked[1] = true;
                var buttonTypeFind = buttons.Find(x => x.type == "event");
                buttonTypeFind.text = "Bot is knocking";
                if (hasKnocked[0])
                {
                    WinCondition();
                }
            }
            else
            {
                // Choose to pick card from trash or new card
                string cardToTest = null;
                if (lastThrownCard != null && (totalValue > 31 && CardValue(lastThrownCard) <= 10)
                        || (totalValue < 27 && CardValue(lastThrownCard) >= 8))
                {
                    cardToTest = lastThrownCard;
                }
                else
                {
                    cardToTest = ChooseRandomCard();
                }
                BotTestCard(cardToTest, totalValue);
            }
        }
    }

    static void BotTestCard(string cardTest, int totalValue)
    {
        string[] cardWorks = new string[3];
        string[] tempBotCards = new string[3];
        for (int i = 0; i < botCards.Length; i++)
        {
            for (int h = 0; h < botCards.Length; h++)
            {
                tempBotCards[h] = botCards[h];
            }
            if ((totalValue < 27 && CardValue(cardTest) > CardValue(tempBotCards[i]))
                    || (totalValue > 31 && CardValue(cardTest) < CardValue(tempBotCards[i])))
            {
                tempBotCards[i] = cardTest;
                if (CardValue(tempBotCards[0]) + CardValue(tempBotCards[1]) + CardValue(tempBotCards[2]) <= 31)
                {
                    cardWorks[i] = botCards[i];
                }
            }
        }
        int minPositionCard = 0;
        int min = 20;
        for (int x = 0; x < botCards.Length; x++)
        {
            if (!(cardWorks[x] == null))
            {
                if (CardValue(cardWorks[x]) <= min)
                {
                    min = CardValue(cardWorks[x]);
                    minPositionCard = x;
                }
            }
        }
        if (!(cardWorks[minPositionCard] == null))
        {
            lastThrownCard = botCards[minPositionCard];
            botCards[minPositionCard] = cardTest;
        }
        else
        {
            lastThrownCard = cardTest;
        }
        var buttonTypeFind = buttons.Find(x => x.type == "trash");
        buttonTypeFind.text = "Trash\n:" + lastThrownCard;
        if (hasKnocked[0])
        {
            WinCondition();
        }
    }
    public static void WinCondition()
    {
        var buttonTypeFind = buttons.Find(x => x.type == "playerCard" + (0));
        int botValue = 0;
        int playerValue = 0;
        // Gets value of player and bot
        for (int i = 0; i < playerCards.Length; i++)
        {
            playerValue += CardValue(playerCards[i]);
            botValue += CardValue(botCards[i]);
            buttonTypeFind = buttons.Find(x => x.type == "botCard" + (i + 1));
            buttonTypeFind.text = botCards[i];
        }
        buttonTypeFind = buttons.Find(x => x.type == "event");
        if (botValue > playerValue && botValue <= 31)
        {
            buttonTypeFind.text = "Bot won!\nPress R to restart or Q to quit";
        }
        else if (playerValue > botValue && playerValue <= 31)
        {
            buttonTypeFind.text = "Player won!\nPress R to restart or Q to quit";
        }
        else if (playerValue == botValue)
        {
            buttonTypeFind.text = "It's a draw!\nPress R to restart or Q to quit";
        }
    }
    static string ChooseRandomCard()
    {
        Random rnd = new Random();
        int cardSelect = rnd.Next(0, cardsLeft.Count);
        string selectedCard = cardsLeft[cardSelect];
        cardsLeft.RemoveAt(cardSelect);
        return selectedCard;
    }
    static int CardValue(string card)
    {
        card = card.Substring(1);
        int value = int.Parse(card);
        if (value > 10)
        {
            value = 10;
        }
        else if (value == 1)
        {
            value = 11;
        }
        return value;
    }
    static int CardsLeftInt()
    {
        int totalCardsLeft = 0;
        for (int i = 0; i < cardsLeft.Count; i++)
        {
            totalCardsLeft++;
        }
        return totalCardsLeft;
    }
    static string[] CreateAllCards()
    {
        string[] cardTypes = { "H", "C", "S", "D" };
        string[] allCards = new string[52];
        int cardNumber = 0;
        for (int i = 0; i < cardTypes.Length; i++)
        {
            for (int y = 0; y < allCards.Length / cardTypes.Length; y++)
            {
                allCards[cardNumber] = cardTypes[i] + (y + 1);
                cardNumber++;
            }
        }
        return allCards;
    }
    static void CreateButtons()
    {
        buttons.Clear();
        buttons.Add(new Button(Raylib.GetScreenWidth() - 120, (Raylib.GetScreenHeight() / 8) * 2, 110, 80, "new", "New\ncard"));
        buttons.Add(new Button(Raylib.GetScreenWidth() - 120, (Raylib.GetScreenHeight() / 8) * 3, 110, 80, "trash", ""));
        buttons.Add(new Button(Raylib.GetScreenWidth() - 120, (Raylib.GetScreenHeight() / 8) * 4, 110, 80, "throw", "Throw"));
        buttons.Add(new Button(Raylib.GetScreenWidth() - 120, (Raylib.GetScreenHeight() / 8) * 5, 110, 80, "knock", "Knock"));
        buttons.Add(new Button((Raylib.GetScreenWidth() / 8) * 1, Raylib.GetScreenHeight() - 100, 90, 80, "playerCard1", ""));
        buttons.Add(new Button((Raylib.GetScreenWidth() / 8) * 3, Raylib.GetScreenHeight() - 100, 90, 80, "playerCard2", ""));
        buttons.Add(new Button((Raylib.GetScreenWidth() / 8) * 5, Raylib.GetScreenHeight() - 100, 90, 80, "playerCard3", ""));
        buttons.Add(new Button((Raylib.GetScreenWidth() / 8) * 1, 20, 90, 80, "botCard1", "?"));
        buttons.Add(new Button((Raylib.GetScreenWidth() / 8) * 3, 20, 90, 80, "botCard2", "?"));
        buttons.Add(new Button((Raylib.GetScreenWidth() / 8) * 5, 20, 90, 80, "botCard3", "?"));

        buttons.Add(new Button(Raylib.GetScreenWidth() - 500, Raylib.GetScreenHeight() - 200, 0, 0, "event", ""));
    }
    static void RenderButtons()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Button button = buttons[i];
            Raylib.DrawRectangle(button.x, button.y, button.width, button.height, Color.GREEN);
            Raylib.DrawText(button.text, button.x, button.y, 30, Color.BLACK);
        }
        Raylib.DrawText("Card picked up: " + cardPickedUp, 500, 500, 20, Color.BLACK);
        // Raylib.DrawText("Player" + (currentPlayer + 1) + "'s turn ", 500, 300, 30, Color.BLACK);

        ClickButtons();
    }
    static void ClickButtons()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Button button = buttons[i];
            if (Raylib.GetMouseX() > button.x && Raylib.GetMouseY() > button.y && Raylib.GetMouseX() < button.x + button.width && Raylib.GetMouseY() < button.y + button.height && !button.type.Contains("botCard"))
            {
                Raylib.DrawRectangle(button.x, button.y, button.width, button.height, Color.DARKGREEN);
                Raylib.DrawText(button.text, button.x, button.y, 30, Color.BLACK);
                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
                {
                    ActionListener(button.type);
                }
            }
        }
    }
    static void ResetGame()
    {
        var buttonTypeFind = buttons.Find(x => x.type == "event");
        buttonTypeFind.text = "";
        buttonTypeFind = buttons.Find(x => x.type == "trash");
        buttonTypeFind.text = "";
        cardsLeft.AddRange(CreateAllCards());
        cardPickedUp = null;
        lastThrownCard = null;
        hasKnocked[0] = false;
        hasKnocked[1] = false;
        for (int i = 0; i < playerCards.Length; i++)
        {
            playerCards[i] = ChooseRandomCard();
            buttonTypeFind = buttons.Find(x => x.type == "playerCard" + (i + 1));
            buttonTypeFind.text = playerCards[i] + "\n= " + (CardValue(playerCards[i]) + "p");
        }
        for (int i = 0; i < botCards.Length; i++)
        {
            botCards[i] = ChooseRandomCard();
            buttonTypeFind = buttons.Find(x => x.type == "botCard" + (i + 1));
            buttonTypeFind.text = "?";
        }
    }
}
public class Button
{
    public int x;
    public int y;
    public int width;
    public int height;
    public string type;
    public string text;
    public Button(int x, int y, int width, int height, string type, string text)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.type = type;
        this.text = text;
    }
}