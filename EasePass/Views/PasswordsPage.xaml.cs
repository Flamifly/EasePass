using EasePass.Dialogs;
using EasePass.Helper;
using EasePass.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

namespace EasePass.Views
{
    public sealed partial class PasswordsPage : Page
    {
        private ObservableCollection<PasswordManagerItem> PasswordItems = new ObservableCollection<PasswordManagerItem>();

        private PasswordManagerItem SelectedItem = null;
        private SecureString masterPassword = null;

        public PasswordsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter == null)
            {
                return;
            }

            if(e.Parameter is SecureString pw)
            {
                masterPassword = pw;
                LoadData(masterPassword);
            }

            base.OnNavigatedTo(e);
        }

        private void LoadData(SecureString pw)
        {
            var data = DatabaseHelper.LoadDatabase(pw);
            if (data == null)
                return;

            PasswordItems = data;
        }
        private void SaveData()
        {
            DatabaseHelper.SaveDatabase(PasswordItems, masterPassword);
        }

        private void ShowPasswordItem(PasswordManagerItem item)
        {
            notesTB.Text = item.Notes;
            pwTB.Password = item.Password;
            emailTB.Text = item.Email;
            usernameTB.Text = item.Username;
            itemnameTB.Text = item.DisplayName;
            iconFI.Glyph = item.IconId;

            passwordShowArea.Visibility = Visibility.Visible;
        }


        private void passwordItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (passwordItemListView.SelectedItem == null)
                return;

            if (passwordItemListView.SelectedItem is PasswordManagerItem pwItem)
            {
                SelectedItem = pwItem;
                ShowPasswordItem(pwItem);
            }
        }
        private async void AddPasswordItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var newItem = await new AddItemDialog().ShowAsync(PasswordItems);
            if (newItem == null)
                return;

            PasswordItemsManager.AddItem(PasswordItems, newItem);
            SaveData();
        }
        private async void EditPasswordItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem == null)
                return;

            var editItem = await new EditItemDialog().ShowAsync(PasswordItems, SelectedItem);
            if (editItem == null)
                return;

            SaveData();
        }
        private void searchbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(searchbox.Text == "")
            {
                passwordItemListView.ItemsSource = PasswordItems;
                return;
            }
            passwordItemListView.ItemsSource = PasswordItemsManager.FindItemsByName(PasswordItems, searchbox.Text);
        }
        private void ShowPassword_Changed(object sender, RoutedEventArgs e)
        {
            if(sender is CheckBox cb)
                pwTB.PasswordRevealMode = cb.IsChecked ?? false ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
        }
        private void pwTB_PreviewKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            e.Handled = true;
            if (e.Key == VirtualKey.A && KeyHelper.IsKeyPressed(VirtualKey.Control))
                pwTB.SelectAll();
            else if(e.Key == VirtualKey.C && KeyHelper.IsKeyPressed(VirtualKey.Control))
                ClipboardHelper.Copy(pwTB.Password);
        }
        private void TB_SelectAl_Click(object sender, RoutedEventArgs e)
        {
            if(sender is TextBox tb)
                tb.SelectAll();
            else if(sender is PasswordBox pb) 
                pb.SelectAll();
        }
        private void TB_Copy_Click(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
                ClipboardHelper.Copy(tb.Text);
            else if (sender is PasswordBox pb)
                ClipboardHelper.Copy(pb.Password);
        }

        private async void DeletePasswordItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem == null)
                return;

            if(await new DeleteConfirmationDialog().ShowAsync(SelectedItem))
            {
                PasswordItemsManager.DeleteItem(PasswordItems, SelectedItem);
            }
        }
    }
}
