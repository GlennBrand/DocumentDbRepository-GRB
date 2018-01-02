using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Documents.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Santhos.DocumentDb.Repository.IntegrationTest
{
    [TestClass]
    public class MainTest
    {
        [TestMethod]
        public async Task Main()
        {
            using (var documentClient = DocumentClientFactory.Create())
            {
                try
                {
                    await new BasicDatabaseProviderTest(documentClient).RunOrderedTest();

                    await new GenericCollectionProviderTest(documentClient).RunOrderedTest();

                    await new RepositoryTest(documentClient).RunOrderedTest();
                }
                finally
                {
                    
                    documentClient.DeleteDatabaseAsync(
                        UriFactory.CreateDatabaseUri(Config.DocDbDatabase))
                        .Wait();
                        
                }
            }
        }

        [TestMethod]
        public async Task DeviceControllerTest()
        {
            using (var documentClient = DocumentClientFactory.Create())
            {
                try
                {
                    await new RepositoryTest(documentClient).RunDeviceControllerOrderedTest();
                }
                finally
                {
                    //documentClient.DeleteDatabaseAsync(
                    //        UriFactory.CreateDatabaseUri(Config.DocDbDatabase))
                    //    .Wait();
                }
            }
        }

    }
}
