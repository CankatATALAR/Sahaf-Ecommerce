using DataAccessLayer.Context;
using EntityLayer.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace E_Ticaret.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        DataContext db = new DataContext();
        public ActionResult Index(decimal? Tutar)
        {
            if(User.Identity.IsAuthenticated)
            {
                var username = User.Identity.Name;
                var kullanici = db.Users.FirstOrDefault(x => x.Email == username);
                var model = db.Carts.Where(x => x.UserId == kullanici.Id).ToList();
                var kid = db.Carts.FirstOrDefault(x => x.UserId == kullanici.Id);
                if (model != null)
                {
                    if (kid == null)
                    {
                        ViewBag.Tutar = "Sepetinizde Ürün Bulunmamaktadır";
                    }
                    else if (kid != null)
                    {
                        Tutar = db.Carts.Where(x => x.UserId == kid.UserId).Sum(x => x.Product.Price * x.Quantity);
                        ViewBag.Tutar = "Toplam Tutar " + Tutar + "TL";
                    }
                    return View(model);
                }
            }
            else
            {
                var cartCookie = Request.Cookies["Cart"];
                List<Cart> model = new List<Cart>();

                if (cartCookie != null)
                {
                    model = JsonConvert.DeserializeObject<List<Cart>>(cartCookie.Value);

                    if (model.Count == 0)
                    {
                        ViewBag.Tutar = "Sepetinizde Ürün Bulunmamaktadır";
                    }
                    else
                    {
                        foreach (var item in model)
                        {
                            item.Product = db.Products.Find(item.ProductId);
                        }
                        Tutar = model.Sum(x => x.Product.Price * x.Quantity);
                        ViewBag.Tutar = "Toplam Tutar " + Tutar + "TL";
                    }
                }
                else
                {                    
                    ViewBag.Tutar = "Sepetinizde Ürün Bulunmamaktadır";
                }
                ViewBag.Tutar = Tutar.HasValue ? Tutar.Value.ToString("C") : "0,00";
                return View(model);
            }
            return HttpNotFound();
        }

        public ActionResult AddCart(int id) 
        {
            if (User.Identity.IsAuthenticated)
            {
                var kullaniciadi = User.Identity.Name;
                var model = db.Users.FirstOrDefault(x => x.Email == kullaniciadi);
                var u = db.Products.Find(id);
                var sepet = db.Carts.FirstOrDefault(x => x.UserId == model.Id && x.ProductId == id);
                if(model != null)
                {
                    if (sepet != null)
                    {
                        sepet.Quantity++;
                        sepet.Price = u.Price * sepet.Quantity;
                        db.SaveChanges();
                        return RedirectToAction("Index", "cart");
                    }
                    var s = new Cart
                    {
                        UserId = model.Id,
                        ProductId = u.Id,
                        Quantity = 1,
                        Price = u.Price,
                        Date = DateTime.Now
                    };
                    db.Carts.Add(s);
                    db.SaveChanges();
                    return RedirectToAction("Index", "Cart");
                }
            }
            else
            {
                List<Cart> cart = new List<Cart>();
                var cartCookie = Request.Cookies["Cart"];
                if(cartCookie != null)
                {
                    cart = JsonConvert.DeserializeObject<List<Cart>>(cartCookie.Value);
                }
                var product = db.Products.Find(id);
                var cartItem = cart.FirstOrDefault(x => x.ProductId == id);
                if (cartItem != null)
                {
                    cartItem.Quantity++;
                    cartItem.Price = product.Price * cartItem.Quantity;
                }
                else
                {
                    var newCartItem = new Cart
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        Price = product.Price,
                        Date = DateTime.Now
                    };

                    cart.Add(newCartItem);
                }
                var jsonCart = JsonConvert.SerializeObject(cart);
                var cartCookieNew = new HttpCookie("Cart", jsonCart)
                {
                    Expires = DateTime.Now.AddDays(7)
                };

                Response.Cookies.Add(cartCookieNew);

                return RedirectToAction("Index", "Cart");
            }
            
            return View();
        }

        public ActionResult TotalCount(int? count)
        {
            if (User.Identity.IsAuthenticated)
            {
                var model = db.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
                count = db.Carts.Where(x => x.UserId == model.Id).Count();
                ViewBag.count = count;
                if(count == 0)
                {
                    ViewBag.count = "";
                }
            }
            return PartialView();
        }

        public void DinamikMiktar(int id, int miktari)
        {
            if (User.Identity.IsAuthenticated)
            {
                var model = db.Carts.Find(id);
                model.Quantity = miktari;
                model.Price = model.Product.Price * model.Quantity;
                db.SaveChanges();
            }
            else
            {
                var cartCookie = Request.Cookies["Cart"];
                if (cartCookie != null)
                {
                    var cartItems = JsonConvert.DeserializeObject<List<Cart>>(cartCookie.Value);
                    var itemToUpdate = cartItems.FirstOrDefault(item => item.Id == id);
                    if (itemToUpdate != null)
                    {
                        itemToUpdate.Quantity = miktari;
                        itemToUpdate.Price = itemToUpdate.Product.Price * miktari;

                        Response.Cookies["Cart"].Value = JsonConvert.SerializeObject(cartItems);
                        Response.Cookies["Cart"].Expires = DateTime.Now.AddDays(1); 
                    }
                }
            }
        }

        public ActionResult azalt(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var model = db.Carts.Find(id);
                if (model == null)
                {
                    return RedirectToAction("Index");
                }
                model.Quantity--;
                if (model.Quantity == 0)
                {
                    db.Carts.Remove(model);
                }
                else
                {
                    model.Price = model.Product.Price * model.Quantity;
                }
                db.SaveChanges();               
            }
            else 
            {
                var cartCookie = Request.Cookies["Cart"];
                if (cartCookie != null)
                {
                    var cartItems = JsonConvert.DeserializeObject<List<Cart>>(cartCookie.Value);
                    var itemToUpdate = cartItems.FirstOrDefault(item => item.Id == id);
                    if (itemToUpdate != null)
                    {
                        itemToUpdate.Quantity--;
                        if (itemToUpdate.Quantity == 0)
                        {
                            cartItems.Remove(itemToUpdate);
                        }
                        else
                        {
                            var product = db.Products.Find(itemToUpdate.ProductId);
                            if (product != null)
                            {
                                itemToUpdate.Price = product.Price * itemToUpdate.Quantity;
                            }
                            itemToUpdate.Id = new Random().Next(1, 100000);
                        }                       
                        Response.Cookies["Cart"].Value = JsonConvert.SerializeObject(cartItems);
                        Response.Cookies["Cart"].Expires = DateTime.Now.AddDays(1); 
                    }
                }
            }
            return RedirectToAction("Index");

        }       
        
        public ActionResult arttir(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var model = db.Carts.Find(id);
                if(model == null)
                {
                    return RedirectToAction("Index");
                }
                model.Quantity++;
                model.Price = model.Product.Price * model.Quantity;
                db.SaveChanges();
            }
            else
            {
                var cartCookie = Request.Cookies["Cart"];
                if (cartCookie != null)
                {
                    var cartItems = JsonConvert.DeserializeObject<List<Cart>>(cartCookie.Value);
                    var itemToUpdate = cartItems.FirstOrDefault(item => item.Id == id);
                    if (itemToUpdate != null)
                    {
                        itemToUpdate.Quantity++;
                        
                        var product = db.Products.Find(itemToUpdate.ProductId);
                        if (product != null)
                        {
                            itemToUpdate.Price = product.Price * itemToUpdate.Quantity;
                        }

                        itemToUpdate.Id = new Random().Next(1, 100000);

                        Response.Cookies["Cart"].Value = JsonConvert.SerializeObject(cartItems);
                        Response.Cookies["Cart"].Expires = DateTime.Now.AddDays(1); 
                    }
                }
            }                       
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var sil = db.Carts.Find(id);
                db.Carts.Remove(sil);
                db.SaveChanges();
                //return RedirectToAction("Index");
            }
            else
            {
                var cartCookie = Request.Cookies["Cart"];
                if (cartCookie != null)
                {
                    var cartItems = JsonConvert.DeserializeObject<List<Cart>>(cartCookie.Value);
                    var itemToRemove = cartItems.FirstOrDefault(item => item.Id == id);
                    if (itemToRemove != null)
                    {
                        cartItems.Remove(itemToRemove);
                        foreach (var item in cartItems)
                        {
                            item.Id = new Random().Next(1, 100000);
                        }                     
                        Response.Cookies["Cart"].Value = JsonConvert.SerializeObject(cartItems);
                        Response.Cookies["Cart"].Expires = DateTime.Now.AddDays(1);
                    }
                }
            }
            return RedirectToAction("Index");
        }
        

        public ActionResult DeleteRange()
        {
            if (User.Identity.IsAuthenticated)
            {
                var kullanici = User.Identity.Name;
                var model = db.Users.FirstOrDefault(x => x.Email == kullanici);
                var sil = db.Carts.Where(x => x.UserId == model.Id);
                db.Carts.RemoveRange(sil);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {                
                var cartCookie = Request.Cookies["Cart"];
                if (cartCookie != null)
                {                    
                    cartCookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(cartCookie);
                }
            }
            return RedirectToAction("Index");
            //return HttpNotFound();
        }
    }
}