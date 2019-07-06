using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Provision
{
    class SiteCreator
    {
        public static void Create()
        {
            var iisManager = new ServerManager();
            var site = iisManager.GetSiteWithName(_siteName);
            if (site == null)
            {
                site = iisManager.Sites.Add(name: _siteName,
                    bindingProtocol: "http",
                    bindingInformation: ":80:" + _hostName,
                    physicalPath: _folder);

                site.ServerAutoStart = true;
                var appPool = iisManager.ApplicationPools.Add(_siteName);

                //appPool.ManagedRuntimeVersion = "v4.0";
                site.Applications.Single().ApplicationPoolName = appPool.Name;
                iisManager.CommitChanges();
            }
        }
    }
    public static class ServerManagerExtensions
    {
        public static Site GetSiteWithName(this ServerManager iisManager, string sitename) =>
            iisManager.Sites.SingleOrDefault(s => s.Name.Equals(sitename, StringComparison.InvariantCultureIgnoreCase));
        public static ApplicationPool GetApplicationPoolWithName(this ServerManager iisManager, string appPoolName) =>
            iisManager.ApplicationPools.SingleOrDefault(appPool => appPool.Name.Equals(appPoolName, StringComparison.InvariantCultureIgnoreCase));
    }
}
