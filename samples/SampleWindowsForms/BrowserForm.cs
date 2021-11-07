using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SampleWindowsForms
{
    public partial class BrowserForm : Form
    {
        public BrowserForm()
        {
            InitializeComponent();
        }

        private void RecentPostBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void RecentPostBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {

        }

        private void RecentPostBrowser_NewWindow(object sender, CancelEventArgs e)
        {

        }

        private void RecentPostBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            RecentPostBrowser.Url = new Uri("https://google.com");
        }

        private void button1_DragDrop(object sender, DragEventArgs e)
        {

        }

        private void button1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Link;
        }
    }
}
