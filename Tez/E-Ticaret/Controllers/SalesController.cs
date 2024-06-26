using DataAccessLayer.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using System.CodeDom;
using EntityLayer.Entities;
using Newtonsoft.Json;

namespace E_Ticaret.Controllers
{
    public class SalesController : Controller
    {
        // GET: Sales
        DataContext db = new DataContext();
        public ActionResult Index(int sayfa = 1)
        {
            if (User.Identity.IsAuthenticated)
            {
                var kullaniciadi = User.Identity.Name;
                var kullanici = db.Users.FirstOrDefault(x => x.Email == kullaniciadi);
                var model = db.Sales.Where(x => x.UserId == kullanici.Id).ToList().ToPagedList(sayfa, 5);
                return View(model);
            }
            else
            {
                var cartItems = CookieHelper.GetCartItemsFromCookie(Request);
                var pagedCartItems = cartItems.ToPagedList(sayfa, 5);
                return View(pagedCartItems);
            }            
        }
        public static class CookieHelper
        {
            private const string CartCookieName = "Cart";

            public static List<Cart> GetCartItemsFromCookie(HttpRequestBase request)
            {
                var cartItems = new List<Cart>();
                var cartCookie = request.Cookies[CartCookieName];
                if (cartCookie != null)
                {
                    var cartJson = cartCookie.Value;
                    cartItems = JsonConvert.DeserializeObject<List<Cart>>(cartJson);
                }
                foreach (var item in cartItems)
                {
                    if (item.Quantity <= 0) 
                    {
                        item.Quantity = 1;
                    }
                }
                return cartItems;
            }

            public static void SaveCartItemsToCookie(HttpResponseBase response, List<Cart> cartItems)
            {
                var jsonCart = JsonConvert.SerializeObject(cartItems);
                var cartCookie = new HttpCookie(CartCookieName, jsonCart)
                {
                    Expires = DateTime.Now.AddDays(7) // Örnek olarak 7 gün geçerli
                };
                response.Cookies.Add(cartCookie);
            }
        }

        public ActionResult Buy(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var model = db.Carts.FirstOrDefault(x => x.Id == id);
                if(model != null)
                {
                    return View(model);
                }               
            }
            else
            {
                var cartItems = CookieHelper.GetCartItemsFromCookie(Request);
                var cartItem = cartItems.FirstOrDefault(x => x.Id == id);
                if (cartItem != null)
                {
                    return View(cartItem);
                }
            }
            return View("Index");
        }

        [HttpPost]
        public ActionResult Buy2(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if(User.Identity.IsAuthenticated)
                    {
                        var model = db.Carts.FirstOrDefault(x => x.Id == id);
                        if(model != null)
                        {
                            var satis = new Sales
                            {
                                UserId = model.UserId,
                                ProductId = model.ProductId,
                                Quantity = model.Quantity,
                                Image = model.Image,
                                Price = model.Price,
                                Date = DateTime.Now,
                            };
                            db.Carts.Remove(model);
                            db.Sales.Add(satis);
                            db.SaveChanges();
                            ViewBag.islem = "Başarıyla Satın Alındı.";
                        }
                    }
                    else
                    {
                        var product = db.Products.FirstOrDefault(x => x.Id == id);
                        if (product != null)
                        {
                            var cartItems = GetCartItemsFromCookie();
                            var cartItem = cartItems.FirstOrDefault(x => x.ProductId == id);

                            if (cartItem == null)
                            {
                                // Yeni bir ürün eklenirken miktarı 1 olarak başlat
                                cartItem = new Cart
                                {
                                    Id = new Random().Next(1, 100000), // Benzersiz ID oluşturma
                                    ProductId = id,
                                    Price = product.Price,
                                    Image = product.Image,
                                    Quantity = 1,
                                    Date = DateTime.Now,
                                };
                                cartItems.Add(cartItem);
                                SaveCartItemsToCookie(cartItems);
                            }

                            ViewBag.islem = "Başarıyla Sepete Eklendi.";
                        }
                    }
                }
            }
            catch (Exception)
            {
                ViewBag.islem = "Satın Alma Başarısız.";
            }
            return View("islem");

        }
        private List<Cart> GetCartItemsFromCookie()
        {
            var cartItems = new List<Cart>();
            var cartCookie = Request.Cookies["Cart"];
            if (cartCookie != null)
            {
                cartItems = JsonConvert.DeserializeObject<List<Cart>>(cartCookie.Value);
            }
            return cartItems;
        }
        private void SaveCartItemsToCookie(List<Cart> cartItems)
        {
            var cookie = new HttpCookie("Cart", JsonConvert.SerializeObject(cartItems))
            {
                Expires = DateTime.Now.AddDays(7) // Cookie geçerlilik süresi
            };
            Response.Cookies.Add(cookie);
        }

        public ActionResult BuyAll(decimal? Tutar)
        {
            if(User.Identity.IsAuthenticated)
            {
                var kullaniciadi = User.Identity.Name;
                var kullanici = db.Users.FirstOrDefault(x => x.Email == kullaniciadi);
                var model = db.Carts.Where(x => x.UserId == kullanici.Id).ToList();
                var kid = db.Carts.FirstOrDefault(x => x.UserId == kullanici.Id);
                if(model != null)
                {
                    if(kid == null)
                    {                      
                        ViewBag.Tutar = "Sepetinizde Ürün Bulunmamaktadır";
                    }
                    else if(kid != null)
                    {
                        Tutar = db.Carts.Where(x => x.UserId == kid.UserId).Sum(x => x.Product.Price * x.Quantity);
                        ViewBag.Tutar = "Toplam Tutar " + Tutar + "TL";
                    }
                    return View(model);
                }
                return View();
            }
            else
            {
                // Misafir kullanıcılar için çerezden sepeti oku
                var cartCookie = Request.Cookies["Cart"];
                if (cartCookie != null)
                {
                    var cartItems = JsonConvert.DeserializeObject<List<Cart>>(cartCookie.Value);
                    if (cartItems != null && cartItems.Any())
                    {
                        Tutar = cartItems.Sum(x => x.Product.Price * x.Quantity);
                        ViewBag.Tutar = "Toplam Tutar " + Tutar + "TL";
                        return View(cartItems);
                    }
                }
                ViewBag.Tutar = "Sepetinizde Ürün Bulunmamaktadır";
                return View(new List<Cart>());
            }
            //return HttpNotFound();
        }

        [HttpPost]
        public ActionResult BuyAll2()
        {
            //var username = User.Identity.Name;
            //var kullanici = db.Users.FirstOrDefault(x => x.Email == username);
            //var model = db.Carts.Where(x => x.UserId == kullanici.Id).ToList();
            //int row = 0;
            //foreach(var item in model) 
            //{
            //    var satis = new Sales
            //    {
            //        UserId = model[row].UserId,
            //        ProductId = model[row].ProductId,
            //        Quantity = model[row].Quantity,
            //        Price = model[row].Price,
            //        Image = model[row].Image,
            //        Date = DateTime.Now,
            //    };
            //    db.Sales.Add(satis);
            //    db.SaveChanges();
            //    row++;
            //}
            //db.Carts.RemoveRange(model);
            //db.SaveChanges();
            //return RedirectToAction("Index", "Cart");

            if (User.Identity.IsAuthenticated)
            {
                var username = User.Identity.Name;
                var kullanici = db.Users.FirstOrDefault(x => x.Email == username);
                var model = db.Carts.Where(x => x.UserId == kullanici.Id).ToList();
                foreach (var item in model)
                {
                    var satis = new Sales
                    {
                        UserId = item.UserId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price,
                        Date = DateTime.Now,
                    };
                    db.Sales.Add(satis);
                }
                db.SaveChanges();
                db.Carts.RemoveRange(model);
                db.SaveChanges();
            }
            else
            {               
                var cartCookie = Request.Cookies["GuestCart"];
                if (cartCookie != null)
                {
                    var cartItems = JsonConvert.DeserializeObject<List<Cart>>(cartCookie.Value);
                    if (cartItems != null)
                    {
                        foreach (var item in cartItems)
                        {
                            var satis = new Sales
                            {
                                UserId = new Random().Next(1, 100000),  
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                Price = item.Product.Price,
                                Date = DateTime.Now,
                            };
                            db.Sales.Add(satis);
                        }
                        db.SaveChanges();

                        // Sepeti temizle
                        cartCookie.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(cartCookie);
                    }
                }
            }
            return RedirectToAction("Index", "Cart");
        }
    }
}