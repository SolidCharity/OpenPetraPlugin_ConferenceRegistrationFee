//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
//
// Copyright 2004-2014 by OM International
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
using System.IO;
using System.Data;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing.Printing;
using System.Net.Mail;
using GNU.Gettext;
using Ict.Common;
using Ict.Common.Data;
using Ict.Common.IO;
using Ict.Common.Controls;
using Ict.Common.Printing;
using Ict.Common.Verification;
using Ict.Petra.Client.CommonDialogs;
using Ict.Petra.Client.App.Core.RemoteObjects;
using Ict.Petra.Client.CommonControls;
using Ict.Petra.Shared;
using Ict.Petra.Shared.MConference.Data;
using Ict.Petra.Shared.MPartner;
using Ict.Petra.Shared.MFinance.Gift.Data;
using Ict.Petra.Shared.MPartner.Partner.Data;
using Ict.Petra.Shared.Interfaces.MFinance;
using Ict.Petra.Plugins.ConferenceRegistrationFees.Data;

namespace Ict.Petra.Plugins.ConferenceRegistrationFees.Client
{
    partial class TFrmCreateGiftBatchForParticipants
    {
        private void PrepareSEPAEmails()
        {
            if (FMainDS.SEPADirectDebitDetails.Rows.Count == 0)
            {
                MessageBox.Show(Catalog.GetString("There are no emails for your current parameters"),
                    Catalog.GetString("Nothing to email"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // TODO show more data in the grid, phone number of parents, etc

            // TODO: for some reason, the columns' initialisation in the constructor does not have any effect; need to do here again???
            //grdDetails.Columns.Clear();
//  TODO      grdDetails.AddTextColumn("Donor", FMainDS.AGift.ColumnDonorKey);
//            grdDetails.AddTextColumn("DonorShortName", FMainDS.AGift.ColumnDonorShortName);
//            grdDetails.AddTextColumn("Recipient", FMainDS.AGift.ColumnRecipientDescription);

            FMainDS.SEPADirectDebitDetails.DefaultView.Sort = SEPADirectDebitTDSSEPADirectDebitDetailsTable.GetParticipantNameDBName();

            FMainDS.SEPADirectDebitDetails.DefaultView.AllowNew = false;
            grdParticipantDetails.DataSource = new DevAge.ComponentModel.BoundDataView(FMainDS.SEPADirectDebitDetails.DefaultView);
        }

        private string LoadEmailTemplate()
        {
            string emailTemplateFilename = TAppSettingsManager.GetValue(FConferenceName + ".SEPAEmail");

            if (!File.Exists(emailTemplateFilename))
            {
                OpenFileDialog DialogOpen = new OpenFileDialog();

                DialogOpen.Filter = Catalog.GetString("HTML file (*.html)|*.html;*.htm");
                DialogOpen.RestoreDirectory = true;
                DialogOpen.Title = Catalog.GetString("Select the template for the letter to the bank account owners");

                if (DialogOpen.ShowDialog() != DialogResult.OK)
                {
                    return string.Empty;
                }

                emailTemplateFilename = DialogOpen.FileName;
            }

            StreamReader sr = new StreamReader(emailTemplateFilename);

            string htmlTemplate = sr.ReadToEnd();

            sr.Close();

            return htmlTemplate;
        }

        private string LoadReportTemplate()
        {
            string templateFilename = TAppSettingsManager.GetValue(FConferenceName + ".SEPAReport");

            if (!File.Exists(templateFilename))
            {
                OpenFileDialog DialogOpen = new OpenFileDialog();

                DialogOpen.Filter = Catalog.GetString("HTML file (*.html)|*.html;*.htm");
                DialogOpen.RestoreDirectory = true;
                DialogOpen.Title = Catalog.GetString("Select the template for the SEPA report");

                if (DialogOpen.ShowDialog() != DialogResult.OK)
                {
                    return string.Empty;
                }

                templateFilename = DialogOpen.FileName;
            }

            StreamReader sr = new StreamReader(templateFilename);

            string htmlTemplate = sr.ReadToEnd();

            sr.Close();

            return htmlTemplate;
        }

        private void FocusedRowChanged(object sender, EventArgs e)
        {
            SEPADirectDebitTDSSEPADirectDebitDetailsRow r =
                (SEPADirectDebitTDSSEPADirectDebitDetailsRow)((DataRowView)grdParticipantDetails.SelectedDataRows[0]).Row;

            DisplayEmail(CreateEmail(r));
        }

        private MailMessage CreateEmail(SEPADirectDebitTDSSEPADirectDebitDetailsRow row)
        {
            string toAddress = "\"" + row.BankAccountOwnerName + "\" <" + row.BankAccountOwnerEmail + ">";

            if (TAppSettingsManager.HasValue(FConferenceName + ".TestSendEmail"))
            {
                toAddress = TAppSettingsManager.GetValue(FConferenceName + ".TestSendEmail");
            }

            MailMessage m = new MailMessage(txtSendingEmailAddress.Text, toAddress);
            m.Subject = row.EmailSubject;
            m.Body = row.EmailBody;
            m.Bcc.Add(txtSendingEmailAddress.Text);
            m.IsBodyHtml = true;
            return m;
        }

        /// <summary>
        /// display the email in the web browser control below the list
        /// </summary>
        private void DisplayEmail(MailMessage ASelectedMail)
        {
            if (ASelectedMail == null)
            {
                // should not get here
                return;
            }

            string header = "<html><body>";
            header += String.Format("{0}: {1}<br/>",
                Catalog.GetString("From"),
                ASelectedMail.From.ToString().Replace("<", "&lt;").Replace(">", "&gt;"));
            header += String.Format("{0}: {1}<br/>",
                Catalog.GetString("To"),
                ASelectedMail.To.ToString().Replace("<", "&lt;"));

            if (ASelectedMail.CC.Count > 0)
            {
                header += String.Format("{0}: {1}<br/>",
                    Catalog.GetString("Cc"),
                    ASelectedMail.CC.ToString().Replace("<", "&lt;").Replace(">", "&gt;"));
            }

            if (ASelectedMail.Bcc.Count > 0)
            {
                header += String.Format("{0}: {1}<br/>",
                    Catalog.GetString("Bcc"),
                    ASelectedMail.Bcc.ToString().Replace("<", "&lt;").Replace(">", "&gt;"));
            }

            header += String.Format("<b>{0}: {1}</b><br/>",
                Catalog.GetString("Subject"),
                ASelectedMail.Subject);
            header += "<hr></body></html>";

            if (ASelectedMail.IsBodyHtml)
            {
                brwEmailPreview.DocumentText = header + ASelectedMail.Body;
            }
            else
            {
                brwEmailPreview.DocumentText = header +
                                               "<html><body>" + ASelectedMail.Body +
                                               "</body></html>";
            }
        }

        private void SendEmails(object sender, EventArgs e)
        {
            if (txtEmailPassword.Text.Trim().Length == 0)
            {
                MessageBox.Show("please enter Email password", "error");
                return;
            }

            // TODO fortschrittsbalken
            // TODO send from the server???
            TSmtpSender smtp = new TSmtpSender(
                TAppSettingsManager.GetValue("SmtpHost"),
                TAppSettingsManager.GetInt16("SmtpPort", 25),
                TAppSettingsManager.GetBoolean("SmtpEnableSsl", false),
                txtEmailUser.Text,
                txtEmailPassword.Text,
                string.Empty);

            // send the emails
            this.UseWaitCursor = true;
            int count = 0;

            foreach (SEPADirectDebitTDSSEPADirectDebitDetailsRow r in FMainDS.SEPADirectDebitDetails.Rows)
            {
                smtp.SendMessage(CreateEmail(r));
                count++;
            }

            this.UseWaitCursor = false;
            MessageBox.Show(count.ToString() + " Emails have been sent successfully!");
        }
    }

    /// <summary>
    /// dummy implementation
    /// </summary>
    public class SEPADirectDebitTDSSEPADirectDebitDetailsValidation
    {
        /// <summary>
        /// dummy implementation of the validation for a custom table
        /// </summary>
        public static void Validate(object AContext, SEPADirectDebitTDSSEPADirectDebitDetailsRow ARow,
            ref TVerificationResultCollection AVerificationResultCollection, TValidationControlsDict AValidationControlsDict)
        {
        }
    }
}