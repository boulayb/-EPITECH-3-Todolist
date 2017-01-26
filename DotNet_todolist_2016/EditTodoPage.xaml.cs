using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DotNet_todolist_2016
{
    public sealed partial class EditTodoPage : Page
    {
        public EditTodoPage()
        {
            this.InitializeComponent();
            var obj = App.Current as App;
            TitleBox.Text = obj.Selected.Title;
            DescriptionBox.Text = obj.Selected.Description;
            DueDate.Date = obj.Selected.DueDate.Date;
        }

        private async void DeleteTodo_OnClick(object sender, RoutedEventArgs e)
        {
            var title = "Delete this todo?";
            var content = "Do you really want to delete this todo?";
            var yesCommand = new UICommand("Yes");
            var noCommand = new UICommand("No");

            var dialog = new MessageDialog(content, title);
            dialog.Options = MessageDialogOptions.None;
            dialog.Commands.Add(yesCommand);
            dialog.Commands.Add(noCommand);
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 0;

            var command = await dialog.ShowAsync();

            if (command == yesCommand)
            {
                var obj = App.Current as App;
                obj.Conn.Query<ToDo>("DELETE FROM ToDo WHERE Id =" + obj.Selected.Id);
                Frame.Navigate(typeof(MainPage));
            }
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

            obj.Selected.Title = TitleBox.Text;
            obj.Selected.DueDate = DueDate.Date.Value;
            obj.Selected.Description = DescriptionBox.Text;

            obj.Conn.RunInTransaction(() =>
            {
                obj.Conn.Update(obj.Selected);
            });

            Frame.Navigate(typeof(MainPage));
        }
    }
}
