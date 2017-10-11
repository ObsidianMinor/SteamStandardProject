namespace Steam.KeyValues.Utilities
{
    internal static class KeyValueTokenUtils
    {
        internal static bool IsEndToken(KeyValueToken token)
        {
            switch (token)
            {
                case KeyValueToken.End:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsStartToken(KeyValueToken token)
        {
            switch (token)
            {
                case KeyValueToken.Start:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsPrimitiveToken(KeyValueToken token)
        {
            switch (token)
            {
                case KeyValueToken.Int32:
                case KeyValueToken.Float32:
                case KeyValueToken.String:
                case KeyValueToken.Pointer:
                case KeyValueToken.Int64:
                case KeyValueToken.UInt64:
                case KeyValueToken.WideString:
                case KeyValueToken.Color:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsIntegerToken(KeyValueToken token)
        {
            switch(token)
            {
                case KeyValueToken.Int32:
                case KeyValueToken.Int64:
                case KeyValueToken.UInt64:
                    return true;
                default:
                    return false;
            }
        }
    }
}
