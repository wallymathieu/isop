using System;
using Xamarin.Forms;
using Isop.Client.Transfer;

namespace Isop.Xamarin
{
    public class SelectControllerPage : MasterDetailPage
    {
        public SelectControllerPage()
        {
            Label header = new Label
                {
                    Text = "Select controller",
                    HorizontalOptions = LayoutOptions.Center
                };
            var controllers = new []{
                new Controller(){
                    Name="test ",
                },
                new Controller(){
                    Name="test 2"
                }
            };
            // Create ListView for the master page.
            ListView listView = new ListView
                {
                    ItemsSource = controllers
                };
            this.Master = new ContentPage
                {
                    Title = header.Text,
                    Content = new StackLayout
                        {
                            Children = 
                                {
                                    header, 
                                    listView
                                }
                        }
                };
            Binding binding = new Binding() { Path = "Name" };
            var nameLabel = new Label(){ Text="Name" };
            //nameLabel.SetBinding(BindableProperty.CreateReadOnly(), Binding.Create());
            // Create the detail page using NamedColorPage and wrap it in a
            // navigation page to provide a NavigationBar and Toggle button
            this.Detail = new NavigationPage(new ContentPage{ Content= new StackLayout
                { 
                    Children =
                        {
                            new Label { Text = "Select command placeholder" },
                                nameLabel
                        }
                }
            });
            this.IsPresented = false;
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
            listView.ItemSelected += (sender, args) =>
                {
                    // Set the BindingContext of the detail page.
                    this.Detail.BindingContext = args.SelectedItem;

                    // Show the detail page.
                    this.IsPresented = false;
                };

            // Initialize the ListView selection.
            listView.SelectedItem = new Controller[0];

        }
    }

}



