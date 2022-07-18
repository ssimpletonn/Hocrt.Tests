using Hocr.Controllers;
using Hocr.Converter;
using Hocr.DataBase;
using Hocr.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using System.Collections.Generic;
using Hocr.Contracts;

namespace Hocrt.Tests
{
    public class FileModelControllerTests 
    {
        //FileModelController controller;
        private IFileDataBase<FileModel> _db;
        public FileModelControllerTests()
        {
            var dbConfig = new DBConfig();
            dbConfig.dbname = "Data Source=test.db";
            _db = new FileDataBase(dbConfig);
            _db.SetUp();
        }

        [Fact]
        public async Task GetCurrentTest()
        {

            var httpContext = new DefaultHttpContext();
            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            MockHttpSession mockSession = new MockHttpSession();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession);

            var controller = new FileModelController(new HocrConverter(), _db)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockHttpContext.Object
                }
            };
            // controller.ControllerContext.HttpContext = mockHttpContext.Object;
            using (var stream = File.OpenRead(@"./document.png.hocr"))
            {
                var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(@"./document.png.hocr"));
                file.Headers = new HeaderDictionary();
                file.ContentType = "application/png.hocr";
                await controller.Post(file);
                var result = await controller.GetCurrent() as FileContentResult;
                Assert.Equal(file.FileName, result.FileDownloadName);
            }
            await _db.DeleteAllAsync();
        }

        [Fact]
        public async Task PostAndDeleteCurrentTest()
        {
            var httpContext = new DefaultHttpContext();
            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            MockHttpSession mockSession = new MockHttpSession();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession);
            var controller = new FileModelController(new HocrConverter(), _db)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockHttpContext.Object
                }
            };
            using (var stream = File.OpenRead(@"./document.png.hocr"))
            {
                var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(@"./document.png.hocr"));
                file.Headers = new HeaderDictionary();
                file.ContentType = "application/png.hocr";
                List<int> ids = await _db.GetIds();
                var result = ids.Count == 0;
                Assert.True(result);
                await controller.Post(file);
                ids = await _db.GetIds();
                result = ids.Count == 0;
                Assert.False(result);
                await controller.Delete();
                ids = await _db.GetIds();
                result = ids.Count == 0;
                Assert.True(result);
            }
            await _db.DeleteAllAsync();
        }

        [Fact]
        public async Task PostAndDeleteTest()
        {
            var httpContext = new DefaultHttpContext();
            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            MockHttpSession mockSession = new MockHttpSession();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession);
            var controller = new FileModelController(new HocrConverter(), _db)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            using (var stream = File.OpenRead(@"./document.png.hocr"))
            {
                var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(@"./document.png.hocr"));
                file.Headers = new HeaderDictionary();
                file.ContentType = "application/png.hocr";
                await controller.Post(file);
                await controller.Post(file);
                await controller.Post(file);
                await controller.Post(file);
                var list = await _db.GetIds();
                var result = list.Count;
                Assert.Equal(4, result);
                await controller.Delete(3);
                list = await _db.GetIds();
                result = list.Count;
                Assert.Equal(3, result);
            }
            await _db.DeleteAllAsync();
        }
    }
}
