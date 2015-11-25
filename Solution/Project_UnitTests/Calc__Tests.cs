using Microsoft.VisualStudio.TestTools.UnitTesting;
using F.Project_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F.Project_Library.Tests
{
    [TestClass()]
    public class Calc__Tests
    {
        [TestMethod()]
        public void AreaOfTriangle__Test()
        {
            StringBuilder fails = new StringBuilder();
            fails.AppendLine();
            bool failed = false;

            var testArgsList = new List<Tuple<double, double, double>>();

            testArgsList.Add(new Tuple<double, double, double>(0, 0, 0));
            testArgsList.Add(new Tuple<double, double, double>(-1, -1, -1));
            testArgsList.Add(new Tuple<double, double, double>(-1, 1, 1));
            testArgsList.Add(new Tuple<double, double, double>(-1, 1, 1));

            foreach (var testArgs in testArgsList)
            {
                try
                {
                    Calc.AreaOfTriangle(testArgs.Item1, testArgs.Item2, testArgs.Item3);

                    failed = true;

                    fails.AppendLine(string.Format("Тест не прошедши {0}", testArgs.ToString()));

                }
                catch { }

            }

            if(failed)
            {
                Assert.Fail(fails.ToString());

            }

        }

    }

}