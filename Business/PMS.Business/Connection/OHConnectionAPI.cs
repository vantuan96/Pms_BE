using DataAccess.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PMS.Contract.Models;
using PMS.Contract.Models.AdminModels;
using PMS.Contract.Models.ApigwModels;
using PMS.Contract.Models.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using VM.Common;

namespace PMS.Business.Connection
{
    public class OHConnectionAPI:HISConnectionApi
    {
        #region Site info
        public static List<Site> GetSites(string siteCode)
        {
            #region For statistic performance
            var start_time = DateTime.Now;
            TimeSpan tp;
            #endregion .For statistic performance

            string url_postfix = string.Format(
                "/PMSVinmecCom/1.0.0/getFacilityInfo?1=1"
            );
            if (!string.IsNullOrEmpty(siteCode))
            {
                url_postfix += string.Format("&FacilityCode={0}", siteCode);
            }
            IFormatProvider viVNDateFormat = new CultureInfo("vi-VN").DateTimeFormat;
            bool bThrowEx = false;
            var response = RequestAPI(url_postfix, "Entries", "Entry", out bThrowEx, ConfigHelper.CF_ApiTimeout_minutes);

            #region Log Performace
            tp = DateTime.Now - start_time;
            CustomLog.performancejoblog.Info(string.Format("API.GetSites[SiteCode:{0}]: step processing spen time in {1} (ms)", siteCode, tp.TotalMilliseconds));
            #endregion .Log Performace

            if (response != null)
                return response.Select(e => new Site
                {
                    HospitalId = e["FacilityId"]?.ToString(),
                    Code = e["FacilityCode"]?.ToString(),
                    FullNameL = e["FacilityNameL"]?.ToString(),
                    FullNameE = e["FacilityNameE"]?.ToString(),
                    AddressL = e["AddressL"]?.ToString(),
                    AddressE = e["AddressE"]?.ToString(),
                    Tel = e["Tel"]?.ToString(),
                    Fax = e["Fax"]?.ToString(),
                    Hotline = e["Hotline"]?.ToString(),
                    Emergency = e["Emergency"]?.ToString(),
                    //Active or InActive Site
                    IsActived= e["EffectiveFromDate"].ToObject<DateTime>().Date<=Constant.CurrentDate && (e["EffectiveUntilDate"] == null ||  e["EffectiveUntilDate"].Type == JTokenType.Null || (e["EffectiveUntilDate"] != null && e["EffectiveUntilDate"].Type != JTokenType.Null && e["EffectiveUntilDate"].ToObject<DateTime>().Date>=Constant.CurrentDate)),
                }).ToList();
            return null;
        }
        #endregion .Site info
        #region Department
        public static List<HISDepartmentModel> GetDepartment()
        {
            #region For statistic performance
            var start_time = DateTime.Now;
            TimeSpan tp;
            #endregion .For statistic performance

            string url_postfix = "/PMSVinmecCom/1.0.0/getDepartments";
            bool bThrowEx = false;
            var response = RequestAPI(url_postfix, "Entries", "Entry", out bThrowEx, ConfigHelper.CF_ApiTimeout_minutes);

            #region Log Performace
            tp = DateTime.Now - start_time;
            CustomLog.performancejoblog.Info(string.Format("API.GetDepartment: step processing spen time in {0} (ms)", tp.TotalMilliseconds));
            #endregion .Log Performace

            if (response != null)
                return response.Select(e => new HISDepartmentModel
                {
                    ViName = e["DepartmentName"]?.ToString(),
                    EnName = e["DepartmentName"]?.ToString(),
                    Code = e["DepartmentCode"]?.ToString(),
                    HospitalCode = e["HospitalCode"]?.ToString(),
                    DepartmentId = e["DepartmentId"]?.ToString(),
                    IsActivated = e["ActiveFlag"].ToObject<bool>(),
                }).ToList();
            return new List<HISDepartmentModel>();
        }
        #endregion
        #region Service
        /// <summary>
        /// Get List Service
        /// </summary>
        /// <param name="serviceCode"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static List<HISServiceModel> GetService(string serviceCode, string serviceName)
        {
            #region For statistic performance
            var start_time = DateTime.Now;
            TimeSpan tp;
            #endregion .For statistic performance

            string url_postfix = string.Format(
                "/PMSVinmecCom/1.0.0/getServiceInformation?ServiceCode={0}&ServiceName={1}"
                , serviceCode
                , serviceName
            );
            bool bThrowEx = false;
            var response = RequestAPI(url_postfix, "Entries", "Entry", out bThrowEx, ConfigHelper.CF_ApiTimeout_minutes);

            #region Log Performace
            tp = DateTime.Now - start_time;
            CustomLog.performancejoblog.Info(string.Format("API.GetService[serviceCode:{0}; ServiceName:{1}]: step processing spen time in {2} (ms)",serviceCode,serviceName, tp.TotalMilliseconds));
            #endregion .Log Performace

            if (response != null)
                return response.Select(e => new HISServiceModel
                {
                    ServiceId= e["ServiceId"]?.ToObject<Guid?>(),
                    ServiceType = e["ServiceType"]?.ToString(),
                    ServiceGroupCode = e["ServiceGroupCode"]?.ToString(),
                    ServiceGroupViName = e["ServiceGroupName"]?.ToString(),
                    ServiceGroupEnName = e["ServiceGroupNameE"]?.ToString(),
                    ServiceCode = e["ServiceCode"]?.ToString(),
                    ServiceViName = e["ServiceName"]?.ToString(),
                    ServiceEnName = e["ServiceNameE"]?.ToString(),
                    IsActive = e["ActiveFlag"] != null ? Convert.ToBoolean(e["ActiveFlag"].ToString()) : false
                }).ToList();
            return new List<HISServiceModel>();
        }
        #endregion
        #region ChargeType
        /// <summary>
        /// Get List chargetype from HIS core
        /// </summary>
        /// <param name="siteCode"></param>
        /// <returns></returns>
        public static List<ChargeTypeModel> GetChargeType(string siteCode)
        {
            #region For statistic performance
            var start_time = DateTime.Now;
            TimeSpan tp;
            #endregion .For statistic performance

            string url_postfix = string.Format(
                "/PMSVinmecCom/1.0.0/getChargeTypeByHospital?HospitalCode={0}"
                , siteCode
            );
            bool bThrowEx = false;
            var response = RequestAPI(url_postfix, "Entries", "Entry", out bThrowEx, ConfigHelper.CF_ApiTimeout_minutes);

            #region Log Performace
            tp = DateTime.Now - start_time;
            CustomLog.performancejoblog.Info(string.Format("API.GetChargeType[SiteCode:{0}]: step processing spen time in {1} (ms)", siteCode, tp.TotalMilliseconds));
            #endregion .Log Performace

            if (response != null)
                return response.Select(e => new ChargeTypeModel
                {
                    HospitalCode = e["HospitalCode"]?.ToString(),
                    ChargeTypeCode = e["ChargeTypeCode"]?.ToString(),
                    ChargeTypeName = e["ChargeTypeName"]?.ToString(),
                }).ToList();
            return new List<ChargeTypeModel>();
        }
        #endregion .ChargeType
        #region ServicePrice
        public static List<ServicePriceModel> GetServicePrice(string chargeTypeCode, List<string> serviceCodes)
        {
            #region For statistic performance
            var start_time = DateTime.Now;
            TimeSpan tp;
            #endregion .For statistic performance

            string url_postfix = string.Format(
                "/PMSVinmecCom/1.0.0/getServicePrice?ChargeTypeCode={0}"
                , chargeTypeCode
            );
            if(serviceCodes!=null && serviceCodes.Count > 0)
            {
                foreach(var item in serviceCodes)
                    url_postfix += string.Format("&ServiceCode={0}", item);
            }
            bool bThrowEx = false;
            var response = RequestAPI(url_postfix, "Prices", "Price", out bThrowEx, ConfigHelper.CF_ApiTimeout_minutes);

            #region Log Performace
            tp = DateTime.Now - start_time;
            CustomLog.performancejoblog.Info(string.Format("API.GetServicePrice[ChargeTypeCode={0}; serviceCodes={1}]: step processing spen time in {2} (ms)", chargeTypeCode, string.Join(",", serviceCodes), tp.TotalMilliseconds));
            #endregion .Log Performace

            if (response != null)
                return response.Select(e => new ServicePriceModel
                {
                    ServiceCode = e["ServiceCode"]?.ToString(),
                    Price = e["Price"]?.ToObject<double?>(),
                    EffectTo = e["EffectTo"]?.ToObject<DateTime?>(),
                    EffectFrom = e["EffectFrom"]?.ToObject<DateTime?>(),
                }).ToList();
            return new List<ServicePriceModel>();
        }
        #endregion .ServicePrice
        #region Patient
        public static List<PatientInformationModel> GetPatients(PatientParameterModel request)
        {
            #region For statistic performance
            var start_time = DateTime.Now;
            TimeSpan tp;
            #endregion .For statistic performance

            string url_postfix = string.Format(
                "/PMSVinmecCom/1.0.0/searchPatient?1=1"
            );
            if (!string.IsNullOrEmpty(request?.Pid))
            {
                url_postfix += string.Format("&PID={0}", request?.Pid);
            }
            if (!string.IsNullOrEmpty(request?.Name))
            {
                url_postfix += string.Format("&TenKhachHang={0}", request?.Name);
            }
            if (!string.IsNullOrEmpty(request?.Birthday))
            {
                url_postfix += string.Format("&NgaySinh={0}", request?.GetBirthDay()?.ToString(Constant.DATE_SQL));
            }
            if (!string.IsNullOrEmpty(request?.Mobile))
            {
                url_postfix += string.Format("&SoDienThoai={0}", request?.Mobile);
            }
            IFormatProvider viVNDateFormat = new CultureInfo("vi-VN").DateTimeFormat;
            bool bThrowEx = false;
            var response = RequestAPI(url_postfix, "Entries", "Entry", out bThrowEx, ConfigHelper.CF_ApiTimeout_minutes);

            #region Log Performace
            tp = DateTime.Now - start_time;
            CustomLog.performancejoblog.Info(string.Format("API.GetPatients: step processing spen time in {0} (ms)", tp.TotalMilliseconds));
            #endregion .Log Performace

            if (response != null)
                return response.Select(e => new PatientInformationModel
                {
                    PID = e["PID"]?.ToString(),
                    FullName = e["TenKhachHang"]?.ToString(),
                    Gender= PatientInformationModel.GenderFromText(e["GioiTinh"]?.ToString()),
                    DateOfBirth = e["NgaySinh"]!=null && e["NgaySinh"].Type!= JTokenType.Null ? Convert.ToDateTime(e["NgaySinh"].ToString(), viVNDateFormat):(DateTime?)null,
                    //DateOfBirth = e["NgaySinh"]?.ToObject<DateTime?>(),
                    Mobile = e["SoDienThoai"]?.ToString(),
                    Address = e["DiaChi"]?.ToString(),
                    PatientId = e["PatientId"]?.ToObject<Guid?>(),
                    National= e["QuocTich"]?.ToString(),
                }).ToList();
            return new List<PatientInformationModel>();
        }
        #endregion .Patient
        #region Charge
        public static List<HISChargeModel> GetCharges(string pId, string visitCode,string chargeids)
        {
            List<HISChargeModel> listReturn = new List<HISChargeModel>();
            int currentPage = 1;
            #region For statistic performance
            var start_time = DateTime.Now;
            TimeSpan tp;
            #endregion .For statistic performance

            string url_postfix = string.Format(
                "/PMSVinmecCom/1.0.0/getChargesByPID?1=1"
            );
            if (!string.IsNullOrEmpty(pId))
            {
                url_postfix += string.Format("&PID={0}", pId);
            }
            if (!string.IsNullOrEmpty(visitCode))
            {
                url_postfix += string.Format("&VisitCode={0}", visitCode);
            }
        StepPaging:
            string url_charges = string.Empty;
            if (!string.IsNullOrEmpty(chargeids))
            {
                #region paging
                string[] arrCharges = chargeids.Split(';');
                List<string> listChargeIds = arrCharges.ToList();
                var chargeIds2Get=listChargeIds.Skip((currentPage - 1) * 20)
                .Take(20);
                #endregion
                if (chargeIds2Get.Any())
                {
                    string strCharges = string.Join(";", chargeIds2Get?.Select(x => x)?.ToList());
                    url_charges = string.Format("&ChargeId={0}", strCharges);
                }
                else
                {
                    goto StepReturn;
                }
            }
            //IFormatProvider viVNDateFormat = new CultureInfo("vi-VN").DateTimeFormat;
            bool bThrowEx = false;
            var response = RequestAPI(string.Format("{0}{1}", url_postfix, url_charges), "Entries", "Entry", out bThrowEx, ConfigHelper.CF_ApiTimeout_minutes);

            #region Log Performace
            tp = DateTime.Now - start_time;
            CustomLog.performancejoblog.Info(string.Format("API.GetCharges[PId={0}; VisitCode={1}; ChargeIds:{2}]: step processing spen time in {3} (ms)", pId, visitCode, chargeids, tp.TotalMilliseconds));
            #endregion .Log Performace
            //List VisitType Package
            //List<string> visitTypes = new List<string>() { "VMPK", "PKIPD" };
            if (response != null)
            {
                listReturn.AddRange(response.Where(x=> !listReturn.Any(el=>el.ItemId== x["ItemId"]?.ToObject<Guid?>() && el.ChargeId== x["ChargeId"].ToObject<Guid>())).Select(e => new HISChargeModel
                {
                    ItemId = e["ItemId"]?.ToObject<Guid?>(),
                    ItemCode = e["ItemCode"]?.ToString(),
                    ChargeId = e["ChargeId"].ToObject<Guid>(),
                    NewChargeId = e["NewChargeId"]?.ToObject<Guid?>(),
                    ChargeSessionId = e["ChargeSessionId"]?.ToObject<Guid?>(),
                    ChargeDate = e["ChargeDate"]?.ToObject<DateTime?>(),
                    ChargeCreatedDate = e["ChargeCreatedDate"]?.ToObject<DateTime?>(),
                    ChargeUpdatedDate = e["ChargeUpdatedDate"]?.ToObject<DateTime?>(),
                    ChargeDeletedDate = e["ChargeDeletedDate"]?.ToObject<DateTime?>(),
                    ChargeStatus = e["ChargeStatus"]?.ToString(),
                    VisitType = e["VisitType"]?.ToString(),
                    VisitCode = e["VisitCode"]?.ToString(),
                    VisitDate = e["VisitDate"]?.ToObject<DateTime?>(),
                    InvoicePaymentStatus = e["InvoicePaymentStatus"]?.ToString(),
                    HospitalId = e["HospitalId"].ToObject<Guid>(),
                    HospitalCode = e["HospitalCode"]?.ToString(),
                    PID = e["PID"]?.ToString(),
                    CustomerId = e["CustomerId"]?.ToObject<Guid?>(),
                    CustomerName = e["CustomerName"]?.ToString(),
                    UnitPrice = e["UnitPrice"]?.ToObject<double?>(),
                    Quantity = (int?)e["Quantity"]?.ToObject<double?>(),
                    PricingClass= e["PricingClass"]?.ToString()
                    /*13-07-2022: Phubq bỏ điều kiện lọc các charge nằm trong Visit Package*/
                })/*?.Where(x => visitTypes.Contains(x.VisitType))*/);
            }
            if (!string.IsNullOrEmpty(chargeids))
            {
                currentPage++;
                goto StepPaging;
            }
            StepReturn:
            return listReturn;
        }
        public static List<HISChargeModel> GetCharges(string pId, string visitCode, string chargeIds, bool IsIncludeChild=false, List<PatientInformationModel> Children=null)
        {
            List<HISChargeModel> oHEntities = null;
            oHEntities = GetCharges(pId, visitCode, string.Empty);
            //Get Charge what is Maternity package
            #region Get Charge what is Maternity package
            if (IsIncludeChild && Children?.Count > 0)
            {
                //Get List Charge by child 
                if (oHEntities == null) oHEntities = new List<HISChargeModel>();
                foreach (var itemChild in Children)
                {
                    oHEntities.AddRange(OHConnectionAPI.GetCharges(itemChild.PID, string.Empty, string.Empty));
                }
            }
            #endregion .Get Charge what is Maternity package
            return oHEntities;
        }
        public static List<PatientInPackageVisitModel> GetVisitByPID(string pId)
        {
            string url_postfix = string.Format(
                "/PMSVinmecCom/1.0.0/getVisitByPID?1=1"
            );
            if (!string.IsNullOrEmpty(pId))
            {
                url_postfix += string.Format("&PID={0}", pId);
            }
            //IFormatProvider viVNDateFormat = new CultureInfo("vi-VN").DateTimeFormat;
            bool bThrowEx = false;
            var response = RequestAPI(url_postfix, "Entries", "Entry", out bThrowEx, ConfigHelper.CF_ApiTimeout_minutes);
            if (response != null)
                return response.Select(e => new PatientInPackageVisitModel
                {
                    PID = e["PID"]?.ToString(),
                    VisitType = e["VisitType"]?.ToString().Trim(),
                    VisitCode = e["VisitCode"]?.ToString(),
                    VisitDate = e["VisitDate"]?.ToString(),
                    VisitClosedDate = e["VisitClosedDate"]?.ToString()
                }).ToList();
            return null;
        }
        public static bool CheckExistVisitPackageOpen(string pId)
        {
            //Kiểm tra xem khách hàng có Visit đang mở hay không
            #region Check on OH have visit open
            var curVisit = GetVisitByPID(pId);
            if (curVisit != null)
            {
                bool haveExistVisitOpen = curVisit.Any(x => !string.IsNullOrEmpty(x.VisitDate) && string.IsNullOrEmpty(x.VisitClosedDate));
                if (haveExistVisitOpen)
                {
                    return true;
                }
                else
                    return false;
            }
            return false;
            //return true;
            #endregion .Check on OH have visit open
        }
        /// <summary>
        /// Updatedate price on OH
        /// </summary>
        /// <param name="listCharges"></param>
        /// <returns></returns>
        public static bool UpdateChargePrice(List<ChargeInPackageModel> listCharges, out string outMsg)
        {
            #region For statistic performance
            var start_time = DateTime.Now;
            TimeSpan tp;
            #endregion .For statistic performance

            #region 4 Test
            //return true;
            #endregion . 4 Test
            bool returnValue = true;
            string strMsgReturn = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                var xmlContent = CreateSoapForUpdatePrice(listCharges);
                var content = new StringContent(xmlContent.InnerXml, Encoding.UTF8, "text/xml");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", ConfigHelper.OHService_Token);
                client.DefaultRequestHeaders.Add("SOAPAction", "http://www.vinmec.com/ChargeDetailService/UpdateUnitPrice");
                var response = client.PostAsync(ConfigHelper.OHService_URL, content).Result;
                if (!response.IsSuccessStatusCode)
                {
                    returnValue = false;
                    var strReturn = response.Content.ReadAsStringAsync().Result;
                    VM.Common.CustomLog.intervaljoblog.Info(string.Format("UpdateChargePrice response. Infor [{0}]. Response code: {1}. Response Data: {2}", JsonConvert.SerializeObject(listCharges), response.StatusCode, strReturn));
                }
                else
                {
                    var strReturn = response.Content.ReadAsStringAsync().Result;
                    VM.Common.CustomLog.intervaljoblog.Info(string.Format("UpdateChargePrice response. Infor [{0}]. Response code: {1}. Response Data: {2}", JsonConvert.SerializeObject(listCharges), response.StatusCode, strReturn));
                    if (!string.IsNullOrEmpty(strReturn))
                    {
                        try
                        {
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(strReturn);
                            var elBody = xmlDoc.GetElementsByTagName("soapenv:Body");
                            XElement xmlElements = XElement.Parse(elBody[0].InnerXml);
                            
                            if (xmlElements!=null && !xmlElements.IsEmpty)
                            {
                                List<ChargeInPackageModel> listChargesReturn = xmlElements.Elements("ChargeDetail").Select(x => new ChargeInPackageModel()
                                {
                                    ChargeId= x.Element("ChargeDetailId") != null ? new Guid(x.Element("ChargeDetailId").Value) : Guid.Empty,
                                    Result_Code_OH = x.Element("Result_Code").Value,
                                    Result_Message_OH = x.Element("Result_Message").Value,
                                    UpdatedDateTime_OH = x.Element("UpdatedDateTime").Value,
                                })?.ToList();
                                if (listChargesReturn?.Count > 0)
                                {
                                    foreach(var item in listChargesReturn)
                                    {
                                        listCharges.Where(x => x.ChargeId == item.ChargeId)?.ToList().ForEach(x => {
                                            x.Result_Code_OH = item.Result_Code_OH;
                                            x.Result_Message_OH = item.Result_Message_OH;
                                            x.UpdatedDateTime_OH = x.UpdatedDateTime_OH;
                                        });
                                        if(!string.IsNullOrEmpty(item.Result_Code_OH))
                                            strMsgReturn = item.Result_Code_OH;
                                    }
                                }
                                else
                                {
                                    returnValue = false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            returnValue = false;
                            VM.Common.CustomLog.errorlog.Debug(string.Format("UpdateChargePrice response error. Infor [{0}]. Ex: {1}", JsonConvert.SerializeObject(listCharges), ex));
                        }
                    }
                    else
                    {
                        returnValue = false;
                    }
                }
            }
            if (Constant.StatusUpdatePriceFAILs.Contains(strMsgReturn))
                returnValue = false;

            #region Log Performace
            tp = DateTime.Now - start_time;
            CustomLog.performancejoblog.Info(string.Format("API.UpdateChargePrice[PiPkgId={0}]: step processing spen time in {1} (ms)", listCharges?.Count>0?listCharges[0].PatientInPackageId?.ToString():string.Empty, tp.TotalMilliseconds));
            #endregion .Log Performace

            outMsg = strMsgReturn;
            return returnValue;
        }
        private static XmlDocument CreateSoapForUpdatePrice(List<ChargeInPackageModel> listCharges)
        {
            XmlDocument soapEnvelopeDocument = new XmlDocument();
            soapEnvelopeDocument.LoadXml(
            @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:char=""http://www.vinmec.com/ChargeDetailService/"">
            <soapenv:Header/>
            <soapenv:Body>
            <ChargeDetail>
            </ChargeDetail>
            </soapenv:Body>
            </soapenv:Envelope>");
            if (listCharges?.Count > 0)
            {
                XmlNodeList NodeChargeDetails = soapEnvelopeDocument.GetElementsByTagName("ChargeDetail");
                foreach (var item in listCharges.Where(x=>x.InPackageType!= (int)InPackageType.QTYINCHARGEGREATTHANREMAIN))
                {
                    if (item.IsChecked)
                    {
                        XmlNode detailNode = soapEnvelopeDocument.CreateNode(XmlNodeType.Element, "ChargeDetail", string.Empty);
                        //Create element node ChargeDetailId
                        XmlNode chargeDetailIdNode = soapEnvelopeDocument.CreateNode(XmlNodeType.Element, "ChargeDetailId", string.Empty);
                        chargeDetailIdNode.InnerText = item.ChargeId.ToString();
                        detailNode.AppendChild(chargeDetailIdNode);
                        //Create element node UnitPrice
                        XmlNode unitPriceNode = soapEnvelopeDocument.CreateNode(XmlNodeType.Element, "UnitPrice", string.Empty);
                        if (item.InPackageType == (int)InPackageType.INPACKAGE)
                        {
                            unitPriceNode.InnerText = item.PkgPrice!=null ?item.PkgPrice.ToString():"0";
                        }
                        else
                        {
                            unitPriceNode.InnerText = item.Price != null?item.Price.ToString():"0";
                        }
                        detailNode.AppendChild(unitPriceNode);
                        //Create element node UpdatedBy
                        XmlNode UpdatedByNode = soapEnvelopeDocument.CreateNode(XmlNodeType.Element, "UpdatedBy", string.Empty);
                        string currentUser = UserHelper.CurrentUserName();
                        currentUser = string.IsNullOrEmpty(currentUser) ? ConfigHelper.OHUserDefault : currentUser;
                        UpdatedByNode.InnerText = currentUser;
                        detailNode.AppendChild(UpdatedByNode);

                        //Create element node UpdatedDateTime
                        XmlNode UpdatedDateTimeNode = soapEnvelopeDocument.CreateNode(XmlNodeType.Element, "UpdatedDateTime", string.Empty);
                        UpdatedDateTimeNode.InnerText = DateTime.Now.ToString(Constant.DATETIME_SQL);
                        detailNode.AppendChild(UpdatedDateTimeNode);

                        //Create element node Result_Code
                        XmlNode Result_CodeNode = soapEnvelopeDocument.CreateNode(XmlNodeType.Element, "Result_Code", string.Empty);
                        detailNode.AppendChild(Result_CodeNode);

                        //Create element node Result_Message
                        XmlNode Result_MessageNode = soapEnvelopeDocument.CreateNode(XmlNodeType.Element, "Result_Message", string.Empty);
                        detailNode.AppendChild(Result_MessageNode);

                        NodeChargeDetails.Item(0).AppendChild(detailNode);
                    }
                }
            }
            return soapEnvelopeDocument;
        }

        #endregion .Charge
        #region Update Dims service
        public static bool UpdateDimsHisRevenue(HisChargeRevenueModel entity)
        {
            string url_postfix = string.Format(
                "/DimsVinmecCom/1.0.0/UpdateHisRevenue?chargeid={0}&inPackageType={1}"
                , entity.ChargeId
                ,entity.InPackageType
            );
            if (!string.IsNullOrEmpty(entity.PackageCode) && entity.InPackageType==(int)InPackageType.INPACKAGE)
            {
                url_postfix += string.Format("&packageCode={0}&groupPackageCode={1}", entity.PackageCode,entity.GroupPackageCode);
            }
            bool bThrowEx = false;
            var response = RequestAPI(url_postfix, "", "Data", out bThrowEx, ConfigHelper.CF_ApiTimeout_minutes);
            if (response != null)
            {
                var returnValue = response["Status"].ToString();
                return returnValue =="1" ? true:false;
            }
            return false;
        }
        #endregion .Update Dims service
    }
}
