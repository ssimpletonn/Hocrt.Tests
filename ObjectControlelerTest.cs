using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hocr.Controllers;
using Hocr.Converter;
using Hocr.DataBase;
using Hocr.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Xunit;
using Moq;
using Hocr.Contracts;

namespace Hocrt.Tests
{
    public class ObjectControlelerTest
    {
        IFileDataBase<FileModel> _db;
        public ObjectControlelerTest()
        {
            var dbConfig = new DBConfig();
            dbConfig.dbname = "Data Source=test.db";
            _db = new FileDataBase(dbConfig);
            _db.SetUp();
        }

        [Fact]
        public async Task GetObjectTest()
        {
            var httpContext = new DefaultHttpContext();
            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            MockHttpSession mockSession = new MockHttpSession();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession);
            var controller = new ObjectController(_db)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = mockHttpContext.Object
                }
            };
            var filecontroller = new FileModelController(new HocrConverter(), _db)
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
                await filecontroller.Post(file);
            }
            var ar = await controller.GetObjectFromCurrent("word_1_72");
            string value = ar.Value;
            Assert.Equal("проживающей", value);
            ar = await controller.GetObjectFromCurrent("word_1_79");
            value = ar.Value;
            Assert.Equal("Екатеринбург,", value);
            ar = await controller.GetObjectFromCurrent("word_1_211");
            value = ar.Value;
            Assert.Equal("в", value);
        }
    }
}
