using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext context;
        private IWebHostEnvironment environment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }
        public IActionResult Index()
        {
            var products = this.context.Products.ToList();
            return View(products);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(ProductDto pro)
        {
            if (pro.ImageFileName == null)
            {
                ModelState.AddModelError("ImageFileName", "The image file is required");
            }

            if (!ModelState.IsValid)
            {
                return View(pro);
            }
            // Lưu file ảnh
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(pro.ImageFileName!.FileName);
            string imageFullPath = environment.WebRootPath + "/products/" + newFileName;
            using (var stream = System.IO.File.Create(imageFullPath))
            {
                pro.ImageFileName.CopyTo(stream);
            }
            Product product = new Product()
            {
                Name = pro.Name,
                Brand = pro.Brand,
                Category = pro.Category,
                Price = pro.Price,
                Description = pro.Description,
                ImageFileName = newFileName,
                CreateAt = DateTime.Now,
            };


            context.Products.Add(product);
            context.SaveChanges();
            return RedirectToAction("Index", "Products");
        }
        public IActionResult Edit(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            // Create productDto from product
            var productDto = new ProductDto()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,
            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreateAt"] = product.CreateAt.ToString("MM/dd/yyyy");
            return View(productDto);
        }
        [HttpPost]
        public IActionResult Edit(int id , ProductDto productDto)
        {
            var pro = context.Products.Find(id);
            if(pro == null)
            {
                return RedirectToAction("Index", "Products");
            }
            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] = pro.Id;
                ViewData["ImageFileName"] = pro.ImageFileName;
                ViewData["CreateAt"] = pro.CreateAt.ToString("MM/dd/yyyy");
                return View(pro);
            }

            // update the image file if we have a new image file
            string newFileName = pro.ImageFileName;
            if (productDto.ImageFileName != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(productDto.ImageFileName.FileName);
                string imageFullPath = environment.WebRootPath + "/products/" + newFileName;
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFileName.CopyTo(stream);
                }
                // delete the old image
                string oldImageFullPath = environment.WebRootPath + "/products/" + pro.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }
            // update the product in the database
            pro.Name = productDto.Name;
            pro.Brand = productDto.Brand;
            pro.Category = productDto.Category;
            pro.Price = productDto.Price;
            pro.Description = productDto.Description;
            pro.ImageFileName = newFileName;
            context.SaveChanges();
            return RedirectToAction("Index", "Products"); 
        }
        public IActionResult Delete(int id)
        {
            var pro = context.Products.Find(id);
            if (pro == null)
            {
                return RedirectToAction("Index", "Products");
            }
            string imageFullPath = environment.WebRootPath + "/products/" + pro.ImageFileName;
            System.IO.File.Delete(imageFullPath);
            context.Products.Remove(pro);
            context.SaveChanges(true);
            return RedirectToAction("Index", "Products");
        }
    }
}
