using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json;

namespace SRQ.Formulas {
    public class Calculator {
        private List<int> paramValues;
        private Func<int?, double> random;

        public Calculator(List<int> paramValues, Func<int?, double> random) {
            this.paramValues = paramValues;
            this.random = random;
        }

        public double CalculateAst(Expression ast) {
            if (ast.Type == ExpressionType.number) {
                return ((NumberExpression)ast).Value;
            }
            else if (ast.Type == ExpressionType.parameter) {
                var peAst = (ParameterExpression)ast;
                if (!(peAst.ParameterId < paramValues.Count)) {
                    throw new Exception($"Parameter p{peAst.ParameterId + 1} is not defined");
                }

                return paramValues[peAst.ParameterId];
            }
            else if (ast.Type == ExpressionType.binary) {
                var biAst = (BinaryExpression)ast;
                if (biAst.Operator == "plus token") {
                    double a = CalculateAst(biAst.Left);
                    double b = CalculateAst(biAst.Right);
                    return NumberMinMax(a + b);
                }
                else if (biAst.Operator == "minus token") {
                    double a = CalculateAst(biAst.Left);
                    double b = CalculateAst(biAst.Right);
                    return NumberMinMax(a - b);
                }
                else if (biAst.Operator == "slash token") {
                    double a = CalculateAst(biAst.Left);
                    double b = CalculateAst(biAst.Right);
                    return NumberMinMax(b != 0 ? a / b : a > 0 ? Constants.MAX_NUMBER : -Constants.MAX_NUMBER);
                }
                else if (biAst.Operator == "div keyword") {
                    double a = CalculateAst(biAst.Left);
                    double b = CalculateAst(biAst.Right);
                    if (b != 0) {
                        double val = a / b;
                        return NumberMinMax(FloorCeil(val));
                    }
                    else {
                        return a > 0 ? Constants.MAX_NUMBER : -Constants.MAX_NUMBER;
                    }
                }
                else if (biAst.Operator == "mod keyword") {
                    double a = CalculateAst(biAst.Left);
                    double b = CalculateAst(biAst.Right);
                    return NumberMinMax(b != 0 ? a % b : a > 0 ? Constants.MAX_NUMBER : -Constants.MAX_NUMBER);
                }
                else if (biAst.Operator == "asterisk token") {
                    double a = CalculateAst(biAst.Left);
                    double b = CalculateAst(biAst.Right);
                    return NumberMinMax(a * b);
                }
                else if (biAst.Operator == "to keyword") {
                    var newRanges = TransformToIntoRanges(biAst);
                    return PickRandomForRanges(newRanges);
                }
                else if (biAst.Operator == "in keyword") {
                    bool reversed = biAst.Left.Type == ExpressionType.range && biAst.Right.Type != ExpressionType.range;
                    var left = reversed ? biAst.Right : biAst.Left;
                    var right = reversed ? biAst.Left : biAst.Right;

                    double leftVal = NumberMinMax(CalculateAst(left));
                    var ranges = right.Type == ExpressionType.range
                        ? CalculateRange((RangeExpression)right)
                        : right.Type == ExpressionType.binary && ((BinaryExpression)right).Operator == "to keyword"
                            ? TransformToIntoRanges((BinaryExpression)right)
                            : null;
                    if (ranges != null) {
                        foreach (var range in ranges) {
                            if (leftVal >= range.From && leftVal <= range.To) {
                                return 1;
                            }
                        }
                        return 0;
                    }
                    else {
                        double rightVal = NumberMinMax(CalculateAst(biAst.Right));
                        return leftVal == rightVal ? 1 : 0;
                    }
                }
                else if (biAst.Operator == "greater than eq token") {
                    double a = CalculateAst(biAst.Left);
                    double b = CalculateAst(biAst.Right);
                    return a >= b ? 1 : 0;
                }
                else if (biAst.Operator == "greater than token") {
                    double a = CalculateAst(biAst.Left);
                    double b = CalculateAst(biAst.Right);
                    return a > b ? 1 : 0;
                }
                else if (biAst.Operator == "less than eq token") {
                    double a = CalculateAst(biAst.Left);
                    double b = CalculateAst(biAst.Right);
                    return a <= b ? 1 : 0;
                }
                else if (biAst.Operator == "less than token") {
                    double a = CalculateAst(biAst.Left);
                    double b = CalculateAst(biAst.Right);
                    return a < b ? 1 : 0;
                }
                else if (biAst.Operator == "equals token") {
                    double a = CalculateAst(biAst.Left);
                    double b = CalculateAst(biAst.Right);
                    return a == b ? 1 : 0;
                }
                else if (biAst.Operator == "not equals token") {
                    double a = CalculateAst(biAst.Left);
                    double b = CalculateAst(biAst.Right);
                    return a != b ? 1 : 0;
                }
                else if (biAst.Operator == "and keyword") {
                    double a = CalculateAst(biAst.Left);
                    double b = CalculateAst(biAst.Right);
                    return (a != 0 && b != 0) ? 1 : 0;
                }
                else if (biAst.Operator == "or keyword") {
                    double a = CalculateAst(biAst.Left);
                    double b = CalculateAst(biAst.Right);
                    return (a != 0 || b != 0) ? 1 : 0;
                }
                else {
                    throw new InvalidOperationException($"Unknown operator: {biAst.Operator}");
                }
            }
            else if (ast.Type == ExpressionType.unary) {
                var uAst = (UnaryExpression)ast;
                if (uAst.Operator == "minus token") {
                    return -CalculateAst(uAst.Expression);
                }
                else {
                    throw new InvalidOperationException($"Unknown AST type: {ast}");
                }
            }
            else if (ast.Type == ExpressionType.range) {
                return PickRandomForRanges(CalculateRange((RangeExpression)ast));
            }
            else {
                throw new InvalidOperationException($"Unknown AST type: {ast}");
            }
        }

        private static double NumberMinMax(double n) {
            return Math.Min(Math.Max(n, -Constants.MAX_NUMBER), Constants.MAX_NUMBER);
        }

        private static int FloorCeil(double val) {
            return val > 0 ? (int)Math.Floor(val) : (int)Math.Ceiling(val);
        }

        private double PickRandomForRanges(List<RangeCalculated> ranges) {
            int totalValuesAmount = ranges.Aggregate(0, (totalItems, range) => {
                return totalItems + range.To - range.From + 1;
            });

            double rnd = random(totalValuesAmount);

            foreach (var range in ranges) {
                int len = range.To - range.From + 1;
                if (rnd > len) {
                    rnd = rnd - len;
                }
                else {
                    double result = rnd + range.From;
                    return result;
                }
            }

            throw new Exception("Error in finding random value for " + JsonConvert.SerializeObject(new { ranges, rnd }, Formatting.Indented));
        }

        private List<RangeCalculated> TransformToIntoRanges(BinaryExpression node) {
            if (node.Type != ExpressionType.binary || node.Operator != "to keyword") {
                throw new Exception("Wrong usage");
            }

            var left = node.Left;
            var right = node.Right;
            var leftRanges = left.Type == ExpressionType.range
                ? CalculateRange((RangeExpression)left)
                : ValToRanges(FloorCeil(CalculateAst(left)));
            var rightRanges = right.Type == ExpressionType.range
                ? CalculateRange((RangeExpression)right)
                : ValToRanges(FloorCeil(CalculateAst(right)));

            int leftRangeMax = Math.Max(leftRanges.Max(x => x.To), 0);
            int rightRangeMax = Math.Max(rightRanges.Max(x => x.To), 0);

            int leftRangeMin = Math.Min(leftRanges.Min(x => x.From), Constants.MAX_NUMBER);
            int rightRangeMin = Math.Min(rightRanges.Min(x => x.From), Constants.MAX_NUMBER);

            int newRangeMax = Math.Max(leftRangeMax, rightRangeMax);
            int newRangeMin = Math.Min(leftRangeMin, rightRangeMin);

            var newRanges = new List<RangeCalculated>
            {
                new RangeCalculated (newRangeMin, newRangeMax)
            };

            return newRanges;
        }

        private List<RangeCalculated> CalculateRange(RangeExpression node) {
            if (node.Type != ExpressionType.range) {
                throw new Exception("Wrong usage");
            }


            return node.Ranges.Select(range => {
                int from = FloorCeil(CalculateAst(range.From));
                int to = range.To != null ? FloorCeil(CalculateAst((NumberExpression)range.To)) : from;

                bool reversed = from > to;

                return new RangeCalculated(reversed ? to : from, reversed ? from : to);
            }).ToList();
        }

        private List<RangeCalculated> ValToRanges(int val) => new List<RangeCalculated> {new RangeCalculated(val, val)};

    }
}