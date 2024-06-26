using BusinessLayer.Concrete;
using DataAccessLayer.Context;
using EntityLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace E_Ticaret.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        ProductRepository productRepository = new ProductRepository();
        DataContext db = new DataContext();
        public PartialViewResult PopularProduct()
        {
            var product=productRepository.GetPopularProduct();
            ViewBag.popular = product;
            return PartialView();
        }
        public ActionResult ProductDetails(int id)
        {
            var details = productRepository.GetById(id);
            var yorum = db.Comments.Where(x=>x.ProductId ==  id).ToList();
            ViewBag.yorum = yorum;
            return View(details);
        }
         
        [HttpPost]
        public ActionResult Comment(Comment data)
        {
            if(User.Identity.IsAuthenticated)
            {
                db.Comments.Add(data);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
            //return View();
        }

        public ActionResult ProductSearch(string searchString)
        {
            IQueryable<Product> query = db.Products;

            if (!String.IsNullOrEmpty(searchString))
            {
                query = query.Where(x => x.Name.Contains(searchString));
            }

            List<Product> searchResult = query.ToList();

            if(searchResult.Count == 1) 
            {
                return RedirectToAction("ProductDetails", new {id = searchResult.First().Id});
            }

            if(searchResult.Count == 0) 
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrEmpty(searchString) || searchString.Length < 2)
            {
                return RedirectToAction("Index", "Home"); 
            }

            return View(searchResult);
        }

        public ActionResult AutoCompleteSearch(string term)
        {
            var products = db.Products
                .Where(x => x.Name.Contains(term)) // Ürün adında term'i içeren ürünleri filtrele
                .Select(x => x.Name) // Sadece ürün adını seç
                .ToList();

            return Json(products, JsonRequestBehavior.AllowGet);
        }
    }
}