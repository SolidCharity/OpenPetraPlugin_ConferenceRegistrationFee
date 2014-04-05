// auto generated with nant generateORM using DataSets.cs
// Do not modify this file manually!
//
//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       auto generated
//
// Copyright 2004-2013 by OM International
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

using Ict.Common;
using Ict.Common.Data;
using System;
using System.Data;
using System.Data.Odbc;
using Ict.Petra.Shared.MConference.Data;
using Ict.Petra.Shared.MPersonnel.Personnel.Data;
using Ict.Petra.Shared.MPartner.Partner.Data;

namespace Ict.Petra.Plugins.ConferenceRegistrationFees.Data
{
     /// auto generated
    [Serializable()]
    public class SEPADirectDebitTDS : TTypedDataSet
    {

        private SEPADirectDebitTDSSEPADirectDebitDetailsTable TableSEPADirectDebitDetails;

        /// auto generated
        public SEPADirectDebitTDS() :
                base("SEPADirectDebitTDS")
        {
        }

        /// auto generated for serialization
        public SEPADirectDebitTDS(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) :
                base(info, context)
        {
        }

        /// auto generated
        public SEPADirectDebitTDS(string ADatasetName) :
                base(ADatasetName)
        {
        }

        /// auto generated
        public SEPADirectDebitTDSSEPADirectDebitDetailsTable SEPADirectDebitDetails
        {
            get
            {
                return this.TableSEPADirectDebitDetails;
            }
        }

        /// auto generated
        public new virtual SEPADirectDebitTDS GetChangesTyped(bool removeEmptyTables)
        {
            SEPADirectDebitTDS result = (SEPADirectDebitTDS)(base.GetChangesTyped(removeEmptyTables));

            if (removeEmptyTables
                && (result != null))
            {
                result.InitVars();
            }

            return result;
        }

        /// auto generated
        protected override void InitTables()
        {
            this.Tables.Add(new SEPADirectDebitTDSSEPADirectDebitDetailsTable("SEPADirectDebitDetails"));
        }

        /// auto generated
        protected override void InitTables(System.Data.DataSet ds)
        {
            if ((ds.Tables.IndexOf("SEPADirectDebitDetails") != -1))
            {
                this.Tables.Add(new SEPADirectDebitTDSSEPADirectDebitDetailsTable("SEPADirectDebitDetails"));
            }
        }

        /// auto generated
        protected override void MapTables()
        {
            this.InitVars();
            base.MapTables();
            if ((this.TableSEPADirectDebitDetails != null))
            {
                this.TableSEPADirectDebitDetails.InitVars();
            }
        }

        /// auto generated
        public override void InitVars()
        {
            this.DataSetName = "SEPADirectDebitTDS";
            this.TableSEPADirectDebitDetails = ((SEPADirectDebitTDSSEPADirectDebitDetailsTable)(this.Tables["SEPADirectDebitDetails"]));
        }

        /// auto generated
        protected override void InitConstraints()
        {
        }
    }

    ///
    [Serializable()]
    public class SEPADirectDebitTDSSEPADirectDebitDetailsTable : TTypedDataTable
    {
        /// TableId for Ict.Common.Data generic functions
        public static short TableId = 9000;
        /// used for generic TTypedDataTable functions
        public static short ColumnBankAccountOwnerShortNameId = 0;
        /// used for generic TTypedDataTable functions
        public static short ColumnBankAccountOwnerNameId = 1;
        /// used for generic TTypedDataTable functions
        public static short ColumnBankAccountOwnerEmailId = 2;
        /// used for generic TTypedDataTable functions
        public static short ColumnEmailSubjectId = 3;
        /// used for generic TTypedDataTable functions
        public static short ColumnEmailBodyId = 4;
        /// used for generic TTypedDataTable functions
        public static short ColumnIBANId = 5;
        /// used for generic TTypedDataTable functions
        public static short ColumnBICId = 6;
        /// used for generic TTypedDataTable functions
        public static short ColumnSEPAMandateIDId = 7;
        /// used for generic TTypedDataTable functions
        public static short ColumnParticipantNameId = 8;
        /// used for generic TTypedDataTable functions
        public static short ColumnParticipantPartnerKeyId = 9;
        /// used for generic TTypedDataTable functions
        public static short ColumnOnlineRegistrationKeyId = 10;
        /// used for generic TTypedDataTable functions
        public static short ColumnOrderInGiftBatchId = 11;
        /// used for generic TTypedDataTable functions
        public static short ColumnAmountId = 12;

        private static bool FInitInfoValues = InitInfoValues();
        private static bool InitInfoValues()
        {
            TableInfo.Add(TableId, new TTypedTableInfo(TableId, "SEPADirectDebitDetails", "SEPADirectDebitTDSSEPADirectDebitDetails",
                new TTypedColumnInfo[] {
                    new TTypedColumnInfo(0, "BankAccountOwnerShortName", "BankAccountOwnerShortName", "", OdbcType.VarChar, -1, false),
                    new TTypedColumnInfo(1, "BankAccountOwnerName", "BankAccountOwnerName", "", OdbcType.VarChar, -1, false),
                    new TTypedColumnInfo(2, "BankAccountOwnerEmail", "BankAccountOwnerEmail", "", OdbcType.VarChar, -1, false),
                    new TTypedColumnInfo(3, "EmailSubject", "EmailSubject", "", OdbcType.VarChar, -1, false),
                    new TTypedColumnInfo(4, "EmailBody", "EmailBody", "", OdbcType.VarChar, -1, false),
                    new TTypedColumnInfo(5, "IBAN", "IBAN", "", OdbcType.VarChar, -1, false),
                    new TTypedColumnInfo(6, "BIC", "BIC", "", OdbcType.VarChar, -1, false),
                    new TTypedColumnInfo(7, "SEPAMandateID", "SEPAMandateID", "", OdbcType.VarChar, -1, false),
                    new TTypedColumnInfo(8, "ParticipantName", "ParticipantName", "", OdbcType.VarChar, -1, false),
                    new TTypedColumnInfo(9, "ParticipantPartnerKey", "ParticipantPartnerKey", "", OdbcType.Decimal, -1, false),
                    new TTypedColumnInfo(10, "OnlineRegistrationKey", "OnlineRegistrationKey", "", OdbcType.Decimal, -1, false),
                    new TTypedColumnInfo(11, "OrderInGiftBatch", "OrderInGiftBatch", "", OdbcType.Int, -1, false),
                    new TTypedColumnInfo(12, "Amount", "Amount", "", OdbcType.Int, -1, false)
                },
                new int[] {
                }));

            // try to avoid a compiler warning about unused variable FInitInfoValues which we need for initially calling InitInfoValues once
            FInitInfoValues = true;
            return FInitInfoValues;
        }

        /// constructor
        public SEPADirectDebitTDSSEPADirectDebitDetailsTable() :
                base("SEPADirectDebitDetails")
        {
        }

        /// constructor
        public SEPADirectDebitTDSSEPADirectDebitDetailsTable(string ATablename) :
                base(ATablename)
        {
        }

        /// constructor for serialization
        public SEPADirectDebitTDSSEPADirectDebitDetailsTable(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) :
                base(info, context)
        {
        }

        ///
        public DataColumn ColumnBankAccountOwnerShortName;
        ///
        public DataColumn ColumnBankAccountOwnerName;
        ///
        public DataColumn ColumnBankAccountOwnerEmail;
        ///
        public DataColumn ColumnEmailSubject;
        ///
        public DataColumn ColumnEmailBody;
        ///
        public DataColumn ColumnIBAN;
        ///
        public DataColumn ColumnBIC;
        ///
        public DataColumn ColumnSEPAMandateID;
        ///
        public DataColumn ColumnParticipantName;
        ///
        public DataColumn ColumnParticipantPartnerKey;
        ///
        public DataColumn ColumnOnlineRegistrationKey;
        ///
        public DataColumn ColumnOrderInGiftBatch;
        ///
        public DataColumn ColumnAmount;

        /// create the columns
        protected override void InitClass()
        {
            this.Columns.Add(new System.Data.DataColumn("BankAccountOwnerShortName", typeof(string)));
            this.Columns.Add(new System.Data.DataColumn("BankAccountOwnerName", typeof(string)));
            this.Columns.Add(new System.Data.DataColumn("BankAccountOwnerEmail", typeof(string)));
            this.Columns.Add(new System.Data.DataColumn("EmailSubject", typeof(string)));
            this.Columns.Add(new System.Data.DataColumn("EmailBody", typeof(string)));
            this.Columns.Add(new System.Data.DataColumn("IBAN", typeof(string)));
            this.Columns.Add(new System.Data.DataColumn("BIC", typeof(string)));
            this.Columns.Add(new System.Data.DataColumn("SEPAMandateID", typeof(string)));
            this.Columns.Add(new System.Data.DataColumn("ParticipantName", typeof(string)));
            this.Columns.Add(new System.Data.DataColumn("ParticipantPartnerKey", typeof(Int64)));
            this.Columns.Add(new System.Data.DataColumn("OnlineRegistrationKey", typeof(Int64)));
            this.Columns.Add(new System.Data.DataColumn("OrderInGiftBatch", typeof(Int32)));
            this.Columns.Add(new System.Data.DataColumn("Amount", typeof(Decimal)));
        }

        /// assign columns to properties, set primary key
        public override void InitVars()
        {
            this.ColumnBankAccountOwnerShortName = this.Columns["BankAccountOwnerShortName"];
            this.ColumnBankAccountOwnerName = this.Columns["BankAccountOwnerName"];
            this.ColumnBankAccountOwnerEmail = this.Columns["BankAccountOwnerEmail"];
            this.ColumnEmailSubject = this.Columns["EmailSubject"];
            this.ColumnEmailBody = this.Columns["EmailBody"];
            this.ColumnIBAN = this.Columns["IBAN"];
            this.ColumnBIC = this.Columns["BIC"];
            this.ColumnSEPAMandateID = this.Columns["SEPAMandateID"];
            this.ColumnParticipantName = this.Columns["ParticipantName"];
            this.ColumnParticipantPartnerKey = this.Columns["ParticipantPartnerKey"];
            this.ColumnOnlineRegistrationKey = this.Columns["OnlineRegistrationKey"];
            this.ColumnOrderInGiftBatch = this.Columns["OrderInGiftBatch"];
            this.ColumnAmount = this.Columns["Amount"];
        }

        /// Access a typed row by index
        public SEPADirectDebitTDSSEPADirectDebitDetailsRow this[int i]
        {
            get
            {
                return ((SEPADirectDebitTDSSEPADirectDebitDetailsRow)(this.Rows[i]));
            }
        }

        /// create a new typed row
        public SEPADirectDebitTDSSEPADirectDebitDetailsRow NewRowTyped(bool AWithDefaultValues)
        {
            SEPADirectDebitTDSSEPADirectDebitDetailsRow ret = ((SEPADirectDebitTDSSEPADirectDebitDetailsRow)(this.NewRow()));
            if ((AWithDefaultValues == true))
            {
                ret.InitValues();
            }
            return ret;
        }

        /// create a new typed row, always with default values
        public SEPADirectDebitTDSSEPADirectDebitDetailsRow NewRowTyped()
        {
            return this.NewRowTyped(true);
        }

        /// new typed row using DataRowBuilder
        protected override System.Data.DataRow NewRowFromBuilder(System.Data.DataRowBuilder builder)
        {
            return new SEPADirectDebitTDSSEPADirectDebitDetailsRow(builder);
        }

        /// get typed set of changes
        public SEPADirectDebitTDSSEPADirectDebitDetailsTable GetChangesTyped()
        {
            return ((SEPADirectDebitTDSSEPADirectDebitDetailsTable)(base.GetChangesTypedInternal()));
        }

        /// return the CamelCase name of the table
        public static string GetTableName()
        {
            return "SEPADirectDebitDetails";
        }

        /// return the name of the table as it is used in the database
        public static string GetTableDBName()
        {
            return "SEPADirectDebitTDSSEPADirectDebitDetails";
        }

        /// return the 'Label' of the table as it is used in the database (the 'Label' is usually a short description of what the db table is about)
        public static string GetTableDBLabel()
        {
            return "";
        }

        /// get an odbc parameter for the given column
        public override OdbcParameter CreateOdbcParameter(Int32 AColumnNr)
        {
            return CreateOdbcParameter(TableId, AColumnNr);
        }

        /// get the name of the field in the database for this column
        public static string GetBankAccountOwnerShortNameDBName()
        {
            return "BankAccountOwnerShortName";
        }

        /// get character length for column
        public static short GetBankAccountOwnerShortNameLength()
        {
            return -1;
        }

        /// get the help text for the field in the database for this column
        public static string GetBankAccountOwnerShortNameHelp()
        {
            return "";
        }

        /// get the name of the field in the database for this column
        public static string GetBankAccountOwnerNameDBName()
        {
            return "BankAccountOwnerName";
        }

        /// get character length for column
        public static short GetBankAccountOwnerNameLength()
        {
            return -1;
        }

        /// get the help text for the field in the database for this column
        public static string GetBankAccountOwnerNameHelp()
        {
            return "";
        }

        /// get the name of the field in the database for this column
        public static string GetBankAccountOwnerEmailDBName()
        {
            return "BankAccountOwnerEmail";
        }

        /// get character length for column
        public static short GetBankAccountOwnerEmailLength()
        {
            return -1;
        }

        /// get the help text for the field in the database for this column
        public static string GetBankAccountOwnerEmailHelp()
        {
            return "";
        }

        /// get the name of the field in the database for this column
        public static string GetEmailSubjectDBName()
        {
            return "EmailSubject";
        }

        /// get character length for column
        public static short GetEmailSubjectLength()
        {
            return -1;
        }

        /// get the help text for the field in the database for this column
        public static string GetEmailSubjectHelp()
        {
            return "";
        }

        /// get the name of the field in the database for this column
        public static string GetEmailBodyDBName()
        {
            return "EmailBody";
        }

        /// get character length for column
        public static short GetEmailBodyLength()
        {
            return -1;
        }

        /// get the help text for the field in the database for this column
        public static string GetEmailBodyHelp()
        {
            return "";
        }

        /// get the name of the field in the database for this column
        public static string GetIBANDBName()
        {
            return "IBAN";
        }

        /// get character length for column
        public static short GetIBANLength()
        {
            return -1;
        }

        /// get the help text for the field in the database for this column
        public static string GetIBANHelp()
        {
            return "";
        }

        /// get the name of the field in the database for this column
        public static string GetBICDBName()
        {
            return "BIC";
        }

        /// get character length for column
        public static short GetBICLength()
        {
            return -1;
        }

        /// get the help text for the field in the database for this column
        public static string GetBICHelp()
        {
            return "";
        }

        /// get the name of the field in the database for this column
        public static string GetSEPAMandateIDDBName()
        {
            return "SEPAMandateID";
        }

        /// get character length for column
        public static short GetSEPAMandateIDLength()
        {
            return -1;
        }

        /// get the help text for the field in the database for this column
        public static string GetSEPAMandateIDHelp()
        {
            return "";
        }

        /// get the name of the field in the database for this column
        public static string GetParticipantNameDBName()
        {
            return "ParticipantName";
        }

        /// get character length for column
        public static short GetParticipantNameLength()
        {
            return -1;
        }

        /// get the help text for the field in the database for this column
        public static string GetParticipantNameHelp()
        {
            return "";
        }

        /// get the name of the field in the database for this column
        public static string GetParticipantPartnerKeyDBName()
        {
            return "ParticipantPartnerKey";
        }

        /// get character length for column
        public static short GetParticipantPartnerKeyLength()
        {
            return -1;
        }

        /// get the help text for the field in the database for this column
        public static string GetParticipantPartnerKeyHelp()
        {
            return "";
        }

        /// get the name of the field in the database for this column
        public static string GetOnlineRegistrationKeyDBName()
        {
            return "OnlineRegistrationKey";
        }

        /// get character length for column
        public static short GetOnlineRegistrationKeyLength()
        {
            return -1;
        }

        /// get the help text for the field in the database for this column
        public static string GetOnlineRegistrationKeyHelp()
        {
            return "";
        }

        /// get the name of the field in the database for this column
        public static string GetOrderInGiftBatchDBName()
        {
            return "OrderInGiftBatch";
        }

        /// get character length for column
        public static short GetOrderInGiftBatchLength()
        {
            return -1;
        }

        /// get the help text for the field in the database for this column
        public static string GetOrderInGiftBatchHelp()
        {
            return "";
        }

        /// get the name of the field in the database for this column
        public static string GetAmountDBName()
        {
            return "Amount";
        }

        /// get character length for column
        public static short GetAmountLength()
        {
            return -1;
        }

        /// get the help text for the field in the database for this column
        public static string GetAmountHelp()
        {
            return "";
        }

    }

    ///
    [Serializable()]
    public class SEPADirectDebitTDSSEPADirectDebitDetailsRow : System.Data.DataRow
    {
        private SEPADirectDebitTDSSEPADirectDebitDetailsTable myTable;

        /// Constructor
        public SEPADirectDebitTDSSEPADirectDebitDetailsRow(System.Data.DataRowBuilder rb) :
                base(rb)
        {
            this.myTable = ((SEPADirectDebitTDSSEPADirectDebitDetailsTable)(this.Table));
        }

        ///
        public string BankAccountOwnerShortName
        {
            get
            {
                object ret;
                ret = this[this.myTable.ColumnBankAccountOwnerShortName.Ordinal];
                if ((ret == System.DBNull.Value))
                {
                    return String.Empty;
                }
                else
                {
                    return ((string)(ret));
                }
            }
            set
            {
                if ((this.IsNull(this.myTable.ColumnBankAccountOwnerShortName)
                            || (((string)(this[this.myTable.ColumnBankAccountOwnerShortName])) != value)))
                {
                    this[this.myTable.ColumnBankAccountOwnerShortName] = value;
                }
            }
        }

        ///
        public string BankAccountOwnerName
        {
            get
            {
                object ret;
                ret = this[this.myTable.ColumnBankAccountOwnerName.Ordinal];
                if ((ret == System.DBNull.Value))
                {
                    return String.Empty;
                }
                else
                {
                    return ((string)(ret));
                }
            }
            set
            {
                if ((this.IsNull(this.myTable.ColumnBankAccountOwnerName)
                            || (((string)(this[this.myTable.ColumnBankAccountOwnerName])) != value)))
                {
                    this[this.myTable.ColumnBankAccountOwnerName] = value;
                }
            }
        }

        ///
        public string BankAccountOwnerEmail
        {
            get
            {
                object ret;
                ret = this[this.myTable.ColumnBankAccountOwnerEmail.Ordinal];
                if ((ret == System.DBNull.Value))
                {
                    return String.Empty;
                }
                else
                {
                    return ((string)(ret));
                }
            }
            set
            {
                if ((this.IsNull(this.myTable.ColumnBankAccountOwnerEmail)
                            || (((string)(this[this.myTable.ColumnBankAccountOwnerEmail])) != value)))
                {
                    this[this.myTable.ColumnBankAccountOwnerEmail] = value;
                }
            }
        }

        ///
        public string EmailSubject
        {
            get
            {
                object ret;
                ret = this[this.myTable.ColumnEmailSubject.Ordinal];
                if ((ret == System.DBNull.Value))
                {
                    return String.Empty;
                }
                else
                {
                    return ((string)(ret));
                }
            }
            set
            {
                if ((this.IsNull(this.myTable.ColumnEmailSubject)
                            || (((string)(this[this.myTable.ColumnEmailSubject])) != value)))
                {
                    this[this.myTable.ColumnEmailSubject] = value;
                }
            }
        }

        ///
        public string EmailBody
        {
            get
            {
                object ret;
                ret = this[this.myTable.ColumnEmailBody.Ordinal];
                if ((ret == System.DBNull.Value))
                {
                    return String.Empty;
                }
                else
                {
                    return ((string)(ret));
                }
            }
            set
            {
                if ((this.IsNull(this.myTable.ColumnEmailBody)
                            || (((string)(this[this.myTable.ColumnEmailBody])) != value)))
                {
                    this[this.myTable.ColumnEmailBody] = value;
                }
            }
        }

        ///
        public string IBAN
        {
            get
            {
                object ret;
                ret = this[this.myTable.ColumnIBAN.Ordinal];
                if ((ret == System.DBNull.Value))
                {
                    return String.Empty;
                }
                else
                {
                    return ((string)(ret));
                }
            }
            set
            {
                if ((this.IsNull(this.myTable.ColumnIBAN)
                            || (((string)(this[this.myTable.ColumnIBAN])) != value)))
                {
                    this[this.myTable.ColumnIBAN] = value;
                }
            }
        }

        ///
        public string BIC
        {
            get
            {
                object ret;
                ret = this[this.myTable.ColumnBIC.Ordinal];
                if ((ret == System.DBNull.Value))
                {
                    return String.Empty;
                }
                else
                {
                    return ((string)(ret));
                }
            }
            set
            {
                if ((this.IsNull(this.myTable.ColumnBIC)
                            || (((string)(this[this.myTable.ColumnBIC])) != value)))
                {
                    this[this.myTable.ColumnBIC] = value;
                }
            }
        }

        ///
        public string SEPAMandateID
        {
            get
            {
                object ret;
                ret = this[this.myTable.ColumnSEPAMandateID.Ordinal];
                if ((ret == System.DBNull.Value))
                {
                    return String.Empty;
                }
                else
                {
                    return ((string)(ret));
                }
            }
            set
            {
                if ((this.IsNull(this.myTable.ColumnSEPAMandateID)
                            || (((string)(this[this.myTable.ColumnSEPAMandateID])) != value)))
                {
                    this[this.myTable.ColumnSEPAMandateID] = value;
                }
            }
        }

        ///
        public string ParticipantName
        {
            get
            {
                object ret;
                ret = this[this.myTable.ColumnParticipantName.Ordinal];
                if ((ret == System.DBNull.Value))
                {
                    return String.Empty;
                }
                else
                {
                    return ((string)(ret));
                }
            }
            set
            {
                if ((this.IsNull(this.myTable.ColumnParticipantName)
                            || (((string)(this[this.myTable.ColumnParticipantName])) != value)))
                {
                    this[this.myTable.ColumnParticipantName] = value;
                }
            }
        }

        ///
        public Int64 ParticipantPartnerKey
        {
            get
            {
                object ret;
                ret = this[this.myTable.ColumnParticipantPartnerKey.Ordinal];
                if ((ret == System.DBNull.Value))
                {
                    throw new System.Data.StrongTypingException("Error: DB null", null);
                }
                else
                {
                    return ((Int64)(ret));
                }
            }
            set
            {
                if ((this.IsNull(this.myTable.ColumnParticipantPartnerKey)
                            || (((Int64)(this[this.myTable.ColumnParticipantPartnerKey])) != value)))
                {
                    this[this.myTable.ColumnParticipantPartnerKey] = value;
                }
            }
        }

        ///
        public Int64 OnlineRegistrationKey
        {
            get
            {
                object ret;
                ret = this[this.myTable.ColumnOnlineRegistrationKey.Ordinal];
                if ((ret == System.DBNull.Value))
                {
                    throw new System.Data.StrongTypingException("Error: DB null", null);
                }
                else
                {
                    return ((Int64)(ret));
                }
            }
            set
            {
                if ((this.IsNull(this.myTable.ColumnOnlineRegistrationKey)
                            || (((Int64)(this[this.myTable.ColumnOnlineRegistrationKey])) != value)))
                {
                    this[this.myTable.ColumnOnlineRegistrationKey] = value;
                }
            }
        }

        ///
        public Int32 OrderInGiftBatch
        {
            get
            {
                object ret;
                ret = this[this.myTable.ColumnOrderInGiftBatch.Ordinal];
                if ((ret == System.DBNull.Value))
                {
                    throw new System.Data.StrongTypingException("Error: DB null", null);
                }
                else
                {
                    return ((Int32)(ret));
                }
            }
            set
            {
                if ((this.IsNull(this.myTable.ColumnOrderInGiftBatch)
                            || (((Int32)(this[this.myTable.ColumnOrderInGiftBatch])) != value)))
                {
                    this[this.myTable.ColumnOrderInGiftBatch] = value;
                }
            }
        }

        ///
        public Decimal Amount
        {
            get
            {
                object ret;
                ret = this[this.myTable.ColumnAmount.Ordinal];
                if ((ret == System.DBNull.Value))
                {
                    throw new System.Data.StrongTypingException("Error: DB null", null);
                }
                else
                {
                    return ((Decimal)(ret));
                }
            }
            set
            {
                if ((this.IsNull(this.myTable.ColumnAmount)
                            || (((Decimal)(this[this.myTable.ColumnAmount])) != value)))
                {
                    this[this.myTable.ColumnAmount] = value;
                }
            }
        }

        /// set default values
        public virtual void InitValues()
        {
            this.SetNull(this.myTable.ColumnBankAccountOwnerShortName);
            this.SetNull(this.myTable.ColumnBankAccountOwnerName);
            this.SetNull(this.myTable.ColumnBankAccountOwnerEmail);
            this.SetNull(this.myTable.ColumnEmailSubject);
            this.SetNull(this.myTable.ColumnEmailBody);
            this.SetNull(this.myTable.ColumnIBAN);
            this.SetNull(this.myTable.ColumnBIC);
            this.SetNull(this.myTable.ColumnSEPAMandateID);
            this.SetNull(this.myTable.ColumnParticipantName);
            this.SetNull(this.myTable.ColumnParticipantPartnerKey);
            this.SetNull(this.myTable.ColumnOnlineRegistrationKey);
            this.SetNull(this.myTable.ColumnOrderInGiftBatch);
            this.SetNull(this.myTable.ColumnAmount);
        }

        /// test for NULL value
        public bool IsBankAccountOwnerShortNameNull()
        {
            return this.IsNull(this.myTable.ColumnBankAccountOwnerShortName);
        }

        /// assign NULL value
        public void SetBankAccountOwnerShortNameNull()
        {
            this.SetNull(this.myTable.ColumnBankAccountOwnerShortName);
        }

        /// test for NULL value
        public bool IsBankAccountOwnerNameNull()
        {
            return this.IsNull(this.myTable.ColumnBankAccountOwnerName);
        }

        /// assign NULL value
        public void SetBankAccountOwnerNameNull()
        {
            this.SetNull(this.myTable.ColumnBankAccountOwnerName);
        }

        /// test for NULL value
        public bool IsBankAccountOwnerEmailNull()
        {
            return this.IsNull(this.myTable.ColumnBankAccountOwnerEmail);
        }

        /// assign NULL value
        public void SetBankAccountOwnerEmailNull()
        {
            this.SetNull(this.myTable.ColumnBankAccountOwnerEmail);
        }

        /// test for NULL value
        public bool IsEmailSubjectNull()
        {
            return this.IsNull(this.myTable.ColumnEmailSubject);
        }

        /// assign NULL value
        public void SetEmailSubjectNull()
        {
            this.SetNull(this.myTable.ColumnEmailSubject);
        }

        /// test for NULL value
        public bool IsEmailBodyNull()
        {
            return this.IsNull(this.myTable.ColumnEmailBody);
        }

        /// assign NULL value
        public void SetEmailBodyNull()
        {
            this.SetNull(this.myTable.ColumnEmailBody);
        }

        /// test for NULL value
        public bool IsIBANNull()
        {
            return this.IsNull(this.myTable.ColumnIBAN);
        }

        /// assign NULL value
        public void SetIBANNull()
        {
            this.SetNull(this.myTable.ColumnIBAN);
        }

        /// test for NULL value
        public bool IsBICNull()
        {
            return this.IsNull(this.myTable.ColumnBIC);
        }

        /// assign NULL value
        public void SetBICNull()
        {
            this.SetNull(this.myTable.ColumnBIC);
        }

        /// test for NULL value
        public bool IsSEPAMandateIDNull()
        {
            return this.IsNull(this.myTable.ColumnSEPAMandateID);
        }

        /// assign NULL value
        public void SetSEPAMandateIDNull()
        {
            this.SetNull(this.myTable.ColumnSEPAMandateID);
        }

        /// test for NULL value
        public bool IsParticipantNameNull()
        {
            return this.IsNull(this.myTable.ColumnParticipantName);
        }

        /// assign NULL value
        public void SetParticipantNameNull()
        {
            this.SetNull(this.myTable.ColumnParticipantName);
        }

        /// test for NULL value
        public bool IsParticipantPartnerKeyNull()
        {
            return this.IsNull(this.myTable.ColumnParticipantPartnerKey);
        }

        /// assign NULL value
        public void SetParticipantPartnerKeyNull()
        {
            this.SetNull(this.myTable.ColumnParticipantPartnerKey);
        }

        /// test for NULL value
        public bool IsOnlineRegistrationKeyNull()
        {
            return this.IsNull(this.myTable.ColumnOnlineRegistrationKey);
        }

        /// assign NULL value
        public void SetOnlineRegistrationKeyNull()
        {
            this.SetNull(this.myTable.ColumnOnlineRegistrationKey);
        }

        /// test for NULL value
        public bool IsOrderInGiftBatchNull()
        {
            return this.IsNull(this.myTable.ColumnOrderInGiftBatch);
        }

        /// assign NULL value
        public void SetOrderInGiftBatchNull()
        {
            this.SetNull(this.myTable.ColumnOrderInGiftBatch);
        }

        /// test for NULL value
        public bool IsAmountNull()
        {
            return this.IsNull(this.myTable.ColumnAmount);
        }

        /// assign NULL value
        public void SetAmountNull()
        {
            this.SetNull(this.myTable.ColumnAmount);
        }
    }
}