using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MotionHighlighter.Localization;

namespace MotionHighlighter.Tests
{
    /// <summary>
    /// Integration tests verifying that the main application window correctly reflects
    /// multi-language selections and updates all text on the fly.
    /// </summary>
    [TestClass]
    public class FormMainLocalizationTests
    {
        [TestMethod]
        public void FormMain_PopulatesLanguageComboBoxWithAllLocales()
        {
            using FormMain form = new();

            ComboBox? cmbLang = null;
            foreach (Control top in form.Controls)
            {
                if (top is Panel p && p.Name == "panelTop")
                {
                    foreach (Control child in p.Controls)
                    {
                        if (child is ComboBox cmb && cmb.Name == "_cmbLanguage")
                        {
                            cmbLang = cmb;
                            break;
                        }
                    }
                }
            }

            Assert.IsNotNull(cmbLang, "panelTop should contain _cmbLanguage ComboBox.");
            Assert.IsTrue(cmbLang.Items.Count >= 4, "Language selector should have at least 4 locales (en, de, fr, es).");
        }

        [TestMethod]
        public void FormMain_SwitchesLanguageDynamically()
        {
            using FormMain form = new();

            Button? btnAbout = null;
            Button? btnLoad = null;

            foreach (Control top in form.Controls)
            {
                if (top is Panel p && p.Name == "panelTop")
                {
                    foreach (Control child in p.Controls)
                    {
                        if (child is Button btn)
                        {
                            if (btn.Name == "_btnAbout")
                            {
                                btnAbout = btn;
                            }
                            else if (btn.Name == "_btnLoad")
                            {
                                btnLoad = btn;
                            }
                        }
                    }
                }
            }

            Assert.IsNotNull(btnAbout);
            Assert.IsNotNull(btnLoad);

            // Switch to German
            LocalizationManager.Instance.SetLanguage(AppLanguage.German);
            Assert.AreEqual("ℹ Über", btnAbout.Text);
            Assert.AreEqual("📁 Video öffnen...", btnLoad.Text);
            Assert.AreEqual("Bewegungs-Highlighter", form.Text);

            // Switch to French
            LocalizationManager.Instance.SetLanguage(AppLanguage.French);
            Assert.AreEqual("ℹ À propos", btnAbout.Text);
            Assert.AreEqual("📁 Ouvrir une vidéo...", btnLoad.Text);
            Assert.AreEqual("Surligneur de mouvement", form.Text);

            // Switch to Spanish
            LocalizationManager.Instance.SetLanguage(AppLanguage.Spanish);
            Assert.AreEqual("ℹ Acerca de", btnAbout.Text);
            Assert.AreEqual("📁 Abrir video...", btnLoad.Text);
            Assert.AreEqual("Resaltador de movimiento", form.Text);

            // Switch back to English
            LocalizationManager.Instance.SetLanguage(AppLanguage.English);
            Assert.AreEqual("ℹ About", btnAbout.Text);
            Assert.AreEqual("📁 Open Video...", btnLoad.Text);
            Assert.AreEqual("Motion Highlighter", form.Text);
        }
    }
}
