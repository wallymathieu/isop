using System;

using Xamarin.Forms;

namespace Isop.Xamarin
{
    public class ExecuteCommandPage : ContentPage
    {
        public ExecuteCommandPage()
        {
            Content = new StackLayout
            { 
                Children =
                {
                    new Label { Text = "Command" },
                    new Button{ Text = "Exec"}
                }
            };
        }
    }
}


