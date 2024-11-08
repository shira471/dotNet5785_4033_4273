using System;
namespace Targil0
{
    partial class Program
    {
        static void Main(string[] args)
        {
            welcome_4033();
            welcome_2560();
            Console.ReadKey();
        }
        static partial void welcome_2560();
        private static void welcome_4033()
        {
            Console.WriteLine("Enter your name: ");
            string userName = Console.ReadLine();
            Console.WriteLine("{0}, welcome to my first console application", userName);
        }
    }
}