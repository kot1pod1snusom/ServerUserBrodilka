using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class UserConsole
{
    private void ChangeSymbol(ref User me)
    {
        bool exit = false;
        while (exit != true)
        {
            Console.Clear();
            Console.WriteLine($"Your symbol now {me.Skin}");
            Console.WriteLine("Введите 1 чтобы поменять символ");
            Console.WriteLine("Введите 2 чтобы вернуть на начало");
            string str = Console.ReadLine();
            switch (str)
            {
                case "1":
                    Console.WriteLine("Введите новый символ");
                    string t = Console.ReadLine();
                    if (t[0] != ' ') me.Skin = t[0];
                    break;
                case "2":
                    exit = true;
                    break;
                default:
                    break;
            }
        }

    }

    private void ColorChange(ref User me)
    {

        bool exit = false;
        while (exit != true)
        {
            Console.Clear();
            Console.WriteLine("Введите 1 чтобы сменить цвет");
            Console.WriteLine("Введите 2 чтобы вернуться на начало");
            string str = Console.ReadLine();

            switch (str)
            {
                case "1":
                    Console.WriteLine("Введите число от 1 до 15");
                    int color;
                    try { color = Convert.ToInt16(Console.ReadLine()); } catch { color = 1; };
                    ConsoleColor cColor = (ConsoleColor)(color < 1 ? 1 : (color > 15 ? 15 : color));
                    me.Color = cColor;
                    break;
                case "2":
                    exit = true;
                    break;
                default:
                    break;
            }
        }
    }


    public UserConsole(ref User me)
    {

        Console.Clear();

        bool exit = false;
        while (exit != true)
        {
            Console.WriteLine("Введите 1 чтобы поменять символ");
            Console.WriteLine("Введите 2 чтобы поменять цвет");
            Console.WriteLine("Введите 3 чтобы закрыть консоль");
            string str = Console.ReadLine();

            switch (str)
            {
                case "1":
                    ChangeSymbol(ref me);
                    break;
                case "2":
                    ColorChange(ref me);
                    break;
                case "3":
                    exit = true;
                    break;
                default:
                    break;
            }
        }
    }
}