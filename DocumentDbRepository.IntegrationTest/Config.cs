﻿using System.Configuration;

namespace Santhos.DocumentDb.Repository.IntegrationTest
{
    internal static class Config
    {
        public static string DocDbEndpoint => ConfigurationManager.AppSettings["docdb.endpoint"];

        public static string DocDbAuth => ConfigurationManager.AppSettings["docdb.auth"];

        public static string DocDbDatabase => ConfigurationManager.AppSettings["docdb.database"];
    }
}
