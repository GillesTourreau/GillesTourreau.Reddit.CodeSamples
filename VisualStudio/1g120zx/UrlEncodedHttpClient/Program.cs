namespace UrlEncodedHttpClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
var keyValues = new Dictionary<string, string>
{
    { "login", "The login" },
    { "password", "The password" },
};

var urlEncodedContent = new FormUrlEncodedContent(keyValues);

var httpClient = new HttpClient();
var response = httpClient.PostAsync("http://.../api", urlEncodedContent);
        }
    }
}
