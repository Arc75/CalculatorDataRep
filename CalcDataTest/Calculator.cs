using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalcDataTest
{
    [TestClass]
    public class Calculator
    {
        CalculateData.Calculator _calculator;

        public Calculator()
        {
            var list = new List<string>
            {

            };
            _calculator = new CalculateData.Calculator(list);
        }

        [TestMethod]
        public void TestMethod1()
        {
            

        }
    }
}
