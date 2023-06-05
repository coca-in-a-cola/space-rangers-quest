using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SRQ.Formulas {
    public class Parser {
        const int MAX_PRECEDENCE = 8;
        private ITokenReader reader;

        public class TokenReaderSkipWhitespaces : ITokenReader {
            private Func<Token> _reader;
            private Token _currentToken;

            public TokenReaderSkipWhitespaces(Func<Token> reader) {
                _reader = reader;
                ReadNext();
            }

            public Token Current() {
                return _currentToken;
            }

            public void ReadNext() {
                _currentToken = _reader();
                while (_currentToken.Kind == "white space token") {
                    _currentToken = _reader();
                }
            }
        }

        public Parser(Func<Token> reader) {
            this.reader = new TokenReaderSkipWhitespaces(reader);
        }

        public Expression ParseExpression() {
            var expr = ReadExpr();

            if (reader.Current().Kind != "end token") {
                throw new Exception($"Unexpected data at {reader.Current().Start}: '{reader.Current().Text}' kind={reader.Current().Kind}");

            }

            return expr;
        }


        /*
        * If candidate is binary token on precedence, then return corresponding binary operator
        */
        string IsTokenBinaryOperator(int precedence, string candidate) {
            switch (precedence) {
                // TODO: Why or/and have different priority?
                case 8:
                    return candidate == "or keyword" ? "or keyword" : null;
                case 7:
                    return candidate == "and keyword" ? "and keyword" : null;
                case 6:
                    return candidate == "greater than eq token" ? "greater than eq token" :
                            candidate == "less than eq token" ? "less than eq token" :
                            candidate == "greater than token" ? "greater than token" :
                            candidate == "less than token" ? "less than token" :
                            candidate == "equals token" ? "equals token" :
                            candidate == "not equals token" ? "not equals token" :
                            candidate == "in keyword" ? "in keyword" : null;

                case 5:
                    return null; // here was "in keyword";
                case 4:
                    return candidate == "to keyword" ? "to keyword" : null;
                case 3:
                    return candidate == "plus token" ? "plus token" :
                            candidate == "minus token" ? "minus token" : null;

                case 2:
                    return candidate == "asterisk token" ? "asterisk token" :
                            candidate == "slash token" ? "slash token" : null;

                case 1:
                    return candidate == "div keyword" ? "div keyword" :
                            candidate == "mod keyword" ? "mod keyword" : null;

                default:
                    throw new Exception($"Unknown precedence {precedence}");
            }
        }

        public Expression ReadPrim() {
            var primStartToken = reader.Current();

            if (primStartToken.Kind == "numeric literal") {
                var expr = new NumberExpression {
                    Value = float.Parse(primStartToken.Text.Replace(",", ".").Replace(" ", ""))
                };

                reader.ReadNext();
                return expr;
            }
            else if (primStartToken.Kind == "open paren token") {
                var expr = ReadParenExpression();
                return expr;
            }
            else if (primStartToken.Kind == "open brace token") {
                reader.ReadNext();
                var expr = ReadExpr();

                if (reader.Current().Kind != "close brace token") {
                    throw new Exception($"Expected close brace token but got {reader.Current().Text} at {reader.Current().Start}");
                }
                reader.ReadNext();
                return expr;
            }
            else if (primStartToken.Kind == "minus token") {
                reader.ReadNext();
                var innerExpr = ReadPrim();
                var expr = new UnaryExpression {
                    Expression = innerExpr,
                    Operator = "minus token"
                };
                return expr;
            }
            else {
                if (reader.Current().Kind == "end token") {
                    throw new Exception($"Expected value at {reader.Current().Start}");
                }
                else {
                    throw new Exception($"Expecting primary value at {reader.Current().Start} but got '{reader.Current().Text}' kind={reader.Current().Kind}");
                }
            }
        }

        public Expression ReadExpr(int currentPriority = MAX_PRECEDENCE) {
            if (currentPriority == 0) {
                var prim = ReadPrim();
                return prim;
            }

            Expression left = ReadExpr(currentPriority - 1);

            while (true) {
                var possibleBinaryTokenKind = reader.Current().Kind;
                if (possibleBinaryTokenKind == "end token") {
                    return left;
                }
                var possibleBinaryToken = IsTokenBinaryOperator(currentPriority, possibleBinaryTokenKind);

                if (possibleBinaryToken == null) {
                    return left;
                }

                reader.ReadNext();

                var right = ReadExpr(currentPriority - 1);

                var newLeft = new BinaryExpression {
                    Operator = possibleBinaryToken,
                    Left = left,
                    Right = right
                };
                left = newLeft;
            }
        }


        Expression ReadParenExpression() {
            reader.ReadNext();

            if (reader.Current().Kind == "identifier") {
                Match paramRegexpMatch = Regex.Match(reader.Current().Text, @"^p(\d+)$");
                if (!paramRegexpMatch.Success) {
                    throw new Exception($"Unknown parameter '{reader.Current().Text}' at {reader.Current().Start}");
                }
                string pNumber = paramRegexpMatch.Groups[1].Value;

                int pId = int.Parse(pNumber) - 1;

                ParameterExpression exp = new ParameterExpression {
                    ParameterId = pId
                };
                reader.ReadNext();

                if (reader.Current().Kind != "close paren token") {
                    throw new Exception($"Expected ], but got '{reader.Current().Text}' at {reader.Current().Start}");
                }
                reader.ReadNext();

                return exp;
            }
            else {
                List<RangePart> ranges = new List<RangePart>();

                while (true) {
                    if (reader.Current().Kind == "semicolon token") {
                        reader.ReadNext();
                        continue;
                    }

                    if (reader.Current().Kind == "close paren token") {
                        reader.ReadNext();
                        break;
                    }

                    Expression from = ReadExpr();
                    if (reader.Current().Kind == "dotdot token") {
                        reader.ReadNext();
                        Expression to = ReadExpr();

                        ranges.Add(new RangePart (from, to));
                    }
                    else if (reader.Current().Kind == "close paren token" || reader.Current().Kind == "semicolon token") {
                        ranges.Add(new RangePart (from));
                    }
                    else {
                        throw new Exception($"Unexpected token inside paren '{reader.Current().Text}' pos={reader.Current().Start}");
                    }
                }

                RangeExpression expr = new RangeExpression {
                    Ranges = ranges
                };

                return expr;
            }
        }

    }
}