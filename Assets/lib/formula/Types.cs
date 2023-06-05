using System.Collections.Generic;

namespace SRQ.Formulas {
    public struct Token {
        public string Kind { get; }
        public int Start { get; }
        public int End { get; }
        public string Text { get; }

        public Token(string kind, int start, int end, string text) {
            this.Kind = kind;
            this.Start = start;
            this.End = end;
            this.Text = text;
        }
    }

    public struct RangeCalculated {
        public int From { get; }
        public int To { get; }

        public RangeCalculated(int from, int to) {
            this.From = from;
            this.To = to;
        }
    }


    public enum ExpressionType {
        number,
        range,
        parameter,
        binary,
        unary
    }

    public abstract class Expression {
        public ExpressionType Type { get; set; }
    }

    public class NumberExpression : Expression {
        public NumberExpression() => Type = ExpressionType.number;

        public float Value { get; set; }
    }

    public class RangePart {
        public Expression From { get; }
        public Expression To { get; }

        public RangePart(Expression from = null, Expression to = null) {
            From = from;
            To = to;
        }
    }

    public class RangeExpression : Expression {
        public RangeExpression() => Type = ExpressionType.range;

        public List<RangePart> Ranges { get; set; }
    }

    public class ParameterExpression : Expression {
        public ParameterExpression() => Type = ExpressionType.parameter;

        public int ParameterId { get; set; }
    }

    public class BinaryExpression : Expression {
        public BinaryExpression() => Type = ExpressionType.binary;

        public Expression Left { get; set; }
        public Expression Right { get; set; }
        public string Operator { get; set; }
    }

    public class UnaryExpression : Expression {
        public UnaryExpression() => Type = ExpressionType.unary;

        public Expression Expression { get; set; }
        public string Operator { get; set; }
    }

    public interface ITokenReader {
        Token Current();
        void ReadNext();
    }
}