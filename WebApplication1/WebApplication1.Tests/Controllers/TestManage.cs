﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebApplication1.Controllers;
using WebApplication1.Models;
using System.Transactions;

namespace WebApplication1.Tests.Controllers
{
    [TestClass]
    public class TestManage
    {
        [TestMethod]
        public void TestIndex()
        {
            var controller = new ManageController();
            var result = controller.Index() as ViewResult;
            var db = new CS4PEEntities();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(List<BangSanPham>));
            Assert.AreEqual(db.BangSanPhams.Count(),((List<BangSanPham>)result.Model).Count);
        }

        [TestMethod]
        public void TestEdit()
        {
            var controller = new ManageController();
        }
        [TestMethod]
        public void TestCreate()
        {
            var controller = new ManageController();
            var result = controller.Create() as ViewResult;

            
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ViewData["Loai_id"]);
            Assert.IsInstanceOfType(result.ViewData["Loai_id"],typeof(SelectList));

        }
        [TestMethod]
        public void TestDetails()
        {
            var controller = new ManageController();
            var context = new Mock<HttpContextBase>();
            context.Setup(c => c.Server.MapPath("~/App_Data")).Returns("~/App_Data");
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            var result = controller.Details("1") as FilePathResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("images", result.ContentType);
            var path = Path.Combine("~/App_Data", "1");
            Assert.AreEqual(path, result.FileName);

        }
        [TestMethod]
        public void TestCreate2()
        {
            var controller = new ManageController();
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            var files = new Mock<HttpFileCollectionBase>();
            var file = new Mock<HttpPostedFileBase>();

            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
            context.Setup(c => c.Server.MapPath("~/App_Data")).Returns("~/App_Data");
            context.Setup(c => c.Request).Returns(request.Object);
            request.Setup(r => r.Files).Returns(files.Object);
            files.Setup(f => f["HinhAnh"]).Returns(file.Object);
            file.Setup(f => f.ContentLength).Returns(1);
            file.Setup(f => f.SaveAs(It.IsAny<string>()));

            var db = new CS4PEEntities();
            var model = new BangSanPham();
            model.Loai_id = db.LoaiSanPhams.First().id;
            model.TenSP = "TenSanPham";
            model.MaSP = "MaSP";
            model.GiaGoc = 123;
            model.GiaBan = 456;
            model.GiaGop = 789;
            model.SoLuongTon = 10;
            var count = db.BangSanPhams.Count();
            
            using (var scope = new TransactionScope())
            {
                var result = controller.Create(model) as RedirectToRouteResult;

                Assert.AreEqual(count + 1, db.BangSanPhams.Count());
                file.Verify(f =>
                    f.SaveAs(It.Is<string>(s => s.StartsWith("~/App_Data\\"))));
                Assert.AreEqual("Index", result.RouteValues["action"]);

                file.Setup(f => f.ContentLength).Returns(0);
                var fail = controller.Create(model) as ViewResult;
                Assert.IsNotNull(fail);
                Assert.IsInstanceOfType(fail.ViewData["Loai_id"], typeof(SelectList));
            }
        }


    }
}

