using CalculateData;
using System;
using System.Linq;

namespace Calculate_Data
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"Введите данные через ';'");

            var input = Console.ReadLine();

            var inputList = input.Trim().Split(';').ToList();

            var calculator = new Calculator(inputList);

            var result = calculator.Calculate();

            Console.WriteLine($"Результат: {result} ");

            Console.ReadKey();

        }
    }
}
