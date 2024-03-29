﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Authentication.Web;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace MashapeOAuthRTExample
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : MashapeOAuthRTExample.Common.LayoutAwarePage
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        public async Task MashapeFacebookLogin()
        {
            //NOTE: Check App.xaml.cs for the variables that you need to fill in

            var targeturi = "https://ismaelc-facebooktest.p.mashape.com/oauth_url";

            var client = new System.Net.Http.HttpClient();

            HttpContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("consumerKey", App.ConsumerKey),
                new KeyValuePair<string, string>("consumerSecret", App.ConsumerSecret),
                new KeyValuePair<string, string>("callbackUrl", App.CallbackUrl)
            });

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            content.Headers.Add("X-Mashape-Authorization", App.MashapeHeader);

            progressRing.IsActive = true;
            btnLogin.IsEnabled = false;
            var response = await client.PostAsync(targeturi, content);
            progressRing.IsActive = false;
            btnLogin.IsEnabled = true;

            if (response.IsSuccessStatusCode)
            {
                string respContent = await response.Content.ReadAsStringAsync();
                string loginUrl = await Task.Run(() => JsonObject.Parse(respContent).GetNamedString("url"));

                WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(
                                                        WebAuthenticationOptions.None,
                                                        new Uri(loginUrl),
                                                        new Uri(App.CallbackUrl));

                
                if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    btnLogin.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    var callBackUrl = WebAuthenticationResult.ResponseData.ToString();
                    //Where's ParseQueryString when you need it...
                    App.AccessToken = GetParameter(callBackUrl, "accessToken");
                    //App.AccessSecret = GetParameter(callBackUrl, "accessSecret");

                }
                else if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    throw new InvalidOperationException("HTTP Error returned by AuthenticateAsync() : " + WebAuthenticationResult.ResponseErrorDetail.ToString());
                }
                else
                {
                    // The user canceled the authentication
                }
            }
            else
            {
                //The POST failed, so update the UI accordingly
                //txtBlockResult.Text = "Error";
            }
        }

        #region TwitterScratch
        //public async Task MashapeTwitterSearch()
        //{
        //    var targeturi = "https://twitter.p.mashape.com/1.1/search/tweets.json?q=" + searchBox.Text;

        //    var client = new System.Net.Http.HttpClient();

        //    var request = new HttpRequestMessage(HttpMethod.Get, targeturi);

        //    request.Headers.Add("X-Mashape-Authorization", App.MashapeHeader);
        //    request.Headers.Add("X-Mashape-OAuth-ConsumerKey", App.ConsumerKey);
        //    request.Headers.Add("X-Mashape-OAuth-ConsumerSecret", App.ConsumerSecret);
        //    request.Headers.Add("X-Mashape-OAuth-AccessToken", App.AccessToken);
        //    request.Headers.Add("X-Mashape-OAuth-AccessSecret", App.AccessSecret);

        //    progressRing.IsActive = true;
        //    searchButton.IsEnabled = false;
        //    var response = await client.SendAsync(request);
        //    progressRing.IsActive = false;
        //    searchButton.IsEnabled = true;

        //    if (response.IsSuccessStatusCode)
        //    {
        //        string respContent = await response.Content.ReadAsStringAsync();

        //        var jObjectArr = await Task.Run(() => JsonObject.Parse(respContent).GetNamedArray("statuses"));

        //        List<ExpandoObject> list = new List<ExpandoObject>();

        //        foreach (JsonValue jObjectValue in jObjectArr)
        //        {
        //            JsonObject jObject = jObjectValue.GetObject();

        //            dynamic tempObject = new ExpandoObject();
        //            tempObject.text = jObject.GetNamedValue("text").GetString();
        //            tempObject.handle = jObject.GetNamedObject("user").GetNamedValue("screen_name").GetString();
        //            tempObject.photo = jObject.GetNamedObject("user").GetNamedValue("profile_image_url").GetString();

        //            list.Add(tempObject);
        //        }

        //        listViewResult.DataContext = list;

        //    }
        //    else
        //    {
        //        //The POST failed, so update the UI accordingly
        //        //txtBlockResult.Text = "Error";
        //    }
        //}
        #endregion

        public async Task MashapeFqlCall()
        {
            var fql = "select uid, message from status where uid in (select uid2 from friend where uid1=me())";
            var targeturi = "https://ismaelc-facebooktest.p.mashape.com/fql?q=" + fql;
            targeturi += "&access_token=" + App.AccessToken;

            var client = new System.Net.Http.HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Get, targeturi);

            request.Headers.Add("X-Mashape-Authorization", App.MashapeHeader);

            progressRing.IsActive = true;
            
            var response = await client.SendAsync(request);
            progressRing.IsActive = false;
            

            if (response.IsSuccessStatusCode)
            {
                string respContent = await response.Content.ReadAsStringAsync();

                var jObjectArr = await Task.Run(() => JsonObject.Parse(respContent).GetNamedArray("data"));

                List<ExpandoObject> list = new List<ExpandoObject>();

                foreach (JsonValue jObjectValue in jObjectArr)
                {
                    JsonObject jObject = jObjectValue.GetObject();

                    dynamic tempObject = new ExpandoObject();
                    tempObject.text = jObject.GetNamedValue("message").GetString();
                    //tempObject.handle = jObject.GetNamedObject("user").GetNamedValue("screen_name").GetString();
                    //tempObject.photo = jObject.GetNamedObject("user").GetNamedValue("profile_image_url").GetString();
                    tempObject.photo = "http://graph.facebook.com/" + Math.Truncate(jObject.GetNamedValue("uid").GetNumber()).ToString() + "/picture";

                    list.Add(tempObject);
                }

                listViewResult.DataContext = list;
                listViewResult.Visibility = Windows.UI.Xaml.Visibility.Visible;

                borderSentimentResult.Visibility = Windows.UI.Xaml.Visibility.Visible;

            }
            else
            {
                //The POST failed, so update the UI accordingly
                //txtBlockResult.Text = "Error";
            }
        }

        // Piece of crap that works
        public static string GetParameter(string url, string key)
        {
            string desiredValue;
            foreach(string item in url.Split('&'))
            {
                string[] parts = item.Replace("?", "").Split('=');
                if(parts[0].Contains(key))
                {
	                desiredValue = parts[1];
	                return desiredValue;
                }
            }

            return "";
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            await MashapeFacebookLogin();
            if (App.AccessToken != "") await MashapeFqlCall();
        }

        #region WIP
        public static byte[] StringToAscii(string s)
        {
            byte[] retval = new byte[s.Length];
            for (int ix = 0; ix < s.Length; ++ix)
            {
                char ch = s[ix];
                if (ch <= 0x7f) retval[ix] = (byte)ch;
                else retval[ix] = (byte)'?';
            }
            return retval;
        }

        private static String hmac_sha1(String publicKey, String privateKey)
        {
            var signatureBase = publicKey;
            var keyMaterial = CryptographicBuffer.ConvertStringToBinary(privateKey, BinaryStringEncoding.Utf8); 
            var algorithm = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
            var key = algorithm.CreateKey(keyMaterial);

            var dataToBeSigned = CryptographicBuffer.ConvertStringToBinary(signatureBase, BinaryStringEncoding.Utf8);
            var signatureBuffer = CryptographicEngine.Sign(key, dataToBeSigned);
            var signature = CryptographicBuffer.EncodeToBase64String(signatureBuffer).Replace("-","").ToLower();

            return signature;
        }

        private static string MashapeHeaderValue(string publicKey, string privateKey)
        {
            string hash = hmac_sha1(publicKey, privateKey);
            //System.Convert.ToBase64String((publicKey + ":" + hash));
            return "To be continued";
        }
        #endregion

        private async void listViewResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dynamic selectedObj = listViewResult.SelectedItem as dynamic;
            string text = selectedObj.text;
            await CallSentimentAnalysis("en", text, "0");
        }

        private async Task CallSentimentAnalysis(string lang, string text, string detectlang)
        {
            var targeturi = "https://chatterbox-analytics-sentiment-analysis-free.p.mashape.com/sentiment/current/classify_text/";

            var client = new System.Net.Http.HttpClient();

            HttpContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("lang", lang),
                new KeyValuePair<string, string>("text", text),
                new KeyValuePair<string, string>("detectlang", detectlang)
            });

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.Add("X-Mashape-Authorization", App.MashapeHeader);

            progressRing.IsActive = true;

            var response = await client.PostAsync(targeturi, content);

            progressRing.IsActive = false;

            if (response.IsSuccessStatusCode)
            {
                string respContent = await response.Content.ReadAsStringAsync();
                textBoxResult.Text = respContent;

            }
            else
            {
                //The POST failed, so update the UI accordingly
                //txtBlockResult.Text = "Error";
            }

        }



    }
}
