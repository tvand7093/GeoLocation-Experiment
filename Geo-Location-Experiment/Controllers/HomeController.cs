using AgencyRM.SDK.DocuSign;
using AgencyRM.SDK.DocuSign.Notifications;
using DocuSign.eSign.Model;
using DocuSign.GeneratedClasses.ScopeForm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Geo_Location_Experiment.Controllers
{
    public class HomeController : Controller
    {
        private class Pop : BasePopulator
        {
            public Pop()
            {

            }

            public override void PopulateFields()
            {
                //no fields to populate, so just do nothing.
            }

            public override void PopulateMap()
            {
                Map[AgentSigner.RoleName] = JsonConvert.DeserializeObject<Tabs>(AgentSigner.TabsAsJson);
                Map[BeneficiarySigner.RoleName] = JsonConvert.DeserializeObject<Tabs>(BeneficiarySigner.TabsAsJson);
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> SendDoc()
        {
            string error;
            try
            {
                IDocumentConfig config = SignatureFactory.NewConfig();
                config.IntegratorKey = ConfigurationManager.AppSettings["DocuSign:IntegratorKey"];
                config.Username = ConfigurationManager.AppSettings["DocuSign:Username"];
                config.Password = ConfigurationManager.AppSettings["DocuSign:Password"];
                config.Url = ConfigurationManager.AppSettings["DocuSign:Url"];

                ISignatureRequest request = SignatureFactory.NewRequest();
                request.NotifyCompleteUrl = "https://geolocation-experimental.trafficmanager.net/Home/SaveRequest";
                request.TemplateId = ConfigurationManager.AppSettings["DocuSign:TemplateId"];

                var agent = SignatureFactory.NewUser();
                agent.Email = "tyler.vanderhoef@agencyrm.com";
                agent.Name = "Tyler Agent";
                agent.RoleName = AgentSigner.RoleName;
                agent.Id = "1234";

                var customer = SignatureFactory.NewUser();
                customer.Email = "tyler.vanderhoef@agencyrm.com";
                customer.Name = "Tyler Customer";
                customer.RoleName = BeneficiarySigner.RoleName;
                customer.Id = "1235";

                request.Recipients = new List<ISignatureUser>() { agent, customer };

                //request the signature
                var service = SignatureFactory.NewManager(config);
                service.ChangeSender(config.Username, config.Password);
                ISignatureResult result = await service.RequestEmailBasedSignature(request, new Pop());
                return View(result.EnvelopeId);
            }
            catch(Exception ex)
            {
                error = JsonConvert.SerializeObject(ex);
            }

            return View(error);
        }

        [HttpPost]
        public void SaveRequest(ApiNotification notification)
        {
            if(notification != null)
            {
                System.IO.File.WriteAllText("Notification.json", JsonConvert.SerializeObject(notification));
            }
            else
            {
                //no notification mapped, so just dump the resposne
                System.IO.File.WriteAllText("Notification.json", JsonConvert.SerializeObject(Request));
            }
        }
    }
}
