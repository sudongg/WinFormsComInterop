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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();



            Load += (s, e) =>
            {
                AllowDrop = true;

                DragOver += (s, e) =>
                {      
                    //DragOver  can be triggered
                    e.Effect = DragDropEffects.Link;
                    //MessageBo.Show("DragOver");

                    ////Using the following code will cause a System.NullReferenceException
                    //if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    //{
                    //    MessageBox.Show("DragOver");
                    //}
                };

                DragEnter += (s, e) =>
                {
                    //Unable to trigger  DragEnter event
                    MessageBox.Show("DragEnter");
                };

                DragDrop += (s, e) =>
                {  
                    //Unable to trigger  DragDrop event
                    MessageBox.Show("DragDrop");
                };
            };


        }
    }
}
