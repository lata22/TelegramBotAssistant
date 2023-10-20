namespace VD.TelegramBot.Api.Extensions
{
    public static class LINQExtensions
    {
        public static async Task<T> FirstOrDefaultAsync<T>(this IEnumerable<T> enumerable, Func<T, Task<bool>> predicate)
        {
            foreach (var item in enumerable)
            {
                if (await predicate(item))
                {
                    return item;
                }
            }
            return default!;
        }

    }
}
