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
                    MessageBox.Show($"File decrypted successfully!\n\nSaved at:\n{outputFile}", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string outputFile = selectedFilePath + ".vault";
                    await CryptoEngine.EncryptFileAsync(selectedFilePath, outputFile, password, progressHandler);
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
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}