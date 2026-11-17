using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MotionHighlighter.Tests
{
    [TestClass]
    public class FormAboutTests
    {
        [TestMethod]
        public void FormAbout_InitializesSuccessfullyAndContainsWebsiteLink()
        {
            using FormAbout dlg = new();
            Assert.AreEqual("About Motion Highlighter", dlg.Text);
            Assert.IsNotNull(dlg.Controls);
            Assert.IsTrue(dlg.Controls.Count >= 2);

            // Find link control inside panelContent
            LinkLabel? link = null;
            foreach (Control c in dlg.Controls)
            {
                if (c is Panel p)
                {
                    foreach (Control child in p.Controls)
                    {
                        if (child is LinkLabel ll)
                        {
                            link = ll;
                            break;
                        }
                    }
                }
            }

            Assert.IsNotNull(link, "FormAbout should contain a LinkLabel to generatedpixel.dev");
            Assert.IsTrue(link.Text.Contains("generatedpixel.dev", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void FormAbout_ContainsMascotPictureBoxWithAltText()
        {
            using FormAbout dlg = new();

            PictureBox? pic = null;
            foreach (Control c in dlg.Controls)
            {
                if (c is Panel p)
                {
                    foreach (Control child in p.Controls)
                    {
                        if (child is PictureBox pb)
                        {
                            pic = pb;
                            break;
                        }
                    }
                }
            }

            Assert.IsNotNull(pic, "FormAbout should contain a mascot PictureBox");
            Assert.AreEqual("Generated Pixel kitten mascot", pic.AccessibleName);
            Assert.AreEqual("Generated Pixel kitten mascot", pic.AccessibleDescription);
        }
    }
}
