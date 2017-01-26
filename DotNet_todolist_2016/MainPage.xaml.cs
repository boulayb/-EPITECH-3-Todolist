using System;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using SQLite.Net.Attributes;

namespace DotNet_todolist_2016
{
    public sealed partial class MainPage : Page
    {
        public DisplayTodo selected;
        private static DateTimeOffset dateTmp = DateTimeOffset.Now;
   
        public MainPage()
        {
            this.InitializeComponent();
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            var obj = App.Current as App;
            obj.Conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);
            obj.Conn.CreateTable<ToDo>();
        }

        private void AddTodo_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddTodoPage));
        }

        private void EditTodo_OnClick(object sender, RoutedEventArgs e)
        {
            var obj = App.Current as App;
            obj.Selected = obj.Conn.Query<ToDo>("SELECT * FROM ToDo WHERE Id =" + this.selected.Id).FirstOrDefault();
            Frame.Navigate(typeof(EditTodoPage));
        }

        private void StatusTodo_OnClick(object sender, RoutedEventArgs e)
        {
            var obj = App.Current as App;
            ToDo tmp = obj.Conn.Query<ToDo>("SELECT * FROM ToDo WHERE Id =" + this.selected.Id).FirstOrDefault();
            tmp.Done = !tmp.Done;
            obj.Conn.RunInTransaction(() =>
            {
                obj.Conn.Update(tmp);
            });
            Frame.Navigate(typeof(MainPage));
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
                obj.Conn.Query<ToDo>("DELETE FROM ToDo WHERE Id =" + this.selected.Id);
                Frame.Navigate(typeof(MainPage));
            }
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var obj = App.Current as App;
            var query = obj.Conn.Table<ToDo>();

            foreach (var todo in query)
            {
                Grid grid;
                CalendarDatePicker dueDate;
                TextBlock message;
                       
                TodoList.Children.Add(grid = new DisplayTodo
                {
                    Background = new SolidColorBrush(Colors.Gray),
                    Width = 150,
                    Height = 150,
                    Margin = new Thickness(50, 50, 0, 0),
                    Id = todo.Id,
                    Done = todo.Done
                });

                grid.Children.Add(new TextBlock
                {
                    Text = todo.Title,
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeights.ExtraBlack,
                    TextWrapping = TextWrapping.Wrap,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxHeight = 50,
                    Margin = new Thickness(0, 0, 0, 75)
                });

                grid.Children.Add(dueDate = new CalendarDatePicker
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Date = todo.DueDate,
                });
                dueDate.DateChanged += new TypedEventHandler<CalendarDatePicker, CalendarDatePickerDateChangedEventArgs>(dueDate_OnDateChanged);

                grid.Children.Add(message = new TextBlock
                {
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeights.ExtraBlack,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 100, 0, 0),
                    Visibility = Visibility.Collapsed
                });

                if (todo.Done)
                {
                    message.Text = "This todo is done.";
                    message.Foreground = new SolidColorBrush(Colors.GreenYellow);
                    message.Visibility = Visibility.Visible;
                    dueDate.Foreground = new SolidColorBrush(Colors.GreenYellow);
                }
                else if (todo.DueDate <= DateTimeOffset.Now)
                {
                    message.Text = "This todo due date has passed.";
                    message.Foreground = new SolidColorBrush(Colors.Red);
                    message.Visibility = Visibility.Visible;
                }

                grid.PointerPressed += new PointerEventHandler(grid_OnClick);
            }
        }

        private void grid_OnClick(object obj, PointerRoutedEventArgs ev)
        {
            if (selected != null)
                this.selected.Background = new SolidColorBrush(Colors.Gray);
            this.selected = (DisplayTodo)obj;
            this.selected.Background = new SolidColorBrush(Colors.DarkGray);
            EditTodo.Visibility = Visibility.Visible;
            DeleteTodo.Visibility = Visibility.Visible;
            StatusTodo.Visibility = Visibility.Visible;
        }

        private void dueDate_OnDateChanged(CalendarDatePicker datePicker, CalendarDatePickerDateChangedEventArgs dateChanged)
        {
            if (dateTmp == dateChanged.NewDate)
                return;
            dateTmp = dateChanged.OldDate.Value;
            datePicker.Date = dateChanged.OldDate;
        }

    }

    public class DisplayTodo : Grid
    {
        public int Id { get; set; }
        public bool Done { get; set; }
        public string Title { get; set; }
        public DateTimeOffset DueDate { get; set; }
        public string Description { get; set; }
    }

    public class ToDo
    {
        [PrimaryKey, AutoIncrement]

        public int Id { get; set; }
        public bool Done { get; set; }
        public string Title { get; set; }
        public DateTimeOffset DueDate { get; set; }
        public string Description { get; set; }
    }
}
