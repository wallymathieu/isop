using System;
using Xamarin.Forms;

namespace Isop.Xamarin
{
    public class SelectMethodPage: MasterDetailPage
    {
        public SelectMethodPage()
        {
            Label header = new Label
                {
                    Text = "Select method",
                    HorizontalOptions = LayoutOptions.Center
                };
            // Create ListView for the master page.
            ListView methodsView = new ListView();
            methodsView.SetBinding(
                ListView.ItemsSourceProperty,
                Binding.Create<ControllerViewModel>(c => c.Methods));
            this.Master = new ContentPage
                {
                    Title = header.Text,
                    Content = new StackLayout
                        {
                            Children = 
                                {
                                    header, 
                                    methodsView
                                }
                            }
                };
            // Create the detail page using NamedColorPage and wrap it in a
            // navigation page to provide a NavigationBar and Toggle button
            this.Detail = new NavigationPage(new ExecuteCommandPage());

            this.IsPresented = true;
            // For Windows Phone, provide a way to get back to the master page.
            if (Device.OS == TargetPlatform.WinPhone)
            {
                (this.Detail as ContentPage).Content.GestureRecognizers.Add(
                    new TapGestureRecognizer
                    {

                        Command = new Command( () => 
                            {
                                this.IsPresented = true;
                            })
                    });
            }

            // Define a selected handler for the ListView.
            methodsView.ItemSelected += (sender, args) =>
                {
                    // Set the BindingContext of the detail page.
                    this.Detail.BindingContext = args.SelectedItem;

                    // Show the detail page.
                    this.IsPresented = false;
                };

            // Initialize the ListView selection.
            methodsView.SelectedItem = new MethodViewModel[0];
        }
    }
}

