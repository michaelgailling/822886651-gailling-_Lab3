// Fig. 23.4: FickrViewerForm.cs
// Invoking a web service asynchronously with class HttpClient
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

//Lab 3 Question 2
//Michael Gailling
//822886651
//Comp 212
//Section 003

namespace FlickrViewer
{
    public partial class FickrViewerForm : Form
    {
        // Use your Flickr API key here--you can get one at:
        // https://www.flickr.com/services/apps/create/apply
        private const string KEY = "2d7451d93b03adf7c425717d5c68f80b";

        // object used to invoke Flickr web service      
        private static HttpClient flickrClient = new HttpClient();

        Task<string> flickrTask = null; // Task<string> that queries Flickr

        public FickrViewerForm()
        {
            InitializeComponent();
        }

        // initiate asynchronous Flickr search query; 
        // display results when query completes
        private async void searchButton_Click(object sender, EventArgs e)
        {
            // if flickrTask already running, prompt user 
            //if (flickrTask?.Status != TaskStatus.RanToCompletion)
            //{
            //   var result = MessageBox.Show(
            //      "Cancel the current Flickr search?",
            //      "Are you sure?", MessageBoxButtons.YesNo,
            //      MessageBoxIcon.Question);

            // determine whether user wants to cancel prior search
            //   if (result == DialogResult.No)
            //   {
            //           return;
            //       }
            //     else
            //   {

            //           flickrClient.CancelPendingRequests(); // cancel search
            //       }
            //}

            // Flickr's API URL for searches                         
            var flickrURL = "https://api.flickr.com/services/rest/?method=" +
            $"flickr.photos.search&api_key={KEY}&" +
            $"tags={inputTextBox.Text.Replace(" ", ",")}" +
            "&tag_mode=all&per_page=500&privacy_filter=1";

            imagesListBox.DataSource = null; // remove prior data source
            imagesListBox.Items.Clear(); // clear imagesListBox
            pictureBox.Image = null; // clear pictureBox
            imagesListBox.Items.Add("Loading..."); // display Loading...

            // invoke Flickr web service to search Flick with user's tags
            flickrTask = flickrClient.GetStringAsync(flickrURL);

            // await flickrTask then parse results with XDocument and LINQ
            XDocument flickrXML = XDocument.Parse(await flickrTask);

            // gather information on all photos
            var flickrPhotos =
            from photo in flickrXML.Descendants("photo")
            let id = photo.Attribute("id").Value
            let title = photo.Attribute("title").Value
            let secret = photo.Attribute("secret").Value
            let server = photo.Attribute("server").Value
            let farm = photo.Attribute("farm").Value
            select new FlickrResult
            {
                Title = title,
                URL = $"https://farm{farm}.staticflickr.com/" +
                    $"{server}/{id}_{secret}.jpg"
            };
            imagesListBox.Items.Clear(); // clear imagesListBox

            // set ListBox properties only if results were found
            if (flickrPhotos.Any())
            {
                
                imagesListBox.DataSource = flickrPhotos.ToList();
                imagesListBox.DisplayMember = "Title";
            }
            else // no matches were found
            {
                imagesListBox.Items.Add("No matches");
            }

            imagesListBox.SelectedIndex = -1;
        }

        //The SelectedIndexChanged() method was changed to Click() to prevent an image from 
        //saving as soon as the list is populated, the instructions state that "after user 
        //select one item from the listBox" as a precondition to saving the image or displaying it.
        private async void imagesListBox_Click(object sender, EventArgs e)
        {
            if (imagesListBox.SelectedIndex != -1 && imagesListBox.SelectedItem != null)
            {
                string selectedURL = ((FlickrResult)imagesListBox.SelectedItem).URL;

                // use HttpClient to get selected image's bytes asynchronously
                byte[] imageBytes = await flickrClient.GetByteArrayAsync(selectedURL);

                string filename;

                if (imagesListBox.Text == string.Empty)
                {
                    filename = "unnamed";
                }
                else
                {
                    filename = imagesListBox.Text;
                }

                Action display = () => DisplayImage(imageBytes);
                Action saveImage = () => SaveImage(imageBytes, $"{filename}.jpeg");
                Action saveThumb = () => SaveThumbnail(imageBytes, 64, $"{filename}.jpeg");

                Parallel.Invoke(display, saveImage, saveThumb);
            }
        }

        private void DisplayImage(byte[] imageBytes)
        {
            try
            {
                // display downloaded image in pictureBox   
                using (var memoryStream = new MemoryStream(imageBytes))
                {
                    pictureBox.Image = Image.FromStream(memoryStream);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error encoutered: {e.Message} \nwhile displaying image, please try again!");
            }
            
        }

        private void SaveImage(byte[] imageBytes, string imageFileName)
        {
            try
            {
                using (var stream = new MemoryStream(imageBytes))
                {
                    var image = Image.FromStream(stream);

                    using (var imageStream = new MemoryStream())
                    {
                        var filePath = Path.Combine(Application.StartupPath, $"Images/{imageFileName}");

                        image.Save(imageStream, ImageFormat.Jpeg);

                        File.WriteAllBytes(filePath, imageStream.ToArray());
                    }
                }
                MessageBox.Show("Image saved successfully!");
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error encoutered: {e.Message}\nwhile saving image, please try again!");
            }
            
        }

        private void SaveThumbnail(byte[] imageBytes, int width, string imageFileName)
        {
            try
            {
                using (var stream = new MemoryStream(imageBytes))
                {
                    var image = Image.FromStream(stream);

                    var height = (width * image.Height) / image.Width;
                    var thumbnail = image.GetThumbnailImage(width, height, null, IntPtr.Zero);

                    using (var thumbnailStream = new MemoryStream())
                    {
                        var filePath = Path.Combine(Application.StartupPath, $"Thumbnails/{imageFileName}");

                        thumbnail.Save(thumbnailStream, ImageFormat.Jpeg);

                        File.WriteAllBytes(filePath, thumbnailStream.ToArray());
                    }
                }
                MessageBox.Show("Thumbnail saved successfully!");
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error encoutered: {e.Message} \nwhile saving thumbnail, please try again!");
            }    
        }
    }
}

/**************************************************************************
 * (C) Copyright 1992-2017 by Deitel & Associates, Inc. and               *
 * Pearson Education, Inc. All Rights Reserved.                           *
 *                                                                        *
 * DISCLAIMER: The authors and publisher of this book have used their     *
 * best efforts in preparing the book. These efforts include the          *
 * development, research, and testing of the theories and programs        *
 * to determine their effectiveness. The authors and publisher make       *
 * no warranty of any kind, expressed or implied, with regard to these    *
 * programs or to the documentation contained in these books. The authors *
 * and publisher shall not be liable in any event for incidental or       *
 * consequential damages in connection with, or arising out of, the       *
 * furnishing, performance, or use of these programs.                     *
 **************************************************************************/
