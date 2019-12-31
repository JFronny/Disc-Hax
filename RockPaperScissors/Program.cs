using System;
using System.Linq;

namespace RockPaperScissors
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Random rnd = new Random();
            while (true)
            {
                Array soptions = Enum.GetValues(typeof(RPSOption));
                RPSOption rpsOption = (RPSOption) soptions.GetValue(rnd.Next(soptions.Length));
                Console.Write("Choose: ");
                string input = Console.ReadLine().ToLower();
                RPSOption uoption = soptions.OfType<RPSOption>().First(
                    s =>
                    {
                        string tmp = s.ToString().ToLower();
                        return tmp.StartsWith(input) || tmp.EndsWith(input) || input.StartsWith(tmp) ||
                               input.EndsWith(tmp);
                    });
                Console.Write($"You chose: {uoption}, I chose {rpsOption}. This means ");
                int diff = (int) rpsOption - (int) uoption;
                diff = diff switch {-2 => 1, 2 => -1, _ => diff};
                Console.Write(diff switch
                {
                    -1 => "You've",
                    0 => "No-one has",
                    1 => "I've",
                    _ => throw new Exception($"This should not happen! (diff={diff})")
                });
                Console.WriteLine(" won");
            }
        }

        private enum RPSOption
        {
            Rock,
            Paper,
            Scissor
        }
    }
}