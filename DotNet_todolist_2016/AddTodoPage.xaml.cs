using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DotNet_todolist_2016
{

    public sealed partial class AddTodoPage : Page
    {
        public AddTodoPage()
        {
            this.InitializeComponent();
            this.DueDate.MinDate = DateTime.Now;
        }

        private void BackToMain_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void ValidToDo_OnClick(object sender, RoutedEventArgs e)
        {
            if (TitleBox.Text.Length <= 0)
            {
                FormError.Text = "Please set a title for this todo.";
                FormError.Visibility = Visibility.Visible;
                return;
            }
            else if (DescriptionBox.Text.Length <= 0)
            {
                FormError.Text = "Please set a description for this todo.";
                FormError.Visibility = Visibility.Visible;
                return;
            }
            else if (!DueDate.Date.HasValue)
            {
                FormError.Text = "Please set a due date for this todo.";
                FormError.Visibility = Visibility.Visible;
                return;
            }

            var obj = App.Current as App;
            obj.Conn.Insert(new ToDo()
            {
                Title = TitleBox.Text,
                DueDate = DueDate.Date.Value,
                Description = DescriptionBox.Text,
                Done = false
            });

            Frame.Navigate(typeof(MainPage));
        }
    }
}
