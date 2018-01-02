using System;
using Microsoft.Azure.Documents.Client;

namespace Santhos.DocumentDb.Repository.IntegrationTest
{
    internal static class DocumentClientFactory
    {
        public static DocumentClient Create()
        {
            return new DocumentClient(
                new Uri(Config.DocDbEndpoint),
                Config.DocDbAuth);
        }
    }
}
