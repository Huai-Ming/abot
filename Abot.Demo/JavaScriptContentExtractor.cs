using System.Net;
using System.Text;
using Abot.Core;
using Abot.Poco;
using OpenQA.Selenium;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace Abot.Demo
{
    public class JavaScriptContentExtractor : WebContentExtractor
    {
        private readonly IWebDriver m_WebDriver;

        private List<OpenQA.Selenium.Cookie> allCookies = new List<OpenQA.Selenium.Cookie>();


        public JavaScriptContentExtractor(IWebDriver p_WebDriver)
        {
            m_WebDriver = p_WebDriver;
        }

        public override PageContent GetContent(WebResponse p_Response)
        {

            //////////////////////////////////////////////////////////
            //Add cookie info the the request
            //foreach (System.Net.Cookie cookie in Coo)
            //{
            //    OpenQA.Selenium.Cookie cookieS = new OpenQA.Selenium.Cookie(cookie.Name,cookie.Value,cookie.Path,cookie.Expires);
            //    m_WebDriver.Manage().Cookies.AddCookie(cookieS);
            //}

            // Navigate to the requested page using the WebDriver. PhantomJS will navigate to the page
            // just like a normal browser and the resulting html will be set in the PageSource property.
            m_WebDriver.Navigate().GoToUrl(p_Response.ResponseUri);

            // Let the JavaScript execute for a while if needed, for instance if the pages are doing async calls.
            //Thread.Sleep(1000);

            // Try to retrieve the charset and encoding from the response or body.
            string pageBody = m_WebDriver.PageSource;
            string charset = GetCharsetFromHeaders(p_Response);
            if (charset == null)
            {
                charset = GetCharsetFromBody(pageBody);
            }

            Encoding encoding = GetEncoding(charset);

            PageContent pageContent = new PageContent
            {
                Encoding = encoding,
                Charset = charset,
                Text = pageBody,
                Bytes = encoding.GetBytes(pageBody)
            };

            return pageContent;
        }

        public override PageContent GetContent(WebResponse p_Response, List<System.Net.Cookie> cookies)
        {
            //Add cookie info the the request



            // Navigate to the requested page using the WebDriver. PhantomJS will navigate to the page
            // just like a normal browser and the resulting html will be set in the PageSource property.
           

            m_WebDriver.Navigate().GoToUrl(p_Response.ResponseUri);
            // Let the JavaScript execute for a while if needed, for instance if the pages are doing async calls.
            //Thread.Sleep(1000);

            // Try to retrieve the charset and encoding from the response or body.
            string pageBody = m_WebDriver.PageSource;
            string charset = GetCharsetFromHeaders(p_Response);
            if (charset == null)
            {
                charset = GetCharsetFromBody(pageBody);
            }

            Encoding encoding = GetEncoding(charset);

            PageContent pageContent = new PageContent
            {
                Encoding = encoding,
                Charset = charset,
                Text = pageBody,
                Bytes = encoding.GetBytes(pageBody)
            };

            return pageContent;
        }

        public override void Dispose()
        {
            // Important! Dispose the web driver so that no process are left running in the background.
            if (m_WebDriver != null)
                m_WebDriver.Dispose();
        }
    }
}
