﻿using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMDT_Web.Data;
using TMDT_Web.Models;
using TMDT_Web.Models.Domain;
using TMDT_Web.Models.FPayPal;

namespace TMDT_Web.Controllers
{
    public class CartController : Controller
    {
        DataContext db = new DataContext();
        // GET: Cart

        //Trang giỏ hàng
        public ActionResult Index()
        {
            var acc = db.account.FirstOrDefault(x => x.UserName == User.Identity.Name);
            //Đếm sản phẩm trong giỏ hàng
            List<Models.Item> cart = (List<Models.Item>)Session["cart"];
            if (Session["cart"] == null)
            {
                ViewData["countCartProducts"] = 0;
            }
            else
            {
                ViewData["countCartProducts"] = cart.Count;
            }

            //Kiểm tra giỏ hàng rỗng hay đã có sản phẩm rồi
            if (Session["cart"] == null)
            {
                cart = new List<Models.Item>();
                Session["cart"] = cart;
            }
            else
            {
                var total = cart.Sum(x => x.QuantityBuy * x.product.Price);
                ViewBag.total = total;
            }
            return View(acc);
        }
        public ActionResult Momo(int id)
        {
            var item = db.orderDetail.FirstOrDefault(x => x.OrderID == id);
            ViewBag.total = db.orderDetail.Where(x => x.OrderID == id).Sum(x => x.Price);
            return View(item);
        }
        //Trang xác nhận thanh toán
        public ActionResult verifyOrder(int? PhoneNumber, string Email, string Address, string Coupon, string payment)
        {
            List<Models.Item> cart = (List<Models.Item>)Session["cart"];
            var couponList = db.discounts.FirstOrDefault(x=>x.CodeRnd==Coupon);
            var total = cart.Sum(x => x.QuantityBuy * x.product.Price);
            var acc = db.account.FirstOrDefault(x => x.UserName == User.Identity.Name);
            //Trường hợp có đăng nhập
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.total = total;
                ViewBag.tax = "10%";
                ViewBag.phoneNumber = PhoneNumber;
                ViewBag.email = Email;
                ViewBag.address = Address;
                //trường hợp khách hàng không nhập coupon
                if (couponList == null)
                {
                    //Trường hợp khách hàng dùng paypal thì dc free ship
                    if (payment == "Paypal")
                    {
                        if (acc.Status == null)
                        {
                            Session["finalTotal"] = total + (total * 10 / 100);
                        }
                        else if (acc.Status == "Copper Card")
                        {
                            ViewBag.Status = "-5%";
                            Session["finalTotal"] = total + (total * 10 / 100) - (total * 5 / 100);
                        }
                        else if (acc.Status == "Silver Card")
                        {
                            ViewBag.Status = "-10%";
                            Session["finalTotal"] = total + (total * 10 / 100) - (total * 10 / 100);
                        }
                        else if (acc.Status == "Golden Card")
                        {
                            ViewBag.Status = "-15%";
                            Session["finalTotal"] = total + (total * 10 / 100) - (total * 15 / 100);
                        }
                        else if (acc.Status == "Platinum Card")
                        {
                            ViewBag.Status = "-20%";
                            Session["finalTotal"] = total + (total * 10 / 100) - (total * 20 / 100);
                        }
                        else if (acc.Status == "Diamond Card")
                        {
                            ViewBag.Status = "-25%";
                            Session["finalTotal"] = total + (total * 10 / 100) - (total * 25 / 100);
                        }
                        ViewBag.ship = 0;
                    }
                    //Trường hợp khách hàng ko dùng paypal thì ko dc free ship
                    else
                    {
                        if (acc.Status == null)
                        {
                            Session["finalTotal"] = total + (total * 10 / 100) + 10000;
                        }
                        else if (acc.Status == "Copper Card")
                        {
                            ViewBag.Status = "-5%";
                            Session["finalTotal"] = total + (total * 10 / 100) + 10000 - (total * 5 / 100);
                        }
                        else if (acc.Status == "Silver Card")
                        {
                            ViewBag.Status = "-10%";
                            Session["finalTotal"] = total + (total * 10 / 100) + 10000 - (total * 10 / 100);
                        }
                        else if (acc.Status == "Golden Card")
                        {
                            ViewBag.Status = "-15%";
                            Session["finalTotal"] = total + (total * 10 / 100) + 10000 - (total * 15 / 100);
                        }
                        else if (acc.Status == "Platinum Card")
                        {
                            ViewBag.Status = "-20%";
                            Session["finalTotal"] = total + (total * 10 / 100) + 10000 - (total * 20 / 100);
                        }
                        else if (acc.Status == "Diamond Card")
                        {
                            ViewBag.Status = "-25%";
                            Session["finalTotal"] = total + (total * 10 / 100) + 10000 - (total * 25 / 100);
                        }
                        ViewBag.ship = 10000;
                    }
                    ViewBag.codeDiscount = -1;
                }
                //Trường hợp khách hàng nhập coupon
                else if (couponList.CodeRnd == Coupon)
                {
                    //Trường hợp khách hàng dùng paypal thì dc free ship
                    if (payment == "Paypal")
                    {
                        if (acc.Status == null)
                        {
                            Session["finalTotal"] = total + (total * 10 / 100) - (total * couponList.DiscountPercent / 100);
                        }
                        else if (acc.Status == "Copper Card")
                        {
                            ViewBag.Status = "-5%";
                            Session["finalTotal"] = total + (total * 10 / 100) - (total * 5 / 100) - (total * couponList.DiscountPercent / 100);
                        }
                        else if (acc.Status == "Silver Card")
                        {
                            ViewBag.Status = "-10%";
                            Session["finalTotal"] = total + (total * 10 / 100) - (total * 10 / 100) - (total * couponList.DiscountPercent / 100);
                        }
                        else if (acc.Status == "Golden Card")
                        {
                            ViewBag.Status = "-15%";
                            Session["finalTotal"] = total + (total * 10 / 100) - (total * 15 / 100 - (total * couponList.DiscountPercent / 100));
                        }
                        else if (acc.Status == "Platinum Card")
                        {
                            ViewBag.Status = "-20%";
                            Session["finalTotal"] = total + (total * 10 / 100) - (total * 20 / 100 - (total * couponList.DiscountPercent / 100));
                        }
                        else if (acc.Status == "Diamond Card")
                        {
                            ViewBag.Status = "-25%";
                            Session["finalTotal"] = total + (total * 10 / 100) - (total * 25 / 100) - (total * couponList.DiscountPercent / 100);
                        }
                    }
                    //Trường hợp khách hàng ko dùng paypal thì ko dc free ship
                    else
                    {
                        if (acc.Status == null)
                        {
                            Session["finalTotal"] = total + (total * 10 / 100) + 10000 - (total * couponList.DiscountPercent / 100);
                        }
                        else if (acc.Status == "Copper Card")
                        {
                            ViewBag.Status = "-5%";
                            Session["finalTotal"] = total + (total * 10 / 100) + 10000 - (total * 5 / 100) - (total * couponList.DiscountPercent / 100);
                        }
                        else if (acc.Status == "Silver Card")
                        {
                            ViewBag.Status = "-10%";
                            Session["finalTotal"] = total + (total * 10 / 100) + 10000 - (total * 10 / 100) - (total * couponList.DiscountPercent / 100);
                        }
                        else if (acc.Status == "Golden Card")
                        {
                            ViewBag.Status = "-15%";
                            Session["finalTotal"] = total + (total * 10 / 100) + 10000 - (total * 15 / 100 - (total * couponList.DiscountPercent / 100));
                        }
                        else if (acc.Status == "Platinum Card")
                        {
                            ViewBag.Status = "-20%";
                            Session["finalTotal"] = total + (total * 10 / 100) + 10000 - (total * 20 / 100 - (total * couponList.DiscountPercent / 100));
                        }
                        else if (acc.Status == "Diamond Card")
                        {
                            ViewBag.Status = "-25%";
                            Session["finalTotal"] = total + (total * 10 / 100) + 10000 - (total * 25 / 100) - (total * couponList.DiscountPercent / 100);
                        }
                    }
                    
                    ViewBag.Coupon = "-" + couponList.DiscountPercent + "%";
                    ViewBag.codeDiscount = couponList.EventID;
                }
                else
                {
                    return RedirectToAction("index", "cart");
                }
            }
            //Trường hợp không đăng nhập
            else
            {
                ViewBag.total = total;
                ViewBag.tax = "10%";
                ViewBag.phoneNumber = PhoneNumber;
                ViewBag.email = Email;
                ViewBag.address = Address;
                    
                if (couponList == null)
                {
                    //Trường hợp khách hàng dùng paypal thì dc free ship
                    if (payment == "Paypal")
                    {
                        ViewBag.ship = 0;
                        Session["finalTotal"] = total + (total * 10 / 100);
                    }
                    //Trường hợp khách hàng ko dùng paypal thì ko dc free ship
                    else
                    {
                        ViewBag.ship = 10000;
                        Session["finalTotal"] = total + (total * 10 / 100) + 10000;
                    }
                }
                else if (couponList.CodeRnd == Coupon)
                {
                    //Trường hợp khách hàng dùng paypal thì dc free ship
                    if (payment == "Paypal")
                    {
                        ViewBag.ship = 0;
                        Session["finalTotal"] = total + (total * 10 / 100) - (total * couponList.DiscountPercent / 100);
                    }
                    //Trường hợp khách hàng ko dùng paypal thì ko dc free ship
                    else
                    {
                        ViewBag.ship = 10000;
                        Session["finalTotal"] = total + (total * 10 / 100) + 10000 - (total * couponList.DiscountPercent / 100);
                    }
                    ViewBag.Coupon = "-" + couponList.DiscountPercent +"%";
                    ViewBag.codeDiscount = couponList.EventID;
                    ViewBag.codeDiscount = -1;
                }
                else
                {
                    return RedirectToAction("index", "cart");
                }
            }
            ViewBag.payment = payment;
            return View(cart);
        }
        //Thêm sản phẩm vào giỏ hàng
        public ActionResult Buy(int id)
        {
            if (Session["cart"] == null)
            {
                //List<Models.Item> cart = new List<Models.Item>();
                //cart.Add(new Models.Item { product = db.product.FirstOrDefault(x => x.ProductID == id), QuantityBuy = 1 });
                //Session["cart"] = cart;

                List<Models.Item> cart = new List<Models.Item>
                {
                     new Models.Item(db.product.Find(id), 1,2)
                };
                Session["cart"] = cart;
            }
            else
            {
                List<Models.Item> cart = (List<Models.Item>)Session["cart"];
                int index = isExist(id);
                if (index != -1)
                {
                    cart[index].QuantityBuy++;
                }
                else
                {
                    /* cart.Add(new Models.Item { product = db.product.Find(id), QuantityBuy = 1 });*/
                    cart.Add(new Models.Item(db.product.Find(id), 1, 0));
                }
                Session["cart"] = cart;
            }
            return RedirectToAction("menu", "home");
        }

        //Xóa từng sản phẩm khỏi giỏ hàng
        public ActionResult Remove(int id)
        {
            List<Models.Item> cart = (List<Models.Item>)Session["cart"];
            int index = isExist(id);
            cart.RemoveAt(index);
            Session["cart"] = cart;
            return RedirectToAction("index");
        }
        //Đặt order sản phẩm
        public ActionResult CheckOut(int? phoneNumber, string email, string address, int typePay,int? codeDiscount)
        {
            List<Models.Item> cart = (List<Models.Item>)Session["cart"];
            var discount = db.discounts.FirstOrDefault(x => x.EventID == codeDiscount);
            if (discount == null)
            {
                
            }
            else
            {
                discount.Quantity -= 1;
                db.Entry(discount).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            //Kiểm tra nếu người dùng có tài khoản thêm sp vào giỏ hàng
            if (User.Identity.IsAuthenticated)
            {
                //Orders
                var acc = db.account.FirstOrDefault(x => x.UserName == User.Identity.Name);
                var order = new Models.Domain.Order();
                order.DateTimeOrder = DateTime.Now;
                order.Status = null;
                order.UserID = acc.UserID;
                order.OrderPhoneNumber = phoneNumber;
                order.Address = address;
                order.Email = email;
                order.TypePayment = typePay;
                db.order.Add(order);
                db.SaveChanges();

                //OrderDetail
                foreach (var item in cart)
                {
                    OrderDetail orderDetail = new OrderDetail
                    {
                        QuantityBuy = item.QuantityBuy,
                        Price = item.QuantityBuy * item.product.Price,
                        OrderID = order.OrderID,
                        ProductID = item.product.ProductID,
                    };
                    db.orderDetail.Add(orderDetail);
                    db.SaveChanges();

                    //giảm số lượng sp sau khi đặt
                    var product = db.product.FirstOrDefault(x => x.ProductID == item.product.ProductID);
                    product.Quantity = product.Quantity - item.QuantityBuy;
                    db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                cart.Clear();
                Session["cart"] = cart;
                if (typePay == 1)
                {
                    return View("Success");
                }
                else
                {
                    return RedirectToAction("Momo", "Cart", new { id = order.OrderID });
                }
            }
            //Nếu khách hàng không có tài khoản thêm vào giỏ hàng
            else
            {
                //Orders
                var order = new Models.Domain.Order();
                order.UserID = null;
                order.DateTimeOrder = DateTime.Now;
                order.Status = null;
                order.OrderPhoneNumber = phoneNumber;
                order.Address = address;
                order.Email = email;
                order.TypePayment = typePay;
                db.order.Add(order);
                db.SaveChanges();
                //OrderDetail
                foreach (var item1 in cart)
                {
                    OrderDetail orderDetail = new OrderDetail
                    {
                        QuantityBuy = item1.QuantityBuy,
                        Price = item1.QuantityBuy * item1.product.Price,
                        OrderID = order.OrderID,
                        ProductID = item1.product.ProductID,
                    };
                    db.orderDetail.Add(orderDetail);
                    db.SaveChanges();

                    //giảm số lượng sp sau khi đặt
                    var product = db.product.FirstOrDefault(x => x.ProductID == item1.product.ProductID);
                    product.Quantity = product.Quantity - item1.QuantityBuy;
                    db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                cart.Clear();
                Session["cart"] = cart;
                if (typePay == 1)
                {
                    return View("Success");
                }
                else
                {
                    return RedirectToAction("Momo", "Cart", new { id = order.OrderID });
                }
            }
        }

        //Update số lượng sản phẩm
        public ActionResult UpdateQuantity(int quantity, int id)
        {
            List<Models.Item> cart = (List<Models.Item>)Session["cart"];
            int index = isExist(id);
            if (index != -1)
            {
                cart[index].QuantityBuy = quantity;
            }
            Session["cart"] = cart;
            return RedirectToAction("Index", "Cart");
        }

        //Kiểm tra sản phẩm đã tồn tại chưa
        private int isExist(int id)
        {
            List<Models.Item> cart = (List<Models.Item>)Session["cart"];
            for (int i = 0; i < cart.Count; i++)
            {
                if (cart[i].product.ProductID.Equals(id))
                    return i;
            }
            return -1;
        }

        //Trả về trang thành công
        public ActionResult Success()
        {
            return View();
        }
        
        //Thanh toán Paypal
        private Payment payment;
        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            var listItems = new ItemList() { items = new List<PayPal.Api.Item>() };
            List<Models.Item> listCart = Session["cart"] as List<Models.Item>;
            var f = Session["finalTotal"];

            var list = new Test();

            var acc = db.account.FirstOrDefault(x => x.UserName == User.Identity.Name);
            var total = listCart.Sum(x => x.QuantityBuy * x.product.Price);

           

            foreach (var cart in listCart)
            {
                listItems.items.Add(new PayPal.Api.Item()
                {
                    name = cart.product.ProductName,
                    currency = "USD",
                    price = cart.product.Price.ToString(),
                    quantity = cart.QuantityBuy.ToString(),
                    sku = "sku"
                });
            }

            var payer = new Payer() { payment_method = "paypal" };

            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl
            };

            /*if (acc.Status == "Copper Card")
            {
                final = total + (total * 10 / 100) + 10000 - (total * 5 / 100);
            }
            else if (acc.Status == "Silver Card")
            {
                final = total + (total * 10 / 100) + 10000 - (total * 10 / 100);
            }
            else if (acc.Status == "Golden Card")
            {
                final = total + (total * 10 / 100) + 10000 - (total * 15 / 100);
            }
            else if (acc.Status == "Platinum Card")
            {
                final = total + (total * 10 / 100) + 10000 - (total * 20 / 100);
            }
            else if (acc.Status == "Diamond Card")
            {
                final = total + (total * 10 / 100) + 10000 - (total * 25 / 100);
            }*/

            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = Convert.ToString(listCart.Sum(x => x.product.Price)),
                /*    subtotal = Convert.ToString(10000)*/
            };
            var amount = new Amount()
            {
                currency = "USD",
                total = (Convert.ToDouble(details.tax) + Convert.ToDouble(details.shipping) + Convert.ToDouble(details.subtotal)).ToString(),
                details = details
            };

            var transactionList = new List<Transaction>();
            transactionList.Add(new Transaction()
            {
                description = "Hello test thôi",
                invoice_number = Convert.ToString((new Random()).Next(100000)),
                amount = amount,
                item_list = listItems
            });

            payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };


            return payment.Create(apiContext);
        }
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment() { id = paymentId };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        public ActionResult PaymentWithPaypal(int? phoneNumber, string email, string address)
        {
            List<Models.Item> cart = (List<Models.Item>)Session["cart"];

            if(phoneNumber != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    //Orders
                    var acc = db.account.FirstOrDefault(x => x.UserName == User.Identity.Name);
                    var order = new Models.Domain.Order();
                    order.DateTimeOrder = DateTime.Now;
                    order.Status = null;
                    order.UserID = acc.UserID;
                    order.OrderPhoneNumber = phoneNumber;
                    order.Address = address;
                    order.Email = email;
                    order.TypePayment = 3;
                    db.order.Add(order);
                    db.SaveChanges();

                    //OrderDetail
                    foreach (var item in cart)
                    {
                        OrderDetail orderDetail = new OrderDetail
                        {

                            QuantityBuy = item.QuantityBuy,
                            Price = item.QuantityBuy * item.product.Price,
                            OrderID = order.OrderID,
                            ProductID = item.product.ProductID,
                        };
                        db.orderDetail.Add(orderDetail);
                        db.SaveChanges();

                        //giảm số lượng sp sau khi đặt
                        var product = db.product.FirstOrDefault(x => x.ProductID == item.product.ProductID);
                        product.Quantity = product.Quantity - item.QuantityBuy;
                        db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                else
                {
                    //Orders
                    var order = new Models.Domain.Order();
                    order.DateTimeOrder = DateTime.Now;
                    order.Status = null;
                    order.UserID = null;
                    order.OrderPhoneNumber = phoneNumber;
                    order.Address = address;
                    order.Email = email;
                    order.TypePayment = 3;
                    db.order.Add(order);
                    db.SaveChanges();

                    //OrderDetail
                    foreach (var item in cart)
                    {
                        OrderDetail orderDetail = new OrderDetail
                        {

                            QuantityBuy = item.QuantityBuy,
                            Price = item.QuantityBuy * item.product.Price,
                            OrderID = order.OrderID,
                            ProductID = item.product.ProductID,
                        };
                        db.orderDetail.Add(orderDetail);
                        db.SaveChanges();

                        //giảm số lượng sp sau khi đặt
                        var product = db.product.FirstOrDefault(x => x.ProductID == item.product.ProductID);
                        product.Quantity = product.Quantity - item.QuantityBuy;
                        db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Cart/PaymentWithPaypal?";
                    var guid = Convert.ToString((new Random()).Next(100000));
                    var createdPayment = CreatePayment(apiContext, baseURI + "guid=" + guid);

                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = string.Empty;

                    while (links.MoveNext())
                    {
                        Links link = links.Current;
                        if (link.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = link.href;
                        }
                    }
                    Session.Add(guid, createdPayment.id);

                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    var guid = Request.Params["guid"];
                    var executePayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    if (executePayment.state.ToLower() != "approved")
                    {
                        cart.Clear();
                        return View("Failure");
                    }
                }
            }
            catch (Exception e)
            {
                PaypalLogger.Log("Error: " + e.Message);
                cart.Clear();
                return View("Failure");
            }
            cart.Clear();
            return View("Success");
        }
    }
}



/* List<FinalTotal> f = new List<FinalTotal> {new FinalTotal(total + (total * 10 / 100) + 100000 - (total * 25 / 100)) };
                             Session["final"] = f;*/