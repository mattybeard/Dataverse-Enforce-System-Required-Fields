using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBeard.CDSSystemRequiredFields
{
    public class CheckRequiredFieldsPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var orgFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var org = orgFactory.CreateOrganizationService(context.UserId);

            if (context.MessageName != "Create")
                return;

            var createReq = new CreateRequest()
            {
                Parameters = context.InputParameters
            };

            var retrieveEntitiesRequest = new RetrieveEntityRequest()
            {
                LogicalName = createReq.Target.LogicalName,
                EntityFilters = EntityFilters.Attributes
            };

            var resp = (RetrieveEntityResponse)org.Execute(retrieveEntitiesRequest);
            var requiredFields = resp.EntityMetadata.Attributes.Where(a => a.RequiredLevel.Value == AttributeRequiredLevel.ApplicationRequired);
            var missingFields = new List<string>();
            foreach (var requiredField in requiredFields)
            {
                if (!createReq.Target.Attributes.ContainsKey(requiredField.LogicalName))
                    missingFields.Add(requiredField.LogicalName);
            }

            if (missingFields.Any())
                throw new InvalidPluginExecutionException("Missing Required Fields: " + string.Join(",", missingFields));
        }
    }
}
