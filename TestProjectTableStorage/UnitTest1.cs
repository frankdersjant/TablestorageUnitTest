using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System.Net.Http;
using System.Net;
using Moq;
using Microsoft.AspNetCore.Http;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using FunctionApp6;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace TestProjectTableStorage
{
    [TestClass]
    public class UnitTest1
    {
        private ILogger _logger;

        private static ILoggerFactory LoggerFactory { get; set; }

        [TestInitialize]
        public void Init()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder.AddConsole());
            serviceCollection.AddLogging(builder => builder.AddDebug());
            var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<UnitTest1>();  
        }


        [TestMethod]
        public async Task TestMethod1()
        {
            //arrange
            var body = File.ReadAllText("./samples/company.json");

            var httpResponseMock = new Mock<HttpResponse>();

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(ctx => ctx.Response).Returns(httpResponseMock.Object);

            var reqMock = new Mock<HttpRequest>();
            reqMock.Setup(req => req.HttpContext).Returns(httpContextMock.Object);

            reqMock.Setup(req => req.Headers).Returns(new HeaderDictionary());

            var stream = new MemoryStream();
            using var streamWriter = new StreamWriter(stream);
            await streamWriter.WriteAsync(body).ConfigureAwait(false);
            await streamWriter.FlushAsync().ConfigureAwait(false);
            
            //SPECIAAL voor Yari!!
            stream.Position = 0;
            
            reqMock.Setup(req => req.Body).Returns(stream);

            // 404 {"odata.error":{"code":"ResourceNotFound","message":{"lang":"en-US","value":"The specified resource does not exist.\nRequestId:19ab49c9-2002-001c-145f-7d77db000000\nTime:2022-06-11T06:47:53.2893442Z"}}}
            // 204 -> nocontent
            var mockResponse = new Mock<Azure.Response>();
            mockResponse.SetupGet(x => x.Status).Returns((int)HttpStatusCode.NoContent);

            Mock<TableClient> tableClient = new Mock<TableClient>();
            tableClient.Setup(_ => _.AddEntityAsync(It.IsAny<ITableEntity>(), default))
            .ReturnsAsync(mockResponse.Object);

            //act 
            var actionResult = await FunctionHttpTrigger.Run(reqMock.Object, tableClient.Object, _logger).ConfigureAwait(false);
            
            //assert
            Assert.AreEqual(typeof(NoContentResult), actionResult.GetType());
        }
    }
}