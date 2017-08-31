using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Parameters;
using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace TwitterBot
{
    class Program
    {       
        static void Main(string[] args)
        {
            string logfile_path = @"C:\Users\Ryan\Documents\Visual Studio 2015\Projects\TwitterBot\twitter_log.txt";
            if (!File.Exists(logfile_path))
                File.Create(logfile_path);

            string retweeted_path = @"C:\Users\Ryan\Documents\Visual Studio 2015\Projects\TwitterBot\retweet_log.txt";
            if (!File.Exists(retweeted_path))
                File.Create(retweeted_path);

            string cons_key = "VlzBE8kI2ToSPOZRjHybTG5mm";
            string cons_secret = "r2Qi4Rur75fcXGOa3Dcg7Lygt1yK4quAZjxrgqAzYIUrqNiNHV";
            string access_token = "812413212280778752-4ylaY7wYVC4EGZo9N2Z512PrWt35fIr";
            string secret_access = "l6PW4u39jR3QbEDtqTgYvj8gpkySmDGwzAgzAUL4pOGMM";

            Auth.SetUserCredentials(cons_key, cons_secret, access_token, secret_access);

            

            /*var searchParams = new SearchTweetsParameters("giveaway")
            {
                Lang = Tweetinvi.Models.LanguageFilter.English,
                SearchType = Tweetinvi.Models.SearchResultType.Popular,
                MaximumNumberOfResults = 1
            };*/

            //RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;
            int i = 0;

            while (true)
            {
                i++;
                var searchParams = new SearchTweetsParameters("giveaway -csgo -CSGO -weapons -AK-47 -knife -KNIFE -knives -kill")
                {
                    Lang = Tweetinvi.Models.LanguageFilter.English,
                    SearchType = Tweetinvi.Models.SearchResultType.Mixed,
                    MaximumNumberOfResults = 30
                };

                var tweets = Search.SearchTweets(searchParams);
                if (tweets == null)
                {
                    Console.WriteLine(ExceptionHandler.GetLastException().TwitterDescription);
                }
                foreach (var tweet in tweets)
                {
                    bool retweeted = false;
                    bool followed = false;
                    var tweetjson = JsonConvert.SerializeObject(tweet.TweetDTO);
                    string text = tweetjson.ToString();
                    string newtext = getTweetText(text);
                    string userString = getUser(tweetjson);
                    //if(newtext.Contains("CSGO") || newtext.Contains("csgo") || userString.Contains("csgo") || userString.Contains("CSGO"))
                      //  continue;
                    long id = tweet.Id;
                    if (newtext.Contains("Follow") || newtext.Contains("follow") || newtext.Contains("FOLLOW"))
                    {
                        var authenticateduser = User.GetAuthenticatedUser();
                        var user = User.GetUserFromScreenName(userString);
                        
                        try
                        {
                            long userId = user.Id;
                            authenticateduser.FollowUser(userId);
                            followed = user.Following;
                        }
                        catch (NullReferenceException e)
                        {
                            using (StreamWriter sw = File.AppendText(logfile_path))
                            {
                                sw.WriteLine("Error occured attempting to follow {0} at {1}.", userString, DateTime.Now);
                                sw.WriteLine("{0}", e.Message);
                                sw.WriteLine();
                                sw.Close();
                            }
                        }
                        if (followed)
                        {
                            followed = true;
                            using (StreamWriter sw = File.AppendText(logfile_path))
                            {
                                sw.WriteLine("Followed user {0} at {1}", userString, DateTime.Now);
                                sw.Close();
                            }
                        }

                    }

                    if ((newtext.Contains("Retweet") || newtext.Contains("RT") || newtext.Contains("RETWEET")) && !tweet.Retweeted)
                    {
                        bool foundMatch = false;
                        string[] lines = File.ReadAllLines(retweeted_path);
                        {
                            foreach (string line in lines)
                            {
                                string tweetid = line.ToString();
                                if (tweetid.Equals(id.ToString())) foundMatch = true;
                            }
                        }

                        if (!foundMatch)
                        {
                            var retweet = Tweet.PublishRetweet(id);
                            retweeted = true;
                            using (StreamWriter sw = File.AppendText(logfile_path))
                            {
                                sw.WriteLine("Successfully retweeted {0}'s tweet: {1} at {2}.", userString, newtext, DateTime.Now);
                                sw.WriteLine("Tweet ID: {0}", id.ToString());
                                sw.WriteLine();
                                sw.Close();
                            }

                            using (StreamWriter sw2 = File.AppendText(retweeted_path))
                            {
                                sw2.WriteLine(id.ToString());
                                sw2.Close();
                            }
                        }
                    }

                    //Console.Write(text);
                    Console.WriteLine();
                    if (retweeted) Thread.Sleep(300000);
                }
               // Console.Write(tweets.ToString());
                Console.WriteLine(i.ToString());
                //Console.ReadLine();
            }
        }

        static public string getTweetText(string jsonTweet)
        {
            int start, end;
            start = jsonTweet.IndexOf(",\"text\":", 0) + 9;
            end = jsonTweet.IndexOf(",\"full_text\"", start);
            return jsonTweet.Substring(start, end - start - 1);
        }

        static public string getUser(string jsonTweet)
        {
            int start, end;
            start = jsonTweet.IndexOf("\"screen_name\":\"", 0) + 15;
            end = jsonTweet.IndexOf("\"},\"", start);
            return jsonTweet.Substring(start, end - start);
        }
    }
}
