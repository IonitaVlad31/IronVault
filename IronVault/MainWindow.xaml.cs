using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Configuration.Internal;

namespace IronVault
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public string selectedFilePath = "";

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a file to encrypt or decrypt";

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;
                FilePathTextBox.Text = selectedFilePath;
                UpdateCheckBoxState();
            }
        }

        private void UpdateCheckBoxState()
        {
            if (selectedFilePath.EndsWith(".vault", StringComparison.OrdinalIgnoreCase))
            {
                DeleteOriginalCheckBox.IsEnabled = false;
                DeleteOriginalCheckBox.IsChecked = false;
            }
            else
            {
                DeleteOriginalCheckBox.IsEnabled = true;
            }
        }

        private async void GoButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                MessageBox.Show("Please select a file first!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter a password!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int currentScore = CalculatePasswordScore(password);

            if (currentScore < 40)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Your password is weak. Do you want to proceed anyway?",
                    "Weak Password Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            GoButton.IsEnabled = false;
            EncryptionProgressBar.Value = 0;

            var progressHandler = new Progress<double>(value =>
            {
                EncryptionProgressBar.Value = value;
            });

            try
            {
                if (selectedFilePath.EndsWith(".vault"))
                {
                    string outputFile = selectedFilePath.Replace(".vault", "");
                    await CryptoEngine.DecryptFileAsync(selectedFilePath, outputFile, password, progressHandler);

                    if (DeleteOriginalCheckBox.IsChecked == true)
                    {
                        System.IO.File.Delete(selectedFilePath);
                    }

                    MessageBox.Show($"File decrypted successfully!\n\nSaved at:\n{outputFile}", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string outputFile = selectedFilePath + ".vault";
                    await CryptoEngine.EncryptFileAsync(selectedFilePath, outputFile, password, progressHandler);
                    
                    if (DeleteOriginalCheckBox.IsChecked == true)
                    {
                        System.IO.File.Delete(selectedFilePath);
                    }
                    
                    MessageBox.Show($"File encypted successfully!\n\nSaved at:\n{outputFile}", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (CryptographicException)
            {
                MessageBox.Show("Wrong password or corrupted file!", "Decryption Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                GoButton.IsEnabled = true;
                EncryptionProgressBar.Value = 0;

                FilePathTextBox.Text = string.Empty;
                PasswordBox.Password = string.Empty;
                PasswordVisibleBox.Text = string.Empty;

                DeleteOriginalCheckBox.IsChecked = false;
                DeleteOriginalCheckBox.IsEnabled = true;

                selectedFilePath = "";
            }
        }

        private void FilePathTextBox_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                FilePathTextBox.Background = Brushes.LightGray;
            }
        }

        private void FilePathTextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void FilePathTextBox_PreviewDragLeave(object sender, DragEventArgs e)
        {
            FilePathTextBox.Background = Brushes.White;
        }

        private void FilePathTextBox_Drop(object sender, DragEventArgs e)
        {
            FilePathTextBox.Background = Brushes.White;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files != null && files.Length > 0)
                {
                    selectedFilePath = files[0];
                    FilePathTextBox.Text = selectedFilePath;
                    UpdateCheckBoxState();
                }
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string password = PasswordBox.Password;
            UpdatePasswordStrength(password);
        }

        private void UpdatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                PasswordStrengthBar.Value = 0;
                PasswordStrengthText.Text = "Enter a password";
                PasswordStrengthText.Foreground = Brushes.DimGray;
                PasswordStrengthBar.Foreground = Brushes.DimGray;
                return;
            }

            int score = CalculatePasswordScore(password);
            PasswordStrengthBar.Value = score;

            if (score < 20)
            {
                PasswordStrengthBar.Foreground = Brushes.Red;
                PasswordStrengthText.Text = "Very Weak password (Please introduce a minimum of 8 characters)";
                PasswordStrengthText.Foreground = Brushes.Red;
            }
            else if (score < 40)
            {
                PasswordStrengthBar.Foreground = Brushes.DarkOrange;
                PasswordStrengthText.Text = "Weak password (Add numbers/symbols/lowercase/uppercase)";
                PasswordStrengthText.Foreground = Brushes.DarkOrange;
            }
            else if (score < 60)
            {
                PasswordStrengthBar.Foreground = Brushes.Gold;
                PasswordStrengthText.Text = "Fair password (Add numbers/symbols/lowercase/uppercase for more safety)";
                PasswordStrengthText.Foreground = Brushes.Gold;
            }
            else if (score < 90)
            {
                PasswordStrengthBar.Foreground = Brushes.LimeGreen;
                PasswordStrengthText.Text = "Good password";
                PasswordStrengthText.Foreground = Brushes.LimeGreen;

            }
            else
            {
                PasswordStrengthBar.Foreground = Brushes.ForestGreen;
                PasswordStrengthText.Text = "Strong password!";
                PasswordStrengthText.Foreground = Brushes.ForestGreen;
            }
        }

        private int CalculatePasswordScore(string password)
        {
            if (string.IsNullOrEmpty(password)) return 0;

            int score = 0;

            score += Math.Min(50, password.Length * 6);

            if (password.Length < 8)
                score -= 20;
            
            bool hasLower = false, hasUpper = false, hasDigit = false, hasSymbol = false;
            int typesCount = 0;

            foreach (char c in password)
            {
                if (char.IsLower(c)) hasLower = true;
                else if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (!char.IsWhiteSpace(c)) hasSymbol = true;
            }

            if (hasLower) { score += 10; typesCount++; }
            if (hasUpper) { score += 10; typesCount++; }
            if (hasDigit) { score += 10; typesCount++; }
            if (hasSymbol) { score += 10; typesCount++; }

            if (typesCount == 1)
            {
                score = Math.Min(39, score);
            }
            else if (typesCount == 2)
            {
                score = Math.Min(59, score);
            }

            for (int i = 0; i < password.Length - 2; i++)
            {
                if (password[i] == password[i + 1] && password[i + 1] == password[i + 2])
                {
                    score -= 15;
                }
            }

            string lowerPassword = password.ToLower();
            if (lowerPassword.Contains("123") || lowerPassword.Contains("abc") || lowerPassword.Contains("qwe") || lowerPassword.Contains("password") || lowerPassword.Contains("admin"))
            {
                score -= 20;
            }

            return Math.Max(0, Math.Min(100, score));
        }

        private void ShowPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShowPasswordButton.IsChecked == true)
            {
                PasswordVisibleBox.Text = PasswordBox.Password;
                PasswordVisibleBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;

            }
            else
            {
                PasswordBox.Password = PasswordVisibleBox.Text;
                PasswordVisibleBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
            }
        }

        private void PasswordVisibleBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PasswordVisibleBox.Visibility == Visibility.Visible)
            {
                PasswordBox.Password = PasswordVisibleBox.Text;
                UpdatePasswordStrength(PasswordVisibleBox.Text);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}