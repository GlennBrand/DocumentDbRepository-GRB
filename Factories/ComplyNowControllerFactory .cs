using System;
using System.Web.Mvc;
using System.Web.SessionState;
using ComplyNowWebMVC.Controllers;
using ComplyNowWebMVC.Helpers;
using ComplyNowWebMVC.Models;
using Microsoft.Azure.Documents.Client;
using Santhos.DocumentDb.Repository;

namespace ComplyNowWebMVC.Factories
{
    public class ComplyNowControllerFactory : IControllerFactory
    {

        public static Repository<Device> ComplyNowDoumentDbRepository;
        public static DocumentClient ComplyNowDoumentDbDocumentClient { get; set; }

        public IController CreateController(System.Web.Routing.RequestContext requestContext, string controllerName)
        {
            ComplyNowDoumentDbDocumentClient = DocumentClientFactory.Create();
            ComplyNowDoumentDbRepository = new Repository<Device>(ComplyNowDoumentDbDocumentClient, Config.DocDbDatabase);
            IController controller = null;

            switch (controllerName)
            {
                case "HomeController":
                {
                    controller = new HomeController();
                    break;
                }
                case "AccountController":
                {
                    controller = new AccountController();
                    break;
                }
                case "DevicesController":
                {
                    controller = new DevicesController(ComplyNowDoumentDbRepository, ComplyNowDoumentDbDocumentClient);
                    break;
                }
            }
            return controller;
        }

        public System.Web.SessionState.SessionStateBehavior GetControllerSessionBehavior(
            System.Web.Routing.RequestContext requestContext, string controllerName)
        {
            return SessionStateBehavior.Default;
        }

        public void ReleaseController(IController controller)
        {
            IDisposable disposable = controller as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

    }
}