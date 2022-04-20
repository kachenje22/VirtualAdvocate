using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VirtualAdvocate.DAL;

namespace VirtualAdvocate.Models
{
    public class ServicesRepository
    {
        //public static AccountServicesModel GetDefaultService(int id)
        //{
        //    return GetAllAccountServices().FirstOrDefault(x => x.Id.Equals(id));
        //}

        /// <summary>
        /// for get all account services
        /// </summary>
        //public static IEnumerable<AccountServicesModel> GetAllAccountServices()
        //{
        //    VirtualAdvocateData objData = new VirtualAdvocateData();
        //    List<AccountService> objASM = objData.GetAccountServices();
        //    List<AccountServicesModel> objAvail = new List<AccountServicesModel>();
        //    foreach (AccountService AC in objASM)
        //    {
        //        objAvail.Add(new AccountServicesModel { Id = AC.ServiceId, Name = AC.Service });
        //    }
        //    return objAvail.ToList();
        //    //return new List<AccountServicesModel> {
        //    //                  new AccountServicesModel {Name = "HR", Id = 1 },
        //    //                  new AccountServicesModel {Name = "Bank", Id = 2},
        //    //                  new AccountServicesModel {Name = "Law", Id = 3},
        //    //                  new AccountServicesModel {Name = "Real Estate", Id = 4}

        //    //                };
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static PaymentMethodModel GetPay(int id)
        {
            return GetAllPaymentMethods().FirstOrDefault(x => x.Id.Equals(id));
        }

        /// <summary>
        /// for get all payment methods
        /// </summary>
        public static IEnumerable<PaymentMethodModel> GetAllPaymentMethods()
        {
            VirtualAdvocateData objData = new VirtualAdvocateData();
            List<PaymentMethod> objPM= objData.GetPaymentMethod();
            List<PaymentMethodModel> objAvailPayment = new List<PaymentMethodModel>();
            foreach (PaymentMethod PM in objPM)
            {
                objAvailPayment.Add(new PaymentMethodModel { Id = PM.PaymentTypeId, Name = PM.PaymentDescription });
            }
            return objAvailPayment.ToList();
            
        }
    }
}