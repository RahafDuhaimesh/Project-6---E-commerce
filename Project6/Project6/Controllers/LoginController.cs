using Project6.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;

namespace Project6.Controllers
{
    public class LoginController : Controller
    {
        private AgateCoffeeShopEntities db = new AgateCoffeeShopEntities();

        public ActionResult Login()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Login([Bind(Include = "Email,Password")] USER loginUser)
        {
            var registeredUser = db.USERS.FirstOrDefault(x => x.Email == loginUser.Email && x.Password == loginUser.Password);

            if (registeredUser != null)
            {
                Session["registeredUsers"] = registeredUser.Email;
                Session["UserID"] = registeredUser.ID;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(loginUser);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register([Bind(Include = "Email,Name,Password,City")] USER newUser)
        {
            if (ModelState.IsValid)
            {
                db.USERS.Add(newUser);
                db.SaveChanges();
                return RedirectToAction("Login", new { success = true });
            }
            return View(newUser);
        }

        public ActionResult ProfilePage(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            }
            USER user = db.USERS.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);

        }

        public ActionResult EditProfile(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            }
            USER user = db.USERS.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(USER user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ProfilePage", new { id = user.ID });
            }
            return View(user);
        }

        public ActionResult DeleteProfile(int? id)
        {

            USER user = db.USERS.Find(id);
       
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProfileConfirm(int id)
        {
            USER user = db.USERS.Find(id);
            if (user != null)
            {
                db.USERS.Remove(user);
                db.SaveChanges();
                Session["registeredUsers"] = null;
                Session["UserID"] = null;
            }
            return RedirectToAction("Index", "Home");
        }




        public ActionResult EditPassword(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            USER user = db.USERS.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new PasswordResetViewModel
            {
                UserID = user.ID
                // Other properties if needed
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPassword(PasswordResetViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.USERS.Find(model.UserID);

                if (user != null && user.Password == model.OldPassword)
                {
                    if (model.NewPassword == model.ConfirmNewPassword)
                    {
                        user.Password = model.NewPassword;
                        db.Entry(user).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("ProfilePage", new { id = user.ID });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The new password and confirmation password do not match.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The old password is incorrect.");
                }
            }
            return View(model);
        }

      
    }
}
