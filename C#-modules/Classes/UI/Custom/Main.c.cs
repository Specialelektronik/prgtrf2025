

using Crestron.SimplSharp;
// ReSharper disable once CheckNamespace
using SE_Crestron_Training.Logging;

namespace Programmerartraff
{
    public partial interface IMain
    {
        public void Setup();

        public void ShowMainPage(bool state);

        public void ShowNvxPreview(string? url);
    }

    internal partial class Main
    {
        private const string Username = "user";
        private const string Password = "password";

        private string CreateUrlWithUsernamePassword(string url)
        {
            // Extract the protocol and the rest of the URL
            Uri uri = new Uri(url);
            string protocol = uri.Scheme;
            string hostAndPath = url.Substring(protocol.Length + 3); // Skip "://"

            string authenticatedUrl = $"{protocol}://{Username}:{Password}@{hostAndPath}";

            ErrorLog.Notice($"New URL = {authenticatedUrl}");

            return authenticatedUrl;
        }
        
        public void Setup()
        {
            // Setup all necessary stuff
            // Event handlers, etc
        }

        public void ShowMainPage(bool state)
        {
            Main_VisibilityJoin(state);
        }

        public void ShowNvxPreview(string? url)
        {
            if (url is not null)
            {
                var updatedUrl = CreateUrlWithUsernamePassword(url);
                NVXPreview_Url($"{updatedUrl}");
                SeriLog.Log?.Debug($"NVX Preview URL set to {updatedUrl}");
            }
        }
    }
}