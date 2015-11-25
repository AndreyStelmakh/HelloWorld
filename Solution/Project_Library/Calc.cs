using System;

namespace F.Project_Library
{
    public class Calc
    {
        static public double AreaOfTriangle(double l1, double l2, double l3)
        {
            #region Определяем кто есть кто, и проверки

            if(l1 <= 0 || l2 <= 0 || l3 <= 0)
            {
                throw new ArgumentOutOfRangeException();

            }

            // в "c" - вероятная гипотенуза
            double a, b, c; 
            if(l1 > l2 && l1 > l3)
            {
                c = l1;
                a = l2;
                b = l3;

            }
            else if(l2 > l3)
            {
                c = l2;
                a = l1;
                b = l3;

            }
            else
            {
                c = l3;
                a = l1;
                b = l2;

            }

            if( a*a + b*b != c*c)
            {
                throw new ArgumentOutOfRangeException("Треугольник не прямоугольный или проблемы с точностью");

            }

            #endregion

            return a * b / 2;

        }

    }

}
