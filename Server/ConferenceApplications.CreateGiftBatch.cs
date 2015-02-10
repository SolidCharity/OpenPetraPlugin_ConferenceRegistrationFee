//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
//
// Copyright 2004-2015 by OM International
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
using System.Xml;
using System.IO;
using System.Text;
using System.Data;
using System.Data.Odbc;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.Specialized;

using Ict.Common;
using Ict.Common.DB;
using Ict.Common.IO;
using Ict.Common.Printing;
using Ict.Common.Verification;
using Ict.Common.Data;
using Ict.Petra.Shared.MSysMan.Data;
using Ict.Petra.Shared.MPartner.Partner.Data;
using Ict.Petra.Server.MPartner.Partner.Data.Access;
using Ict.Petra.Shared.MConference;
using Ict.Petra.Shared.MConference.Data;
using Ict.Petra.Server.MConference.Data.Access;
using Ict.Petra.Shared.MPersonnel.Personnel.Data;
using Ict.Petra.Server.MPersonnel.Personnel.Data.Access;
using Ict.Petra.Server.App.Core;
using Ict.Petra.Server.App.Core.Security;
using Ict.Petra.Server.MPartner.Common;
using Ict.Petra.Server.MPartner.Import;
using Ict.Petra.Server.MPartner.ImportExport;
using Ict.Petra.Plugins.SEPA;
using Ict.Petra.Plugins.ConferenceRegistrationFees.Data;

namespace Ict.Petra.Plugins.ConferenceRegistrationFees.WebConnectors
{
    /// <summary>
    /// For creating gift batches for conference payments
    /// </summary>
    public class TConferenceCreateGiftBatch
    {
        /// <summary>
        /// get the name of this partner, and the person key from the local database from the first PmGeneralApplication for this partner
        /// </summary>
        /// <param name="ARegistrationOfficeKey"></param>
        /// <param name="ARegistrationPartnerKey"></param>
        /// <param name="ALocalPartnerKey">will be -1 if the partner has no local key yet</param>
        /// <param name="AFirstnameLastname"></param>
        /// <param name="ATransaction"></param>
        /// <returns>false if person cannot be found at all</returns>
        static private bool GetPartner(
            Int64 ARegistrationOfficeKey,
            Int64 ARegistrationPartnerKey,
            out Int64 ALocalPartnerKey,
            out String AFirstnameLastname,
            TDBTransaction ATransaction)
        {
            ALocalPartnerKey = -1;
            AFirstnameLastname = String.Empty;

            PPersonTable Person = PPersonAccess.LoadByPrimaryKey(ARegistrationPartnerKey, ATransaction);

            if (Person.Count == 0)
            {
                return false;
            }

            AFirstnameLastname = Person[0].FirstName + " " + Person[0].FamilyName;

            PmGeneralApplicationTable GeneralApplication = PmGeneralApplicationAccess.LoadViaPPersonPartnerKey(ARegistrationPartnerKey, ATransaction);

            foreach (PmGeneralApplicationRow row in GeneralApplication.Rows)
            {
                if (row.RegistrationOffice != ARegistrationOfficeKey)
                {
                    // most probably a typo? This person should belong to the registration office
                    return false;
                }
            }

            foreach (PmGeneralApplicationRow row in GeneralApplication.Rows)
            {
                if (!row.IsLocalPartnerKeyNull())
                {
                    ALocalPartnerKey = row.LocalPartnerKey;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// this is needed since TS2013
        /// </summary>
        /// <param name="APartnerKeyMatching"></param>
        /// <returns></returns>
        static private SortedList <long, long>GetMatchingPartnerKeys(DataTable APartnerKeyMatching)
        {
            SortedList <long, long>result = new SortedList <long, long>();

            if (APartnerKeyMatching == null)
            {
                return result;
            }

            foreach (DataRow r in APartnerKeyMatching.Rows)
            {
                if (r.IsNull("RegistrationKey") || r.IsNull("PartnerKey"))
                {
                    continue;
                }

                result.Add(Convert.ToInt64(r["RegistrationKey"]), Convert.ToInt64(r["PartnerKey"]));
            }

            return result;
        }

        /// <summary>
        /// this is needed to create gift transactions CSV file for import into Petra 2.x
        /// </summary>
        /// <param name="AInputPartnerKeysAndPaymentInfo">CSV text with partner key and columns for payment information</param>
        /// <param name="APartnerInfo"></param>
        /// <param name="AUnknownPartner"></param>
        /// <param name="AUnkownPartnerName"></param>
        /// <param name="ADefaultPartnerLedger"></param>
        /// <param name="AValidatePartnerKeys"></param>
        /// <param name="ATemplateApplicationFee"></param>
        /// <param name="ATemplateManualApplication"></param>
        /// <param name="ATemplateConferenceFee"></param>
        /// <param name="ATemplateDonation"></param>
        /// <param name="AEmailTemplate"></param>
        /// <param name="AReportTemplate"></param>
        /// <param name="ACollectionDate"></param>
        /// <param name="ACreditorName"></param>
        /// <param name="ACreditorIBAN"></param>
        /// <param name="ACreditorBIC"></param>
        /// <param name="ACreditorSchemeID"></param>
        /// <param name="ADirectDebitDescription"></param>
        /// <param name="AMandatePrefix"></param>
        /// <param name="AColumnNames"></param>
        /// <param name="AGiftBatchCSV"></param>
        /// <param name="AMainDS"></param>
        /// <param name="ASepaDirectDebitXML"></param>
        /// <param name="ADirectDebitReport"></param>
        /// <param name="AVerificationResult"></param>
        /// <returns></returns>
        [RequireModulePermission("CONFERENCE")]
        static public bool ProcessApplicationPayment(string AInputPartnerKeysAndPaymentInfo,
            DataTable APartnerInfo,
            Int64 AUnknownPartner,
            string AUnkownPartnerName,
            Int64 ADefaultPartnerLedger,
            bool AValidatePartnerKeys,
            string ATemplateApplicationFee,
            string ATemplateManualApplication,
            string ATemplateConferenceFee,
            string ATemplateDonation,
            string AEmailTemplate,
            string AReportTemplate,
            DateTime ACollectionDate,
            string ACreditorName,
            string ACreditorIBAN,
            string ACreditorBIC,
            string ACreditorSchemeID,
            string ADirectDebitDescription,
            string AMandatePrefix,
            string AColumnNames,
            out string AGiftBatchCSV,
            out SEPADirectDebitTDS AMainDS,
            out string ASepaDirectDebitXML,
            out string ADirectDebitReport,
            out TVerificationResultCollection AVerificationResult)
        {
            AVerificationResult = new TVerificationResultCollection();
            AMainDS = new SEPADirectDebitTDS();
            ASepaDirectDebitXML = string.Empty;
            AGiftBatchCSV = string.Empty;
            ADirectDebitReport = string.Empty;
            SortedList <long, long>MatchingPartnerKeys = GetMatchingPartnerKeys(APartnerInfo);
            TSEPAWriterDirectDebit sepaWriter = new TSEPAWriterDirectDebit();
            sepaWriter.Init(ACreditorName, ACollectionDate, ACreditorName, ACreditorIBAN, ACreditorBIC, ACreditorSchemeID);

            decimal PreviousConferenceFee = 0.0m, PreviousApplicationFee = 0.0m, PreviousDonation = 0.0m, PreviousManualApplicationFee = 0.0m;
            Int64 PreviousRegistrationOffice = ADefaultPartnerLedger;

            string InputSeparator = ",";
            string OutputSeparator = ";";
            int RowCount = 1;
            string ResultString = string.Empty;

            if (AInputPartnerKeysAndPaymentInfo.Contains("\t"))
            {
                InputSeparator = "\t";
            }
            else if (AInputPartnerKeysAndPaymentInfo.Contains(";"))
            {
                InputSeparator = ";";
            }

            TLanguageCulture.LoadLanguageAndCulture();

            TDBTransaction Transaction = DBAccess.GDBAccessObj.BeginTransaction();

            try
            {
                string[] InputLines = AInputPartnerKeysAndPaymentInfo.Replace("\r", "").Split(new char[] { '\n' });

                int CountLines = 0;

                foreach (string InputLine in InputLines)
                {
                    string reference = string.Empty;

                    string line = InputLine;

                    if (line.Trim().Length == 0)
                    {
                        continue;
                    }

                    CountLines++;

                    Int64 RegistrationKey = Convert.ToInt64(StringHelper.GetNextCSV(ref line, InputSeparator, ""));

                    if (RegistrationKey < 1000000)
                    {
                        RegistrationKey += ADefaultPartnerLedger;
                    }

                    SEPADirectDebitTDSPartnerInfoTable t = new SEPADirectDebitTDSPartnerInfoTable();
                    SEPADirectDebitTDSPartnerInfoRow PartnerInfoRow = t.NewRowTyped();

                    Int64 OrigRegistrationKey = RegistrationKey;
                    string IBAN = String.Empty;
                    string BIC = String.Empty;

                    if ((APartnerInfo != null) && (APartnerInfo.Rows.Count > 0))
                    {
                        APartnerInfo.DefaultView.Sort = "RegistrationKey";
                        DataRowView[] findPartner = APartnerInfo.DefaultView.FindRows(RegistrationKey);

                        if ((findPartner == null) || (findPartner.Length == 0))
                        {
                            AVerificationResult.Add(new TVerificationResult(
                                    "Problem Zettel Nr " + CountLines.ToString(),
                                    "Registration Key " + RegistrationKey.ToString() + " ist noch nicht Accepted, Petra Abgleich fehlt",
                                    TResultSeverity.Resv_Critical,
                                    new Guid()));

                            continue;
                        }

                        DataRow PartnerInfoRowUntyped = APartnerInfo.DefaultView.FindRows(RegistrationKey)[0].Row;

                        IBAN = PartnerInfoRowUntyped["IBAN"].ToString();
                        BIC = PartnerInfoRowUntyped["BIC"].ToString();

                        PartnerInfoRow.RegistrationKey = Convert.ToInt64(PartnerInfoRowUntyped["RegistrationKey"]);
                        PartnerInfoRow.PartnerKey = Convert.ToInt64(PartnerInfoRowUntyped["PartnerKey"]);
                        PartnerInfoRow.ApplicationDate = Convert.ToDateTime(PartnerInfoRowUntyped["ApplicationDate"]);
                        PartnerInfoRow.FamilyName = PartnerInfoRowUntyped["FamilyName"].ToString();
                        PartnerInfoRow.FirstName = PartnerInfoRowUntyped["FirstName"].ToString();
                        PartnerInfoRow.BankAccountEmail = PartnerInfoRowUntyped["BankAccountEmail"].ToString();
                        PartnerInfoRow.BankAccountName = PartnerInfoRowUntyped["BankAccountName"].ToString();
                    }
                    else
                    {
                        // load BLZ and AccountNumber from manually entered text, AInputPartnerKeysAndPaymentInfo
                        BIC = StringHelper.GetNextCSV(ref line, InputSeparator, "");
                        IBAN = StringHelper.GetNextCSV(ref line, InputSeparator, "");

                        // check if the partner exists in the database with the registration key
                        PPersonTable person = PPersonAccess.LoadByPrimaryKey(RegistrationKey, Transaction);

                        if (person.Rows.Count == 1)
                        {
                            // load PartnerInfoRow from that existing partner
                            PartnerInfoRow.RegistrationKey = RegistrationKey;
                            PartnerInfoRow.PartnerKey = RegistrationKey;
                            // application date: needed for the mandate id
                            PartnerInfoRow.ApplicationDate = DateTime.Today;
                            PartnerInfoRow.FamilyName = person[0].FamilyName;
                            PartnerInfoRow.FirstName = person[0].FirstName;
                            PartnerInfoRow.BankAccountEmail = TMailing.GetBestEmailAddress(RegistrationKey);
                            PartnerInfoRow.BankAccountName = PartnerInfoRow.FirstName + " " + PartnerInfoRow.FamilyName;
                        }
                        else
                        {
                            AVerificationResult.Add(new TVerificationResult(
                                    "Problem Zettel Nr " + CountLines.ToString(),
                                    "kann keinen Partner mit Partnerkey " + RegistrationKey.ToString() + " finden.",
                                    TResultSeverity.Resv_Critical,
                                    new Guid()));

                            continue;
                        }
                    }

                    IBAN = IBAN.ToString().Trim().ToUpper().Replace(" ", "");
                    BIC = BIC.ToString().Trim().ToUpper().Replace(" ", "");

                    if (BIC.ToString().Length == 22)
                    {
                        // someone confused BIC and IBAN
                        string temp = IBAN.ToString();
                        IBAN = BIC.ToString();
                        BIC = temp;
                    }

                    // calculate IBAN and BIC if we got the old bank number format
                    if (!Regex.IsMatch(IBAN.ToString(), "^[A-Z]"))
                    {
                        string iban, bic;

                        if (ConvertBankAccountCodeToIBANandBic(IBAN.ToString(), BIC.ToString(), out iban, out bic))
                        {
                            IBAN = iban;
                            BIC = bic;
                        }
                        else
                        {
                            AVerificationResult.Add(new TVerificationResult(
                                    "Problem Zettel Nr " + CountLines.ToString(),
                                    "cannot convert bank account for " + OrigRegistrationKey.ToString(),
                                    TResultSeverity.Resv_Critical,
                                    new Guid()));

                            continue;
                        }
                    }

                    IBAN = IBAN.ToString().Replace(" ", "");
                    BIC = BIC.ToString().Replace(" ", "");

                    string OrigIBAN = IBAN;
                    string OrigBIC = BIC;

                    if (IBAN.Length < 22)
                    {
                        // this is not good. perhaps a zero was forgotten?
                        IBAN = IBAN.Replace("000", "000" + new String('0', 22 - IBAN.Length));
                    }

                    if (!ValidateIBANandBic(IBAN, ref BIC) || ((OrigIBAN != IBAN) && (OrigBIC != BIC)))
                    {
                        AVerificationResult.Add(new TVerificationResult(
                                "Problem Zettel Nr " + CountLines.ToString(),
                                "ungültige IBAN " + OrigIBAN + " für " + RegistrationKey.ToString(),
                                TResultSeverity.Resv_Critical,
                                new Guid()));
                        continue;
                    }

                    PartnerInfoRow["BIC"] = BIC;
                    PartnerInfoRow["IBAN"] = IBAN;

                    if (MatchingPartnerKeys.ContainsKey(RegistrationKey))
                    {
                        RegistrationKey = MatchingPartnerKeys[RegistrationKey];
                    }
                    else if (MatchingPartnerKeys.Count > 0)
                    {
                        Console.WriteLine("Cannot find partner key " + RegistrationKey.ToString() + " in row " + RowCount.ToString());
                        RegistrationKey = AUnknownPartner;
                    }

                    if (RegistrationKey == 0)
                    {
                        AVerificationResult.Add(new TVerificationResult(
                                "Problem Zettel Nr " + CountLines.ToString(),
                                "Registration Key " + OrigRegistrationKey.ToString() + ": Petra Abgleich fehlt",
                                TResultSeverity.Resv_Critical,
                                new Guid()));

                        continue;
                    }

                    decimal ConferenceFee = Convert.ToDecimal(StringHelper.GetNextCSV(ref line, InputSeparator, PreviousConferenceFee.ToString()));
                    PreviousConferenceFee = ConferenceFee;

                    decimal ApplicationFee = Convert.ToDecimal(StringHelper.GetNextCSV(ref line, InputSeparator, PreviousApplicationFee.ToString()));
                    PreviousApplicationFee = ApplicationFee;

                    decimal Donation = 0;

                    if (AColumnNames.Contains("Donation"))
                    {
                        Donation = Convert.ToDecimal(StringHelper.GetNextCSV(ref line, InputSeparator, PreviousDonation.ToString()));
                    }

                    PreviousDonation = Donation;

                    Int64 RegistrationOffice = PreviousRegistrationOffice;

                    if (AColumnNames.Contains("RegistrationOffice"))
                    {
                        RegistrationOffice = Convert.ToInt64(StringHelper.GetNextCSV(ref line, InputSeparator, PreviousRegistrationOffice.ToString()));
                    }

                    PreviousRegistrationOffice = RegistrationOffice;

                    decimal ManualApplicationFee = 0.0m;

                    if (AColumnNames.Contains("ManualApplicationFee") && (line.Length > 0))
                    {
                        ManualApplicationFee =
                            Convert.ToDecimal(StringHelper.GetNextCSV(ref line, InputSeparator, PreviousManualApplicationFee.ToString()));
                        PreviousManualApplicationFee = ManualApplicationFee;
                    }

                    // account owner might be different from participant
                    if (AColumnNames.Contains("AccountOwnerName") && (line.Length > 0))
                    {
                        string s = StringHelper.GetNextCSV(ref line, InputSeparator);

                        if (s.Length > 0)
                        {
                            PartnerInfoRow.BankAccountName = s;
                        }
                    }

                    if (AColumnNames.Contains("AccountOwnerEmail") && (line.Length > 0))
                    {
                        string s = StringHelper.GetNextCSV(ref line, InputSeparator);

                        if (s.Length > 0)
                        {
                            PartnerInfoRow.BankAccountEmail = s;
                        }
                    }

                    string PersonFirstnameLastname = string.Empty;
                    Int64 LocalPartnerKey;

                    if (!AValidatePartnerKeys)
                    {
                        LocalPartnerKey = RegistrationKey;
                    }
                    else if (!GetPartner(RegistrationOffice, RegistrationKey, out LocalPartnerKey, out PersonFirstnameLastname, Transaction))
                    {
                        Console.WriteLine("Cannot find partner key " + RegistrationKey.ToString() + " in row " + RowCount.ToString());
                        LocalPartnerKey = AUnknownPartner;
                        PersonFirstnameLastname = AUnkownPartnerName;

                        // we need to have a different reference, otherwise the gifts will be grouped for unknown donor, split gifts
                        reference = RowCount.ToString();
                    }

                    if (LocalPartnerKey == -1)
                    {
                        Console.WriteLine(
                            "Problem: no person key available from Petra. " + RegistrationKey.ToString() + " in row " + RowCount.ToString());
                        LocalPartnerKey = AUnknownPartner;

                        // we need to have a different reference, otherwise the gifts will be grouped for unknown donor, split gifts
                        reference = RowCount.ToString();
                    }

                    if (ApplicationFee > 0.0m)
                    {
                        string newLine = String.Empty;

                        string template = ATemplateApplicationFee;

                        newLine = StringHelper.AddCSV(newLine, LocalPartnerKey.ToString(), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, PersonFirstnameLastname, OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, reference, OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "<none>", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "0", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, ApplicationFee.ToString(), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "no", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, StringHelper.GetNextCSV(ref template), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, StringHelper.GetNextCSV(ref template), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, StringHelper.GetNextCSV(ref template), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "Both", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "Both", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "Both", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "yes", OutputSeparator);

                        ResultString += Environment.NewLine + newLine;
                    }

                    if (ManualApplicationFee > 0.0m)
                    {
                        string newLine = String.Empty;
                        string template = ATemplateManualApplication;

                        newLine = StringHelper.AddCSV(newLine, LocalPartnerKey.ToString(), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, PersonFirstnameLastname, OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, reference, OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "<none>", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "0", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, ManualApplicationFee.ToString(), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "no", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, StringHelper.GetNextCSV(ref template), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, StringHelper.GetNextCSV(ref template), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, StringHelper.GetNextCSV(ref template), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "Both", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "Both", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "Both", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "yes", OutputSeparator);

                        ResultString += Environment.NewLine + newLine;
                    }

                    if (ConferenceFee > 0.0m)
                    {
                        string newLine = String.Empty;
                        string template = ATemplateConferenceFee;

                        newLine = StringHelper.AddCSV(newLine, LocalPartnerKey.ToString(), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, PersonFirstnameLastname, OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, reference, OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "<none>", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "0", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, ConferenceFee.ToString(), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "no", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, StringHelper.GetNextCSV(ref template), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, StringHelper.GetNextCSV(ref template), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, StringHelper.GetNextCSV(ref template), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "Both", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "Both", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "Both", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "yes", OutputSeparator);

                        ResultString += Environment.NewLine + newLine;
                    }

                    if (Donation > 0.0m)
                    {
                        string newLine = String.Empty;
                        string template = ATemplateDonation;

                        // TODO: depending on amount??? assign to family key?
                        newLine = StringHelper.AddCSV(newLine, AUnknownPartner.ToString(), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, AUnkownPartnerName, OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "CASH", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, reference, OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "<none>", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, StringHelper.GetNextCSV(ref template), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, StringHelper.GetNextCSV(ref template), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, Donation.ToString(), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "no", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, StringHelper.GetNextCSV(ref template), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, StringHelper.GetNextCSV(ref template), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, StringHelper.GetNextCSV(ref template), OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "Both", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "Both", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "Both", OutputSeparator);
                        newLine = StringHelper.AddCSV(newLine, "yes", OutputSeparator);

                        ResultString += Environment.NewLine + newLine;
                    }

                    SEPADirectDebitTDSSEPADirectDebitDetailsRow SepaRow;

                    try
                    {
                        SepaRow =
                            PrepareEmail(PartnerInfoRow, AEmailTemplate, ref AMainDS,
                                ConferenceFee,
                                ApplicationFee,
                                Donation,
                                ManualApplicationFee,
                                AMandatePrefix,
                                ACollectionDate,
                                ACreditorSchemeID);
                    }
                    catch (Exception e)
                    {
                        TLogging.Log(e.ToString());
                        AVerificationResult.Add(new TVerificationResult(
                                "Problem Zettel Nr " + CountLines.ToString(),
                                "Partner Key " + OrigRegistrationKey.ToString() + ": " + e.Message,
                                TResultSeverity.Resv_Critical,
                                new Guid()));

                        continue;
                    }

                    sepaWriter.AddPaymentToSEPADirectDebitFile(
                        "OOFF",
                        SepaRow.BankAccountOwnerShortName,
                        SepaRow.IBAN,
                        SepaRow.BIC,
                        SepaRow.SEPAMandateID,
                        Convert.ToDateTime(PartnerInfoRow["ApplicationDate"]),
                        SepaRow.Amount,
                        ADirectDebitDescription.Replace("#PARTICIPANT", SepaRow.ParticipantName),
                        SepaRow.ParticipantPartnerKey.ToString());
                    RowCount++;
                }
            }
            finally
            {
                DBAccess.GDBAccessObj.RollbackTransaction();
            }

            AGiftBatchCSV = ResultString;
            ASepaDirectDebitXML = sepaWriter.Document.OuterXml;
            ADirectDebitReport = GenerateDirectDebitReport(AMainDS, AReportTemplate);

            if (AVerificationResult.HasCriticalErrors)
            {
                TLogging.Log(AVerificationResult.BuildVerificationResultString());
            }

            return AVerificationResult.HasCriticalErrors == false;
        }

        private static string GenerateDirectDebitReport(SEPADirectDebitTDS AMainDS, string AReportTemplate)
        {
            // recognise detail lines automatically
            string RowTemplate;
            string report = TPrinterHtml.GetTableRowByDetailComment(AReportTemplate, out RowTemplate);

            StringBuilder rowTexts = new StringBuilder(string.Empty);

            Decimal Sum = 0.0m;
            Int32 NumberPrinted = 0;

            AMainDS.SEPADirectDebitDetails.DefaultView.Sort = SEPADirectDebitTDSSEPADirectDebitDetailsTable.GetBankAccountOwnerShortNameDBName();

            foreach (DataRowView rv in AMainDS.SEPADirectDebitDetails.DefaultView)
            {
                SEPADirectDebitTDSSEPADirectDebitDetailsRow SepaRow = (SEPADirectDebitTDSSEPADirectDebitDetailsRow)rv.Row;
                string rowToPrint = RowTemplate;

                rowToPrint = rowToPrint.Replace("#ACCOUNTNAME", SepaRow.BankAccountOwnerShortName);
                rowToPrint = rowToPrint.Replace("#DESCRIPTION", SepaRow.ParticipantName);
                rowToPrint = rowToPrint.Replace("#PARTICIPANTKEY", SepaRow.ParticipantPartnerKey.ToString());
                rowToPrint = rowToPrint.Replace("#GIFTBATCHORDERNR", SepaRow.OrderInGiftBatch.ToString());
                rowToPrint = rowToPrint.Replace("#AMOUNT", String.Format("{0:C}", SepaRow.Amount));
                rowToPrint = rowToPrint.Replace("#IBAN", FormatIBANReadable(SepaRow.IBAN));
                rowToPrint = rowToPrint.Replace("#BIC", SepaRow.BIC);
                rowToPrint = rowToPrint.Replace("#MANDATEID", SepaRow.SEPAMandateID);
                rowTexts.Append(rowToPrint);

                Sum += Convert.ToDecimal(SepaRow.Amount);
                NumberPrinted++;
            }

            Sum = Math.Round(Sum, 2);

            report = report.Replace("#ROWTEMPLATE", rowTexts.ToString());
            report = report.Replace("#PRINTDATE", DateTime.Now.ToShortDateString());
            report = report.Replace("#TOTALAMOUNT", String.Format("{0:C}", Sum));
            report = report.Replace("#TOTALNUMBER", NumberPrinted.ToString());

            return report;
        }

        private static bool ConvertBankAccountCodeToIBANandBic(string AAccountNumber, string ABankSortCode, out string AIBAN, out string ABIC)
        {
            string url =
                TAppSettingsManager.GetValue("SEPA.ConvertBankAccountNumbersToIBAN.URL", "https://kontocheck.solidcharity.com/index.php", false) +
                "?kto=" + AAccountNumber.Replace(" ", "") + "&blz=" + ABankSortCode.Replace(" ", "");
            string result = THTTPUtils.ReadWebsite(url);

            if (result == null)
            {
                throw new Exception("ConvertBankAccountCodeToIBANandBic: problem reading IBAN and BIC from " + url);
            }

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(result.Replace("&uuml;", "ue"));

            if (!TXMLParser.FindNodeRecursive(doc.DocumentElement, "kontocheck").InnerText.StartsWith("ok"))
            {
                TLogging.Log("Problem with converting bank account number to IBAN " + AAccountNumber + " " + ABankSortCode);
                TLogging.Log(result);
                ABIC = string.Empty;
                AIBAN = string.Empty;
                return false;
            }

            AIBAN = TXMLParser.FindNodeRecursive(doc.DocumentElement, "iban").InnerText;
            ABIC = TXMLParser.FindNodeRecursive(doc.DocumentElement, "bic").InnerText;
            return true;
        }

        private static bool ValidateIBANandBic(string AIban, ref string ABic)
        {
            string url =
                TAppSettingsManager.GetValue("SEPA.ConvertBankAccountNumbersToIBAN.URL", "https://kontocheck.solidcharity.com/index.php", false) +
                "?iban=" + AIban + "&bic=" + ABic;
            string result = THTTPUtils.ReadWebsite(url);

            if (result == null)
            {
                throw new Exception("ValidateIBANandBic: problem validating IBAN and BIC from " + url);
            }

            try
            {
                XmlDocument doc = new XmlDocument();

                doc.LoadXml(result);

                if (TXMLParser.FindNodeRecursive(doc.DocumentElement, "iban").InnerText != "1")
                {
                    return false;
                }

                if (TXMLParser.FindNodeRecursive(doc.DocumentElement, "bic").InnerText != "1")
                {
                    // use the proposed BIC
                    ABic = TXMLParser.FindNodeRecursive(doc.DocumentElement, "bic").InnerText;
                    // return false;
                }
            }
            catch (Exception e)
            {
                TLogging.Log(e.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// create emails to be sent to bank account owners before the SEPA direct debit is initiated
        /// </summary>
        private static SEPADirectDebitTDSSEPADirectDebitDetailsRow PrepareEmail(
            SEPADirectDebitTDSPartnerInfoRow APartnerInfoRow, string AEmailTemplate, ref SEPADirectDebitTDS AMainDS,
            decimal AConferenceFee,
            decimal AApplicationFee,
            decimal ADonation,
            decimal AManualApplicationFee,
            string AMandatePrefix,
            DateTime ACollectionDate,
            string ACreditorSchemeID)
        {
            SEPADirectDebitTDSSEPADirectDebitDetailsRow SepaRow = AMainDS.SEPADirectDebitDetails.NewRowTyped();

            Decimal TotalAmount = AConferenceFee + AApplicationFee + ADonation + AManualApplicationFee;
            string AmountDetail =
                (AApplicationFee > 0 ? "Anmeldegebühr: " + String.Format("{0:C}", AApplicationFee) + "<br/>" : String.Empty) +
                (AManualApplicationFee > 0 ? "Aufschlag manuelle Anmeldung: " +
                 String.Format("{0:C}", AManualApplicationFee) + "<br/>" : String.Empty) +
                (AConferenceFee > 0 ? "Teilnehmerbeitrag: " + String.Format("{0:C}", AConferenceFee) + "<br/>" : String.Empty) +
                (ADonation > 0 ? "Spende: " + String.Format("{0:C}", ADonation) + "<br/>" : String.Empty);

            SepaRow.ParticipantName = APartnerInfoRow.FirstName + " " + APartnerInfoRow.FamilyName;
            SepaRow.Amount = TotalAmount;
            SepaRow.OrderInGiftBatch = AMainDS.SEPADirectDebitDetails.Rows.Count + 1;
            SepaRow.IBAN = APartnerInfoRow.IBAN.Replace(" ", "");
            SepaRow.BIC = APartnerInfoRow.BIC.Replace(" ", "");
            try
            {
                SepaRow.SEPAMandateID = AMandatePrefix + APartnerInfoRow.ApplicationDate.ToString("yyyyMMdd") +
                                        StringHelper.FormatStrToPartnerKeyString(APartnerInfoRow.PartnerKey.ToString());
            }
            catch (Exception)
            {
                throw new Exception(
                    "problem building mandate from application date: " + APartnerInfoRow.ApplicationDate.ToString());
            }
            SepaRow.OnlineRegistrationKey = APartnerInfoRow.RegistrationKey;
            SepaRow.ParticipantPartnerKey = APartnerInfoRow.PartnerKey;
            SepaRow.BankAccountOwnerEmail = APartnerInfoRow.BankAccountEmail;
            SepaRow.BankAccountOwnerEmail = SepaRow.BankAccountOwnerEmail.ToLower().
                                            Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss");
            string BankAccountName = APartnerInfoRow.BankAccountName;

            // capitalize first letters of each word, if word has more than 3 characters
            if (BankAccountName.Length == 0)
            {
                throw new Exception("Name des Kontoinhabers fehlt");
            }

            if (char.IsLower(BankAccountName[0]))
            {
                string[] words = BankAccountName.Split(new char[] { ' ' });
                BankAccountName = string.Empty;

                foreach (string word in words)
                {
                    if (BankAccountName.Length > 0)
                    {
                        BankAccountName = BankAccountName + " ";
                    }

                    if (word.Length > 3)
                    {
                        BankAccountName = BankAccountName + char.ToUpper(word[0]) + word.Substring(1);
                    }
                    else
                    {
                        BankAccountName = BankAccountName + word;
                    }
                }
            }

            SepaRow.BankAccountOwnerName = BankAccountName;
            SepaRow.BankAccountOwnerShortName = BankAccountName;

            if (BankAccountName.Contains(" "))
            {
                SepaRow.BankAccountOwnerShortName = BankAccountName.Substring(BankAccountName.LastIndexOf(" ") + 1) + ", " +
                                                    BankAccountName.Substring(0, BankAccountName.LastIndexOf(" "));
            }

            string toAddress = "\"" + SepaRow.BankAccountOwnerName + "\" <" + SepaRow.BankAccountOwnerEmail + ">";

            TVerificationResult validEmail = TStringChecks.ValidateEmail(SepaRow.BankAccountOwnerEmail);

            if ((validEmail != null) && (validEmail.ResultSeverity == TResultSeverity.Resv_Critical))
            {
                throw new Exception("Email Adresse " + SepaRow.BankAccountOwnerEmail + " ist ungültig");
            }

            // hide last 3 digits of account number and BLZ
            string iban = SepaRow.IBAN;
            iban = iban.Substring(0, 9) + "xxx" + iban.Substring(12);
            iban = iban.Substring(0, iban.Length - 3) + "xxx";
            iban = FormatIBANReadable(iban);
            string bic = SepaRow.BIC;
            bic = "xxx" + bic.Substring(3, bic.Length - 6) + "xxx";

            string body = AEmailTemplate;
            body = body.Replace("#PARTICIPANT", SepaRow.ParticipantName);
            body = body.Replace("#BANKACCOUNTOWNER", SepaRow.BankAccountOwnerName);
            body = body.Replace("#IBAN", iban);
            body = body.Replace("#BIC", bic);
            body = body.Replace("#AMOUNTDETAIL", AmountDetail);
            body = body.Replace("#AMOUNT", String.Format("{0:C}", TotalAmount));
            body = body.Replace("#DATESEPA", ACollectionDate.ToShortDateString());
            body = body.Replace("#MANDATEID", SepaRow.SEPAMandateID);
            body = body.Replace("#CREDITORSCHEMEID", ACreditorSchemeID);

            if (SepaRow.ParticipantName == SepaRow.BankAccountOwnerName)
            {
                body = TPrinterHtml.RemoveDivWithClass(body, "AndererKontoinhaber");
            }
            else
            {
                body = TPrinterHtml.RemoveDivWithClass(body, "EigenesKonto");
                // TODO if bank account owner is different than participant, but email the same:
                // tell email recipient to forward to account owner
            }

            SepaRow.EmailBody = body;
            SepaRow.EmailSubject = body.Substring(body.IndexOf("<title>") + "<title>".Length);
            SepaRow.EmailSubject = SepaRow.EmailSubject.Substring(0, SepaRow.EmailSubject.IndexOf("</title>"));

            AMainDS.SEPADirectDebitDetails.Rows.Add(SepaRow);

            return SepaRow;
        }

        /// <summary>
        /// format an IBAN in groups of 4 characters, for human eyes
        /// </summary>
        private static string FormatIBANReadable(string AIban)
        {
            string result = string.Empty;
            int pos = 0;

            while (pos + 4 < AIban.Length)
            {
                result += AIban.Substring(pos, 4) + " ";
                pos += 4;
            }

            return result + AIban.Substring(pos);
        }
    }
}
