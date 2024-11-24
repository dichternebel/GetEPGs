namespace GetEPGs
{
    internal class UriValidator
    {
        internal static Uri? IsValiUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult))
                return null;

            if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
                return null;

            return uriResult;
        }

        internal static async Task<bool> IsUriAccessible(Uri uri)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
