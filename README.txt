Mashape Facebook Sentiment example (Windows 8) - http://www.youtube.com/watch?v=4cLe_l8l2Jc

INSTRUCTIONS: Ask chris@mashape.com for access to the Facebook Graph API in Mashape.  Fill in the details in App.xaml.cs for 1 - 3 below, and press F5.  

//1. Create your Facebook app in http://developer.facebook.com/apps then get the values to plug below
//Point your FB app's site url to https://www.mashape.com/oauth/2/callback
        public static string ConsumerKey = "";
        public static string ConsumerSecret = "";
        
//2. This is where your app will be redirected to (behind the scenes) so you can retrieve the AccessToken from the URL parameter
        public static string CallbackUrl = "http://chrispogi.wordpress.com/"; //in this case it doesn't matter where you point this

//3. Get your Mashape header value at https://www.mashape.com/keys

        public static string MashapeHeader = "";

 THAT'S IT.  HIT F5