using Spectre.Console;

class Program
{
    static char[] board = new char[9]
    {
        '1', '2', '3',
        '4', '5', '6',
        '7', '8', '9'
    };

    static int scoreX = 0;
    static int scoreO = 0;
    static int draws = 0;

    static readonly int[][] winningCombinations =
    {
        new[] { 0, 1, 2 },
        new[] { 3, 4, 5 },
        new[] { 6, 7, 8 },

        new[] { 0, 3, 6 },
        new[] { 1, 4, 7 },
        new[] { 2, 5, 8 },

        new[] { 0, 4, 8 },
        new[] { 2, 4, 6 }
    };

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        AnsiConsole.Clear();

        AnsiConsole.Write(
            new FigletText("Tic Tac Toe")
                .Centered()
                .Color(Color.Green));

        string mode = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Оберіть режим гри:[/]")
                .AddChoices(
                    "Гравець проти Гравця",
                    "Гравець проти Комп'ютера"));

        bool vsComputer = mode == "Гравець проти Комп'ютера";

        bool playAgain = true;

        while (playAgain)
        {
            ResetBoard();

            char currentPlayer = 'X';
            char? winner = null;

            while (true)
            {
                ShowGame();

                // Хід людини
                if (currentPlayer == 'X' || !vsComputer)
                {
                    MakePlayerMove(currentPlayer);
                }
                else
                {
                    MakeComputerMove();
                }

                // Перевірка перемоги
                if (CheckWinner(currentPlayer))
                {
                    winner = currentPlayer;
                    break;
                }

                // Перевірка нічиєї
                if (IsDraw())
                {
                    break;
                }

                // Зміна гравця
                currentPlayer = currentPlayer == 'X' ? 'O' : 'X';
            }

            ShowGame();

            if (winner == 'X')
            {
                scoreX++;

                AnsiConsole.MarkupLine(
                    "\n[red bold]🎉 Переміг гравець X![/]");
            }
            else if (winner == 'O')
            {
                scoreO++;

                if (vsComputer)
                {
                    AnsiConsole.MarkupLine(
                        "\n[cyan bold]🤖 Комп'ютер переміг![/]");
                }
                else
                {
                    AnsiConsole.MarkupLine(
                        "\n[cyan bold]🎉 Переміг гравець O![/]");
                }
            }
            else
            {
                draws++;

                AnsiConsole.MarkupLine(
                    "\n[yellow bold]🤝 Нічия![/]");
            }

            ShowScore();

            playAgain = AnsiConsole.Confirm(
                "\n[yellow]Бажаєте зіграти ще раз?[/]",
                true);
        }

        AnsiConsole.Clear();

        AnsiConsole.Write(
            new FigletText("Game Over")
                .Centered()
                .Color(Color.Green));

        ShowScore();

        AnsiConsole.MarkupLine(
            "\n[grey]Дякуємо за гру![/]");
    }

    // ==========================================
    // Відображення гри
    // ==========================================

    static void ShowGame()
    {
        AnsiConsole.Clear();

        AnsiConsole.Write(
            new FigletText("Tic Tac Toe")
                .Centered()
                .Color(Color.Green));

        ShowScore();

        var table = new Table();

        table.Border = TableBorder.Rounded;

        table.AddColumn("");
        table.AddColumn("");
        table.AddColumn("");

        for (int row = 0; row < 3; row++)
        {
            string cell1 = FormatCell(board[row * 3]);
            string cell2 = FormatCell(board[row * 3 + 1]);
            string cell3 = FormatCell(board[row * 3 + 2]);

            table.AddRow(cell1, cell2, cell3);
        }

        AnsiConsole.Write(table);

        AnsiConsole.MarkupLine(
            "\n[grey]X — гравець 1 | O — гравець 2 / бот[/]");
    }

    static string FormatCell(char cell)
    {
        if (cell == 'X')
            return "[red bold]X[/]";

        if (cell == 'O')
            return "[cyan bold]O[/]";

        return $"[grey]{cell}[/]";
    }

    // ==========================================
    // Хід гравця
    // ==========================================

    static void MakePlayerMove(char player)
    {
        List<string> availableCells = new();

        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] != 'X' && board[i] != 'O')
            {
                availableCells.Add(board[i].ToString());
            }
        }

        string selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow]Гравець [{GetPlayerColor(player)}]{player}[/] — оберіть клітинку:[/]")
                .PageSize(9)
                .HighlightStyle(new Style(Color.Green))
                .AddChoices(availableCells));

        int index = int.Parse(selected) - 1;

        board[index] = player;
    }

    // ==========================================
    // Хід комп'ютера
    // ==========================================

    static void MakeComputerMove()
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start(
                "[cyan]Комп'ютер думає...[/]",
                ctx =>
                {
                    Thread.Sleep(1200);
                });

        List<int> available = new();

        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] != 'X' && board[i] != 'O')
            {
                available.Add(i);
            }
        }

        if (available.Count == 0)
            return;

        // Спочатку бот намагається перемогти
        foreach (int position in available)
        {
            board[position] = 'O';

            if (CheckWinner('O'))
            {
                return;
            }

            board[position] = (char)('1' + position);
        }

        // Потім бот блокує X
        foreach (int position in available)
        {
            board[position] = 'X';

            if (CheckWinner('X'))
            {
                board[position] = 'O';
                return;
            }

            board[position] = (char)('1' + position);
        }

        // Якщо немає термінового ходу — вибираємо випадковий
        Random random = new Random();

        int selected = available[random.Next(available.Count)];

        board[selected] = 'O';
    }

    // ==========================================
    // Перевірка перемоги
    // ==========================================

    static bool CheckWinner(char player)
    {
        foreach (int[] combination in winningCombinations)
        {
            if (board[combination[0]] == player &&
                board[combination[1]] == player &&
                board[combination[2]] == player)
            {
                return true;
            }
        }

        return false;
    }

    // ==========================================
    // Перевірка нічиєї
    // ==========================================

    static bool IsDraw()
    {
        foreach (char cell in board)
        {
            if (cell != 'X' && cell != 'O')
                return false;
        }

        return true;
    }

    // ==========================================
    // Очищення поля
    // ==========================================

    static void ResetBoard()
    {
        for (int i = 0; i < board.Length; i++)
        {
            board[i] = (char)('1' + i);
        }
    }

    // ==========================================
    // Рахунок
    // ==========================================

    static void ShowScore()
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[red bold]X[/]")
            .AddColumn("[cyan bold]O[/]")
            .AddColumn("[yellow bold]Нічиї[/]");

        table.AddRow(
            scoreX.ToString(),
            scoreO.ToString(),
            draws.ToString());

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    static string GetPlayerColor(char player)
    {
        return player == 'X' ? "red bold" : "cyan bold";
    }
}