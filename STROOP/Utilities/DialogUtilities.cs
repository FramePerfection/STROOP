﻿using STROOP.Controls;
using STROOP.Forms;
using STROOP.Managers;
using STROOP.Structs.Configurations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using STROOP.Structs;

namespace STROOP.Utilities
{
    public static class DialogUtilities
    {
        private static string GetFilterString(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Xml:
                    return "*.xml";
                case FileType.StroopVariables:
                    return "STROOP Variables|*.stv";
                case FileType.StroopVarHackVariables:
                    return "STROOP Var Hack Variables|*.stvhv";
                case FileType.MupenMovie:
                    return "Mupen Movies|*.m64|All Files|*.*";
                case FileType.Image:
                    return "Image files|*.jpg;*.jpeg;*.jpe;*.jfif;*.png|All Files|*.*";
                case FileType.Mapping:
                    return "Mapping|*.map";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool AskQuestionAboutM64Pasting(int numInputs)
        {
            return AskQuestion(
                String.Format("You are about to paste {0} inputs. " +
                    "Pasting more than {1} inputs at a time can be slow. " +
                    "Are you sure you wish to proceed?", numInputs, M64Config.PasteWarningLimit),
                "High Paste Count Warning");
        }

        public static bool AskQuestionAboutSavingVariableFileInPlace()
        {
            return AskQuestion(
                "You are about to save the variables in place. " +
                    "This action will replace the default variables of this tab with the current set of variables. " +
                    "Then from now on, STROOP will open with this set of variables in this tab. " +
                    "This action cannot be undone, except by re-downloading STROOP. " +
                    "Are you sure you wish to proceed?",
                "Save Variables In Place Warning");
        }

        public static bool AskQuestion(string message, string title)
        {
            DialogResult result = MessageBox.Show(
                message,
                title,
                MessageBoxButtons.YesNoCancel);
            return result == DialogResult.Yes;
        }

        public static void DisplayMessage(string message, string title)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK);
        }

        public static OpenFileDialog CreateOpenFileDialog(FileType fileType, string path = null)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = GetFilterString(fileType),
            };
            if (path != null)
            {
                dialog.InitialDirectory = path;
            }
            return dialog;
        }

        public static SaveFileDialog CreateSaveFileDialog(FileType fileType)
        {
            return new SaveFileDialog()
            {
                Filter = GetFilterString(fileType),
            };
        }

        public static List<XElement> OpenXmlElements(FileType fileType, string fileName = null)
        {
            if (fileName == null)
            {
                OpenFileDialog openFileDialog = CreateOpenFileDialog(fileType);
                DialogResult result = openFileDialog.ShowDialog();
                if (result != DialogResult.OK) return new List<XElement>();
                fileName = openFileDialog.FileName;
            }
            XDocument varXml = XDocument.Load(fileName);
            return ConvertDocumentIntoElements(varXml);
        }

        public static void SaveXmlElements(
            FileType fileType, string xmlName, List<XElement> elements, string fileName = null)
        {
            if (fileName == null)
            {
                SaveFileDialog saveFileDialog = CreateSaveFileDialog(fileType);
                DialogResult result = saveFileDialog.ShowDialog();
                if (result != DialogResult.OK) return;
                fileName = saveFileDialog.FileName;
            }
            XDocument document = ConvertElementsIntoDocument(xmlName, elements);
            document.Save(fileName);
        }

        private static XDocument ConvertElementsIntoDocument(
            string xmlName, List<XElement> elements)
        {
            XDocument doc = new XDocument();
            XElement root = new XElement(XName.Get(xmlName));
            doc.Add(root);

            foreach (XElement element in elements)
                root.Add(element);

            return doc;
        }

        private static List<XElement> ConvertDocumentIntoElements(XDocument doc)
        {
            XElement root = doc.Root;
            return root.Elements().ToList();
        }

        public static byte[] ReadFileBytes(string filePath)
        {
            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                return bytes;
            }
        }

        public static void WriteFileBytes(string filePath, byte[] bytes)
        {
            using (FileStream fs = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                fs.Write(bytes, 0, bytes.Length);
                fs.SetLength(bytes.Length);
            }
        }

        public delegate bool Parser<T>(string s, out T result);
        public static void UpdateNumberFromDialog<T>(
            ref T number,
            string textboxText = "",
            string labelText = "Enter Value:",
            string buttonText = "OK",
            Parser<T> parser = null) where T : IConvertible
        {
            var str = GetStringFromDialog(textboxText, labelText, buttonText);
            if (str == null) return;
            if (parser != null)
            {
                if (parser(str, out T result))
                    number = result;
            }
            else if (decimal.TryParse(str, out var dec))
                number = (T)Convert.ChangeType(dec, typeof(T));
        }

        public static string GetStringFromDialog(
            string textBoxText = "",
            string labelText = "Enter Value:",
            string buttonText = "OK")
        {
            ValueForm valueForm = new ValueForm(textBoxText, labelText, buttonText);
            if (valueForm.ShowDialog(AccessScope<StroopMainForm>.content) == DialogResult.OK)
                return valueForm.StringValue;
            return null;
        }

        public static double GetDoubleFromDialog(
            double defaultValue = 0,
            string textBoxText = "",
            string labelText = "Enter Value:",
            string buttonText = "OK")
        {
            string text = GetStringFromDialog(textBoxText, labelText, buttonText);
            float? relativeHeightNullable = Utilities.ParsingUtilities.ParseFloatNullable(text);
            if (relativeHeightNullable.HasValue)
                return relativeHeightNullable.Value;
            return defaultValue;
        }

        public static (string text, bool rightButtonClicked)? GetStringAndSideFromDialog(
            string textBoxText = "",
            string labelText = "Enter Value:",
            string button1Text = "OK",
            string button2Text = "OK")
        {
            ValueSplitForm valueSplitForm = new ValueSplitForm(textBoxText, labelText, button1Text, button2Text);
            if (valueSplitForm.ShowDialog() == DialogResult.OK)
                return (valueSplitForm.StringValue, valueSplitForm.RightButtonClicked);
            return null;
        }

        public static List<string> ReadFileLines(string filePath)
        {
            List<string> lines = new List<string>();
            string line;

            StreamReader file = new StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {
                lines.Add(line);
            }

            file.Close();
            return lines;
        }

        public static Image GetImage()
        {
            string directory = Directory.GetCurrentDirectory() + "\\Resources\\Maps\\Object Images";
            OpenFileDialog openFileDialog = CreateOpenFileDialog(FileType.Image, directory);
            DialogResult result = openFileDialog.ShowDialog();
            if (result != DialogResult.OK) return null;
            string fileName = openFileDialog.FileName;
            return Image.FromFile(fileName);
        }

        public static List<Image> GetImages()
        {
            string directory = Directory.GetCurrentDirectory() + "\\Resources\\Maps\\Object Images";
            OpenFileDialog openFileDialog = CreateOpenFileDialog(FileType.Image, directory);
            openFileDialog.Multiselect = true;
            DialogResult result = openFileDialog.ShowDialog();
            if (result != DialogResult.OK) return null;
            List<string> fileNames = openFileDialog.FileNames.ToList();
            return fileNames.ConvertAll(fileName => Image.FromFile(fileName));
        }
    }
}
