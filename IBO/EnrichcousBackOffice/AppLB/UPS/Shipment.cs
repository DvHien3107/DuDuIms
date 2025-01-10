using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.ShipWebReference;
using System.Diagnostics;

namespace EnrichcousBackOffice.AppLB.UPS
{
    public class Shipment
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ord"></param>
        /// <param name="scode">ups service code</param>
        /// <param name="package_W">package weight-packagetype|...</param>
        /// <param name="desc">shipment description</param>
        /// <returns></returns>
        public UPSResultMessage Ship(O_Orders ord, string scode, string package_W, string desc)
        {
            using (var db = new WebDataModel())
            {
                try
                {
                    var upsConfig = db.SystemConfigurations.FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(upsConfig.UPS_AccessLicenseNumber))
                    {
                        throw new Exception("UPS account have not been configured.");
                    }

                    var customer = db.C_Customer.Where(c => c.CustomerCode == ord.CustomerCode).FirstOrDefault();

                    try
                    {
                        ShipService shpSvc = new ShipService();
                        ShipmentRequest shipmentRequest = new ShipmentRequest();
                        UPSSecurity upss = new UPSSecurity();

                        UPSSecurityServiceAccessToken upssSvcAccessToken = new UPSSecurityServiceAccessToken();
                        upssSvcAccessToken.AccessLicenseNumber = upsConfig.UPS_AccessLicenseNumber;//"BCBE8EEC50CF3B56";
                        upss.ServiceAccessToken = upssSvcAccessToken;

                        UPSSecurityUsernameToken upssUsrNameToken = new UPSSecurityUsernameToken();
                        upssUsrNameToken.Username = upsConfig.UPS_AccountName;//username
                        upssUsrNameToken.Password = upsConfig.UPS_AccountPassword;//password
                        upss.UsernameToken = upssUsrNameToken;
                        shpSvc.UPSSecurityValue = upss;

                        RequestType request = new RequestType();
                        String[] requestOption = { "nonvalidate" };
                        request.RequestOption = requestOption;
                        shipmentRequest.Request = request;

                        ShipmentType shipment = new ShipmentType();
                        shipment.Description = desc;
                        ShipperType shipper = new ShipperType();
                        shipper.ShipperNumber = upsConfig.UPS_ShipperAccountNumber;//"Your Shipper Number";


                        PaymentInfoType paymentInfo = new PaymentInfoType();
                        ShipmentChargeType shpmentCharge = new ShipmentChargeType();
                        BillShipperType billShipper = new BillShipperType();
                        billShipper.AccountNumber = upsConfig.UPS_ShipperAccountNumber;
                        shpmentCharge.BillShipper = billShipper;
                        shpmentCharge.Type = "01";
                        ShipmentChargeType[] shpmentChargeArray = { shpmentCharge };
                        paymentInfo.ShipmentCharge = shpmentChargeArray;
                        shipment.PaymentInformation = paymentInfo;

                        var shipAddress = upsConfig.UPS_ShipperAddress.Split(new char[] { '|' });
                        var shipper_Country = db.Ad_Country.AsEnumerable().Where(c => c.Name.Replace(" ", "").ToLower().Equals(shipAddress[4].ToLower().Replace(" ", ""))).FirstOrDefault();

                        //thong tin/dia chi nguoi ban/giao hang
                        ShipAddressType shipperAddress = new ShipAddressType();
                        String[] addressLine = { shipAddress[0] };
                        shipperAddress.AddressLine = addressLine;
                        shipperAddress.City = shipAddress[1];
                        shipperAddress.StateProvinceCode = shipAddress[2];
                        shipperAddress.PostalCode = shipAddress[3];
                        shipperAddress.CountryCode = shipper_Country == null ? shipAddress[4] : shipper_Country.CountryCode;
                        shipper.Address = shipperAddress;
                        shipper.Name = upsConfig.UPS_ShipAttentionName;
                        shipper.AttentionName = upsConfig.UPS_ShipAttentionName;
                        ShipPhoneType shipperPhone = new ShipPhoneType();
                        shipperPhone.Number = System.Text.RegularExpressions.Regex.Replace(upsConfig.UPS_ShipperCellPhone, "[-() ]", "");
                        shipper.Phone = shipperPhone;
                        shipment.Shipper = shipper;


                       // var companyAddress = upsConfig.UPS_ShipperAddress.Split(new char[] { '|' });
                        //var shipper_Country = db.Ad_Country.Where(c => c.Name.Replace(" ", "").ToLower().Equals(companyAddress[4].ToLower().Replace(" ", ""))).FirstOrDefault();

                        //giao tu dau
                        ShipFromType shipFrom = new ShipFromType();
                        ShipAddressType shipFromAddress = new ShipAddressType();
                        String[] shipFromAddressLine = { shipAddress[0] };
                        shipFromAddress.AddressLine = addressLine;
                        shipFromAddress.City = shipAddress[1];
                        shipFromAddress.PostalCode = shipAddress[3];
                        shipFromAddress.StateProvinceCode = shipAddress[2];
                        shipFromAddress.CountryCode = shipper_Country == null ? shipAddress[4] : shipper_Country.CountryCode;
                        shipFrom.Address = shipFromAddress;

                        shipFrom.AttentionName = upsConfig.UPS_ShipAttentionName;
                        shipFrom.Name = upsConfig.UPS_ShipAttentionName;
                        shipment.ShipFrom = shipFrom;



                        var shippingAdd = ord.ShippingAddress?.Split(new char[] { '|' });
                        ////giao den ai
                        ShipToType shipTo = new ShipToType();
                        ShipToAddressType shipToAddress = new ShipToAddressType();
                        String[] addressLine1 = { shippingAdd[0] };
                        shipToAddress.AddressLine = addressLine1;
                        shipToAddress.City = shippingAdd[1];
                        shipToAddress.StateProvinceCode = shippingAdd[2];
                        shipToAddress.PostalCode = shippingAdd[3];
                        string cusCountry = string.IsNullOrWhiteSpace(shippingAdd[4]) == true ? "US" : shippingAdd[4];
                        var country = db.Ad_Country.AsEnumerable().Where(c => c.Name.Replace(" ", "").ToLower() == cusCountry.Replace(" ", "").ToLower()).FirstOrDefault();
                        string countrycode = country == null ? cusCountry : country.CountryCode;

                        shipToAddress.CountryCode = countrycode;
                        shipTo.Address = shipToAddress;
                        shipTo.AttentionName = ord.CustomerName;
                        shipTo.Name = ord.CustomerName;
                        ShipPhoneType shipToPhone = new ShipPhoneType();
                        shipToPhone.Number = System.Text.RegularExpressions.Regex.Replace(customer.OwnerMobile,"[-() ]","");
                        shipTo.Phone = shipToPhone;
                        shipment.ShipTo = shipTo;

                        ServiceType service = new ServiceType();
                        service.Code = string.IsNullOrWhiteSpace(scode) == true ? "03" : scode;//03: Ground
                        shipment.Service = service;

                        ShipmentTypeShipmentServiceOptions shpServiceOptions = new ShipmentTypeShipmentServiceOptions();

                        /** **** International Forms ***** */
                        InternationalFormType internationalForms = new InternationalFormType();

                        /** **** Commercial Invoice ***** */
                        String[] formTypeList = { "01" };
                        internationalForms.FormType = formTypeList;


                        
                        /** **** Contacts and Sold To ***** */
                        //ban cho ai
                        ContactType contacts = new ContactType();
                        SoldToType soldTo = new SoldToType();
                        soldTo.Option = "1";
                        soldTo.AttentionName = customer.OwnerName;
                        soldTo.Name = customer.BusinessName;
                        PhoneType soldToPhone = new PhoneType();
                        soldToPhone.Number = customer.CellPhone;
                        soldToPhone.Extension = "";
                        soldTo.Phone = soldToPhone;
                        AddressType soldToAddress = new AddressType();
                        String[] soldToAddressLine = addressLine1;//{ customer.BusinessAddressStreet };
                        soldToAddress.AddressLine = soldToAddressLine;
                        soldToAddress.City = shippingAdd[1]; //customer.BusinessCity;
                        soldToAddress.StateProvinceCode = shippingAdd[2];
                        soldToAddress.PostalCode = shippingAdd[3];//customer.BusinessZipCode;
                        soldToAddress.CountryCode = countrycode;
                        soldTo.Address = soldToAddress;
                        contacts.SoldTo = soldTo;

                        internationalForms.Contacts = contacts;

                        /** **** Product ***** */
                        List<ProductType> productList = new List<ProductType>();
                        foreach (var item in db.Order_Products.Where(o=>o.OrderCode == ord.OrdersCode).ToList())
                        {
                            ProductType product1 = new ProductType();
                            String[] description = { item.ProductName };
                            product1.Description = description;
                            product1.CommodityCode = item.ModelCode;
                            product1.OriginCountryCode = "US";
                            UnitType unit = new UnitType();
                            unit.Number = "147";
                            unit.Value = "478";
                            UnitOfMeasurementType uomProduct = new UnitOfMeasurementType();
                            uomProduct.Code = "BOX";
                            uomProduct.Description = "BOX";
                            unit.UnitOfMeasurement = uomProduct;
                            product1.Unit = unit;
                            ProductWeightType productWeight = new ProductWeightType();
                            productWeight.Weight = "";
                            UnitOfMeasurementType uomForWeight = new UnitOfMeasurementType();
                            uomForWeight.Code = "LBS";
                            uomForWeight.Description = "LBS";
                            productWeight.UnitOfMeasurement = uomForWeight;
                            product1.ProductWeight = productWeight;
                            productList.Add(product1);
                        }


                        internationalForms.Product = productList.ToArray();

                        /** **** InvoiceNumber, InvoiceDate, PurchaseOrderNumber, TermsOfShipment, ReasonForExport, Comments and DeclarationStatement  ***** */
                        internationalForms.InvoiceNumber = ord.InvoiceNumber.ToString();
                        internationalForms.InvoiceDate = (ord.UpdatedAt == null ? ord.CreatedAt : ord.UpdatedAt).Value.ToString("yyyyMMdd");
                        internationalForms.PurchaseOrderNumber = "PO" + ord.InvoiceNumber.ToString();
                        internationalForms.TermsOfShipment = "CFR";
                        internationalForms.ReasonForExport = "Sale";
                        internationalForms.Comments = "";
                        internationalForms.DeclarationStatement = "";

                        /** **** Discount, FreightCharges, InsuranceCharges, OtherCharges and CurrencyCode  ***** */
                        //IFChargesType discount = new IFChargesType();
                        //discount.MonetaryValue = "0"; //discount bao nhieu
                        //internationalForms.Discount = discount;
                        //IFChargesType freight = new IFChargesType();
                        //freight.MonetaryValue = "0"; //tien ship
                        //internationalForms.FreightCharges = freight;
                        //IFChargesType insurance = new IFChargesType();
                        //insurance.MonetaryValue = "0"; //tien bao hiem
                        //internationalForms.InsuranceCharges = insurance;
                        //OtherChargesType otherCharges = new OtherChargesType();
                        //otherCharges.MonetaryValue = "0"; //chi phi khac
                        //otherCharges.Description = "";
                        //internationalForms.OtherCharges = otherCharges;
                        internationalForms.CurrencyCode = "USD";


                        shpServiceOptions.InternationalForms = internationalForms;
                        shipment.ShipmentServiceOptions = shpServiceOptions;

                        List<PackageType> pkgArray = new List<PackageType>();
                        foreach (var item in package_W.Split(new char[] { '|' }))
                        {
                            if (string.IsNullOrWhiteSpace(item))
                            {
                                continue;
                            }

                            //format:3-02- Box|...
                            var item_data = item.Split(new char[] { '-' });

                            PackageType package = new PackageType();
                            PackageWeightType packageWeight = new PackageWeightType();
                            packageWeight.Weight = item_data[0];
                            ShipUnitOfMeasurementType uom = new ShipUnitOfMeasurementType();
                            uom.Code = "LBS";
                            packageWeight.UnitOfMeasurement = uom;
                            package.PackageWeight = packageWeight;
                            PackagingType packType = new PackagingType();
                            packType.Code = item_data[1];//02:box
                            package.Packaging = packType;

                            pkgArray.Add(package);
                        }

                        shipment.Package = pkgArray.ToArray();

                        LabelSpecificationType labelSpec = new LabelSpecificationType();
                        LabelStockSizeType labelStockSize = new LabelStockSizeType();
                        labelStockSize.Height = "6";
                        labelStockSize.Width = "4";
                        labelSpec.LabelStockSize = labelStockSize;
                        LabelImageFormatType labelImageFormat = new LabelImageFormatType();
                        labelImageFormat.Code = "GIF";
                        labelSpec.LabelImageFormat = labelImageFormat;
                        shipmentRequest.LabelSpecification = labelSpec;
                        shipmentRequest.Shipment = shipment;


                        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11; //This line will ensure the latest security protocol for consuming the web service call.
                        ShipmentResponse shipmentResponse = shpSvc.ProcessShipment(shipmentRequest);
                    //    Trace.WriteLine("The transaction was a " + shipmentResponse.Response.ResponseStatus.Description);
                    //    Trace.WriteLine("The 1Z number of the new shipment is " + shipmentResponse.ShipmentResults.ShipmentIdentificationNumber);
                        List<string> packageImage64base = new List<string>();
                        foreach (var item in shipmentResponse.ShipmentResults.PackageResults)
                        {
                           // Trace.WriteLine("tracking Number: " + item.TrackingNumber + "\nImage format: " + item.ShippingLabel.ImageFormat.Code + "\nBase64:" + item.ShippingLabel.GraphicImage);
                            packageImage64base.Add(item.ShippingLabel.GraphicImage);
                        }

                        return new UPSResultMessage { Result = 1,
                            status= shipmentResponse.Response.ResponseStatus.Description,
                            TrackingNumber = shipmentResponse.ShipmentResults.ShipmentIdentificationNumber, 
                            Msg = shipmentResponse.Response.ResponseStatus.Description,
                            Data = packageImage64base };

                    }
                    catch (System.Web.Services.Protocols.SoapException ex)
                    {

                        return new UPSResultMessage { Result = -1, Msg = ex.Message + ". " + ex.Detail?.InnerText };
                    }
                    catch (System.ServiceModel.CommunicationException ex)
                    {
                        return new UPSResultMessage { Result = -1, Msg = ex.Message };

                    }
                    catch (Exception ex)
                    {

                        return new UPSResultMessage { Result = -1, Msg = ex.Message };
                    }


                   
                }
                catch (Exception e)
                {
                    return new UPSResultMessage { Result = -1, Msg = e.Message };
                }
              
            }
            
        }


        public UPSResultMessage Void(string trackingNumber)
        {
            var db = new WebDataModel();
            try
            {
                var upsConfig = db.SystemConfigurations.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(upsConfig.UPS_AccessLicenseNumber))
                {
                    throw new Exception("UPS account have not been configured.");
                }


                VoidWebReference.VoidService voidService = new VoidWebReference.VoidService();
                VoidWebReference.VoidShipmentRequest voidRequest = new VoidWebReference.VoidShipmentRequest();
                VoidWebReference.RequestType request = new VoidWebReference.RequestType();
                String[] requestOption = { "1" };
                request.RequestOption = requestOption;
                voidRequest.Request = request;
                VoidWebReference.VoidShipmentRequestVoidShipment voidShipment = new VoidWebReference.VoidShipmentRequestVoidShipment();
                voidShipment.ShipmentIdentificationNumber = trackingNumber;
                voidRequest.VoidShipment = voidShipment;

                VoidWebReference.UPSSecurity upss = new VoidWebReference.UPSSecurity();
                VoidWebReference.UPSSecurityServiceAccessToken upssSvcAccessToken = new VoidWebReference.UPSSecurityServiceAccessToken();
                upssSvcAccessToken.AccessLicenseNumber = upsConfig.UPS_AccessLicenseNumber;
                upss.ServiceAccessToken = upssSvcAccessToken;
                VoidWebReference.UPSSecurityUsernameToken upssUsrNameToken = new VoidWebReference.UPSSecurityUsernameToken();
                upssUsrNameToken.Username = upsConfig.UPS_AccountName;
                upssUsrNameToken.Password = upsConfig.UPS_AccountPassword;
                upss.UsernameToken = upssUsrNameToken;
                voidService.UPSSecurityValue = upss;

                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11; //This line will ensure the latest security protocol for consuming the web service call.
             //   Trace.WriteLine(voidRequest);
                VoidWebReference.VoidShipmentResponse voidResponse = voidService.ProcessVoid(voidRequest);
             //   Trace.WriteLine("The transaction was a " + voidResponse.Response.ResponseStatus.Description);
             //   Trace.WriteLine("The shipment has been   : " + voidResponse.SummaryResult.Status.Description);
                return new UPSResultMessage { Result = 1, Msg = voidResponse.SummaryResult.Status.Description };
                
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {

                return new UPSResultMessage { Result = -1, Msg = ex.Message + "." + ex.Detail.LastChild.InnerText };
            }
            catch (System.ServiceModel.CommunicationException ex)
            {

                return new UPSResultMessage { Result = -1, Msg = ex.Message };

            }
            catch (Exception ex)
            {

                return new UPSResultMessage { Result = -1, Msg = ex.Message };
            }

          
        }




    }


    public class UPSResultMessage
    {
        public int Result { get; set; }
        public string TrackingNumber { get; set; }
        public string status { get; set; }
        public string Msg { get; set; }
        public object Data { get; set; }
    }



}