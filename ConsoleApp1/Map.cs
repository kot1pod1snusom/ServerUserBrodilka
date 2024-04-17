using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Map
{
    public static int height;
    public static int width;
    public static List<Block> field = new List<Block>();
    public static void GenerateMap(int height, int wight)
    {
        Map.height = height;
        Map.width = wight;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < wight; x++)
            {
                if (x == 0 || y == 0 || x == wight - 1 || y == height - 1)
                {
                    field.Add(new Block() { X = x, Y = y, Skin = '#', Color = ConsoleColor.Green, status = Block.Status.BORDER });
                }
                else
                {
                    field.Add(new Block() { X = x, Y = y, Skin = ' ', Color = ConsoleColor.Green, status = Block.Status.VOID });
                }
            }
        }
    }
    public static void OutputField()
    {
        for (int i = 0; i < field.Count; i++)
        {
            Console.SetCursorPosition(field[i].X, field[i].Y);
            Console.ForegroundColor = field[i].Color;
            Console.Write(field[i].Skin);
        }
    }
}
