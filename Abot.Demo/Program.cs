
using Abot.Core;
using Abot.Crawler;
using Abot.Poco;
using CsQuery.HtmlParser;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Abot.Demo
{
    class Program
    {
        public static readonly Uri FeedUrl = new Uri(@"http://news.cnblogs.com/");

        /// <summary>
        ///匹配新闻详细页面的正则
        /// </summary>
        public static Regex NewsUrlRegex = new Regex("^http://news.cnblogs.com/n/\\d+/$", RegexOptions.Compiled);

        /// <summary>
        /// 匹配分页正则
        /// </summary>
        public static Regex NewsPageRegex = new Regex("^http://news.cnblogs.com/n/page/\\d+/$", RegexOptions.Compiled);


        /// <summary>
        /// 新浪微博正则
        /// </summary>
        public static Regex WeiboAccountRegex = new Regex("http://weibo.com/[^p]/\\d+|http://weibo.com/u/\\d+", RegexOptions.Compiled);

        public static Regex PageToCrawl = new Regex("http://e.weibo.com/[a-zA-Z]+|http://weibo.com/+|http://weibo.com/u/\\d+", RegexOptions.Compiled);

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            PrintDisclaimer();

            //Uri uriToCrawl = GetSiteToCrawl(args);

            IWebCrawler crawler;

            ////Uncomment only one of the following to see that instance in action
            //crawler = GetDefaultWebCrawler();
            ////crawler = GetManuallyConfiguredWebCrawler();
            ////crawler = GetCustomBehaviorUsingLambdaWebCrawler();

            ////Subscribe to any of these asynchronous events, there are also sychronous versions of each.
            ////This is where you process data about specific events of the crawl
            //crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
            //crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;
            //crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
            //crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;

            ////Start the crawl
            ////This is a synchronous call
            //CrawlResult result = crawler.Crawl(uriToCrawl);

            #region New
            ///////////////////////////////////***************************New ****************************//////////////////////////////////
            CrawlConfiguration config = new CrawlConfiguration();
            config.UserAgentString = "spider";
            config.MaxConcurrentThreads = 1; // Web Extractor is not currently thread-safe.
            config.IsSendingCookiesEnabled = true;
            config.IsHttpRequestAutoRedirectsEnabled = false;
            config.IsAlwaysLogin = true;
            config.LoginUser = "13917506403";
            config.LoginPassword = "abcde19900414F";

           

            CookieContainer container = CookContainerForSina.GetCookieContainer();

            /////////////////////////////////////////////set cookie to a txt//////////////////////////////////////////////////////////////////////
           
            foreach (System.Net.Cookie cookie in container.GetCookies(new Uri("http://weibo.com")))
            {
                string cookieInfo = string.Format("document.cookie = '{0}={1}; path={2}; domain={3}; expires={4}';\n",cookie.Name,cookie.Value,cookie.Path,cookie.Domain,cookie.Expires);
                System.IO.File.AppendAllText("cookies.txt", cookieInfo);
            }
            
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////

            // Create the PhantomJS instance. This will spawn a new PhantomJS process using phantomjs.exe.
            // Make sure to dispose this instance or you will have a zombie process!
            IWebDriver driver = CreatePhantomJsDriver(config);

            // Create the content extractor that uses PhantomJS.
            IWebContentExtractor extractor = new JavaScriptContentExtractor(driver);
            

            // Create a PageRequester that will use the extractor.
            IPageRequester requester = new PageRequester(config, container, extractor);

            using (crawler = new PoliteWebCrawler(config, null, null, null, requester, null, null, null, null))
            {
                //crawler.ShouldCrawlPage((pageToCrawl, crawlContext) =>
                //{
                //    CrawlDecision decision = new CrawlDecision { Allow = true };
                //    if (!PageToCrawl.IsMatch(pageToCrawl.Uri.AbsoluteUri))
                //    {
                //        return new CrawlDecision { Allow = false, Reason = "Dont want to crawl  pages with less pages" };
                //    }
                //    if (pageToCrawl.Uri.Authority == "google.com")
                //        return new CrawlDecision { Allow = false, Reason = "Dont want to crawl google pages" };

                //    return decision;
                //});

                crawler.ShouldCrawlPageLinks((crawledPage, crawlContext) =>
                {
                    CrawlDecision decision = new CrawlDecision { Allow = true };
                    if (crawledPage.Content.Bytes.Length < 100)
                        return new CrawlDecision { Allow = false, Reason = "Just crawl links in pages that have at least 100 bytes" };

                    return decision;
                });


                //crawler.PageCrawlCompleted += OnPageCrawlCompleted;
                crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
                crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;
                crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
                crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;

                CrawlResult result = crawler.Crawl(new Uri("http://weibo.com/p/1006051192515960/follow"));
                if (result.ErrorOccurred)
                    Console.WriteLine("Crawl of {0} completed with error: {1}", result.RootUri.AbsoluteUri, result.ErrorException.Message);
                else
                    Console.WriteLine("Crawl of {0} completed without error.", result.RootUri.AbsoluteUri);
            }
            ///////////////////////////////////***************************New ****************************//////////////////////////////////
            #endregion

            //Now go view the log.txt file that is in the same directory as this executable. It has
            //all the statements that you were trying to read in the console window :).
            //Not enough data being logged? Change the app.config file's log4net log level from "INFO" TO "DEBUG"

            PrintDisclaimer();
        }

        private static IWebDriver CreatePhantomJsDriver(CrawlConfiguration p_Config)
        {
            // Optional options passed to the PhantomJS process.
            PhantomJSOptions options = new PhantomJSOptions();
            options.AddAdditionalCapability("phantomjs.page.settings.userAgent", p_Config.UserAgentString);
            options.AddAdditionalCapability("phantomjs.page.settings.javascriptCanCloseWindows", false);
            options.AddAdditionalCapability("phantomjs.page.settings.javascriptCanOpenWindows", false);
            options.AddAdditionalCapability("acceptSslCerts", !p_Config.IsSslCertificateValidationEnabled);

            // Basic auth credentials.
            options.AddAdditionalCapability("phantomjs.page.settings.userName", p_Config.LoginUser);
            options.AddAdditionalCapability("phantomjs.page.settings.password", p_Config.LoginPassword);

            // Create the service while hiding the prompt window.
            PhantomJSDriverService service = PhantomJSDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            service.CookiesFile = "cookiePersistent.txt";
           
            IWebDriver phantomDriver = new PhantomJSDriver(service, options);
            //string txt = System.IO.File.ReadAllText("cookies.txt");
            //phantomDriver.ExecutePhantomJS(txt);
       

            return phantomDriver;
        }

        private static IWebCrawler GetDefaultWebCrawler()
        {
            return new PoliteWebCrawler();
        }

        private static IWebCrawler GetManuallyConfiguredWebCrawler()
        {
            ////Create a config object manually
            //CrawlConfiguration config = new CrawlConfiguration();
            //config.CrawlTimeoutSeconds = 0;
            //config.DownloadableContentTypes = "text/html, text/plain";
            //config.IsExternalPageCrawlingEnabled = false;
            //config.IsExternalPageLinksCrawlingEnabled = false;
            //config.IsRespectRobotsDotTextEnabled = false;
            //config.IsUriRecrawlingEnabled = false;
            //config.MaxConcurrentThreads = 10;
            //config.MaxPagesToCrawl = 10;
            //config.MaxPagesToCrawlPerDomain = 0;
            //config.MinCrawlDelayPerDomainMilliSeconds = 1000;

            ////Add you own values without modifying Abot's source code.
            ////These are accessible in CrawlContext.CrawlConfuration.ConfigurationException object throughout the crawl
            //config.ConfigurationExtensions.Add("Somekey1", "SomeValue1");
            //config.ConfigurationExtensions.Add("Somekey2", "SomeValue2");

            ////Initialize the crawler with custom configuration created above.
            ////This override the app.config file values
            //return new PoliteWebCrawler(config, null, null, null, null, null, null, null, null);

            var config = new CrawlConfiguration
            {
                CrawlTimeoutSeconds = 0,
                DownloadableContentTypes = "text/html, text/plain",
                HttpServicePointConnectionLimit = 200,
                HttpRequestTimeoutInSeconds = 35,
                HttpRequestMaxAutoRedirects = 7,
                IsExternalPageCrawlingEnabled = false,
                IsExternalPageLinksCrawlingEnabled = false,
                IsUriRecrawlingEnabled = true,
                IsHttpRequestAutoRedirectsEnabled = true,
                IsHttpRequestAutomaticDecompressionEnabled = false,
                IsRespectRobotsDotTextEnabled = false,
                IsRespectMetaRobotsNoFollowEnabled = false,
                IsRespectAnchorRelNoFollowEnabled = false,
                IsForcedLinkParsingEnabled = false,
                /* activate */
                IsSendingCookiesEnabled = true,
                MaxConcurrentThreads = 10,
                MaxPagesToCrawl = 100,
                MaxPagesToCrawlPerDomain = 0,
                MaxPageSizeInBytes = 0,
                MaxMemoryUsageInMb = 0,
                MaxMemoryUsageCacheTimeInSeconds = 0,
                MaxRobotsDotTextCrawlDelayInSeconds = 5,
                MaxCrawlDepth = 100,
                MinAvailableMemoryRequiredInMb = 0,
                MinCrawlDelayPerDomainMilliSeconds = 1000,
                UserAgentString = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)"
            };

            // var onDiskScheduler = new Scheduler(true, _crawledUrlRepository, _pagesToCrawlRepository);



            return new PoliteWebCrawler(config, null, null, null, null, null, null, null, null);
        }

        private static CookieContainer Login()
        {
            var cookieContainer = new CookieContainer();
            var encoding = new ASCIIEncoding();
            var postData = "postdata=1&...";
            var postDataBytes = encoding.GetBytes(postData);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(@"https://www.website.com");
            httpWebRequest.Method = "POST";
            httpWebRequest.Referer = @"https://www.website.com";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.ContentLength = postDataBytes.Length;
            httpWebRequest.AllowAutoRedirect = false;
            httpWebRequest.CookieContainer = new CookieContainer();

            using (var stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(postDataBytes, 0, postDataBytes.Length);
                stream.Close();
            }

            using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                var responseStream = httpWebResponse.GetResponseStream();

                if (responseStream != null)
                {
                    if (httpWebResponse.StatusCode == HttpStatusCode.Found || httpWebResponse.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine("Login successful");
                        cookieContainer.Add(httpWebResponse.Cookies);

                    }
                    else
                    {
                        Console.WriteLine("Login NOT successful");
                    }
                }
            }

            return cookieContainer;
        }

        private static IWebCrawler GetCustomBehaviorUsingLambdaWebCrawler()
        {
            IWebCrawler crawler = GetDefaultWebCrawler();

            //Register a lambda expression that will make Abot not crawl any url that has the word "ghost" in it.
            //For example http://a.com/ghost, would not get crawled if the link were found during the crawl.
            //If you set the log4net log level to "DEBUG" you will see a log message when any page is not allowed to be crawled.
            //NOTE: This is lambda is run after the regular ICrawlDecsionMaker.ShouldCrawlPage method is run.
            crawler.ShouldCrawlPage((pageToCrawl, crawlContext) =>
            {
                if (pageToCrawl.Uri.AbsoluteUri.Contains("ghost"))
                    return new CrawlDecision { Allow = false, Reason = "Scared of ghosts" };

                return new CrawlDecision { Allow = true };
            });

            //Register a lambda expression that will tell Abot to not download the page content for any page after 5th.
            //Abot will still make the http request but will not read the raw content from the stream
            //NOTE: This lambda is run after the regular ICrawlDecsionMaker.ShouldDownloadPageContent method is run
            crawler.ShouldDownloadPageContent((crawledPage, crawlContext) =>
            {
                if (crawlContext.CrawledCount >= 5)
                    return new CrawlDecision { Allow = false, Reason = "We already downloaded the raw page content for 5 pages" };

                return new CrawlDecision { Allow = true };
            });

            //Register a lambda expression that will tell Abot to not crawl links on any page that is not internal to the root uri.
            //NOTE: This lambda is run after the regular ICrawlDecsionMaker.ShouldCrawlPageLinks method is run
            crawler.ShouldCrawlPageLinks((crawledPage, crawlContext) =>
            {
                if (!crawledPage.IsInternal)
                    return new CrawlDecision { Allow = false, Reason = "We dont crawl links of external pages" };

                return new CrawlDecision { Allow = true };
            });

            return crawler;
        }

        private static Uri GetSiteToCrawl(string[] args)
        {
            string userInput = "";
            if (args.Length < 1)
            {
                System.Console.WriteLine("Please enter ABSOLUTE url to crawl:");
                userInput = System.Console.ReadLine();
            }
            else
            {
                userInput = args[0];
            }

            if (string.IsNullOrWhiteSpace(userInput))
                throw new ApplicationException("Site url to crawl is as a required parameter");

            return new Uri(userInput);
        }

        private static void PrintDisclaimer()
        {
            PrintAttentionText("The demo is configured to only crawl a total of 10 pages and will wait 1 second in between http requests. This is to avoid getting you blocked by your isp or the sites you are trying to crawl. You can change these values in the app.config or Abot.Console.exe.config file.");
        }

        private static void PrintAttentionText(string text)
        {
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine(text);
            System.Console.ForegroundColor = originalColor;
        }

        static void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            //Process data
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("About to crawl link {0} which was found on page {1}", pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri);
        }

        static void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            //Process data
            //CrawledPage crawledPage = e.CrawledPage;
            //if (crawledPage.WebException != null || crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
            //    Console.WriteLine("Crawl of page failed {0}", crawledPage.Uri.AbsoluteUri);
            //else
            //    Console.WriteLine("Crawl of page succeeded {0}", crawledPage.Uri.AbsoluteUri);
            //if (string.IsNullOrEmpty(crawledPage.Content.Text))
            //    Console.WriteLine("Page had no content {0}", crawledPage.Uri.AbsoluteUri);

            //判断是否是新浪微博个人用户页面
            if (WeiboAccountRegex.IsMatch(e.CrawledPage.Uri.AbsoluteUri))
            {
                var cqDocument = e.CrawledPage.CsQueryDocument;
                var csUserName = cqDocument.Select(".pf_username > h1");
                var csTitle = cqDocument.Select(".pf_intro");
                var csImage = cqDocument.Select(".photo_wrap>img");
                if (csUserName[0] != null)
                {
                    string userName = HtmlData.HtmlDecode(csUserName[0].InnerText);
                    string title = csTitle[0] != null ? HtmlData.HtmlDecode(csTitle[0].GetAttribute("title")) : string.Empty;
                    string imageUrl = csImage[0] != null ? HtmlData.HtmlDecode(csImage[0].GetAttribute("src")) : string.Empty;

                    string basicInformation = string.Format("UserName: {0}; Brief:: {1}; Image Url: {2}", userName, title, imageUrl);

                    var str = (e.CrawledPage.Uri.AbsoluteUri + "\t" + basicInformation + "\r\n");
                    System.IO.File.AppendAllText("fake.txt", str);
                }
            }


            //////判断是否是新闻详细页面
            //if (NewsUrlRegex.IsMatch(e.CrawledPage.Uri.AbsoluteUri))
            //{
            //    //获取信息标题和发表的时间
            //    var csTitle = e.CrawledPage.CsQueryDocument.Select("#news_title");
            //    var linkDom = csTitle.FirstElement().FirstChild;

            //    var newsInfo = e.CrawledPage.CsQueryDocument.Select("#news_info");
            //    var dateString = newsInfo.Select(".time", newsInfo);


            //    var str = (e.CrawledPage.Uri.AbsoluteUri + "\t" + HtmlData.HtmlDecode(linkDom.InnerText) + "\r\n");
            //    System.IO.File.AppendAllText("fake.txt", str);
            //}


        }

        static bool IsPublishToday(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            const string prefix = "发布于";
            int index = str.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                str = str.Substring(prefix.Length).Trim();
            }

            DateTime date;
            return DateTime.TryParse(str, out date) && date.Date.Equals(DateTime.Today);
        }

        static void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            //Process data
            CrawledPage crawledPage = e.CrawledPage;
            Console.WriteLine("Did not crawl the links on page {0} due to {1}", crawledPage.Uri.AbsoluteUri, e.DisallowedReason);
        }

        static void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            //Process data
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("Did not crawl page {0} due to {1}", pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason);
        }
    }
}
