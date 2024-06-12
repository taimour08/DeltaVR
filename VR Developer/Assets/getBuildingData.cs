using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        Console.WriteLine("Enter a string:");
        string input = Console.ReadLine();
        
        string numericalValue = ExtractNumericalValue(input);
        
        if (numericalValue.Length > 0)
        {
           // Console.WriteLine("Extracted numerical values: " + numericalValue);
        }
        else
        {
            Console.WriteLine("No numerical values found in the input string.");
        }
    }

    static string ExtractNumericalValue(string input)
    {
        // Use Regex to find all digits in the input string
        MatchCollection matches = Regex.Matches(input, @"\d+");
        string result = "";

        foreach (Match match in matches)
        {
            result += match.Value;
        }

        return result;
    }
}
