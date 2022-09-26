﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PxWeb.Code.Api2.DataSource.Cnmm;

namespace PxWeb.UnitTests.DataSource
{
    [TestClass]
    public class CnmmDataSourceTest
    {
        [TestMethod]
        public void ResolveShouldResolveItemCollection()
        {
            string language = "sv";
            var memorymock = new Mock<IMemoryCache>();
            var entryMock = new Mock<ICacheEntry>();
            var configMock = new Mock<IPxApiConfigurationService>();

            memorymock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(entryMock.Object);
            
            var pcAxisFactory = new Mock<IItemSelectionResolverFactory>();

            var testFactory = new TestFactory();
            var dict = testFactory.GetMenuLookup();
            
            var config = testFactory.GetPxApiConfiguration();
            configMock.Setup(x => x.GetConfiguration()).Returns(config);
            
            pcAxisFactory.Setup(x => x.GetMenuLookup(language)).Returns(dict);
            
            var resolver = new ItemSelectionResolverCnmm(memorymock.Object, pcAxisFactory.Object, configMock.Object);

            bool selectionExists;

            var result = resolver.Resolve(language, "AA0003", out selectionExists);

            Assert.IsNotNull(result);
            Assert.AreEqual("AA", result.Menu);
            Assert.IsTrue(selectionExists);
        }


        [TestMethod]
        public void ResolveEmtySelectionItemShouldReturnStart()
        {
            string language = "en";
            var memorymock = new Mock<IMemoryCache>();
            var entryMock = new Mock<ICacheEntry>();
            var configMock = new Mock<IPxApiConfigurationService>();

            memorymock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(entryMock.Object);

            var pcAxisFactory = new Mock<IItemSelectionResolverFactory>();

            var testFactory = new TestFactory();
            var dict = testFactory.GetMenuLookup();

            var config = testFactory.GetPxApiConfiguration();
            configMock.Setup(x => x.GetConfiguration()).Returns(config);

            pcAxisFactory.Setup(x => x.GetMenuLookup(language)).Returns(dict);

            var resolver = new ItemSelectionResolverCnmm(memorymock.Object, pcAxisFactory.Object, configMock.Object);

            bool selectionExists;

            var result = resolver.Resolve(language, "", out selectionExists);

            Assert.AreEqual("START",result.Menu );
            Assert.AreEqual("START", result.Selection);
        }


        [Ignore]
        [TestMethod]
        public void ShouldReturnMenu()
        {
            //todo, mock database 
            string language = "en";
            var memorymock = new Mock<IMemoryCache>();
            var entryMock = new Mock<ICacheEntry>();
            var configMock = new Mock<IPxApiConfigurationService>();

            memorymock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(entryMock.Object);

            var configServiceMock = new Mock<ICnmmConfigurationService>(); 

            var pcAxisFactory = new Mock<IItemSelectionResolverFactory>();

            var testFactory = new TestFactory();
            var dict = testFactory.GetMenuLookup();

            var config = testFactory.GetPxApiConfiguration();
            configMock.Setup(x => x.GetConfiguration()).Returns(config);

            pcAxisFactory.Setup(x => x.GetMenuLookup(language)).Returns(dict);

            var resolver = new ItemSelectionResolverCnmm(memorymock.Object, pcAxisFactory.Object, configMock.Object);

            var datasource = new CnmmDataSource(configServiceMock.Object,  resolver );

            bool selectionExists;

            var result = datasource.CreateMenu("AA0003", language, out selectionExists);

            Assert.IsNotNull(result);
        }

    }
}
