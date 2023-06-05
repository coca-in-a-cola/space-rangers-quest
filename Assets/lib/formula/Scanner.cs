using System.Collections.Generic;
using System;
using System.Text;

namespace SRQ.Formulas {
    public class Scanner {
        private string str;
        private int pos; 
        private int end;

        private string constructedString = "";


        // TODO: строго типизировать все строки. В typescript это делается с помошью Type Literals
        private static readonly Dictionary<string, string> KeywordsToKind = new Dictionary<string, string>
        {
            {"mod", "mod keyword"},
            {"div", "div keyword"},
            {"to", "to keyword"},
            {"in", "in keyword"},
            {"and", "and keyword"},
            {"or", "or keyword"},
        };

        public Scanner(string str) {
            int pos = 0;
            int end = str.Length;

            this.pos = pos;
            this.end = end;
            this.str = str;
        }

        public Token Scan() {
            Token token = ScanUnsafe();
            if (token.End - token.Start <= 0 && token.Kind != "end token") {
                throw new Exception($"Scanner fail: end={token.End} start={token.Start} str='{str}'");
            }
            if (str[token.Start..token.End] != token.Text) {
                throw new Exception("Scanner fail: token slice differs");
            }
            if (token.Kind != "end token") {
                constructedString += token.Text;
            }
            else {
                if (constructedString != str) {
                    throw new Exception("Scanner fail: constructed string differs!");
                }
            }
            return token;
        }

        private Token ScanUnsafe() {
            if (pos >= end) {
                return new Token ("end token", pos, pos, "");
            }
            char currentChar = str[pos];
            if (IsWhitespace(currentChar)) {
                return ScanWhitespace();
            }

            char lookAheadChar = LookAhead();
            if (currentChar == '.' && lookAheadChar == '.') {
                Token token = new Token ("dotdot token", pos, pos + 2, $"{currentChar}{lookAheadChar}");
                pos += 2;
                return token;
            }

            if (currentChar == '<' && lookAheadChar == '>') {
                Token token = new Token ("not equals token", pos, pos + 2, $"{currentChar}{lookAheadChar}");
                pos += 2;
                return token;
            }
            if (currentChar == '>' && lookAheadChar == '=') {
                Token token = new Token("greater than eq token", pos, pos + 2, $"{currentChar}{lookAheadChar}");
                pos += 2;
                return token;
            }
            if (currentChar == '<' && lookAheadChar == '=') {
                Token token = new Token("less than eq token", pos, pos + 2, $"{currentChar}{lookAheadChar}");
                pos += 2;
                return token;
            }

            if (currentChar == '>' && lookAheadChar != '=') {
                Token token = new Token("greater than token", pos, pos + 1, currentChar.ToString());
                pos++;
                return token;
            }

            if (currentChar == '<' && lookAheadChar != '=') {
                Token token = new Token("less than token", pos, pos + 1, currentChar.ToString());
                pos++;
                return token;
            }

            if (char.IsDigit(currentChar)) {
                return ScanNumber();
            }

            string oneCharKind = OneCharTokenToKind(currentChar);
            if (oneCharKind != null) {
                Token token = new Token(oneCharKind, pos, pos + 1, currentChar.ToString());
                pos++;
                return token;
            }
            return ScanIdentifierOrKeyword();
        }

        private Token ScanNumber() {
            bool dotSeen = false;
            int start = pos;
            int? trailingSpacesStartsAtPos = null;

            while (pos < end) {
                bool thisCharacterIsASpace = false;
                char currentChar = str[pos];

                if (char.IsDigit(currentChar)) {
                    // ok
                }
                else if (currentChar == '.' || currentChar == ',') {
                    if (dotSeen) {
                        break;
                    }

                    char nextNextChar = LookAhead();

                    if (nextNextChar != '.' && nextNextChar != ',') {
                        dotSeen = true;
                    }
                    else {
                        break;
                    }
                }
                else if (currentChar == ' ') {
                    thisCharacterIsASpace = true;
                }
                else {
                    break;
                }

                // Allow spaces inside digits but keep spaces as separate token if they are trailing spaces
                if (thisCharacterIsASpace) {
                    if (trailingSpacesStartsAtPos == null) {
                        // Ok, looks like a series of spaces have been begun
                        trailingSpacesStartsAtPos = pos;
                    }
                }
                else {
                    // Character is not a space and belongs to digit chars set
                    // So, spaces are not trailing spaces
                    trailingSpacesStartsAtPos = null;
                }

                pos++;
            }

            if (trailingSpacesStartsAtPos.HasValue) {
                if (!System.Text.RegularExpressions.Regex.IsMatch(str.Substring(trailingSpacesStartsAtPos.Value, pos - trailingSpacesStartsAtPos.Value), @"^\s*$")) {
                    throw new ApplicationException("Unknown internal state: trailingSpacesStartsAtPos is set but tail is not spaces");
                }

                pos = trailingSpacesStartsAtPos.Value;
            }

            Token token = new Token("numeric literal", start, pos, str.Substring(start, pos - start));
            return token;
        }

        private string OneCharTokenToKind(char currentChar) {
          return currentChar == '('
                ? "open brace token"
                : currentChar == ')'
                    ? "close brace token"
                    : currentChar == '['
                        ? "open paren token"
                        : currentChar == ']'
                            ? "close paren token"
                            : currentChar == '/'
                                ? "slash token"
                                : currentChar == '*'
                                    ? "asterisk token"
                                    : currentChar == '+'
                                        ? "plus token"
                                        : currentChar == '-'
                                            ? "minus token"
                                            : currentChar == '='     
                                                ? "equals token"
                                                : currentChar == ';'
                                                    ? "semicolon token"
                                                    : null;                 
        }

        private char LookAhead(int charCount = 1) {
            return pos + charCount < end ? str[pos + charCount] : '\0';
        }

        private Token ScanIdentifierOrKeyword() {
            int start = pos;

            string text = "";
            string keywordKind = null;
            while (
                pos < end &&
                "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM01234567890_".IndexOf(str[pos]) > -1
            ) {
                pos++;
                text = str.Substring(start, pos - start);
                keywordKind = KeywordsToKind.TryGetValue(text, out string skind) ? skind : null;

                if (keywordKind != null) {
                    // Some quests have "[p1] mod1000" (without spaces)
                    break;
                }
            }

            string kind = keywordKind != null ? keywordKind : "identifier";

            if (start == pos) {
                throw new ApplicationException($"Unknown char {str[pos]}");
            }

            return new Token(kind, start, pos, text);
        }

        private bool IsWhitespace(char currentChar) {
            return currentChar == ' ' || currentChar == '\n' || currentChar == '\r' || currentChar == '\t';
        }

        private Token ScanWhitespace() {
            int start = pos;
            while (pos < end && IsWhitespace(str[pos])) {
                pos++;
            }
            Token token = new Token ("white space token", start, pos, str.Substring(start, pos - start));
            return token;
        }
    }
}