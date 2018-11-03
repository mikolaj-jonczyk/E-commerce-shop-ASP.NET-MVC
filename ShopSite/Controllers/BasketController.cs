﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using ShopSite.App_Start;
using ShopSite.DAL;
using ShopSite.Infrastructure;
using ShopSite.Models;
using ShopSite.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShopSite.Controllers
{
    public class BasketController : Controller
    {
        private BasketManager basketManager;
        private SessionManager sessionManager { get; set; }
        private ItemsContext db;

        public BasketController()
        {
            db = new ItemsContext();
            sessionManager = new SessionManager();
            basketManager = new BasketManager(sessionManager, db);
        }


        public ActionResult Index()
        {
            var basketStatus = basketManager.DownloadBucket();
            var totalPrice = basketManager.GetBucketValues();
            BasketViewModel basketVM = new BasketViewModel()
            {
                BasketStatus = basketStatus,
                PriceSum = totalPrice
            };

            return View(basketVM);
        }

        public ActionResult AddToBasket(int id)
        {
            basketManager.AddToBucket(id);
            return RedirectToAction("Index");
        }

        public int GetBasketQuantity()
        {
            return basketManager.GetBucketQuantity();
        }

        public ActionResult Delete(int itemId)
        {
            int quantityToDelete = basketManager.DeleteFromBucket(itemId);
            int quantity = basketManager.GetBucketQuantity();
            decimal basketValue = basketManager.GetBucketValues();

            var vd = new BasketDeletionViewModel
            {
                BasketQuantity = quantity,
                BasketValue = basketValue,
                QuantityToDelete = quantityToDelete,
                ItemIdToRemove = itemId
            };

            return Json(vd);
        }

        public async Task<ActionResult> Pay()
        {
            if (Request.IsAuthenticated)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

                var order = new Order
                {
                    Name = user.UsersData.Name,
                    Surname = user.UsersData.Surname,
                    City = user.UsersData.City,
                    Street = user.UsersData.Street,
                    EMail = user.UsersData.Email,
                    PhoneNumber = user.UsersData.Number,
                    PostalCode = user.UsersData.PostalCode
                };
                return View(order);
            }
            else
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Pay", "Basket") });
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }

        }
    }
}