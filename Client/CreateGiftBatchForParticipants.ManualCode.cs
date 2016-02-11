//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
//
// Copyright 2004-2016 by OM International
//
// This file is part of OpenPetra.org.
//
// OpenPetra.org is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// OpenPetra.org is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
//
using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using GNU.Gettext;
using Ict.Common.Verification;
using Ict.Common;
using Ict.Common.IO;
using Ict.Common.Printing;
using Ict.Petra.Shared.Interfaces.MConference;
using Ict.Common.Remoting.Shared;
using Ict.Common.Remoting.Client;
using Ict.Petra.Plugins.ConferenceRegistrationFees.Data;
using Ict.Petra.Plugins.ConferenceRegistrationFees.RemoteObjects;

namespace Ict.Petra.Plugins.ConferenceRegistrationFees.Client
{
    public partial class TFrmCreateGiftBatchForParticipants
    {
        SEPADirectDebitTDS FMainDS;
        TMConferenceRegistrationFeesNamespace FPluginRemote;
        string FConferenceName = "ConferenceTool";

        /// <summary>
        /// needed to specify which config values apply
        /// </summary>
        public string ConferenceName
        {
            set
            {
                FConferenceName = value;
                InitializeManualCode();
            }
        }

        private void InitializeManualCode()
        {
            FMainDS = new SEPADirectDebitTDS();
            dtpCollectionDate.Date = DateTime.Today.AddDays(5.0);
            txtSendingEmailAddress.Text = TAppSettingsManager.GetValue(FConferenceName + ".SendingAddress", string.Empty, false);
            FPluginRemote = new TMConferenceRegistrationFeesNamespace();
        }

        DataTable FPartnerInfoTable = null;

        private bool LoadPartnerInfo(bool reload = false)
        {
            if ((FPartnerInfoTable != null) && !reload)
            {
                return true;
            }

            // load partner keys from xlsx file into DataTable
            string filename = TAppSettingsManager.GetValue(FConferenceName + ".OnlineRegistrationExport");

            if (filename.Length == 0)
            {
                // if partners have already been imported into OpenPetra, we do not need a separate file with partner details
                return true;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (FileStream fs = File.OpenRead(filename))
                {
                    fs.CopyTo(ms);
                }

                List <string>columnsToImport = new List <string>();
                columnsToImport.Add("Participant Nr");
                columnsToImport.Add("Petra Nr");
                columnsToImport.Add("First Name");
                columnsToImport.Add("Family Name");

                // for SEPA payment
                columnsToImport.Add("Bank Account Name");
                columnsToImport.Add("Bank Account Number");
                columnsToImport.Add("Bank Branch Name");
                columnsToImport.Add("Bank Sort Code");
                columnsToImport.Add("Bank Account Address"); // email of bank account owner
                // for price calculation
                columnsToImport.Add("Application Date");
                columnsToImport.Add("Accepted Date");
                columnsToImport.Add("Role");

                if (filename.EndsWith(".xlsx"))
                {
                    FPartnerInfoTable = TCsv2Xml.ParseExcelStream2DataTable(ms, true, 0, columnsToImport);
                }
                else if (filename.EndsWith(".ods"))
                {
                    FPartnerInfoTable = TOpenDocumentParser.ParseODSStream2DataTable(ms, true, 0, columnsToImport);
                }
                else
                {
                    MessageBox.Show(Catalog.GetString("We can only import xlsx and ods files"), Catalog.GetString("Error"));
                    return false;
                }

                if (FPartnerInfoTable.Columns["Participant Nr"] == null)
                {
                    MessageBox.Show(Catalog.GetString(
                            "There was a problem importing the partner file from the online registration"), Catalog.GetString("Error"));
                    return false;
                }

                FPartnerInfoTable.Columns["Participant Nr"].ColumnName = "RegistrationKey";
                FPartnerInfoTable.Columns["Petra Nr"].ColumnName = "PartnerKey";
                FPartnerInfoTable.Columns["First Name"].ColumnName = "FirstName";
                FPartnerInfoTable.Columns["Family Name"].ColumnName = "FamilyName";
                FPartnerInfoTable.Columns["Bank Account Address"].ColumnName = "BankAccountEmail";
                FPartnerInfoTable.Columns["Bank Account Name"].ColumnName = "BankAccountName";
                FPartnerInfoTable.Columns["Bank Account Number"].ColumnName = "IBAN";
                FPartnerInfoTable.Columns["Bank Sort Code"].ColumnName = "BIC";
                FPartnerInfoTable.Columns["Application Date"].ColumnName = "ApplicationDate";
                FPartnerInfoTable.Columns["Accepted Date"].ColumnName = "AcceptedDate";
            }

            return true;
        }

        private void ProcessPayment(object sender, EventArgs e)
        {
            if (txtPaymentData.Text.Trim().Length == 0)
            {
                MessageBox.Show("please paste payment data", "error");
                return;
            }

            if (txtPaymentData.Text.Trim().Length == 0)
            {
                MessageBox.Show("please paste payment data", "error");
                return;
            }

            if (!LoadPartnerInfo())
            {
                return;
            }

            string GiftBatchFilename = TAppSettingsManager.GetValue(FConferenceName + ".GiftBatchCSVFilename", "", false);

            if (GiftBatchFilename.Length == 0)
            {
                SaveFileDialog sd = new SaveFileDialog();

                sd.AddExtension = true;
                sd.DefaultExt = ".csv";
                sd.Filter = "Gift batches (*.csv)|*.csv";

                if (sd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                GiftBatchFilename = sd.FileName;
            }

            string SEPAFilename = TAppSettingsManager.GetValue(FConferenceName + ".SEPAFilename", "", false);

            if (SEPAFilename.Length == 0)
            {
                SaveFileDialog sd = new SaveFileDialog();

                sd.AddExtension = true;
                sd.DefaultExt = ".xml";
                sd.Filter = "SEPA File (*.xml)|*.xml";

                if (sd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                SEPAFilename = sd.FileName;
            }

            string giftbatchcontent;
            string sepaxml;
            string directdebitreport;
            TVerificationResultCollection verification;
            bool success = FPluginRemote.WebConnectors.ProcessApplicationPayment(
                txtPaymentData.Text,
                FPartnerInfoTable,
                TAppSettingsManager.GetInt64(FConferenceName + ".UnknownPartnerKey"),
                TAppSettingsManager.GetValue(FConferenceName + ".UnknownPartnerName"),
                TAppSettingsManager.GetInt64(FConferenceName + ".DefaultPartnerLedger"),
                TAppSettingsManager.GetBoolean(FConferenceName + ".ValidatePartnerKeys", true),
                TAppSettingsManager.GetValue(FConferenceName + ".TemplateApplication"),
                TAppSettingsManager.GetValue(FConferenceName + ".TemplateManualApplication"),
                TAppSettingsManager.GetValue(FConferenceName + ".TemplateConferenceFee"),
                TAppSettingsManager.GetValue(FConferenceName + ".TemplateDonation"),
                LoadEmailTemplate(),
                LoadReportTemplate(),
                dtpCollectionDate.Date.Value,
                TAppSettingsManager.GetValue(FConferenceName + ".CreditorName"),
                TAppSettingsManager.GetValue(FConferenceName + ".CreditorIBAN"),
                TAppSettingsManager.GetValue(FConferenceName + ".CreditorBIC"),
                TAppSettingsManager.GetValue(FConferenceName + ".CreditorSchemeID"),
                TAppSettingsManager.GetValue(FConferenceName + ".DirectDebitDescription"),
                TAppSettingsManager.GetValue(FConferenceName + ".MandatePrefix"),
                TAppSettingsManager.GetValue(FConferenceName + ".ColumnNames"),
                out giftbatchcontent,
                out FMainDS,
                out sepaxml,
                out directdebitreport,
                out verification);

            if (!success && verification.HasCriticalErrors)
            {
                MessageBox.Show(verification.BuildVerificationResultString(), "Error");
                return;
            }

            using (StreamWriter sw = new StreamWriter(GiftBatchFilename, false, System.Text.ASCIIEncoding.ASCII))
            {
                sw.Write(giftbatchcontent);
                sw.Close();

                MessageBox.Show(String.Format(Catalog.GetString(
                            "The gift batch file has been written to {0}."), GiftBatchFilename), Catalog.GetString(
                        "Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            using (StreamWriter sw = new StreamWriter(SEPAFilename, false, UTF8Encoding.UTF8))
            {
                sw.Write(sepaxml);
                sw.Close();

                MessageBox.Show(String.Format(Catalog.GetString("The SEPA file has been written to {0}."), SEPAFilename), Catalog.GetString(
                        "Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            PrepareSEPAEmails();
            PrintDirectDebitReport(directdebitreport);
        }

        private PrintDocument FDocument;
        private void PrintDirectDebitReport(string AHtmlDocument)
        {
            if (AHtmlDocument.Length == 0)
            {
                return;
            }

            PrintDocument doc = new PrintDocument();
            TGfxPrinter gfxPrinter = new TGfxPrinter(doc, TGfxPrinter.ePrinterBehaviour.eFormLetter);
            TPrinterHtml htmlPrinter = new TPrinterHtml(AHtmlDocument,
                String.Empty,
                gfxPrinter);
            gfxPrinter.Init(eOrientation.ePortrait, htmlPrinter, eMarginType.ePrintableArea);

            // show print preview dialog
            PrintPreviewDialog dlg = new PrintPreviewDialog();
            ToolStrip toolbar = ((ToolStrip)dlg.Controls["toolStrip1"]);
            ToolStripButton oldBtn = (ToolStripButton)toolbar.Items["printToolStripButton"];
            ToolStripButton btn = new ToolStripButton();
            btn.ImageIndex = oldBtn.ImageIndex;
            btn.Visible = true;
            toolbar.Items.Insert(0, btn);
            toolbar.Items.Remove(oldBtn);
            btn.Click += new EventHandler(this.SelectPrinterAfterPreview);
            dlg.Document = gfxPrinter.Document;
            FDocument = dlg.Document;
            dlg.ShowDialog();
        }

        private void SelectPrinterAfterPreview(Object sender, EventArgs e)
        {
            PrintDialog dlgPrint = new PrintDialog();

            try
            {
                dlgPrint.AllowSelection = true;
                dlgPrint.ShowNetwork = true;
                dlgPrint.Document = FDocument;

                if (dlgPrint.ShowDialog() == DialogResult.OK)
                {
                    FDocument.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Print Error: " + ex.Message);
            }
        }
    }
}