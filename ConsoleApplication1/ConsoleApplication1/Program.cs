using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static bool CheckPass4334xxOs(string login)
        {
           if (1 <= login.Length && login.Length <= 21)
            {
                // в начале буква
                if (Regex.IsMatch(login, @"\A[A-z]{1}"))
                {
                    // в конце буква или цифра
                    if(Regex.IsMatch(login, @"[A-z\d]{1}\z"))
                    {
                        // состоит только из букв, цифр, точек и минусов
                        if (Regex.IsMatch(login, @"\A[A-z\d\.-]*\z"))
                        {
                            return true;

                        }

                    }

                }

            }

            return false;

        }

        static void Main(string[] args)
        {
            string login = "";

            var x = CheckPass4334xxOs("eA.e");
            

            var b = Regex.IsMatch(" EA123456789AA", @"\AE[A-Z]{1}?\d{9}?[A-Z]{2}?\z");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var t = await DoSomeWorkAsync();
            int result = t;
        }

        private async Task<int> DoSomeWorkAsync()
        {
            await Task.Delay(100).ConfigureAwait(true);
            return 1;
        }

    }

}
