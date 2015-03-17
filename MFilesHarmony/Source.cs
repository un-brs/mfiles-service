using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using HarmonyInterfaces;
using MFilesAPI;
using NLog;


namespace MFilesHarmony
{
    public class Source : ISource
    {
        private readonly string _user;
        private readonly string _password;
        private readonly string _host;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly MFilesServerApplication _server;
        private readonly string[] _requestedVaults;
        private readonly IList<Vault> _vaults = new List<Vault>();
        private readonly string _viewName;
        private readonly DateTime _startDate;

        public Source(string user, string password, string host, string[] requestedVaults, string viewName,
            DateTime startDate)
        {
            _user = user;
            _password = password;
            _host = host;
            _requestedVaults = requestedVaults;
            _viewName = viewName;
            _startDate = startDate;

            _server = new MFilesServerApplication();
        }
        public bool Connect()
        {
            Logger.Info(String.Format("Connecting to MFiles server {0}. User is {1}", _host, _user));
            var conn = _server.Connect(MFAuthType.MFAuthTypeSpecificMFilesUser, _user, _password,
                NetworkAddress:  _host);
            if (conn != MFServerConnection.MFServerConnectionAuthenticated)
            {
                return false;
            }
            var vaultsOnServer = _server.GetVaults();
            foreach (var requestedVault in _requestedVaults)
            {
                VaultOnServer vaultOnServer = null;
                try
                {
                    vaultOnServer = vaultsOnServer.GetVaultByName(requestedVault);
                }
                catch (COMException ex)
                {
                    
                }

                if (vaultOnServer != null)
                {
                    var vault = vaultOnServer.LogIn();
                    if (vault.LoggedIn)
                    {
                        IView view = vault.ViewOperations.GetViews().Cast<IView>().FirstOrDefault(v => v.Name == _viewName);
                        if (view == null)
                        {
                            Logger.Error("Could not find view {0} in vault {1}", _viewName, requestedVault);
                        }
                        else
                        {
                            _vaults.Add(new Vault(requestedVault, vault, view));
                        }
                    }
                    else
                    {
                        Logger.Error("Could not login to vault {0}");
                    }
                }
                else
                {
                    Logger.Error("Could not find vault {0}", requestedVault);
                }
            }
            return true;
        }

        public IEnumerable<IRepository> GetRepositories()
        {
            return _vaults.AsEnumerable();
        }

        public IEnumerable<ISourceDocument> GetDocuments(IRepository repository)
        {
            var vault = repository as Vault;
            if (vault == null)
            {
               yield break;
            }

            var conditions = vault.MfView.SearchConditions;
            var dfDate = new DataFunctionCall();
            dfDate.SetDataDate();

            var search = new SearchCondition();
            var expression = new Expression();
            var value = new TypedValue();

            expression.SetPropertyValueExpression((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified,
                MFParentChildBehavior.MFParentChildBehaviorNone, dfDate);
            search.Set(expression, MFConditionType.MFConditionTypeGreaterThanOrEqual, value);

            conditions.Add(-1, search);

            search = new SearchCondition();
            expression = new Expression();
            value = new TypedValue();
            expression.SetPropertyValueExpression((int)MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified,
                MFParentChildBehavior.MFParentChildBehaviorNone, dfDate);
            search.Set(expression, MFConditionType.MFConditionTypeLessThan, value);

            conditions.Add(-1, search);


            var currentDateTime = _startDate;

            //var internalDocuments = new List<MFilesInternalDocument>();


            while (currentDateTime < DateTime.Now)
            {
                conditions[conditions.Count - 1].TypedValue.SetValue(MFDataType.MFDatatypeDate, currentDateTime);
                currentDateTime = currentDateTime.AddMonths(1);
                conditions[conditions.Count].TypedValue.SetValue(MFDataType.MFDatatypeDate, currentDateTime);


                ObjectSearchResults objects = vault.MfVault.ObjectSearchOperations.SearchForObjectsByConditionsEx(conditions,
                    MFSearchFlags.MFSearchFlagReturnLatestVisibleVersion, true, 0);

                foreach (ObjectVersion objVer in objects)
                {
                    yield return new SourceDocument(vault, objVer);
                }
                //internalDocuments.AddRange(from ObjectVersion obj in objects
               //                            select new MFilesInternalDocument(internalVault, obj));
            }
            // Logger.Info("Number of document to process {0}", internalDocuments.Count);
        } 
    }
}
