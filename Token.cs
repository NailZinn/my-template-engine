namespace HTML_Engine_Library
{
    internal class Token
    {
        public TokenType TokenType { get; }
        public dynamic Value { get; }

        public bool IsMathOperator
        {
            get
            {
                return TokenType == TokenType.Plus ||
                       TokenType == TokenType.Minus ||
                       TokenType == TokenType.Multiply ||
                       TokenType == TokenType.Divide;
            }
        }

        public bool IsBoolOperator
        {
            get
            {
                return TokenType == TokenType.And ||
                       TokenType == TokenType.Or ||
                       TokenType == TokenType.Equal ||
                       TokenType == TokenType.NotEqual ||
                       TokenType == TokenType.Greater ||
                       TokenType == TokenType.GreaterOrEqual ||
                       TokenType == TokenType.Less ||
                       TokenType == TokenType.LessOrEqual;
            }
        }

        public Token(TokenType tokenType, dynamic value)
        {
            TokenType = tokenType;
            Value = value;
        }
    }
}
