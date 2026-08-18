using EndlessDungeon.Input;
using EndlessDungeon.Rendering;

namespace EndlessDungeon.UI;

public class GlyphTestScreen
{
    private readonly ConsoleRenderer _renderer;
    private readonly InputManager _inputManager;

    public GlyphTestScreen(ConsoleRenderer renderer, InputManager inputManager)
    {
        _renderer = renderer;
        _inputManager = inputManager;
    }

    public void Show()
    {
        bool isViewing = true;

        while (isViewing)
        {
            _renderer.Clear();
            _renderer.WriteTitle("GLYPH TEST");

            Console.WriteLine();
            Console.WriteLine("Symbols should remain one terminal cell wide.");
            Console.WriteLine("The | characters should line up vertically.");
            Console.WriteLine();

            WriteGlyph("Explorer", "₽", "U+20BD", ConsoleColor.Cyan);
            WriteGlyph("Slime", "●", "U+25CF", ConsoleColor.Green);

            Console.WriteLine();

            WriteGlyph("Potion", "¡", "U+00A1", ConsoleColor.Magenta);
            WriteGlyph("Weapon", "†", "U+2020", ConsoleColor.White);
            WriteGlyph("Armor", "◈", "U+25C8", ConsoleColor.DarkYellow);
            WriteGlyph("Closed Chest", "▣", "U+25A3", ConsoleColor.DarkYellow);
            WriteGlyph("Opened Chest", "□", "U+25A1", ConsoleColor.DarkYellow);

            Console.WriteLine();

            WriteGlyph("Stairs Up", "▲", "U+25B2", ConsoleColor.White);
            WriteGlyph("Stairs Down", "▼", "U+25BC", ConsoleColor.White);
            WriteGlyph("Exit Portal", "֍", "U+058D", ConsoleColor.Cyan);

            Console.WriteLine();
            Console.WriteLine("Candidate Explorer / Job Glyphs");
            Console.WriteLine();

            WriteGlyph("Fighter", "Ϯ", "U+03EE", ConsoleColor.Red);
            WriteGlyph("Rogue", "⚿", "U+26BF", ConsoleColor.DarkYellow);
            WriteGlyph("Ranger", "⌖", "U+2316", ConsoleColor.Green);
            WriteGlyph("Mage", "✦", "U+2726", ConsoleColor.Magenta);
            WriteGlyph("Scout", "♠", "U+2660", ConsoleColor.DarkGreen);

            Console.WriteLine();
            Console.WriteLine("Additional Fighter Candidates");
            Console.WriteLine();

            WriteGlyph("Fighter A", "‡", "U+2021", ConsoleColor.Red);
            WriteGlyph("Fighter B", "╬", "U+256C", ConsoleColor.Red);
            WriteGlyph("Fighter C", "Ϯ", "U+03EE", ConsoleColor.Red);
            WriteGlyph("Fighter D", "Ӿ", "U+04FE", ConsoleColor.Red);

            Console.WriteLine();
            Console.WriteLine("Candidate Creature / Item Glyphs");
            Console.WriteLine();

            WriteGlyph("Candidate", "♣", "U+2663", ConsoleColor.Green);
            WriteGlyph("Candidate", "¤", "U+00A4", ConsoleColor.Yellow);
            WriteGlyph("Candidate", "◆", "U+25C6", ConsoleColor.Cyan);
            WriteGlyph("Candidate", "Ψ", "U+03A8", ConsoleColor.Red);
            WriteGlyph("Candidate", "Ѻ", "U+047A", ConsoleColor.DarkMagenta);
            WriteGlyph("Candidate", "Ӝ", "U+04DC", ConsoleColor.DarkYellow);

            Console.WriteLine();
            Console.WriteLine("Wall Ends");
            Console.WriteLine();

            WriteGlyph("Wall End Up", "╵", "U+2575", ConsoleColor.Gray);
            WriteGlyph("Wall End Down", "╷", "U+2577", ConsoleColor.Gray);
            WriteGlyph("Wall End Right", "╶", "U+2576", ConsoleColor.Gray);
            WriteGlyph("Wall End Left", "╴", "U+2574", ConsoleColor.Gray);

            Console.WriteLine();
            Console.WriteLine("T / Escape - Return");

            ConsoleKey key = _inputManager.ReadKey();

            if (key == ConsoleKey.T || key == ConsoleKey.Escape)
            {
                isViewing = false;
            }
        }

        _renderer.Clear();
    }

    private void WriteGlyph(string label, string glyph, string codePoint, ConsoleColor color)
    {
        Console.Write($"{label,-18} ");

        Console.ForegroundColor = color;
        Console.Write(glyph);
        Console.ResetColor();

        Console.WriteLine($"|  {codePoint}");
    }
}