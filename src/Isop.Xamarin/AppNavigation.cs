using System;
using Xamarin.Forms;

namespace Isop.Xamarin
{
    public class AppNavigation
    {
        public NavigationPage NavigationPage;
        public AppNavigation()
        {
            
        }


        public void ShowSettings(){
            NavigationPage.PushAsync(new SettingsPage());
        }

        public void ShowControllers(){
            NavigationPage.PushAsync(new SelectControllerPage());
        }
    }
}

