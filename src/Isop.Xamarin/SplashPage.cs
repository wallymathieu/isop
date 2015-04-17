using System;

using Xamarin.Forms;

namespace Isop.Xamarin
{
    public class SplashPage : ContentPage
    {
        public SplashPage(AppNavigation navigation)
        {
            TableView tableView = new TableView
                {
                    Intent = TableIntent.Menu,
                    Root = new TableRoot
                        {
                            new TableSection
                            {
                                new TextCell
                                {
                                    Text = "Settings",
                                    Command = new Command(navigation.ShowSettings)
                                }
                            },
                            new TableSection
                            {
                                new TextCell
                                {
                                    Text = "Controllers",
                                    Command = new Command(navigation.ShowControllers)
                                }
                            }
                        }
                };
            // should be a 
            Content = new StackLayout
                { 
                    Children = { tableView }
                };
        }
    }
}


