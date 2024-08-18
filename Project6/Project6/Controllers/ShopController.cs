using Project6.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Project6.Controllers
{
    public class ShopController : Controller
    {
        private AgateCoffeeShopEntities db = new AgateCoffeeShopEntities();
        public ActionResult AllProducts()
        {
            var items = db.Products.ToList();
            return View(items);
        }
        public ActionResult Categories()
        {
            var categories = db.Categories.ToList();
            return View(categories);
        }

        public ActionResult CategoryProducts(int? id)
        {
            var productsInCategory = db.Products.Where(x => x.CategoriesID == id).ToList();
            return View(productsInCategory);
        }

        [HttpPost]
        public ActionResult AddAllToCart()
        {
            var userId = (int?)Session["UserID"];
            var products = db.Products.ToList();

            foreach (var product in products)
            {
                var quantity = Request.Form["quantity-" + product.ID];
                if (int.TryParse(quantity, out int qty) && qty > 0)
                {
                    var cartItem = db.Carts.FirstOrDefault(x => x.UserID == userId && x.ProductID == product.ID);
                    if (cartItem != null)
                    {
                        cartItem.Quantity += qty;
                    }
                    else
                    {
                        cartItem = new Cart
                        {
                            UserID = userId.Value,
                            ProductID = product.ID,
                            Quantity = qty,
                        };
                        db.Carts.Add(cartItem);
                    }
                }
            }

            db.SaveChanges();
            return RedirectToAction("Cart");
        }


        public ActionResult AddToCart(int productID)
        {
            var userId = (int?)Session["UserID"];
            var product = db.Products.Find(productID); 
            var cartItem = db.Carts.FirstOrDefault(x => x.UserID == userId && x.ProductID == productID);
            if (cartItem != null)
            {
                cartItem.Quantity += 1;
            }
            else
            {
                cartItem = new Cart
                {
                    UserID = userId.Value,
                    ProductID = productID,
                    Quantity = 1,
                };
                db.Carts.Add(cartItem);
            }
            db.SaveChanges();
            var cartItemCount = db.Carts.Where(x => x.UserID == userId).Sum(x => x.Quantity);
            ViewBag.CartItemCount = cartItemCount;

            return RedirectToAction("ProductDetails", new { id = productID });
        }

        public ActionResult Cart()
        {
            var userId = (int?)Session["UserID"];   
            var allItemsInCart = db.Carts.Where(x => x.UserID == userId).ToList();
            var cartViewModels = allItemsInCart.Select(item => new CartViewModel
            {
                CartItem = item,
                Product = db.Products.Find(item.ProductID)
            }).ToList();

            return View(cartViewModels);
        }

        public ActionResult ProductDetails(int? id)
        {

            var product = db.Products.FirstOrDefault(x => x.ID == id);
            var reviews = db.Reviews.Where(x => x.ProductID == id).ToList();
            var viewModel = new ProductDetailsViewModel { Reviews = reviews, Product = product };
            return View(viewModel);
        }


        public ActionResult LeaveReview(int? id)
        {
            Session["ProductID"] = id;
            return View();
        }
        [HttpPost]
        public ActionResult LeaveReview(ProductDetailsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var review = new Review
                {
                    ProductID = model.Product.ID,
                    UserID = (int)Session["UserID"],
                    UserName = model.Review.UserName,
                    Content = model.Review.Content,
                    Rating = model.Review.Rating,
                    ReviewDate = DateTime.Now,
                };

                db.Reviews.Add(review);
                db.SaveChanges();
                return RedirectToAction("ProductDetails", new { id = model.Product.ID });
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult UpdateQuantity(int cartItemId, int quantity)
        {

            var cartItem = db.Carts.Find(cartItemId);
            cartItem.Quantity = quantity;
            db.SaveChanges();
            return Json(new { success = true });


        }

        public ActionResult DeleteItem(int? id)
        {
            Cart cart = db.Carts.Find(id);
            return View(cart);

        }
        [HttpPost]
        public ActionResult DeleteItem(int id)
        {
            Cart cart = db.Carts.Find(id);
            db.Carts.Remove(cart);
            db.SaveChanges();
            return RedirectToAction("Cart");
        }

        public ActionResult Checkout(FormCollection form)
        {
            var userID = (int)Session["UserID"];
            var order = new Order
            {
                UserID = userID,
                DateOfPurchase = DateTime.Now,
            };
            db.Orders.Add(order);
            var cartItems = db.Carts.Where(x => x.UserID == userID).ToList();
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderID = order.ID,
                    ProductID = cartItem.ProductID,
                    Quantity = cartItem.Quantity,
                    Price = db.Products.Find(cartItem.ProductID).Price
                };
                db.OrderItems.Add(orderItem);
            }
            db.SaveChanges();
            db.Carts.RemoveRange(cartItems);
            db.SaveChanges();
            return Json(new { success = true });

        }
        public ActionResult Success()
        {
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }
            return View();
        }
        public class InvoiceItem
        {
            public Product Product { get; set; }
            public int Quantity { get; set; }
        }

        public ActionResult GenerateInvoice()
        {
            var userId = (int?)Session["UserID"];
            var userEmail = (string)Session["registeredUsers"];

            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if user is not logged in
            }

            var cartItems = db.Carts.Where(x => x.UserID == userId).ToList();

            // Prepare data for the invoice
            var invoiceItems = cartItems.Select(item => new InvoiceItem
            {
                Product = db.Products.Find(item.ProductID),
                Quantity = (int)item.Quantity
            }).ToList();

            // Calculate total amount
            decimal total = (decimal)invoiceItems.Sum(x => (x.Product?.Price ?? 0) * x.Quantity); // Handle potential null price

            // Calculate discount
            decimal discount = total > 100 ? 0.15m : 0; // 15% discount for orders over $100

            // Calculate final amount
            decimal finalAmount = total - (total * discount); // Calculate final amount

            ViewBag.UserEmail = userEmail;
            ViewBag.InvoiceItems = invoiceItems;
            ViewBag.TotalAmount = total;
            ViewBag.Discount = discount * 100; // Convert to percentage
            ViewBag.FinalAmount = finalAmount;

            return View();
        }
        public ActionResult MyOrders(int id)
        {
            var MyOrders = db.Orders.Where(x => x.UserID == id).ToList();
            return View(MyOrders);


        }

        public ActionResult orderItems(int id)
        {
            var OrderID = db.OrderItems.Where(x => x.OrderID == id).ToList();

            return View(OrderID);
        }

        public ActionResult Invoice(int id)
        {
            var userId = (int?)Session["UserID"];
            var userEmail = (string)Session["registeredUsers"];
            var OrderID = db.OrderItems.Where(x => x.OrderID == id).ToList();

            var invoiceItems = OrderID.Select(item => new InvoiceItem
            {
                Product = db.Products.Find(item.ProductID),
                Quantity = (int)item.Quantity
            }).ToList();

            decimal total = (decimal)invoiceItems.Sum(x => (x.Product?.Price ?? 0) * x.Quantity); 
            decimal discount = total > 100 ? 0.15m : 0; 

            decimal finalAmount = total - (total * discount); 

            ViewBag.UserEmail = userEmail;
            ViewBag.InvoiceItems = invoiceItems;
            ViewBag.TotalAmount = total;
            ViewBag.Discount = discount * 100; 
            ViewBag.FinalAmount = finalAmount;

            return View();
        }
    }
}