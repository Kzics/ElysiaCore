namespace ElysiaInteractMenu.Discord
{
    public enum EmbedType
    {
        Staff,
        Life,
        Global,
        Biz 
    }

    public static class EmbedTypeExtensions
    {
        public static int GetColor(EmbedType type)
        {
            switch (type)
            {
                case EmbedType.Staff:
                    return 65280;
                case EmbedType.Life:
                    return 16711680;
                case EmbedType.Global:
                    return 255;
                case EmbedType.Biz:
                    return 16776960;
            }

            return 0;
        }
    }
}