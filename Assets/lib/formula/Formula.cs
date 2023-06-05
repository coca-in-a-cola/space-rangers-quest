
using System;
using System.Collections.Generic;

namespace SRQ.Formulas {
    public static class Formula {
        public static Expression Parse(string str) {
            //[p6]+10000
            //"" - WTF???
            string strNoLineBreaks = str.Replace("\r", " ").Replace("\n", " ");
            var scanner = new Scanner(strNoLineBreaks);
            var parser = new Parser(scanner.Scan);
            var ast = parser.ParseExpression();
            return ast;
        }

        // TODO: держать в классе один экземпл€р калькул€тора, а не создавать его каждый раз
        public static int Calculate(string str, List<int> paramValues = null, Func<int?, double> random = null) {
            var ast = Parse(str);
            var calculator = new Calculator(paramValues == null ? new List<int>() : paramValues, random);
            double value = calculator.CalculateAst(ast);
            return (int)Math.Round(value);
        }
    }
}