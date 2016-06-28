using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Axial.Umbraco.EncryptedTextbox.Controllers.Api
{
    [PluginController("EncryptedTextbox")]
    [IsBackOffice]
    public class EncryptedTextboxApiController : UmbracoAuthorizedJsonController
    {
        private string _passPhase = ConfigurationManager.AppSettings["EncryptedTextbox:PassPhase"];

        [HttpGet]
        public string Encrypt(string value, string key)
        {
            if (!String.IsNullOrEmpty(key))
            {
                value = Utilities.SimpleEncrypt.Encrypt(value, key);
            }
            return value;
        }

        [HttpGet]
        public string Decrypt(string value, string key)
        {
            if(!String.IsNullOrEmpty(key))
            {
                value = Utilities.SimpleEncrypt.Decrypt(value, key);
            }            
            return value;
        }
    }
}
