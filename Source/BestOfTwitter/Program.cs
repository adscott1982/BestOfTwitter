using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestOfTwitter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Loading credentials...");

            var path = ConfigurationManager.AppSettings["oAuthPath"];
            var auth = LoadOAuth(path);

            if (auth == null)
                auth = Authorize(path);

            Console.WriteLine("Performing search of 'cats'...");

            using (var twitterCtx = new TwitterContext(auth))
            {
                var srch = (from search in twitterCtx.Search
                            where search.Type == SearchType.Search &&
                                  search.Query == "cats"
                            select search)
                            .SingleOrDefaultAsync();
                var results = srch.Result;

                results.Statuses.ForEach(entry =>
                Console.WriteLine(
                "ID: {0, -15}, Source: {1}\nContent: {2}\n",
                entry.StatusID, entry.Source, entry.Text));
            }

            Console.ReadKey();
        }

        static PinAuthorizer LoadOAuth(string path)
        {
            if (!File.Exists(path))
                return null;

            var lines = File.ReadAllLines(path);

            if (lines.Count() != 2)
                return null;

            var auth = new PinAuthorizer();

            auth.CredentialStore = new InMemoryCredentialStore
            {
                ConsumerKey = ConfigurationManager.AppSettings["consumerKey"],
                ConsumerSecret = ConfigurationManager.AppSettings["consumerSecret"],
                OAuthToken = lines[0],
                OAuthTokenSecret = lines[1]
            };

            return auth;
        }
        static PinAuthorizer Authorize(string path)
        {
            var auth = new PinAuthorizer();
            auth.CredentialStore = new InMemoryCredentialStore
            {
                ConsumerKey = ConfigurationManager.AppSettings["consumerKey"],
                ConsumerSecret = ConfigurationManager.AppSettings["consumerSecret"]
            };

            auth.GoToTwitterAuthorization = pageLink => Process.Start(pageLink);

            auth.GetPin = () =>
            {
                Console.WriteLine(
                    "\nAfter authorizing this application, Twitter " +
                    "will give you a 7-digit PIN Number.\n");
                Console.Write("Enter the PIN number here: ");
                return Console.ReadLine();
            };

            auth.AuthorizeAsync().Wait();

            var lines = new List<string> { auth.CredentialStore.OAuthToken, auth.CredentialStore.OAuthTokenSecret };
            File.WriteAllLines(path, lines);

            return auth;
        }
    }
}
