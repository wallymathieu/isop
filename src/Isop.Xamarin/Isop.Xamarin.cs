using System;

using Xamarin.Forms;

namespace Isop.Xamarin
{
    public class App : Application
    {
        public App()
        {
            //var c = new Isop.Client.IsopClient(new Isop.Client.Json.JsonHttpClient(),"");
            //new RootViewModel(new JsonClient(c))
            // The root page of your application
            var appvm = new AppNavigation();
            var main = new NavigationPage(new SplashPage(appvm));
            MainPage = main;
            appvm.NavigationPage = main;
            this.BindingContext = appvm;

        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}

