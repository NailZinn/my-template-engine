namespace HTML_Engine_Library
{
    internal class ConditionTokenizerHelper
    {
        public static int GetOperatorPrecedence(TokenType type)
        {
            return type switch
            {
                TokenType.OpenBracket or TokenType.CloseBracket => 1,
                TokenType.Plus or TokenType.Minus => 2,
                TokenType.Multiply or TokenType.Divide => 3,

                TokenType.Greater or TokenType.GreaterOrEqual or
                    TokenType.Less or TokenType.LessOrEqual or
                    TokenType.Equal or TokenType.NotEqual or
                    TokenType.Not or TokenType.Or or TokenType.And => 4
            };
        }
    }
}
