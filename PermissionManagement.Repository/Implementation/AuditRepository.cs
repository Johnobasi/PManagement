using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using PermissionManagement.Model;
using PermissionManagement.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Dapper;

namespace PermissionManagement.Repository
{
    public class AuditRepository : IAuditRepository
    {
        private readonly IDbConnection context;
        private readonly DapperContext dapperContext;

        public AuditRepository(DapperContext dbContext)
        {
            dapperContext = dbContext;
            context = dbContext.Connection;
        }

        public void AuditLog(AuditTrail log)
        {
            if (dapperContext.IsTransactionInProgress())
            {
                dapperContext.RollbackTransaction();
                log.AuditMessage = "Operation Failed";
            }
            context.Insert<AuditTrail>(log, excludeFieldList: new string[] { "AuditID" });
        }

        public void CreateAuditChange(object ValueBefore, object ValueAfter, IDbTransaction dbTransaction, string affectedRecordID, string[] propertiesToCompare = null)
        {
            CompareLogic compObjects = new CompareLogic();
            compObjects.Config.MaxDifferences = 99;

            //ComparisonResult compResult = compObjects.Compare(ValueBefore, ValueAfter);

            List<AuditChange> changes = new List<AuditChange>();
            List<string> NewValue = new List<string>();
            StringBuilder sb = new StringBuilder();
            IList<Difference> diffs = diffs = compObjects.Compare(ValueBefore, ValueAfter).Differences;
            IList<AuditChangeDiff> diffForSave = new List<AuditChangeDiff>();
            if (propertiesToCompare != null && propertiesToCompare.Length > 0)
            {
                foreach (Difference diff in diffs)
                {
                    //if (propertiesToCompare.Contains(diff.PropertyName.Substring(1)))
                    if (propertiesToCompare.Contains(diff.PropertyName))
                    {
                        //FinetuneDiff(diff);
                        var d = new AuditChangeDiff()
                        {
                            PropertyName = diff.PropertyName,
                            OldValue = diff.Object1Value,
                            NewValue = diff.Object2Value
                        };
                        diffForSave.Add(d);
                    }
                }
            }
            else
            {
                foreach (Difference diff in diffs)
                {
                    //FinetuneDiff(diff);
                    var d = new AuditChangeDiff()
                    {
                        PropertyName = diff.PropertyName,
                        OldValue = diff.Object1Value,
                        NewValue = diff.Object2Value
                    };
                    diffForSave.Add(d);
                }
            }

            AuditChange audit = new AuditChange();
            audit.Username = Helper.GetLoggedInUserID();
            audit.AuditType = "ChangeLog";
            audit.ActionDateTime = DateTime.Now;
            audit.TableName = ValueBefore.GetType().Name;
            audit.ClientIPAddress = Helper.GetIPAddress();
            audit.AffectedRecordID = affectedRecordID;  // GetAffectedRecordID(ValueBefore);
            audit.ValueBefore = Crypto.Encrypt(JsonConvert.SerializeObject(ValueBefore));
            if (ValueAfter != null)
            {
                audit.ValueAfter = Crypto.Encrypt(JsonConvert.SerializeObject(ValueAfter));
                audit.Changes = Crypto.Encrypt(JsonConvert.SerializeObject(diffForSave));
            }

            if (dbTransaction != null)
                context.Insert<AuditChange>(audit, excludeFieldList: new string[] { "AuditChangeID" }, transaction: dbTransaction);
            else
                context.Insert<AuditChange>(audit, excludeFieldList: new string[] { "AuditChangeID" });
        }


        public AuditTrailListingResponse GetAuditList(PagerItemsII auditparameter)
        {
            var result = new AuditTrailListingResponse() { PagerResource = new PagerItems() };

            var orderByField = string.Empty;
            var sql = new StringBuilder();
            sql.AppendLine("SELECT * FROM (");
            sql.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY ");

            var sortSql = new StringBuilder();
            foreach (var column in auditparameter.SortColumns)
            {
                sortSql.Append(sortSql.Length > 0 ? "," : "");

                if (column.Data == Constants.SortField.AuditID)
                {
                    sql.Append("AuditID ");
                    sortSql.Append("AuditID");
                }
                else if (column.Data == Constants.SortField.ActionStartTime)
                {
                    sql.Append("ActionStartTime ");
                    sortSql.Append("ActionStartTime");
                }
                else if (column.Data == Constants.SortField.ActionEndTime)
                {
                    sql.Append("ActionEndTime ");
                    sortSql.Append("ActionEndTime");
                }
                else if (column.Data == Constants.SortField.Username)
                {
                    sql.Append("Username ");
                    sortSql.Append("Username");
                }
                else if (column.Data == Constants.SortField.AuditAction)
                {
                    sql.Append("AuditAction ");
                    sortSql.Append("AuditAction");
                }
                else if (column.Data == Constants.SortField.AuditPage)
                {
                    sql.Append("AuditPage ");
                    sortSql.Append("AuditPage");
                }
                else if (column.Data == Constants.SortField.AuditHTTPAction)
                {
                    sql.Append("AuditHTTPAction ");
                    sortSql.Append("AuditHTTPAction");
                }
                sql.Append(column.SortDirection == 0 ? " asc" : " desc");
                sortSql.Append(column.SortDirection == 0 ? " asc" : " desc");
            }

            var usernameFilter = string.Empty;
            var auditActionFilter = string.Empty;
            var auditPageFilter = string.Empty;
            var auditMessageFilter = string.Empty;

            DateTime actionStartFilterFrom = DateTime.Now;
            DateTime actionStartFilterTo = DateTime.Now;
            DateTime actionEndFilterFrom = DateTime.Now;
            DateTime actionEndFilterTo = DateTime.Now;

            var whereClause = new StringBuilder();
            whereClause.Append(" WHERE ");
            //dateCulture = new FormatProvider
            foreach (var column in auditparameter.SearchColumns)
            {
                if (column.Data == Constants.SortField.ActionStartTime && column.Search.Value != Constants.General.YadcfDelimiter)
                {
                    var dateFilter = column.Search.Value.Split(Constants.General.YadcfDelimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var start = column.Search.Value.StartsWith(Constants.General.YadcfDelimiter) ? string.Empty : dateFilter[0];
                    var end = column.Search.Value.EndsWith(Constants.General.YadcfDelimiter) ? string.Empty : dateFilter.Length > 1 ? dateFilter[1] : dateFilter[0];
                    if (!string.IsNullOrEmpty(start))
                    {
                        actionStartFilterFrom = DateTime.Parse(start, Thread.CurrentThread.CurrentCulture.DateTimeFormat);
                        whereClause.AppendFormat("(ActionStartTime >= @ActionStartFilterFrom) AND ");
                    }
                    if (!string.IsNullOrEmpty(end))
                    {
                        actionStartFilterTo = DateTime.Parse(end, Thread.CurrentThread.CurrentCulture.DateTimeFormat);
                        actionStartFilterTo = new DateTime(actionEndFilterTo.Year, actionEndFilterTo.Month, actionEndFilterTo.Day, 23, 59, 59);
                        whereClause.AppendFormat("(ActionStartTime <= @ActionStartFilterTo) AND ");
                    }
                }
                else if (column.Data == Constants.SortField.ActionEndTime && column.Search.Value != Constants.General.YadcfDelimiter)
                {
                    var dateFilter = column.Search.Value.Split(Constants.General.YadcfDelimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var start = column.Search.Value.StartsWith(Constants.General.YadcfDelimiter) ? string.Empty : dateFilter[0];
                    var end = column.Search.Value.EndsWith(Constants.General.YadcfDelimiter) ? string.Empty : dateFilter.Length > 1 ? dateFilter[1] : dateFilter[0];
                    if (!string.IsNullOrEmpty(start))
                    {
                        actionEndFilterFrom = DateTime.Parse(start, Thread.CurrentThread.CurrentCulture.DateTimeFormat);
                        whereClause.AppendFormat("(ActionEndTime >= @ActionEndFilterFrom) AND ");
                    }
                    if (!string.IsNullOrEmpty(end))
                    {
                        actionEndFilterTo = DateTime.Parse(end, Thread.CurrentThread.CurrentCulture.DateTimeFormat);
                        actionEndFilterTo = new DateTime(actionEndFilterTo.Year, actionEndFilterTo.Month, actionEndFilterTo.Day, 23, 59, 59);
                        whereClause.AppendFormat("(ActionEndTime <= @ActionEndFilterTo) AND ");
                    }
                }
                else if (column.Data == Constants.SortField.Username && !string.IsNullOrEmpty(column.Search.Value))
                {
                    usernameFilter = column.Search.Value.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                    usernameFilter = string.Format("%{0}%", usernameFilter);
                    whereClause.Append(" (Username LIKE @UsernameFilter) AND ");
                }
                else if (column.Data == Constants.SortField.AuditAction && !string.IsNullOrEmpty(column.Search.Value))
                {
                    auditActionFilter = column.Search.Value.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                    auditActionFilter = string.Format("%{0}%", auditActionFilter);
                    whereClause.Append(" (AuditAction LIKE @AuditActionFilter) AND ");
                }
                else if (column.Data == Constants.SortField.AuditPage && !string.IsNullOrEmpty(column.Search.Value))
                {
                    auditPageFilter = column.Search.Value.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                    auditPageFilter = string.Format("%{0}%", auditPageFilter);
                    whereClause.Append(" (AuditPage LIKE @AuditPageFilter) AND ");
                }
                else if (column.Data == Constants.SortField.AuditMessage && !string.IsNullOrEmpty(column.Search.Value))
                {
                    auditMessageFilter = column.Search.Value.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                    auditMessageFilter = string.Format("%{0}%", auditMessageFilter);
                    whereClause.Append(" (AuditMessage LIKE @AuditMessageFilter) AND ");
                }
            }

            if (whereClause.Length > 7)
            {
                whereClause.Remove(whereClause.Length - 4, 4);
            }
            var globalFilter = string.Empty;
            if (!string.IsNullOrEmpty(auditparameter.siteSearch))
            {
                globalFilter = auditparameter.siteSearch.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                globalFilter = string.Format("%{0}%", globalFilter);
                whereClause.Append(" OR ((Username LIKE @GlobalSearchFilter) OR (AuditAction LIKE @GlobalSearchFilter) OR (AuditPage LIKE @GlobalSearchFilter)) ");
            }

            sql.AppendLine(") AS NUMBER, AuditID, AuditAction, AuditPage, ActionStartTime, Username, ActionEndTime, ISNULL(AuditMessage,'Successful') AS AuditMessage, ClientIPAddress, AuditHTTPAction ");
            sql.AppendLine("From [AuditTrail]");
            sql.AppendFormat("{0}) AS TBL ", whereClause.Length > 7 ? whereClause.ToString() : string.Empty);
            sql.AppendLine("WHERE NUMBER BETWEEN @StartPage AND @EndPage ");
            sql.AppendFormat("ORDER BY {0} ", sortSql.ToString());

            result.PagerResource.ResultCount = (int)context.Query<Int64>(
                string.Format("Select Count(*) From [AuditTrail] {0}",
                whereClause.Length > 7 ? whereClause.ToString() : string.Empty),
                new
                {
                    StartPage = ((auditparameter.PageNumber - 1) * auditparameter.PageSize) + 1,
                    EndPage = (auditparameter.PageNumber * auditparameter.PageSize),
                    GlobalSearchFilter = globalFilter,
                    UsernameFilter = usernameFilter,
                    AuditActionFilter = auditActionFilter,
                    AuditPageFilter = auditPageFilter,
                    ActionStartFilterFrom = actionStartFilterFrom,
                    ActionStartFilterTo = actionStartFilterTo,
                    ActionEndFilterFrom = actionEndFilterFrom,
                    ActionEndFilterTo = actionEndFilterTo,
                    AuditMessageFilter = auditMessageFilter
                }
                ).First();

            result.AuditTrailListingResult = context.Query<AuditTrailListingDto>(sql.ToString(),
                new
                {
                    StartPage = ((auditparameter.PageNumber - 1) * auditparameter.PageSize) + 1,
                    EndPage = (auditparameter.PageNumber * auditparameter.PageSize),
                    GlobalSearchFilter = globalFilter,
                    UsernameFilter = usernameFilter,
                    AuditActionFilter = auditActionFilter,
                    AuditPageFilter = auditPageFilter,
                    ActionStartFilterFrom = actionStartFilterFrom,
                    ActionStartFilterTo = actionStartFilterTo,
                    ActionEndFilterFrom = actionEndFilterFrom,
                    ActionEndFilterTo = actionEndFilterTo,
                    AuditMessageFilter = auditMessageFilter
                }).ToList();

            return result;
        }

        public AuditChangeListingResponse GetAuditChange(PagerItemsII auditChangeparameter)
        {
            var result = new AuditChangeListingResponse() { PagerResource = new PagerItems() };
            var orderByField = string.Empty;
            var sql = new StringBuilder();
            sql.AppendLine("SELECT * FROM (");
            sql.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY ");

            var sortSql = new StringBuilder();
            foreach (var column in auditChangeparameter.SortColumns)
            {
                sortSql.Append(sortSql.Length > 0 ? "," : "");

                if (column.Data == Constants.SortField.AuditChangeID)
                {
                    sql.Append("AuditChangeID ");
                    orderByField = "AuditChangeID";
                }
                else if (column.Data == Constants.SortField.ActionDateTime)
                {
                    sql.Append("ActionDateTime ");
                    sortSql.Append("ActionDateTime");
                }
                else if (column.Data == Constants.SortField.TableName)
                {
                    sql.Append("TableName ");
                    sortSql.Append("TableName");
                }
                else if (column.Data == Constants.SortField.AffectedRecordID)
                {
                    sql.Append("AffectedRecordID ");
                    sortSql.Append("AffectedRecordID");
                }
                else if (column.Data == Constants.SortField.Username)
                {
                    sql.Append("Username ");
                    sortSql.Append("Username");
                }
                sql.Append(column.SortDirection == 0 ? " asc" : " desc");
                sortSql.Append(column.SortDirection == 0 ? " asc" : " desc");
            }
            var usernameFilter = string.Empty;
            var tableNameFilter = string.Empty;
            var affectedRecordIDFilter = string.Empty;

            DateTime actionDateFilterFrom = DateTime.Now;
            DateTime actionDateFilterTo = DateTime.Now;

            var whereClause = new StringBuilder();
            whereClause.Append(" WHERE ");
            foreach (var column in auditChangeparameter.SearchColumns)
            {
                if (column.Data == Constants.SortField.ActionDateTime && column.Search.Value != Constants.General.YadcfDelimiter)
                {
                    var dateFilter = column.Search.Value.Split(Constants.General.YadcfDelimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var start = column.Search.Value.StartsWith(Constants.General.YadcfDelimiter) ? string.Empty : dateFilter[0];
                    var end = column.Search.Value.EndsWith(Constants.General.YadcfDelimiter) ? string.Empty : dateFilter.Length > 1 ? dateFilter[1] : dateFilter[0];
                    if (!string.IsNullOrEmpty(start))
                    {
                        actionDateFilterFrom = DateTime.Parse(start, Thread.CurrentThread.CurrentCulture.DateTimeFormat);
                        whereClause.AppendFormat("(ActionDateTime >= @ActionDateFilterFrom) AND ");
                    }
                    if (!string.IsNullOrEmpty(end))
                    {
                        actionDateFilterTo = DateTime.Parse(end, Thread.CurrentThread.CurrentCulture.DateTimeFormat);
                        actionDateFilterTo = new DateTime(actionDateFilterTo.Year, actionDateFilterTo.Month, actionDateFilterTo.Day, 23, 59, 59);
                        whereClause.AppendFormat("(ActionDateTime <= @ActionDateFilterTo) AND ");
                    }
                }
                else if (column.Data == Constants.SortField.Username && !string.IsNullOrEmpty(column.Search.Value))
                {
                    usernameFilter = column.Search.Value.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                    usernameFilter = string.Format("%{0}%", usernameFilter);
                    whereClause.Append(" (Username LIKE @UsernameFilter) AND ");
                }
                else if (column.Data == Constants.SortField.TableName && !string.IsNullOrEmpty(column.Search.Value))
                {
                    tableNameFilter = column.Search.Value.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                    tableNameFilter = string.Format("%{0}%", tableNameFilter);
                    whereClause.Append(" (TableName LIKE @TableNameFilter) AND ");
                }
                else if (column.Data == Constants.SortField.AffectedRecordID && !string.IsNullOrEmpty(column.Search.Value))
                {
                    affectedRecordIDFilter = column.Search.Value.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                    affectedRecordIDFilter = string.Format("%{0}%", affectedRecordIDFilter);
                    whereClause.Append(" (AffectedRecordID LIKE @AffectedRecordID) AND ");
                }
            }

            if (whereClause.Length > 7)
            {
                whereClause.Remove(whereClause.Length - 4, 4);
            }
            var globalFilter = string.Empty;
            if (!string.IsNullOrEmpty(auditChangeparameter.siteSearch))
            {
                globalFilter = auditChangeparameter.siteSearch.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
                globalFilter = string.Format("%{0}%", globalFilter);
                whereClause.Append(" OR ((Username LIKE @GlobalSearchFilter) OR (TableName LIKE @GlobalSearchFilter)) ");
            }

            sql.AppendLine(") AS NUMBER, AuditChangeID, TableName, AffectedRecordID, Username, ActionDateTime, ClientIPAddress "); //Changes
            sql.AppendLine("From [AuditChange]");
            sql.AppendFormat("{0}) AS TBL ", whereClause.Length > 7 ? whereClause.ToString() : string.Empty);
            sql.AppendLine("WHERE NUMBER BETWEEN @StartPage AND @EndPage ");
            sql.AppendFormat("ORDER BY {0} ", sortSql.ToString());

            result.PagerResource.ResultCount = (int)context.Query<Int64>(
               string.Format("Select Count(*) From [AuditChange] {0}",
               whereClause.Length > 7 ? whereClause.ToString() : string.Empty),
               new
               {
                   StartPage = ((auditChangeparameter.PageNumber - 1) * auditChangeparameter.PageSize) + 1,
                   EndPage = (auditChangeparameter.PageNumber * auditChangeparameter.PageSize),
                   GlobalSearchFilter = globalFilter,
                   UsernameFilter = usernameFilter,
                   TableNameFilter = tableNameFilter,
                   AffectedRecordID = affectedRecordIDFilter,
                   ActionDateFilterFrom = actionDateFilterFrom,
                   ActionDateFilterTo = actionDateFilterTo
               }
               ).First();

            result.AuditChangeListingResult = context.Query<AuditChangeListingDto>(sql.ToString(),
                new
                {
                    StartPage = ((auditChangeparameter.PageNumber - 1) * auditChangeparameter.PageSize) + 1,
                    EndPage = (auditChangeparameter.PageNumber * auditChangeparameter.PageSize),
                    GlobalSearchFilter = globalFilter,
                    UsernameFilter = usernameFilter,
                    TableNameFilter = tableNameFilter,
                    AffectedRecordID = affectedRecordIDFilter,
                    ActionDateFilterFrom = actionDateFilterFrom,
                    ActionDateFilterTo = actionDateFilterTo
                }).ToList();

            return result;
        }

        public string GetAuditChangeRecord(long id)
        {
            var result = new List<string>();
            var sql = new StringBuilder();
            sql.AppendLine("SELECT Changes ");

            sql.AppendLine("From [AuditChange] ");

            sql.AppendLine("WHERE AuditChangeID = @AuditChangeID");

            result = context.Query<string>(sql.ToString(), new { AuditChangeID = id }).ToList();
            return Crypto.Decrypt(result.FirstOrDefault());
        }


        //public AuditTrailListingResponse GetAuditList(int pageNumber, int pageSize, string sortField, string sortOrder, string searchText, string[] searchFields)
        //{
        //    var result = new AuditTrailListingResponse() { PagerResource = new PagerItems() };

        //    result.PagerResource.ResultCount = (int)context.Query<Int64>("Select Count(*) From [AuditTrail]").First();
        //    result.PagerResource.CurrentPage = pageNumber;
        //    result.PagerResource.PageNumber = pageNumber;
        //    result.PagerResource.PageSize = pageSize;
        //    result.PagerResource.SortBy = sortField;
        //    result.PagerResource.SortDirection = sortOrder;

        //    var orderByField = string.Empty;
        //    var sql = new StringBuilder();
        //    sql.AppendLine("SELECT * FROM (");
        //    sql.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY ");
        //    if (sortField == Constants.SortField.AuditID)
        //    {
        //        sql.Append("AuditID ");
        //        orderByField = "AuditID";
        //    }
        //    else if (sortField == Constants.SortField.ActionStartTime)
        //    {
        //        sql.Append("ActionStartTime ");
        //        orderByField = "ActionStartTime";
        //    }
        //    else if (sortField == Constants.SortField.ActionEndTime)
        //    {
        //        sql.Append("ActionEndTime ");
        //        orderByField = "ActionEndTime";
        //    }
        //    sql.AppendLine(") AS NUMBER, AuditID, AuditAction, AuditPage, ActionStartTime, Username, ActionEndTime, ClientIPAddress ");
        //    sql.AppendLine("From [AuditTrail]");
        //    sql.AppendLine(") AS TBL ");
        //    sql.AppendLine("WHERE NUMBER BETWEEN @StartPage AND @EndPage ");
        //    sql.AppendFormat("ORDER BY {0} ", orderByField);

        //    sql.AppendLine(sortOrder == Constants.SortOrder.Descending ? "ASC " : "DESC ");

        //    result.AuditTrailListingResult = context.Query<AuditTrailListingDto>(sql.ToString(), new { StartPage = ((pageNumber - 1) * pageSize) + 1, EndPage = (pageNumber * pageSize) }).ToList();

        //    return result;
        //}


        //public AuditChangeListingResponse GetAuditChange(int pageNumber, int pageSize, string sortField, string sortOrder, string searchText, string[] searchFields)
        //{
        //    var result = new AuditChangeListingResponse() { PagerResource = new PagerItems() };

        //    result.PagerResource.ResultCount = (int)context.Query<Int64>("Select Count(*) From [AuditChange]").First();
        //    result.PagerResource.CurrentPage = pageNumber;
        //    result.PagerResource.PageNumber = pageNumber;
        //    result.PagerResource.PageSize = pageSize;
        //    result.PagerResource.SortBy = sortField;
        //    result.PagerResource.SortDirection = sortOrder;

        //    var orderByField = string.Empty;
        //    var sql = new StringBuilder();
        //    sql.AppendLine("SELECT * FROM (");
        //    if (sortField == Constants.SortField.Username)
        //    {
        //        sql.Append("AuditChangeID ");
        //        orderByField = "AuditChangeID";
        //    }

        //    sql.AppendLine(") AS NUMBER, AuditChangeID, TableName, AuditType, Username, ActionDateTime, ClientIPAddress, ValueBefore, ValueAfter, Changes");
        //    sql.AppendLine("From [AuditChange]");
        //    sql.AppendLine(") AS TBL ");
        //    sql.AppendLine("WHERE NUMBER BETWEEN @StartPage AND @EndPage ");
        //    sql.AppendFormat("ORDER BY {0} ", orderByField);

        //    sql.AppendLine(sortOrder == Constants.SortOrder.Ascending ? "ASC " : "DESC ");

        //    result.AuditChangeListingResult = context.Query<AuditChangeListingDto>(sql.ToString(), new { StartPage = ((pageNumber - 1) * pageSize) + 1, EndPage = (pageNumber * pageSize) }).ToList();

        //    return result;
        //}

        public T MakerCheckerHandller<T>(T dbVersion, T incomingVersion, string operationType, string moduleName, string modelID, IDbTransaction dbTransaction)
        {
            long logID = 0;
            Type type = typeof(T);
            var list = type.GetProperties().Where(g => (Helper.IsItemExistInList(new string[] { "RowVersionNo2", "ApprovalStatus", "ApprovedBy", "InitiatedBy", "ApprovalLogID", "IsDeleted" }, g.Name))).ToList();
            if (list.Count < 6)
            {
                return incomingVersion;  //default(T);
            }

            var approvalNotice = new ApprovalNotification();

            var dbVersionApprovalStatus = (from m in list where m.Name == "ApprovalStatus" select m).FirstOrDefault().GetValue(dbVersion);
            var dbVersionApprovedBy = (from m in list where m.Name == "ApprovedBy" select m).FirstOrDefault().GetValue(dbVersion);
            var dbVersionInitiatedBy = (from m in list where m.Name == "InitiatedBy" select m).FirstOrDefault().GetValue(dbVersion);
            var dbVersionApprovalLogID = (from m in list where m.Name == "ApprovalLogID" select m).FirstOrDefault().GetValue(dbVersion);
            var dbVersionIsDeleted = (from m in list where m.Name == "IsDeleted" select m).FirstOrDefault().GetValue(dbVersion);

            var incomingVersionApprovalStatus = (from m in list where m.Name == "ApprovalStatus" select m).FirstOrDefault().GetValue(incomingVersion);
            var incomingVersionApprovedBy = (from m in list where m.Name == "ApprovedBy" select m).FirstOrDefault().GetValue(incomingVersion);
            var incomingVersionInitiatedBy = (from m in list where m.Name == "InitiatedBy" select m).FirstOrDefault().GetValue(incomingVersion);
            var incomingRowVersionNo = (from m in list where m.Name == "RowVersionNo2" select m).FirstOrDefault().GetValue(incomingVersion);
            var incomingIsDeleted = (from m in list where m.Name == "IsDeleted" select m).FirstOrDefault().GetValue(incomingVersion);


            if (string.IsNullOrEmpty((string)incomingVersionApprovalStatus))
            {
                //the page is being edited by a user who cannot see the approval drop down.
                (from m in list where m.Name == "ApprovalStatus" select m).FirstOrDefault().SetValue(incomingVersion, dbVersionApprovalStatus);
            }

            IList<UserMailDto> possibleApproverList = new List<UserMailDto>();
            //if ((string)incomingVersionApprovalStatus == Constants.ApprovalStatus.Pending)                

            if (operationType == Constants.OperationType.Create)
            {
                incomingVersionInitiatedBy = Helper.GetLoggedInUserID();
                (from m in list where m.Name == "InitiatedBy" select m).FirstOrDefault().SetValue(incomingVersion, incomingVersionInitiatedBy);

                possibleApproverList = GetPossibleApproverList((string)incomingVersionInitiatedBy, moduleName);
                //this is the simple situation
                //get possible approving user 
                if (possibleApproverList.Count > 0)
                {
                    var approvalLog = new ApprovalLog()
                    {
                        ActivityName = string.Format("{0} {1}", Constants.OperationType.Create, typeof(T).Name),
                        ApprovalStatus = Constants.ApprovalStatus.Pending,
                        InitiatorID = (string)incomingVersionInitiatedBy,
                        PossibleVerifierID = Helper.ToStringCSV<string>(possibleApproverList.Select(f => f.Username).ToArray()),
                        ActivityUrl = string.Format("{0}/{1}", Helper.GetCurrentURL().Replace(Constants.OperationType.Create,
                        Constants.OperationType.Edit), modelID),
                        RecordData = Crypto.Encrypt(JsonConvert.SerializeObject(incomingVersion)),
                        ApprovalDate = null
                    };
                    logID = context.Insert<ApprovalLog>(approvalLog, transaction: dbTransaction);

                    if (logID > 0)
                    {
                        (from m in list where m.Name == "ApprovalStatus" select m).FirstOrDefault().SetValue(incomingVersion, Constants.ApprovalStatus.Pending);
                        (from m in list where m.Name == "ApprovalLogID" select m).FirstOrDefault().SetValue(incomingVersion, logID);
                    }

                    //send approval request notification to possible approvers
                    approvalNotice.NoticeType = Constants.ApprovalNoticeType.ApprovalRequest;
                    approvalNotice.ActionUrl = approvalLog.ActivityUrl;
                    approvalNotice.NotificationList = possibleApproverList;
                    approvalNotice.InitiatedBy = GetUserDTO(approvalLog.InitiatorID).FirstOrDefault();
                    approvalNotice.Comment = string.Empty;
                }
                else
                {
                    return incomingVersion;
                }

            }
            if (operationType == Constants.OperationType.Edit || operationType == Constants.OperationType.Delete)
            {
                //case 1: when dbversionapprovalstatus = approved - it should go back to pending
                // create a new entry in approval log.
                // take the snapshot of dbversion and save in approvallog,
                // change initiated by to the id of the staff currently logged in
                // change approval status to pending
                // update the approvalogid in the main record table
                // get list of possible approval and flow right information to them
                if ((string)dbVersionApprovalStatus == Constants.ApprovalStatus.Approved)
                {
                    var initiatedBy = Helper.GetLoggedInUserID();
                    possibleApproverList = GetPossibleApproverList((string)initiatedBy, moduleName);
                    (from m in list where m.Name == "InitiatedBy" select m).FirstOrDefault().SetValue(incomingVersion, initiatedBy);
                    var approvalLog = new ApprovalLog()
                    {
                        ActivityName = string.Format("{0} {1}", operationType, typeof(T).Name),
                        ApprovalStatus = Constants.ApprovalStatus.Pending,
                        InitiatorID = initiatedBy,
                        PossibleVerifierID = Helper.ToStringCSV<string>(possibleApproverList.Select(f => f.Username).ToArray()),
                        ActivityUrl = Helper.GetCurrentURL(),
                        RecordData = Crypto.Encrypt(JsonConvert.SerializeObject(dbVersion)),
                        ApprovalDate = null
                    };

                    //also ensure all rejected for correction for this record is set to Rejected and status completed.
                    var storeApprovalLog = context.GetById<ApprovalLog>((long)dbVersionApprovalLogID, transaction: dbTransaction);
                    if (storeApprovalLog != null && storeApprovalLog.ApprovalStatus == Constants.ApprovalStatus.RejectedForCorrection)
                    {
                        storeApprovalLog.ApprovalStatus = Constants.ApprovalStatus.Rejected;
                        //storeApprovalLog.LastComment = "Completed";
                        context.Update<ApprovalLog>(storeApprovalLog, primaryKeyList: new string[] { "ApprovalLogID" }, transaction: dbTransaction);
                    }

                    logID = context.Insert<ApprovalLog>(approvalLog, transaction: dbTransaction);
                    if (logID > 0)
                    {
                        (from m in list where m.Name == "ApprovalStatus" select m).FirstOrDefault().SetValue(incomingVersion, Constants.ApprovalStatus.Pending);
                        (from m in list where m.Name == "ApprovalLogID" select m).FirstOrDefault().SetValue(incomingVersion, logID);
                        if (operationType == Constants.OperationType.Delete)
                        {
                            (from m in list where m.Name == "IsDeleted" select m).FirstOrDefault().SetValue(incomingVersion, true);
                        }
                    }

                    //send approval request notification to possible approvers
                    approvalNotice.NoticeType = Constants.ApprovalNoticeType.ApprovalRequest;
                    approvalNotice.ActionUrl = approvalLog.ActivityUrl;
                    approvalNotice.NotificationList = possibleApproverList;
                    approvalNotice.InitiatedBy = GetUserDTO(approvalLog.InitiatorID).FirstOrDefault();
                    approvalNotice.Comment = string.Empty;
                }

                //case 2: when dbversionapprovalstatus = pending and incomingversionapprovalstatus = approved
                // change record status to approved.
                // mark the record in approvalog to complete.
                // get the initiated by from the main record, and notify.
                if ((string)dbVersionApprovalStatus == Constants.ApprovalStatus.Pending && (string)incomingVersionApprovalStatus == Constants.ApprovalStatus.Approved)
                {
                    var approvedBy = Helper.GetLoggedInUserID();
                    var approvalLog = new ApprovalLog()
                    {
                        VerifierID = approvedBy,
                        LastComment = "Completed",
                        ApprovalLogID = (long)dbVersionApprovalLogID,
                        ApprovalDate = Helper.GetLocalDate(),
                        ApprovalStatus = Constants.ApprovalStatus.Approved,
                        ActivityUrl = Helper.GetCurrentURL()
                    };

                    (from m in list where m.Name == "InitiatedBy" select m).FirstOrDefault().SetValue(incomingVersion, dbVersionInitiatedBy);
                    (from m in list where m.Name == "ApprovedBy" select m).FirstOrDefault().SetValue(incomingVersion, approvedBy);
                    (from m in list where m.Name == "ApprovalStatus" select m).FirstOrDefault().SetValue(incomingVersion, Constants.ApprovalStatus.Approved);
                    (from m in list where m.Name == "ApprovalLogID" select m).FirstOrDefault().SetValue(incomingVersion, dbVersionApprovalLogID);

                    context.Execute("UPDATE ApprovalLog SET VerifierID = @VerifierID, ApprovalStatus = @ApprovalStatus, ApprovalDate = @ApprovalDate, LastComment = @LastComment WHERE ApprovalLogID = @ApprovalLogID;",
                        approvalLog, transaction: dbTransaction);

                    //send approval executed notification to the initiator
                    approvalNotice.NoticeType = Constants.ApprovalNoticeType.ApprovalExecuted;
                    approvalNotice.ActionUrl = approvalLog.ActivityUrl;
                    approvalNotice.NotificationList = GetUserDTO((string)dbVersionInitiatedBy);
                    approvalNotice.ApprovedBy = GetUserDTO(approvedBy).FirstOrDefault();
                    approvalNotice.Comment = string.Empty;
                }

                //case 3: when dbversionapprovalstatus = pending and incomingversionapprovalstatus = rejected
                // determine if the record is a new record, if it is, delete the record, and notify initiatedby
                // if the record has been approved before, get the last approved snapshot from approval log, and repopulate, and notify the initiatedby
                if ((string)dbVersionApprovalStatus == Constants.ApprovalStatus.Pending && (string)incomingVersionApprovalStatus == Constants.ApprovalStatus.Rejected)
                {
                    var approvedBy = Helper.GetLoggedInUserID();

                    var dbApprovalLog = context.GetById<ApprovalLog>((long)dbVersionApprovalLogID, transaction: dbTransaction);
                    if (string.IsNullOrEmpty((string)dbVersionApprovedBy)) //signify has never been approved before.
                    {
                        (from m in list where m.Name == "ApprovalStatus" select m).FirstOrDefault().SetValue(incomingVersion, Constants.ApprovalStatus.Rejected);
                        (from m in list where m.Name == "IsDeleted" select m).FirstOrDefault().SetValue(incomingVersion, true);
                        (from m in list where m.Name == "ApprovalLogID" select m).FirstOrDefault().SetValue(incomingVersion, dbVersionApprovalLogID);

                        dbApprovalLog.VerifierID = approvedBy;
                        var approvalComment = Helper.GetLastApprovalComment();
                        dbApprovalLog.LastComment = string.IsNullOrEmpty(approvalComment) ? "Completed" : approvalComment;
                        dbApprovalLog.ApprovalDate = Helper.GetLocalDate();
                        dbApprovalLog.ApprovalStatus = Constants.ApprovalStatus.Rejected;
                        context.Update<ApprovalLog>(dbApprovalLog, primaryKeyList: new string[] { "ApprovalLogID" }, transaction: dbTransaction);
                    }
                    else
                    {
                        var rehydratedVersion = JsonConvert.DeserializeObject<T>(Crypto.Decrypt(dbApprovalLog.RecordData));
                        incomingVersion = rehydratedVersion;

                        dbApprovalLog.VerifierID = approvedBy;
                        dbApprovalLog.ApprovalDate = Helper.GetLocalDate();
                        dbApprovalLog.ApprovalStatus = Constants.ApprovalStatus.Rejected;
                        context.Update<ApprovalLog>(dbApprovalLog, primaryKeyList: new string[] { "ApprovalLogID" }, transaction: dbTransaction);
                    }
                    //send approval request rejected notification to the initiator
                    approvalNotice.NoticeType = Constants.ApprovalNoticeType.ApprovalRejected;
                    approvalNotice.ActionUrl = Helper.GetCurrentURL();
                    approvalNotice.NotificationList = GetUserDTO((string)dbVersionInitiatedBy);
                    approvalNotice.ApprovedBy = GetUserDTO(approvedBy).FirstOrDefault();
                    var approvalComment2 = Helper.GetLastApprovalComment();
                    approvalNotice.Comment = string.IsNullOrEmpty(approvalComment2) ? "Completed" : approvalComment2;
                }

                //case 4: when dbversionapprovalstatus = pending and incomingversionapprovalstatus = rejectedforcorrection
                // if this is not a new record, return back to the old snapshot, and status, and notify the user to retry. Mark approvallog as completed
                // if this is a new record, change approvalstatus to RejectedForCorrection - to be made editable only by the initiated by, notify initiated by to correct.
                if ((string)dbVersionApprovalStatus == Constants.ApprovalStatus.Pending && (string)incomingVersionApprovalStatus == Constants.ApprovalStatus.RejectedForCorrection)
                {
                    var approvedBy = Helper.GetLoggedInUserID();
                    var approvalComment = Helper.GetLastApprovalComment();
                    //if (string.IsNullOrEmpty(approvalComment))
                    //{
                    //    approvalComment = "No Comment - Awaiting Correction";
                    //}
                    var dbApprovalLog = context.GetById<ApprovalLog>((long)dbVersionApprovalLogID, transaction: dbTransaction);
                    if (string.IsNullOrEmpty((string)dbVersionApprovedBy)) //signify has never been approved before.
                    {
                        (from m in list where m.Name == "ApprovalStatus" select m).FirstOrDefault().SetValue(incomingVersion, Constants.ApprovalStatus.RejectedForCorrection);
                        (from m in list where m.Name == "ApprovalLogID" select m).FirstOrDefault().SetValue(incomingVersion, dbApprovalLog.ApprovalLogID);
                        (from m in list where m.Name == "InitiatedBy" select m).FirstOrDefault().SetValue(incomingVersion, dbVersionInitiatedBy);
                        (from m in list where m.Name == "ApprovedBy" select m).FirstOrDefault().SetValue(incomingVersion, approvedBy);

                        dbApprovalLog.VerifierID = approvedBy;
                        dbApprovalLog.ApprovalDate = Helper.GetLocalDate();
                        dbApprovalLog.ApprovalStatus = Constants.ApprovalStatus.RejectedForCorrection;
                        dbApprovalLog.LastComment = string.IsNullOrEmpty(approvalComment) ? "Completed" : approvalComment;
                        context.Update<ApprovalLog>(dbApprovalLog, primaryKeyList: new string[] { "ApprovalLogID" }, transaction: dbTransaction);
                    }
                    else
                    {
                        var rehydratedVersion = JsonConvert.DeserializeObject<T>(Crypto.Decrypt(dbApprovalLog.RecordData));
                        incomingVersion = rehydratedVersion;

                        //for rejectedforcorrection we change the approval log in the rehydrated to the recent one.
                        (from m in list where m.Name == "ApprovalLogID" select m).FirstOrDefault().SetValue(incomingVersion, dbApprovalLog.ApprovalLogID);

                        dbApprovalLog.VerifierID = approvedBy;
                        dbApprovalLog.ApprovalDate = Helper.GetLocalDate();
                        dbApprovalLog.ApprovalStatus = Constants.ApprovalStatus.RejectedForCorrection;
                        dbApprovalLog.LastComment = string.IsNullOrEmpty(approvalComment) ? "Completed" : approvalComment;
                        context.Update<ApprovalLog>(dbApprovalLog, primaryKeyList: new string[] { "ApprovalLogID" }, transaction: dbTransaction);
                    }

                    //send approval rejection for correction notification to the initiator
                    approvalNotice.NoticeType = Constants.ApprovalNoticeType.ApprovalRejectedForCorrection;
                    approvalNotice.ActionUrl = Helper.GetCurrentURL();
                    approvalNotice.NotificationList = GetUserDTO((string)dbVersionInitiatedBy);
                    approvalNotice.ApprovedBy = GetUserDTO(approvedBy).FirstOrDefault();
                    approvalNotice.Comment = string.IsNullOrEmpty(approvalComment) ? "Completed" : approvalComment;
                }

                //case 5: when dbversionapprovalstatus = rejectedforcorrection
                // change status to pending, notify possible approvals
                // change initiatedby id to the user performing action.
                if ((string)dbVersionApprovalStatus == Constants.ApprovalStatus.RejectedForCorrection)
                {
                    var initiatedBy = Helper.GetLoggedInUserID();

                    possibleApproverList = GetPossibleApproverList((string)initiatedBy, moduleName);
                    var approvalLog = new ApprovalLog()
                    {
                        InitiatorID = initiatedBy,
                        ApprovalLogID = (long)dbVersionApprovalLogID,
                        ApprovalStatus = Constants.ApprovalStatus.Pending,
                        PossibleVerifierID = Helper.ToStringCSV<string>(possibleApproverList.Select(f => f.Username).ToArray()),
                        ActivityUrl = Helper.GetCurrentURL(),
                        LastComment = "Awaiting Correction"

                    };

                    (from m in list where m.Name == "ApprovalLogID" select m).FirstOrDefault().SetValue(incomingVersion, dbVersionApprovalLogID);
                    (from m in list where m.Name == "InitiatedBy" select m).FirstOrDefault().SetValue(incomingVersion, initiatedBy);
                    (from m in list where m.Name == "ApprovalStatus" select m).FirstOrDefault().SetValue(incomingVersion, Constants.ApprovalStatus.Pending);
                    (from m in list where m.Name == "ApprovedBy" select m).FirstOrDefault().SetValue(incomingVersion, dbVersionApprovedBy);
                    context.Execute("UPDATE ApprovalLog SET InitiatorID = @InitiatorID, ApprovalStatus = @ApprovalStatus, PossibleVerifierID = @PossibleVerifierID WHERE ApprovalLogID = @ApprovalLogID;",
                        approvalLog, transaction: dbTransaction);

                    //send approval request notification back to possible approvers after correction
                    approvalNotice.NoticeType = Constants.ApprovalNoticeType.ApprovalRejectedForCorrection;
                    approvalNotice.ActionUrl = approvalLog.ActivityUrl;
                    approvalNotice.NotificationList = possibleApproverList;
                    approvalNotice.InitiatedBy = GetUserDTO(initiatedBy).FirstOrDefault();
                    approvalNotice.Comment = string.Empty;
                }
            }

            (from m in list where m.Name == "RowVersionNo2" select m).FirstOrDefault().SetValue(incomingVersion, incomingRowVersionNo);

            SendNotification(approvalNotice);

            return incomingVersion;
        }

        private IList<UserMailDto> GetUserDTO(string userName)
        {
            var tr = dapperContext.GetTransaction();
            var verifierNameSql = "Select u.Username, Email AS EmailAddress, FirstName + ' ' + LastName AS DisplayName From [User] u WHERE u.Username = @Username and u.IsDeleted = 0;";
            return context.Query<UserMailDto>(verifierNameSql, new { Username = userName }, transaction: tr).ToList();
        }

        private IList<UserMailDto> GetPossibleApproverList(string incomingVersionInitiatedBy, string moduleName)
        {
            var userInContext = GetUser(incomingVersionInitiatedBy);
            var tr = dapperContext.GetTransaction();
            if (!string.IsNullOrEmpty(userInContext.BranchID))
            {
                var verifierNameSql = "Select u.Username, Email AS EmailAddress, FirstName + ' ' + LastName AS DisplayName From [User] u inner join usersinroles a on u.Username = a.Username inner join rolemoduleaccess m on a.RoleId=m.RoleId and m.VerifyAccess = '1' and u.BranchID = @BranchID and u.Username <> @Username and u.IsDeleted = 0 and m.ModuleId = (SELECT ModuleID FROM Module WHERE ModuleName = @ModuleName);";
                return context.Query<UserMailDto>(verifierNameSql, new { Username = userInContext.Username, BranchID = userInContext.BranchID, ModuleName = moduleName, IsDeleted = 0 }, transaction: tr).ToList();
            }
            else
            {
                var MakeOrCheck = "Select u.Username, Email AS EmailAddress, FirstName + ' ' + LastName AS DisplayName From [User] u inner join usersinroles a on u.Username = a.Username inner join rolemoduleaccess m on a.RoleId=m.RoleId and m.MakeOrCheckAccess = '1' and (u.BranchID is NULL OR u.BranchID = '') and u.Username <> @Username and u.IsDeleted = 0 and m.ModuleId = (SELECT ModuleID FROM Module WHERE ModuleName = @ModuleName);";
                return context.Query<UserMailDto>(MakeOrCheck, new { Username = userInContext.Username, ModuleName = moduleName, IsDeleted = 0 }, transaction: tr).ToList();
            }
        }

        private Model.User GetUser(string username)
        {
            var tr = dapperContext.GetTransaction();
            var sql = "Select *, CONVERT(bigint,RowVersionNo) as RowVersionNo2 From [User] Where Username = @Username and IsDeleted = 0; Select r.* From Role r INNER JOIN UsersInRoles u ON r.RoleId = u.RoleId AND u.Username = (Select Username From [User] Where Username = @Username)";
            User user = null;
            using (var multi = context.QueryMultiple(sql, new { Username = username }, transaction: tr))
            {
                user = multi.Read<User>().FirstOrDefault();
                if (user != null)
                {
                    user.UserRole = multi.Read<Role>().FirstOrDefault();
                }
            }
            user.RoleId = user.UserRole.RoleId;
            return user;
        }

        private bool SendNotification(ApprovalNotification approvalNotice)
        {

            if (!Helper.IsSendApprovalNotificationMail()) return true;

            IList<UserMailDto> toAddressList = approvalNotice.NotificationList;
            IList<UserMailDto> ccAddressList = new List<UserMailDto>();
            //ccAddressList.Add(new UserMailDto() { DisplayName = "Gboyega Suleman", EmailAddress = "gboyega.n.suleman@firstbanknigeria.com", Username = "SN027514" });
            string messageText = string.Empty;
            string messageHtml = string.Empty;
            string subject = string.Format("Notice of {0}", approvalNotice.NoticeType);
            var status = true;

            try
            {
                if (toAddressList == null || toAddressList.Count == 0) return false;

                var message = new MessageItem();
                message.HtmlBody = GetHtmlMessage(approvalNotice);  //  "Notice of approval action"; //messageHtml;
                message.TextBody = messageText;
                message.Subject = subject;
                message.ToAddress = Helper.ToStringCSV<string>(toAddressList.Select(f => string.Format("{0}|{1}", f.DisplayName ?? f.EmailAddress, f.EmailAddress)).ToArray());
                if (ccAddressList != null && ccAddressList.Count > 0)
                    message.Cc = Helper.ToStringCSV<string>(ccAddressList.Select(f => string.Format("{0}|{1}", f.DisplayName ?? f.EmailAddress, f.EmailAddress)).ToArray());

                SendMessageToQueue(message);

            }
            catch (Exception)
            {
                status = false;
            }
            return status;
        }

        private string GetHtmlMessage(ApprovalNotification approvalNotice)
        {
            //approvalNotice.NoticeType = Constants.ApprovalNoticeType.ApprovalRequest; 

            //Read template file from the App_Data folder
            var body = string.Empty;
            using (var sr = new StreamReader(Helper.GetRootPath() + "\\App_Data\\Templates\\" + approvalNotice.NoticeType + ".txt"))
            {
                body = sr.ReadToEnd();
            }

            var addressee = "Team";
            if (approvalNotice.NotificationList.Count == 1)
                addressee = approvalNotice.NotificationList[0].DisplayName;

            body = body.Replace("[ROOTURL]", Helper.GetRootURL());
            body = body.Replace("[FULLNAME]", addressee);
            body = body.Replace("[Here]", approvalNotice.ActionUrl);
            body = body.Replace("[REJECTIONCOMMENT]", approvalNotice.Comment);
            return body;

        }

        private void SendMessageToQueue(MessageItem mailMessage)
        {
            var sql = new StringBuilder();
            sql.AppendLine("INSERT INTO MessageItem (ToAddress, CcAddress, Subject, HtmlBody, TextBody, RemainingRetryCount, IsSending) ");
            sql.AppendLine("VALUES(@ToAddress, @CcAddress, @Subject, @HtmlBody, @TextBody, @RemainingRetryCount, @IsSending); ");
            var tr = dapperContext.GetTransaction();
            context.Execute(sql.ToString(), new
            {
                ToAddress = mailMessage.ToAddress,
                CcAddress = mailMessage.Cc,
                Subject = mailMessage.Subject,
                HtmlBody = mailMessage.HtmlBody,
                TextBody = mailMessage.TextBody,
                RemainingRetryCount = 3,
                IsSending = false
            }, transaction: tr);
        }

        public ItemListingResponse GetItemPendingApprovalList(int pageNumber, int pageSize, string sortField, string sortOrder, string searchText, string[] searchFields)
        {
            var result = new ItemListingResponse() { PagerResource = new PagerItems() };

            result.PagerResource.ResultCount = (int)context.Query<Int64>("Select Count(*) From [ApprovalLog]").First();
            result.PagerResource.CurrentPage = pageNumber;
            result.PagerResource.PageNumber = pageNumber;
            result.PagerResource.PageSize = pageSize;
            result.PagerResource.SortBy = sortField;
            result.PagerResource.SortDirection = sortOrder;

            var userID = Helper.GetLoggedInUserID();
            var userIDFilter = userID.Replace("%", "[%]").Replace("[", "[[]").Replace("]", "[]]");
            userIDFilter = string.Format("%{0}%", userIDFilter);
            var BranchCode = Helper.GetLoggedInUserSolID();

            var orderByField = string.Empty;
            var sql = new StringBuilder();
            sql.AppendLine("SELECT * FROM (");

            sql.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY ");
            if (sortField == Constants.SortField.ActivityName)
            {
                sql.Append(" ActivityName ");
                orderByField = "ActivityName";
            }
            else if (sortField == Constants.SortField.ApprovalLogID)
            {
                sql.Append("ApprovalLogID");
                orderByField = "ApprovalLogID";
            }
            else if (sortField == Constants.SortField.ApprovalStatus)
            {
                sql.Append("ApprovalStatus");
                orderByField = "ApprovalStatus";
            }
            //else if (sortField == Constants.SortField.RecordIdentification)
            //{
            //    sql.Append("RecordIdentification");
            //    orderByField = "RecordIdentification";
            //}
            //else if (sortField == Constants.SortField.EntryDate)
            //{
            //    sql.Append("EntryDate");
            //    orderByField = "EntryDate";
            //}
            sql.AppendLine(") AS NUMBER, ActivityName, ActivityUrl, InitiatorID, ApprovalStatus, ISNULL(LastComment, 'No comment available') AS LastComment ");
            sql.AppendLine("From ( ");

            sql.AppendLine("SELECT ApprovalLogID, ActivityName, ActivityUrl, InitiatorID, ApprovalStatus, LastComment ");
            sql.AppendLine("FROM [ApprovalLog] ");
            sql.AppendLine(" WHERE InitiatorID = @UserID ");
            sql.AppendLine("AND ApprovalStatus = 'RejectedForCorrection' ");
            sql.AppendLine("UNION");
            sql.AppendLine("SELECT [ApprovalLog].ApprovalLogID, ActivityName, ActivityUrl, InitiatorID, [ApprovalLog].ApprovalStatus, LastComment ");
            sql.AppendLine("FROM [ApprovalLog] INNER JOIN [User] u on u.Username = InitiatorID ");
            sql.AppendLine(" WHERE u.BranchID = @BranchCode AND @UserID IN ");
            sql.AppendLine("  (SELECT   ");  //Distinct
            sql.AppendLine(" Split.a.value('.', 'VARCHAR(100)') AS String  ");
            sql.AppendLine(" FROM  (SELECT   ");
            sql.AppendLine(" CAST ('<M>' + REPLACE([PossibleVerifierID], ',', '</M><M>') + '</M>' AS XML)  ");
            sql.AppendLine(" AS String   ");
            sql.AppendLine(" FROM  [dbo].[ApprovalLog] ");
            sql.AppendLine("WHERE [ApprovalLog].ApprovalStatus = 'Pending' AND  PossibleVerifierID  LIKE @UserIDFilter");
            //sql.AppendLine("WHERE (ApprovalStatus <> 'Approved' OR LastComment IS NULL) AND (LastComment <> 'Completed' OR LastComment IS NULL) AND  PossibleVerifierID  LIKE @UserIDFilter");
            //sql.AppendLine("WHERE (ApprovalStatus <> 'Approved' OR LastComment IS NULL) AND (LastComment <> 'Completed' OR LastComment IS NULL) ");
            sql.AppendLine(" ) AS A CROSS APPLY String.nodes ('/M') AS Split(a)) ");
            sql.AppendLine("AND [ApprovalLog].ApprovalStatus = 'Pending' AND InitiatorID <> @UserID) AS TBL1 ) AS TBL ");
            sql.AppendLine("WHERE NUMBER BETWEEN @StartPage AND @EndPage ");
            sql.AppendFormat("ORDER BY {0} ", orderByField);


            sql.AppendLine(sortOrder == Constants.SortOrder.Ascending ? "ASC " : "DESC ");

            result.ItemListingResult = context.Query<ItemListingDto>(sql.ToString(), new { BranchCode = Helper.GetLoggedInUserSolID(), UserID = Helper.GetLoggedInUserID(), StartPage = ((pageNumber - 1) * pageSize) + 1, EndPage = (pageNumber * pageSize), UserIDFilter = userIDFilter }).ToList();

            return result;
        }

        public string GetLastLogin(string username)
        {
            string query = "select top 1 ActionStartTime from (select top 2 ActionStartTime from AuditTrail where Lower(username) = lower(@UserID) and AuditAction = 'LogIn' order by AuditID desc) a order by ActionStartTime";
            return context.Query<string>(query, new { UserID = username }).First();
        }

        public List<AuditTrailListingDto> GetAuditTrailForExport(AuditTrail searchCriteria, DateTime? actionStartTo = null, DateTime? actionEndTo = null)
        {
            DateTimeHelper dtHelper = new DateTimeHelper();
            List<AuditTrailListingDto> AuditTrailListingResult = new List<AuditTrailListingDto>();
            StringBuilder sql = new StringBuilder("SELECT AuditID, AuditAction, AuditPage, ActionStartTime, Username, ActionEndTime, ISNULL(AuditMessage,'Successful') AS AuditMessage, ClientIPAddress, AuditHTTPAction From [AuditTrail] ");
            string userName = string.IsNullOrWhiteSpace(searchCriteria.Username) ? "" : searchCriteria.Username;
            string auditAction = string.IsNullOrWhiteSpace(searchCriteria.AuditAction) ? "" : searchCriteria.AuditAction;
            sql.Append(" WHERE [Username] LIKE '%" + userName + "%' AND ");
            sql.Append("[AuditAction] LIKE '%" + auditAction + "%' ");
            if (searchCriteria.ActionStartTime != null)
            {
                sql.Append("AND [ActionStartTime] >= '" + dtHelper.FormatDateTime(searchCriteria.ActionStartTime.ToString(), format: Constants.DateFormats.LongDateTimeHyphen) + "' ");
            }
            if (actionStartTo != null)
            {
                sql.Append(searchCriteria.ActionStartTime != null ?
                    " AND [ActionStartTime] <= '" + dtHelper.FormatDateTime(actionStartTo.ToString(), format: Constants.DateFormats.LongDateTimeHyphen) + "' " :
                    "' [ActionStartTime] <= '" + dtHelper.FormatDateTime(actionStartTo.ToString(), format: Constants.DateFormats.LongDateTimeHyphen) + "' ");
            }
            if (searchCriteria.ActionEndTime != null)
            {
                sql.Append(" AND [ActionEndTime] >= '" + dtHelper.FormatDateTime(searchCriteria.ActionEndTime.ToString(), format: Constants.DateFormats.LongDateTimeHyphen) + "' ");
            }
            if (actionEndTo != null)
            {
                sql.Append(searchCriteria.ActionStartTime != null ?
                    " AND [ActionEndTime] <= '" + dtHelper.FormatDateTime(actionEndTo.ToString(), format: Constants.DateFormats.LongDateTimeHyphen) + "' " :
                    "[ActionEndTime] <= ' " + dtHelper.FormatDateTime(actionEndTo.ToString(), format: Constants.DateFormats.LongDateTimeHyphen) + "'");
            }
            sql.Append("ORDER BY [AuditID] DESC");
            AuditTrailListingResult = context.Query<AuditTrailListingDto>(sql.ToString()).ToList();
            return AuditTrailListingResult;
        }

        public List<AuditChangeListingDto> GetAuditChangeForExport(AuditChange searchCriteria, DateTime? actionDateTo)
        {
            DateTimeHelper dtHelper = new DateTimeHelper();
            List<AuditChangeListingDto> auditChangeListingResult = new List<AuditChangeListingDto>();
            StringBuilder sql = new StringBuilder("SELECT [AuditChangeID], [TableName], [AffectedRecordID], [AuditType], [Username], [ActionDateTime]  From [AuditChange] ");
            string userName = string.IsNullOrWhiteSpace(searchCriteria.Username) ? "" : searchCriteria.Username;
            string tableName = string.IsNullOrWhiteSpace(searchCriteria.TableName) ? "" : searchCriteria.TableName;
            string affectedRecord = string.IsNullOrWhiteSpace(searchCriteria.AffectedRecordID) ? "" : searchCriteria.AffectedRecordID;
            sql.Append(" WHERE [Username] LIKE '%{0}%' AND [TableName] LIKE '%{1}%' AND [AffectedRecordID] LIKE '%{2}%'");
            if (searchCriteria.ActionDateTime != null)
            {
                sql.Append("AND [ActionDateTime] >= '" + dtHelper.FormatDateTime(searchCriteria.ActionDateTime.ToString(), format: Constants.DateFormats.LongDateTimeHyphen) + "' ");
            }
            if (actionDateTo != null)
            {
                sql.Append(searchCriteria.ActionDateTime != null ?
                  " AND [ActionDateTime] <= '" + dtHelper.FormatDateTime(actionDateTo.ToString(), format: Constants.DateFormats.LongDateTimeHyphen) + "' " :
                  "' [ActionDateTime] <= '" + dtHelper.FormatDateTime(actionDateTo.ToString(), true, format: Constants.DateFormats.LongDateTimeHyphen) + "' ");
            }
            sql.Append("ORDER BY [AuditChangeID] DESC");
            auditChangeListingResult = context.Query<AuditChangeListingDto>(string.Format(sql.ToString(), userName, tableName, affectedRecord)).ToList();
            return auditChangeListingResult;

        }
        //private void FinetuneDiff(Difference diff)
        //{
        //    if (diff.ParentObject1.Target is RoleModuleAccess)
        //    {
        //        string[] splitCurrentProperty = diff.PropertyName.Split('.');
        //        diff.PropertyName = (diff.ParentObject1.Target as RoleModuleAccess).ModuleName + "." + splitCurrentProperty[splitCurrentProperty.Length - 1];
        //    }
        //    else if (diff.ParentObject1.Target is User)
        //    {
        //        string[] splitCurrentProperty = diff.PropertyName.Split('.');
        //        diff.PropertyName = (diff.ParentObject1.Target as User).Username + "." + splitCurrentProperty[splitCurrentProperty.Length - 1];
        //    }
        //}

        private string GetAffectedRecordID(object affectedObject)
        {
            string AffectedRecordID = string.Empty;
            if (affectedObject is RoleViewModel)
            {
                AffectedRecordID = (affectedObject as RoleViewModel).CurrentRole.RoleName;
            }
            else if (affectedObject is User)
            {
                AffectedRecordID = (affectedObject as User).Username;
            }
            return AffectedRecordID;
        }
    }
}
