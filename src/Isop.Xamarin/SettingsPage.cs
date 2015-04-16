using System;

using Xamarin.Forms;

namespace Isop.Xamarin
{
    public class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            TableView tableView = new TableView
                {
                    Intent = TableIntent.Settings,
                    Root = new TableRoot
                        {
                            new TableSection
                            {
                                new EntryCell
                                {
                                    Label = "Url",
                                    Placeholder = "Type The Url To Admin Api"
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


