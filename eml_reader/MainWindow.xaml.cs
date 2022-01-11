using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;

namespace eml_reader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            listBox.Visibility = Visibility.Hidden;
            bodyBox.Visibility = Visibility.Hidden;
            bodyTitle.Visibility = Visibility.Hidden;
            changeBodyButton.Visibility = Visibility.Hidden;
        }

        bool htmlBodyIsDisplayed = false;

        // Extract the selected file
        bool fileIsSelected = false;
        int fileId = -1;

        private void extractFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();

            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                fileIsSelected = true;
                fileId++;
                listBox.Items.Clear();

                // Get infos from the file

                string FillPath = openFileDlg.FileName;
                var fileInfo = new FileInfo(FillPath);
                var eml = MsgReader.Mime.Message.Load(fileInfo);
                string emlName = openFileDlg.SafeFileName;
                fileNameDisplay.Text = emlName;

                string emlFrom = "none",
                       emlTo = "none",
                       emlSubject = "none",
                       emlHtmlBody = "none",
                       emlSignature = "none",
                       emlRegeXBody = "none",
                       emlAttachmentCount = "none",
                       emlDate = "none",
                       emlHour = "none";

                // HEADER
                if (eml.Headers != null)
                {
                    emlDate =  eml.Headers.DateSent.ToLongDateString();
                    emlHour =  eml.Headers.DateSent.ToShortTimeString();
                    emlFrom = eml.Headers.From.Address;

                    if (eml.Headers.Subject != null)
                    {
                        emlSubject = eml.Headers.Subject;
                    }

                    if (eml.Headers.To != null)
                    {
                        int to = 0;
                        string[] emlCc = new string[eml.Headers.To.Count()];
                        foreach (var recipient in eml.Headers.To)
                        {
                            if (to < 1)
                            {
                                emlTo = recipient.Address;
                            }
                            else
                            {
                                emlCc[to-1] = recipient.Address;
                            }
                            to++;
                        }
                    }
                }

                //BODY
                if (eml.HtmlBody != null)
                {
                    emlHtmlBody = System.Text.Encoding.UTF8.GetString(eml.HtmlBody.Body);
                    emlRegeXBody = Regex.Replace(emlHtmlBody, "<.*?>|&.*?;", string.Empty);
                }

                //ATTACHMENTS
                emlAttachmentCount = eml.Attachments.Count().ToString();
                string[] emlAttachmentName = new string[eml.Attachments.Count()],
                         emlAttachmentType = new string[eml.Attachments.Count()],
                         emlAttachmentEncoding = new string[eml.Attachments.Count()];
                for (int i = 0; i < eml.Attachments.Count; i++)
                {
                    emlAttachmentName[i] = eml.Attachments[i].FileName;
                    emlAttachmentType[i] = eml.Attachments[i].ContentType.MediaType;
                    emlAttachmentEncoding[i] = eml.Attachments[i].BodyEncoding.EncodingName;
                }
                if (eml.SignedBy != null)
                {
                    emlSignature = eml.SignedBy;
                }

                // Store infos to list

                fileList.Add(new fileInfos() 
                { 
                    Name = emlName,
                    Date = emlDate,
                    Hour = emlHour,
                    From = emlFrom,
                    To = emlTo,
                    Subject = emlSubject,
                    AttachmentCount = emlAttachmentCount,
                    AttachmentName = emlAttachmentName,
                    AttachmentType = emlAttachmentType,
                    AttachmentEncoding = emlAttachmentEncoding,
                    RegeXBody = emlRegeXBody,
                    HtmlBody = emlHtmlBody,
                });

                // Display succes message
                listBox.Visibility = Visibility.Visible;

                listBox.Items.Add("Fichier extrait avec succès");
            }
        }

        // Create class for file informations storage

        List<fileInfos> fileList = new List<fileInfos>();

        class fileInfos
        {
            public string? Name { get; set; }
            public string? Date { get; set; }
            public string? Hour { get; set; }
            public string? From { get; set; }
            public string? To { get; set; }
            public string[]? Cc { get; set; }
            public string? Subject { get; set; }
            public string? HtmlBody { get; set; }
            public string? RegeXBody { get; set; }
            public string[]? AttachmentName { get; set; }
            public string[]? AttachmentType { get; set; }
            public string[]? AttachmentContent { get; set; }
            public string[]? AttachmentEncoding { get; set; }
            public string? AttachmentCount { get; set; }
            public string? Signature { get; set; }

        }

        // Clear the result
        private void clearResult(object sender, RoutedEventArgs e)
        {
            bodyTitle.Visibility = Visibility.Hidden;
            listBox.Items.Clear();
            bodyBox.Text = "";
            listBox.Items.Clear(); 
            listBox.Visibility = Visibility.Hidden;
            bodyBox.Visibility = Visibility.Hidden;
            changeBodyButton.Visibility = Visibility.Hidden;
        }

        // Show the header Infos

        private void showHeaderInfos(object sender, RoutedEventArgs e)
        {
            if(fileIsSelected)
            {
                changeBodyButton.Visibility = Visibility.Hidden;
                listBox.Visibility = Visibility.Visible;
                bodyTitle.Visibility = Visibility.Hidden;
                bodyBox.Visibility = Visibility.Hidden;
                listBox.Items.Clear();
                listBox.Items.Add("Date : " + fileList[fileId].Date);
                listBox.Items.Add("Hour : " + fileList[fileId].Hour);
                listBox.Items.Add("From : " + fileList[fileId].From);
                listBox.Items.Add("To : " + fileList[fileId].To);
                listBox.Items.Add("Subject : " + fileList[fileId].Subject);
            }
        }
        private void showBodyInfos(object sender, RoutedEventArgs e)
        {
            if (fileIsSelected)
            {
                bodyTitle.Visibility = Visibility.Visible;  
                changeBodyButton.Visibility = Visibility.Visible;
                listBox.Visibility = Visibility.Hidden;
                bodyBox.Visibility = Visibility.Visible;
                bodyBox.Text = fileList[fileId].HtmlBody;
                htmlBodyIsDisplayed = true;
            }
        }
        private void showAttachmentInfos(object sender, RoutedEventArgs e)
        {
            if (fileIsSelected)
            {
                changeBodyButton.Visibility = Visibility.Hidden;
                bodyTitle.Visibility = Visibility.Hidden;
                listBox.Visibility = Visibility.Visible;
                bodyBox.Visibility = Visibility.Hidden;
                listBox.Items.Clear();
                listBox.Items.Add("Nombre de pièce jointes : " + fileList[fileId].AttachmentCount);
                if(Int32.Parse(fileList[fileId].AttachmentCount) > 0)
                {
                    for (int i = 0; i < Int32.Parse(fileList[fileId].AttachmentCount); i++)
                    {
                        listBox.Items.Add("---");
                        listBox.Items.Add("Pièce jointe n°" + (i + 1));
                        listBox.Items.Add("Nom : " + fileList[fileId].AttachmentName[i]);
                        listBox.Items.Add("Type : " + fileList[fileId].AttachmentType[i]);
                        listBox.Items.Add("Encodage : " + fileList[fileId].AttachmentEncoding[i]);
                    }
                }
                else
                {
                    listBox.Items.Add("Aucune pièce jointe");
                }


            }
        }

        private void changeBody(object sender, RoutedEventArgs e)
        {
            if(htmlBodyIsDisplayed)
            {
                bodyTitle.Text = "Contenu filtré";
                bodyBox.Text = string.Join(' ', "Contenu filtré: " + fileList[fileId].RegeXBody);
                changeBodyButton.Content = "Show Html";
                htmlBodyIsDisplayed = false;
            }
            else
            {
                bodyTitle.Text = "Contenu HTML";
                bodyBox.Text = fileList[fileId].HtmlBody;
                htmlBodyIsDisplayed = true;
                changeBodyButton.Content = "Remove Html";
            }

        }
    }
}
