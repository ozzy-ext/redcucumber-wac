﻿using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using TestServer;
using TestServer.Controllers;
using Xunit;
using Xunit.Abstractions;

namespace MyLab.ApiClient.IntegrationTests
{
    public class ApiClientBehavior : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly ITestOutputHelper _output;

        public ApiClientBehavior(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Fact]
        public async Task ShouldRouteToRootMethod()
        {
            //Arrange
            var client = CreateClient();

            //Act
            string res = await client.RootGet();

            //Assert
            Assert.Equal(nameof(IServer.RootGet), res);
        }

        [Fact]
        public async Task ShouldRouteToMethodWithPath()
        {
            //Arrange
            var client = CreateClient();

            //Act
            string res = await client.GetWithPath();

            //Assert
            Assert.Equal(nameof(IServer.GetWithPath), res);
        }

        [Fact]
        public async Task ShouldRouteToMethodWithParametrizedPath()
        {
            //Arrange
            var client = CreateClient();
            var id = Guid.NewGuid().ToString();

            //Act
            string res = await client.GetWithParametrizedPath(id);

            //Assert
            Assert.Equal(id, res);
        }

        [Fact]
        public async Task ShouldPassQueryParameter()
        {
            //Arrange
            var client = CreateClient();
            var id = Guid.NewGuid().ToString();

            //Act
            string res = await client.GetWithQuery(id);

            //Assert
            Assert.Equal(id, res);
        }

        [Fact]
        public async Task ShouldPassAndGetBinaryData()
        {
            //Arrange
            var client = CreateClient();
            var value = Guid.NewGuid().ToString();
            var bin = Encoding.UTF8.GetBytes(value);

            //Act
            var gotBin = await client.PostBinary(bin);
            var gotValue = Encoding.UTF8.GetString(gotBin);

            //Assert
            Assert.Equal(value, gotValue);
        }

        [Fact]
        public async Task ShouldPassAndGetObjectAsXml()
        {
            //Arrange
            var client = CreateClient();
            var obj = new TestObject
            {
                Value = Guid.NewGuid().ToString(),
                Key = Guid.NewGuid().ToString()
            };
            
            //Act
            var gotObj = await client.PostXmlObj(obj);

            //Assert
            Assert.Equal(obj.Key, gotObj.Key);
            Assert.Equal(obj.Value, gotObj.Value);
        }

        [Fact]
        public async Task ShouldPassAndGetObjectAsJson()
        {
            //Arrange
            var client = CreateClient();
            var obj = new TestObject
            {
                Value = Guid.NewGuid().ToString(),
                Key = Guid.NewGuid().ToString()
            };

            //Act
            var gotObj = await client.PostJsonObj(obj);

            //Assert
            Assert.Equal(obj.Key, gotObj.Key);
            Assert.Equal(obj.Value, gotObj.Value);
        }

        [Fact]
        public async Task ShouldPassObjectAsForm()
        {
            //Arrange
            var client = CreateClient();
            var obj = new TestObject
            {
                Value = Guid.NewGuid().ToString(),
                Key = Guid.NewGuid().ToString()
            };

            //Act
            var gotObj = await client.PostFormObj(obj);

            //Assert
            Assert.Equal(obj.Key, gotObj.Key);
            Assert.Equal(obj.Value, gotObj.Value);
        }

        [Fact]
        public async Task ShouldSupportDeferredCall()
        {
            //Arrange
            var client = CreateClient();
            var obj = new TestObject
            {
                Value = Guid.NewGuid().ToString(),
                Key = Guid.NewGuid().ToString()
            };
            var call = client.GetObjUsingCall(obj);

            //Act
            var gotObj = await call.Invoke();

            //Assert
            Assert.Equal(obj.Key, gotObj.Key);
            Assert.Equal(obj.Value, gotObj.Value);
        }

        IServer CreateClient()
        {
            var b = new ApiClientBuilder<IServer>();
            return b.Create(
                new TestHttpClientProvider<Startup>(_factory), 
                new TestConsoleWriterHttpMessageListener(_output));
        }
    }
}
