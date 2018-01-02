using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Mvc;
using System.Web.Routing;
using ComplyNowWebMVC.Helpers;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Santhos.DocumentDb.Repository;
using ComplianceResponse = ComplyNowWebMVC.Models.ComplianceResponse;
using Device = ComplyNowWebMVC.Models.Device;


namespace ComplyNowWebMVC.Controllers
{
    /*
    The WebApiConfig class may require additional changes to add a route for this controller. Merge these statements into the Register method of the WebApiConfig class as applicable. Note that OData URLs are case sensitive.

    using System.Web.Http.OData.Builder;
    using System.Web.Http.OData.Extensions;
    using ComplyNow_Regimen_Data_Model.Models;
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntitySet<Device>("Devices");
    builder.EntitySet<ComplianceResponse>("ComplianceResponses"); 
    config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
    */
    public class DevicesController : ODataController, IController
    {
        private readonly Repository<Device> _complyNowDoumentDbDeviceRepository;

        private DocumentClient DeviceDocumentClient { get; set; } // = new DocumentClient(new Uri(ConfigurationManager.AppSettings["DocumentDbEndpoint"]), ConfigurationManager.AppSettings["DocumentDbPrimaryKey"]);
        public static readonly string DocumentDbName = ConfigurationManager.AppSettings["DocumentDb"];
        private static readonly string ComplianceResponsesCollectionId = ConfigurationManager.AppSettings["DeviceCollection"];


        public DevicesController(Repository<Device> repository = null, DocumentClient deviceDocumentClient = null)
        {
            if (repository != null)
            {
                _complyNowDoumentDbDeviceRepository = repository;
                DeviceDocumentClient = deviceDocumentClient;
            }
            else
            {
                DeviceDocumentClient = DocumentClientFactory.Create();
                this._complyNowDoumentDbDeviceRepository = new Repository<Device>(DeviceDocumentClient, Config.DocDbEndpoint);
            }
        }

        // GET: odata/Devices(5)
        [EnableQuery]
        public async Task<SingleResult<Device>> GetDevice([FromODataUri] int key)
        {
            List<Device> listDevices = new List<Device>();
            Device device;
            if (key != 0)
            {
                listDevices = (await _complyNowDoumentDbDeviceRepository.GetWhere(d => d.UniqueId == key.ToString()))
                    .ToList()
                    .OrderBy(d => d.UniqueId)
                    .ToList();
                device = listDevices.FirstOrDefault();
                if (device != null) return SingleResult.Create(listDevices.AsQueryable());
            }

            device = await CreateNewDevice();
            listDevices.Add(device);

            return SingleResult.Create(listDevices.AsQueryable());
        }

        private async Task<Device> GetLastDevice()
        {
            var lastDocument = await _complyNowDoumentDbDeviceRepository.GetLastDocument();
            return lastDocument;
        }

        private async Task<Device> CreateNewDevice()
        {
            var uniqueId = 1;
            var lastDocument = await GetLastDevice();
            if (lastDocument != null)
            {
                uniqueId = Convert.ToInt32(lastDocument.UniqueId);
                uniqueId++;
            }

            var device = new Device
            {
                UniqueId = uniqueId.ToString("D10"),
                HardwareVersion = "1.0",
                FirmwareVersion = "2.0",
                PatientId = 0,
                DeviceType = "DeviceController",
                CreatedDateTime = DateTime.Now,
                UpdatedDateTime = DateTime.Now,
                ComplianceResponses = new List<ComplianceResponse>()
            };

            // var resultDevice = await ComplyNowDoumentDbRepository.Create(device);

            return device;
        }

        // PUT: odata/Devices(5)
        public async Task<IHttpActionResult> Put([FromODataUri] string key, Delta<Device> patch)
        {
            try
            {
                Validate(patch.GetEntity());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            Device originalDevice;
            var patchDeviceEntity = patch.GetEntity();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                originalDevice = (await _complyNowDoumentDbDeviceRepository.GetWhere(d => d.UniqueId == key))
                    .AsEnumerable()
                    .SingleOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            patch.Patch(originalDevice);
            
            var result = await _complyNowDoumentDbDeviceRepository.Update(originalDevice);

            return Updated(result);
        }

        // POST: odata/Devices
        public async Task<IHttpActionResult> Post(Device device)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Device resultDevice = null;
            try
            {
                resultDevice = await _complyNowDoumentDbDeviceRepository.Create(device);
            }
            catch (Exception exception)
            {
                var errorMessage = exception.Message;
                throw;
            }

            return Created(resultDevice);
        }

        // PATCH: odata/Devices(5)
        [System.Web.Http.AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Device patch)
        {
            IQueryable<Device> deviceList;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                deviceList = await _complyNowDoumentDbDeviceRepository.GetWhere(d => d.UniqueId == patch.UniqueId);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            if (deviceList == null)
            {
                return NotFound();
            }

            var updateDevice = deviceList.AsEnumerable().FirstOrDefault();

            var resultDevice = await _complyNowDoumentDbDeviceRepository.Update(updateDevice);

            return Updated(resultDevice);
        }

        // DELETE: odata/Devices(5)
        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            IQueryable<Device> deviceList = await _complyNowDoumentDbDeviceRepository.GetWhere(d => d.UniqueId == key.ToString());
            if (deviceList == null)
            {
                return NotFound();
            }
            // Device device = await DocumentDbRepository<Device>.GetItemAsync(patch.Id);

            var updateDevice = deviceList.FirstOrDefault();
            await _complyNowDoumentDbDeviceRepository.Delete(updateDevice);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET: odata/Devices(5)/ComplianceResponses
        [EnableQuery]
        public IQueryable<ComplianceResponse> GetComplianceResponses([FromODataUri] Guid key)
        {
            return null; //ComplyNowRegimenEntities.Devices.Where(m => m.Id == key).SelectMany(m => m.ComplianceResponses);
        }

        protected override void Dispose(bool disposing)
        {
            //if (disposing)
            //{
                DeviceDocumentClient.Dispose();
            //}
            base.Dispose(disposing);
        }

        private bool DeviceExists(int key)
        {
            return false; //ComplyNowRegimenEntities.Devices.Count(e => e.UniqueId == key.ToString()) > 0;
        }

        public void Execute(RequestContext requestContext)
        {
            throw new NotImplementedException();
        }
    }
}
