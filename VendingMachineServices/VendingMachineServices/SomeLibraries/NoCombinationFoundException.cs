using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomeLibraries
{
    /// <summary>
    /// Не удается набрать сдачу
    /// </summary>
    public class NoCombinationFoundException : Exception
    {
        /// <summary>
        /// Не удается набрать сдачу
        /// </summary>
        public NoCombinationFoundException()
            :base()
        { }

    }

}
