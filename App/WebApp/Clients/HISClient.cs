using DataAccess.Models;
using DataAccess.Repository;
using VM.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using DrFee.Contract.Models.ApigwModels;
using DrFee.Business.Provider;

namespace DrFee.Clients
{
    public class HISClient
    {
        protected static IUnitOfWork unitOfWork = new EfUnitOfWork();

        #region Request
        protected static JToken RequestAPI(string url_postfix, string json_collection, string json_item)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["HIS_API_SERVER_TOKEN"]);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                string url = string.Format("{0}{1}", ConfigurationManager.AppSettings["HIS_API_SERVER_URL"], url_postfix);
                string raw_data = string.Empty;
                try
                {
                    var response = client.GetAsync(url);
                    raw_data = response.Result.Content.ReadAsStringAsync().Result;
                    if (response.Result.StatusCode != HttpStatusCode.OK)
                        HandleError(url, raw_data);
                    else
                        HandleSuccess(url);

                    JObject json_data = JObject.Parse(raw_data);
                    var log_response = json_data.ToString();
                    CustomLog.apigwlog.Info(new
                    {
                        URI = url,
                        Response = log_response,
                    });
                    try
                    {
                        JToken customer_data = json_data[json_collection][json_item];
                        return customer_data;
                    }
                    catch
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    var log_response = string.Format("{0}\n{1}", ex.ToString(), raw_data);
                    HandleError(url, log_response);
                    CustomLog.apigwlog.Info(new
                    {
                        URI = url,
                        Response = log_response,
                    });
                    return null;
                }
            }
        }
        private static void HandleSuccess(string url)
        {
            try
            {
                var scope = url.Split('?')[0];
                var noti = GetNotification(scope);
                if (noti == null || noti.Status == 2) return;

                if (noti.Status == 0)
                {
                    unitOfWork.SystemNotificationRepository.HardDelete(noti);
                    unitOfWork.Commit();
                    return;
                }

                noti.Status = 2;
                unitOfWork.SystemNotificationRepository.Update(noti);
                unitOfWork.Commit();
            }
            catch { }
        }
        private static void HandleError(string url, string error)
        {
            try { 
                var scope = url.Split('?')[0];
                var noti = GetNotification(scope);
                if (noti == null)
                {
                    noti = new SystemNotification
                    {
                        Service = Constant.SERVICE_APIGW,
                        Scope = scope,
                        Subject = url,
                        Content = error,
                    };
                    unitOfWork.SystemNotificationRepository.Add(noti);
                    unitOfWork.Commit();
                }
                else if (noti.Status == 2)
                {
                    noti.Status = 0;
                    noti.Content = error;
                    unitOfWork.SystemNotificationRepository.Update(noti);
                    unitOfWork.Commit();
                }
            }
            catch { }
        }
        private static SystemNotification GetNotification(string scope)
        {
            return unitOfWork.SystemNotificationRepository.FirstOrDefault(
                e => !string.IsNullOrEmpty(e.Service) &&
                e.Service == Constant.SERVICE_APIGW &&
                !string.IsNullOrEmpty(e.Scope) &&
                e.Scope == scope
            );
        }
        #endregion

        #region Revenue
        /// <summary>
        /// Cập nhật bản ghi doanh thu từ HIS
        /// </summary>
        /// <param name="his_code">Mã HIS</param>
        /// <param name="item">Dữ liệu cập nhật</param>
        /// <returns></returns>
        protected static HISRevenueModel HandleHISRevenue(int his_code, JToken item)
        {
            var revenue = GetOrCreateHISRevenue(
                charge_id: item["ChargeId"]?.ToString(),
                invoice_id: item["InvoiceId"]?.ToString()
            );
            UpdateHISRevenue(revenue, item);
            return CreateHISRevenueModel(his_code, item);
        }
        private static HISRevenue GetOrCreateHISRevenue(string charge_id, string invoice_id)
        {
            var revenue = unitOfWork.HISRevenueRepository.FirstOrDefault(
                e => e.ChargeId == charge_id &&
                e.InvoiceId == invoice_id
            );
            if (revenue != null)
                return revenue;
            revenue = unitOfWork.HISRevenueRepository.FirstOrDefault(
                e => e.ChargeId == charge_id &&
                e.InvoiceId == null
            );
            if (revenue != null)
                return revenue;

            revenue = new HISRevenue
            {
                ChargeId = charge_id,
                InvoiceId = invoice_id,
            };
            unitOfWork.HISRevenueRepository.Add(revenue);
            return revenue;
        }
        private static void UpdateHISRevenue(HISRevenue revenue, JToken item)
        {
            revenue.HISCode = Constant.HIS_CODE["OH"];
            revenue.HospitalId = item["HospitalId"]?.ToString();
            revenue.Service = item["ItemCode"]?.ToString();

            revenue.ParentChargeId = item["ParentChargeId"]?.ToString();
            revenue.ChargeDoctor = item["ChargeDoctorAD"]?.ToString();
            revenue.ChargeDoctorDepartmentCode = item["ChargeDoctorDepartmentCode"]?.ToString();
            revenue.ChargeDate = item["ChargeDate"]?.ToObject<DateTime?>();
            revenue.ChargeCreatedAt = item["ChargeCreatedDate"]?.ToObject<DateTime?>();
            revenue.ChargeUpdatedAt = item["ChargeUpdatedDate"].ToObject<DateTime>();
            revenue.ChargeDeletedAt = item["ChargeDeletedDate"]?.ToObject<DateTime?>();
            var charge_status = item["ChargeStatus"]?.ToObject<int?>();
            revenue.ChargeStatus = charge_status;

            revenue.OperationId = item["OperationId"]?.ToString();
            revenue.OperationDoctorDepartmentCode = item["OperationDoctorDepartmentCode"]?.ToString();
            revenue.OperationDoctor = item["OperationDoctorAD"]?.ToString();
            revenue.OperationCreatedAt = item["OperationCreatedDate"]?.ToObject<DateTime?>();
            revenue.OperationUpdatedAt = item["OperationCreatedDate"]?.ToObject<DateTime?>();
            var operation_status = item["OperationStatus"]?.ToObject<int?>();
            revenue.OperationStatus = operation_status;

            revenue.InvoiceDate = item["InvoiceDate"]?.ToObject<DateTime?>();
            revenue.InvoiceCreatedAt = item["InvoiceCreatedDate"]?.ToObject<DateTime?>();
            revenue.InvoiceUpdatedAt = item["InvoiceUpdatedDate"]?.ToObject<DateTime?>();
            revenue.InvoicePaymentStatus = item["InvoicePaymentStatus"]?.ToString();

            revenue.CustomerName = item["CustomerName"]?.ToString();
            revenue.CustomerPID = item["PID"]?.ToString();
            //if (revenue.CustomerPID == "200521679" && revenue.VisitCode== "290432")
            //{

            //}
            var package_code = item["PackageCode"]?.ToString();
            revenue.PackageCode = package_code;
            revenue.VisitType = item["VisitType"]?.ToString();
            revenue.VisitCode = item["VisitCode"]?.ToString();

            revenue.AmountInPackage = item["AmountInPackage"]?.ToObject<double?>();
            revenue.TaxAmount = item["TaxAmount"]?.ToObject<double?>();
            revenue.TaxPercentage = item["TaxPercentage"]?.ToObject<double?>();
            revenue.DiscountPercentage = item["DiscountPercentage"]?.ToObject<double?>();
            revenue.DiscountAmount = item["DiscountAmount"]?.ToObject<double?>();
            revenue.GrossAmount = item["GrossAmount"]?.ToObject<double?>();
            revenue.NetAmount = item["NetAmount"]?.ToObject<double?>();
            revenue.UnitPrice = item["UnitPrice"]?.ToObject<double?>();
            revenue.Quantity = item["Quantity"]?.ToObject<double?>();
            revenue.InvoiceNumber = item["InvoiceNumber"]?.ToString();

            revenue.IsPackage = !string.IsNullOrEmpty(package_code) || revenue.AmountInPackage>0;

            unitOfWork.HISRevenueRepository.Update(revenue);
            unitOfWork.Commit();
        }
        protected static void UpdateHISRevenueOperationInfo(string pid,string visitcode, string servicecode, string chargeId,DateTime? OperationCreate, DateTime? OperationUpdate)
        {
            var hisRevenue = unitOfWork.HISRevenueRepository.Find(x=>x.CustomerPID==pid&& x.VisitCode==visitcode && x.Service==servicecode && x.ChargeId== chargeId);
            if (hisRevenue.Any())
            {
                foreach(var item in hisRevenue)
                {
                    item.OperationCreatedAt = OperationCreate;
                    item.OperationUpdatedAt = OperationUpdate;
                    unitOfWork.HISRevenueRepository.Update(item);
                }
                unitOfWork.Commit();
            }
        }
        private static HISRevenueModel CreateHISRevenueModel(int his_code, JToken item)
        {
            DateTime charge_updated_date = item["ChargeUpdatedDate"].ToObject<DateTime>();
            var package_code = item["PackageCode"]?.ToString();
            return new HISRevenueModel
            {
                ChargeId = item["ChargeId"]?.ToString(),
                ChargeMonth = int.Parse(charge_updated_date.ToString(Constant.YEAR_MONTH_FORMAT)),
                HISCode = his_code,
                HospitalId = item["HospitalId"]?.ToString(),
                Service = item["ItemCode"]?.ToString(),
                ChargeDoctor = item["ChargeDoctorAD"]?.ToString(),
                ChargeUpdatedDate = charge_updated_date,
                ChargeDoctorDepartmentCode = item["ChargeDoctorDepartmentCode"]?.ToString(),
                OperationId = item["OperationId"]?.ToString(),
                OperationDoctorDepartmentCode = item["OperationDoctorDepartmentCode"]?.ToString(),
                OperationDoctor = item["OperationDoctorAD"]?.ToString(),
                CustomerName = item["CustomerName"]?.ToString(),
                CustomerPID = item["PID"]?.ToString(),
                PackageCode = package_code,
                AmountInPackage = item["AmountInPackage"]?.ToObject<double?>(),
                IsPackage = !string.IsNullOrEmpty(package_code),
                VisitType = item["VisitType"]?.ToString(),
                VisitCode = item["VisitCode"]?.ToString(),
                InvoiceNumber = item["InvoiceNumber"]?.ToString()
            };
        }


        protected static CalculatedRevenue GetOrCreateCalculatedRevenue(HISRevenueModel request)
        {
            var cal_revenue = unitOfWork.CalculatedRevenueRepository.FirstOrDefault(
                e => e.ChargeMonth == request.ChargeMonth &&
                e.ChargeId == request.ChargeId
            );

            Guid? specialty_id = null;
            var department_code_ext = request.GetDepartmentCodeExt();
            if (!string.IsNullOrEmpty(department_code_ext))
                specialty_id = unitOfWork.SpecialtyRepository.FirstOrDefault(e => e.Code.Contains(department_code_ext))?.Id;

            var site = unitOfWork.SiteRepository.FirstOrDefault(e => e.HospitalId == request.HospitalId);

            if (cal_revenue != null)
            {
                cal_revenue.SiteId = site?.Id;
                cal_revenue.SpecialtyId = specialty_id;
                cal_revenue.Service = request.Service;
                cal_revenue.ChargeUpdatedDate = request.ChargeUpdatedDate;
                cal_revenue.ChargeDoctorDepartmentCode = request.ChargeDoctorDepartmentCode;
                cal_revenue.ChargeDoctor = request.ChargeDoctor;
                cal_revenue.OperationId = request.OperationId;
                cal_revenue.OperationDoctorDepartmentCode = request.OperationDoctorDepartmentCode;
                cal_revenue.OperationDoctor = request.OperationDoctor;
                cal_revenue.CustomerName = request.CustomerName;
                cal_revenue.CustomerPID = request.CustomerPID;
                cal_revenue.PackageCode = request.PackageCode;
                cal_revenue.IsPackage = !string.IsNullOrEmpty(request.PackageCode) || request.AmountInPackage>0;
                cal_revenue.VisitType = request.VisitType;
                cal_revenue.VisitCode = request.VisitCode;
                cal_revenue.Note = "";
                cal_revenue.InvoiceNumber = request.InvoiceNumber;
                unitOfWork.CalculatedRevenueRepository.Update(cal_revenue);
                return cal_revenue;
            }

            cal_revenue = new CalculatedRevenue
            {
                ChargeMonth = request.ChargeMonth,
                SiteId = site?.Id,
                SpecialtyId = specialty_id,
                Service = request.Service,
                ChargeId = request.ChargeId,
                ChargeUpdatedDate = request.ChargeUpdatedDate,
                ChargeDoctorDepartmentCode = request.ChargeDoctorDepartmentCode,
                ChargeDoctor = request.ChargeDoctor,
                OperationId = request.OperationId,
                OperationDoctorDepartmentCode = request.OperationDoctorDepartmentCode,
                OperationDoctor = request.OperationDoctor,
                CustomerName = request.CustomerName,
                CustomerPID = request.CustomerPID,
                PackageCode = request.PackageCode,
                IsPackage = !string.IsNullOrEmpty(request.PackageCode) || request.AmountInPackage > 0,
                VisitType = request.VisitType,
                VisitCode = request.VisitCode,
                InvoiceNumber = request.InvoiceNumber
            };
            unitOfWork.CalculatedRevenueRepository.Add(cal_revenue);
            unitOfWork.Commit();
            return cal_revenue;
        }


        /// <summary>
        /// Tính toán doanh thu
        /// </summary>
        /// <param name="cal_revenue">Doanh thu cần tính toán</param>
        protected static void CalculateRevenue(CalculatedRevenue cal_revenue)
        {
            //if (cal_revenue.Service == "23.206")
            //{

            //}
            int month = cal_revenue.ChargeMonth % 100;
            int year = cal_revenue.ChargeMonth / 100;
            var day = DateTime.DaysInMonth(year, month);                               
            var start = DateTime.ParseExact($"01/{month.ToString("D2")}/{year} 00:00:00", Constant.DATE_TIME_FORMAT, null);
            var end = DateTime.ParseExact($"{day}/{month.ToString("D2")}/{year} 23:59:59", Constant.DATE_TIME_FORMAT, null);

            var his_revenue = GetHISRevenue(cal_revenue.ChargeId, start, end);
            dynamic net_amount = 0;
            foreach (var rev in his_revenue)
            {
                cal_revenue.Status = GetRevenueStatus(rev.ChargeStatus, rev.InvoiceId, rev.InvoicePaymentStatus);
                cal_revenue.Quantity = rev.Quantity;

                if (Constant.NEED_CALCULATE_REVENUE_STATUS.Contains(cal_revenue.Status))
                {
                    if(rev.AmountInPackage != null && 0 < rev.AmountInPackage && rev.AmountInPackage < rev.NetAmount)
                        net_amount += rev.AmountInPackage;
                    else
                        net_amount += rev.NetAmount;
                }
            }

            var last_cal_revenue = GetLastCalculatedRevenue(cal_revenue.ChargeMonth, cal_revenue.ChargeId);
            if(last_cal_revenue != null)
            {
                net_amount -= last_cal_revenue.NetAmount;
                cal_revenue.Status = Constant.CALCULATED_REVENUE_STATUS["Debt"];
                cal_revenue.Note = $"Công nợ {last_cal_revenue.ChargeUpdatedDate.ToString(Constant.MONTH_YEAR_FORMAT)}";
            }

            cal_revenue.NetAmount = net_amount;

            var site = cal_revenue.Site;
            if (site == null)
            {
                //Dịch vụ là gói hoặc có dịch vụ CON nên bỏ qua
                CustomLog.intervaljoblog.Info(string.Format("<OH revenue> Service have no site when sync [Pid:{0} | VisitCode:{1} | ServiceCode: {2}]", cal_revenue.CustomerPID, cal_revenue.VisitCode, cal_revenue.Service));
                return;
            }
            var revenue_percent = GetConfigRevenuePercent(cal_revenue, start, end, site.Level);
            //if (cal_revenue.Service == "GD.0004" && cal_revenue.VisitCode == "291009")
            //{

            //}
            if (revenue_percent != null)
            {
                if (cal_revenue.IsPackage)
                {
                    cal_revenue.ChargeAmount = net_amount * revenue_percent.ChargePackagePercent;
                    cal_revenue.OperationAmount = net_amount * revenue_percent.OperationPackagePercent;
                }
                else
                {
                    cal_revenue.ChargeAmount = net_amount * revenue_percent.ChargePercent;
                    cal_revenue.OperationAmount = net_amount * revenue_percent.OperationPercent;
                }
            }
            else
            {
                //Set mặc định theo tỷ lệ 10-90
                cal_revenue.ChargeAmount = net_amount * 0.1;
                cal_revenue.OperationAmount = net_amount * 0.9;
                cal_revenue.Note = "CONFIG_REVENUE_PERCENT_NOTFOUND";
                //Dịch vụ là gói hoặc có dịch vụ CON nên bỏ qua
                CustomLog.intervaljoblog.Info(string.Format("<OH revenue> Service have no config when sync [Pid:{0} | VisitCode:{1} | ServiceCode: {2}]", cal_revenue.CustomerPID, cal_revenue.VisitCode, cal_revenue.Service));
            }
            if (IshaveChild(cal_revenue))
            {
                // Loại bỏ ko lên bc dt với các TH có item Child (IsParrentCharge
                cal_revenue.Status = Constant.CALCULATED_REVENUE_STATUS["Removed"];
            }
            else if (IsNotCalculating(cal_revenue.Service, start, end))
            {
                cal_revenue.Status = Constant.CALCULATED_REVENUE_STATUS["NotCalculating"];
                //cal_revenue.ChargeAmount = 0;
                //cal_revenue.OperationAmount = 0;
            }
            //Ktra dịch vụ xem được phân bổ vào nhóm báo cáo DT nào
            #region Process with Category
            var category = unitOfWork.ServiceRepository.FirstOrDefault(e => e.Code == cal_revenue.Service)?.ServiceCategory;
            if (category != null)
            {
                var EntityCodeTarget = unitOfWork.ConfigRuleRepository.FirstOrDefault(e => e.DataType== "VISITTYPE" && e.DataValue== cal_revenue.VisitType && e.Code == category.Code
                && !e.IsDeleted
                && e.StartAt <= start && (e.EndAt == null || (e.EndAt >= end)));
                if (EntityCodeTarget != null)
                {
                    cal_revenue.CatCodeTarget = EntityCodeTarget.Code_Target;
                    cal_revenue.Status = EntityCodeTarget.CalculatedStatus != null ? EntityCodeTarget.CalculatedStatus.Value : cal_revenue.Status;
                }
                else
                {
                    cal_revenue.CatCodeTarget = category.Code;
                }
            }
            #endregion .Process with Category
            cal_revenue.OperationDoctor = GetOperationDoctor(cal_revenue, category, revenue_percent?.HealthCheckDoctorService);
            //if (cal_revenue.Service == "GD.0004" && cal_revenue.VisitCode == "290998")
            //{
            //    CustomLog.intervaljoblog.Info(string.Format("Get Operation doctor for [visit: {0} - service: {1}] name: {2}", cal_revenue.VisitCode, cal_revenue.Service, cal_revenue.OperationDoctor));
            //}
            #region Cập nhật thông tin & đồng bộ bác sĩ
            using (UserRepo _repo = new UserRepo())
            {
                if(!string.IsNullOrEmpty(cal_revenue.ChargeDoctor))
                    _repo.SyncDoctorIntoUser(cal_revenue.ChargeDoctor, cal_revenue.Site.Id);
                if (!string.IsNullOrEmpty(cal_revenue.OperationDoctor))
                    _repo.SyncDoctorIntoUser(cal_revenue.OperationDoctor, cal_revenue.Site.Id);
            }
                
            #endregion .Cập nhật thông tin & đồng bộ bác sĩ

            unitOfWork.CalculatedRevenueRepository.Update(cal_revenue);
            unitOfWork.Commit();
        }
        /// <summary>
        /// Lấy danh sách các bản ghi doanh thu từ HIS
        /// </summary>
        /// <param name="charge_id">ID chỉ định</param>
        /// <param name="start">Ngày bắt đầu</param>
        /// <param name="end">Ngày kết thúc</param>
        /// <returns>Danh sách doanh thu <see cref="HISRevenue"/> thỏa mãn.</returns>
        private static List<HISRevenue> GetHISRevenue(string charge_id, DateTime start, DateTime end)
        {
            return unitOfWork.HISRevenueRepository.Find(
                e => e.ChargeId == charge_id &&
                e.ChargeUpdatedAt >= start &&
                e.ChargeUpdatedAt <= end
            ).ToList();
        }
        protected static List<HISRevenue> GetHISRevenueHC_Calculate_NotYet(DateTime start, DateTime end)
        {
            var CalculatedRevenued = unitOfWork.CalculatedRevenueRepository.AsQueryable();
            return unitOfWork.HISRevenueRepository.Find(
                e => e.VisitType == Constant.VIHC_CODE &&
                (!CalculatedRevenued.Any(e1 => e1.ChargeId == e.ChargeId && e1.CustomerPID == e.CustomerPID && e1.VisitCode == e1.VisitCode && e1.Service == e.Service)
                ||
                CalculatedRevenued.Any(e1 => e1.ChargeId == e.ChargeId && e1.CustomerPID == e.CustomerPID && e1.VisitCode == e1.VisitCode && e1.Service == e.Service && e1.OperationMonth <= 0)
                ||
                CalculatedRevenued.Any(e1 => e1.ChargeId == e.ChargeId && e1.CustomerPID == e.CustomerPID && e1.VisitCode == e1.VisitCode && e1.Service == e.Service && e1.Status == -2 /*Constant.CALCULATED_REVENUE_STATUS["NotAvailable"]*/))
                //&&
                //e.CustomerPID == "200521679" && e.VisitCode == "290432"
                && e.ChargeUpdatedAt >= start &&
                e.ChargeUpdatedAt <= end
            ).ToList();
        }
        /// <summary>
        /// Xác định trạng thái của doanh thu
        /// </summary>
        /// <param name="charge_status">Trạng thái chỉ định</param>
        /// <param name="invoice_id">ID bảng kê</param>
        /// <param name="invoice_status">Trạng thái bản kê</param>
        /// <returns>Trạng thái của doanh thu</returns>
        private static int GetRevenueStatus(int? charge_status, string invoice_id, string invoice_status)
        {
            if (charge_status == 0)
                return Constant.CALCULATED_REVENUE_STATUS["CancelCharge"];
            if (string.IsNullOrEmpty(invoice_id) || string.IsNullOrEmpty(invoice_status))
                return Constant.CALCULATED_REVENUE_STATUS["VirtualRevenue"];
            if (invoice_status == "VOI")
                return Constant.CALCULATED_REVENUE_STATUS["CancelInvoice"];
            return Constant.CALCULATED_REVENUE_STATUS["Revenue"];
        }
        /// <summary>
        /// Lấy doanh thu tháng gần nhất của chỉ định để xác định công nợ.
        /// </summary>
        /// <param name="charge_month">Tháng</param>
        /// <param name="charge_id">Id chỉ định</param>
        /// <returns>Doanh thu tháng gần nhất mà đã thanh toán cho bác sĩ.</returns>
        private static CalculatedRevenue GetLastCalculatedRevenue(int charge_month, string charge_id)
        {
            return unitOfWork.CalculatedRevenueRepository.Find(
                e => e.ChargeMonth < charge_month &&
                e.ChargeId == charge_id &&
                Constant.NEED_CALCULATE_REVENUE_STATUS.Contains(e.Status)
            ).OrderByDescending(e => e.ChargeMonth).FirstOrDefault();
        }
        /// <summary>
        /// Xác định tỷ lệ phân bổ doanh thu
        /// </summary>
        /// <param name="cal_revenue">Doanh thu</param>
        /// <param name="start">Ngày bắt đầu</param>
        /// <param name="end">Ngày kết thúc</param>
        /// <param name="level">Cấp bệnh viện</param>
        /// <returns>Tỷ lệ phân bổ doanh thu</returns>
        private static HISConfigRevenuePercentModel GetConfigRevenuePercent(CalculatedRevenue cal_revenue, DateTime start, DateTime end, int level)
        {
            var is_health_check = !string.IsNullOrEmpty(cal_revenue.VisitType) && cal_revenue.VisitType == "VMHC";

            var config_revenue_percent_details = unitOfWork.ConfigRevenuePercentDetailRepository.AsQueryable()
                //.Where(e => !e.IsDeleted && e.ServiceCode == "22.1");
                .Where(e => !e.IsDeleted && e.ServiceCode == cal_revenue.Service);
            var config_revenue_percents = unitOfWork.ConfigRevenuePercentRepository.AsQueryable().Where(
                e => !e.IsDeleted && e.IsHealthCheck == is_health_check &&
               
                (e.Level == level || e.Level == 0) &&
                e.StartAt <= start && (e.EndAt == null || (e.EndAt >= end))
            );

            var revenue_percent = (from de_sql in config_revenue_percent_details
                                   join co_sql in config_revenue_percents on de_sql.ConfigRevenuePercentId equals co_sql.Id
                                   select new HISConfigRevenuePercentModel
                                   {
                                       ChargePercent = co_sql.ChargePercent,
                                       OperationPercent = co_sql.OperationPercent,
                                       ChargePackagePercent = co_sql.ChargePackagePercent,
                                       OperationPackagePercent = co_sql.OperationPackagePercent,
                                       HealthCheckDoctorService = co_sql.HealthCheckDoctorService,
                                   }).FirstOrDefault();

            return revenue_percent;
        }
        /// <summary>
        /// Xác định dịch vụ có tính thưởng cho bác sĩ hay không
        /// </summary>
        /// <param name="service_code">Mã dịch vụ</param>
        /// <param name="start">Ngày bắt đầu</param>
        /// <param name="end">Ngày kết thúc</param>
        /// <returns>True nếu không tính thưởng, False nếu tính </returns>
        private static bool IsNotCalculating(string service_code, DateTime start, DateTime end)
        {
            var rule = unitOfWork.ServiceRuleRepository.Include("Service").FirstOrDefault(
                e => !e.IsDeleted &&
                e.Service.Code == service_code &&
                //e.StartAt >= start && (e.EndAt == null || (e.EndAt <= end))
                 e.StartAt <= start && (e.EndAt == null || (e.EndAt >= end))
            );

            return rule != null;
        }
        private static bool IshaveChild(CalculatedRevenue cal_revenue)
        {
            var hisRevenue=  unitOfWork.HISRevenueRepository.Find(
                e => e.ParentChargeId== cal_revenue.ChargeId
            );
            return hisRevenue.Any();
        }
        /// <summary>
        /// Xác định bác sĩ thực hiện
        /// </summary>
        /// <param name="cal_revenue">Doanh thu</param>
        /// <param name="health_check_doctor_service">Dịch vụ của bác sĩ thực hiện</param>
        /// <returns>username bác sĩ thực hiện</returns>
        private static string GetOperationDoctor(CalculatedRevenue cal_revenue, ServiceCategory category, string health_check_doctor_service)
        {
            //if (cal_revenue.Service == "GD.0004" && cal_revenue.VisitCode == "291010")
            //{

            //}
            if (cal_revenue.VisitType == Constant.VIHC_CODE)
            {
                var service_code = cal_revenue.Service;
                if (!string.IsNullOrEmpty(health_check_doctor_service))
                    service_code = health_check_doctor_service;
                //bool IsGPCompleted = false;
                var vihc_operation_doctors = OHClient.GetViHCOperationDoctor(cal_revenue.VisitCode);
                var entityOper= vihc_operation_doctors.FirstOrDefault(e => service_code.Contains(e.ServiceCode));
                if (entityOper != null)
                {
                    //Cập nhật thời gian thực hiện dịch vụ vào bảng HISRevenues
                    UpdateHISRevenueOperationInfo(cal_revenue.CustomerPID, cal_revenue.VisitCode, cal_revenue.Service, cal_revenue.ChargeId, entityOper.CreatedDate, entityOper.UpdatedDate);
                    if (entityOper.UpdatedDate!=null)
                        cal_revenue.OperationMonth = Convert.ToInt32(entityOper.UpdatedDate.Value.ToString("yyyyMM"));
                    return entityOper.DoctorAD;
                }
                else
                {
                    var entitySpecGP = unitOfWork.SpecialtyRepository.Find(x => x.SpecialtyCode== Constant.SPEC_NOIDAKHOA).First();
                    //Là dịch vụ GP
                    if (entitySpecGP!=null)
                    {
                        var listServiceGP = entitySpecGP.ServiceCode.Split(',').ToList();
                        //Ktra xem có dịch vụ GP ko?
                        var isHaveGPService = unitOfWork.HISRevenueRepository.Find(x=>x.CustomerPID==cal_revenue.CustomerPID && x.VisitCode==cal_revenue.VisitCode && listServiceGP.Contains(x.Service));
                        if (isHaveGPService.Any())
                        {
                            //Có dịch vu GP
                            var ServiceIsGP = listServiceGP.Contains(cal_revenue.Service);
                            if (ServiceIsGP)
                            {
                                //Dịch vụ hiện tại là GP
                                //Chưa thực hiện dịch vụ, chưa tính doanh thu
                                cal_revenue.Status = Constant.CALCULATED_REVENUE_STATUS["NotAvailable"];
                            }
                            else
                            {
                                //Ktra xem GP đã thực hiện trong DB hay chưa?
                                var IsGPCompleted = unitOfWork.CalculatedRevenueRepository.Find(x => x.CustomerPID == cal_revenue.CustomerPID && x.VisitCode == cal_revenue.VisitCode && listServiceGP.Contains(x.Service) && x.Status != -2);
                                if (!IsGPCompleted.Any())
                                {
                                    //Chưa thực hiện dịch vụ, chưa tính doanh thu
                                    cal_revenue.Status = Constant.CALCULATED_REVENUE_STATUS["NotAvailable"];
                                }
                                else
                                {
                                    //GP đã hoàn thành
                                    //Set tháng thực hiện = tháng thực hiện của GP
                                    cal_revenue.OperationMonth = IsGPCompleted.Select(x=>x.OperationMonth).First();
                                }
                            }
                            
                        }
                    }
                    return string.Empty;
                }
                //return vihc_operation_doctors.FirstOrDefault(e => service_code.Contains(e.ServiceCode))?.DoctorAD;
            }

            //var category = unitOfWork.ServiceRepository.FirstOrDefault(e => e.Code == cal_revenue.Service)?.ServiceCategory;
            if (category != null && category.Code == Constant.XRAY_CODE)
                return OHClient.GetXRayOperationDoctor(cal_revenue.VisitCode, cal_revenue.Service);

            //category = unitOfWork.ServiceRepository.FirstOrDefault(e => e.Code == cal_revenue.Service)?.ServiceCategory;
            if (category != null && category.Code == Constant.PTTT_CODE)
                return OHClient.GetORData(cal_revenue.CustomerPID, cal_revenue.Service, cal_revenue.ChargeId, cal_revenue.ChargeUpdatedDate);

            return !string.IsNullOrEmpty(cal_revenue.OperationDoctor) ? cal_revenue.OperationDoctor : cal_revenue.ChargeDoctor;
        }
        #endregion

        #region ViHC
        public static List<ViHCDataModel> GetViHCOperationDoctor(string visit_code)
        {
            //string url_postfix = $"/viHC/v1.0/getDataForDoctorFee?visit_code={visit_code}";
            string url_postfix = $"/DimsVinmecCom/1.0.0/getDataFromViHC?visit_code={visit_code}";
            var response = RequestAPI(url_postfix, "Services", "Service");
            if (response != null)
                return response.Select(e => new ViHCDataModel
                {
                    VisitCode = e["VisitCode"]?.ToString(),
                    ServiceCode = e["ServiceCode"]?.ToString(),
                    DoctorAD = e["DoctorAD"]?.ToString(),
                    CreatedDate= e["CreatedDate"] !=null? Convert.ToDateTime(e["CreatedDate"]?.ToString()):(DateTime?)null,
                    UpdatedDate = e["UpdatedDate"] != null ? Convert.ToDateTime(e["UpdatedDate"]?.ToString()) : (DateTime?)null
                }).ToList();
            return new List<ViHCDataModel>();
        }
        #endregion

        #region OR
        public static string GetORData(string pid, string service_code, string charge_id, DateTime charge_date)
        {
            //string url_postfix = $"/or/1.0.0/getThongTinEkipDF?PID={pid}&ItemCode={service_code}";
            string url_postfix = $"/DimsVinmecCom/1.0.0/getThongTinEkipMo?PID={pid}&ItemCode={service_code}";
            var response = RequestAPI(url_postfix, "Services", "Service");
            if (response != null)
                return BuildORData(response, charge_id, charge_date);
            return null;
        }
        private static string BuildORData(JToken data, string charge_id, DateTime charge_date)
        {
            DateTime max_date = charge_date.AddDays(Constant.OR_DATE_RANGE);
            DateTime min_date = charge_date.AddDays(-Constant.OR_DATE_RANGE);
            DateTime? tem_date = null;
            dynamic or_request_data = null;
            foreach (var item in data)
            {
                DateTime or_date = item["ThoiGianThucHien"].ToObject<DateTime>();
                if ((min_date < or_date && or_date < max_date) && (tem_date == null || or_date >= tem_date))
                {
                    or_request_data = item;
                    tem_date = or_date;
                }
            }
            if (or_request_data == null)
                return null;
            var or_data = UpdateOrCreateORData(charge_id, or_request_data);
            return or_data.UserName_PTVChinh;
        }
        private static ORData UpdateOrCreateORData(string charge_id, JToken data)
        {
            ORData current_data = unitOfWork.ORDataRepository.FirstOrDefault(
                e => e.ChargeId == charge_id);

            if (current_data == null)
            {
                current_data = new ORData { ChargeId = charge_id };
                unitOfWork.ORDataRepository.Add(current_data);
            }
            current_data.PID = data["PID"].ToString();
            current_data.ServiceCode = data["ItemCode"].ToString();
            current_data.ORChargeId = data["ChargeId"].ToString();
            current_data.UserName_PTVChinh = data["UserName_PTVChinh"].ToString();
            current_data.UserName_PTV_Phu_1 = data["UserName_PTV_Phu_1"].ToString();
            current_data.UserName_PTV_Phu_2 = data["UserName_PTV_Phu_2"].ToString();
            current_data.UserName_PTV_Phu_3 = data["UserName_PTV_Phu_3"].ToString();
            current_data.UserName_PTV_CEC = data["UserName_PTV_CEC"].ToString();
            current_data.UserName_Nurse_Tool_1 = data["UserName_Nurse_Tool_1"].ToString();
            current_data.UserName_Nurse_Tool_2 = data["UserName_Nurse_Tool_2"].ToString();
            current_data.UserName_Nurse_Runout_1 = data["UserName_Nurse_Runout_1"].ToString();
            current_data.UserName_Nurse_Runout_2 = data["UserName_Nurse_Runout_2"].ToString();
            current_data.UserName_Bs_GayMe = data["UserName_Bs_GayMe"].ToString();
            current_data.UserName_Bs_PhuMe = data["UserName_Bs_PhuMe"].ToString();
            current_data.UserName_Nurse_PhuMe_1 = data["UserName_Nurse_PhuMe_1"].ToString();
            current_data.UserName_Nurse_PhuMe_2 = data["UserName_Nurse_PhuMe_2"].ToString();
            current_data.FullName_PTVChinh = data["FullName_PTVChinh"].ToString();
            current_data.FullName_PTV_Phu_1 = data["FullName_PTV_Phu_1"].ToString();
            current_data.FullName_PTV_Phu_2 = data["FullName_PTV_Phu_2"].ToString();
            current_data.FullName_PTV_Phu_3 = data["FullName_PTV_Phu_3"].ToString();
            current_data.FullName_PTV_CEC = data["FullName_PTV_CEC"].ToString();
            current_data.FullName_Nurse_Tool_1 = data["FullName_Nurse_Tool_1"].ToString();
            current_data.FullName_Nurse_Tool_2 = data["FullName_Nurse_Tool_2"].ToString();
            current_data.FullName_Nurse_Runout_1 = data["FullName_Nurse_Runout_1"].ToString();
            current_data.FullName_Nurse_Runout_2 = data["FullName_Nurse_Runout_2"].ToString();
            current_data.FullName_Bs_GayMe = data["FullName_Bs_GayMe"].ToString();
            current_data.FullName_Bs_PhuMe = data["FullName_Bs_PhuMe"].ToString();
            current_data.FullName_Nurse_PhuMe_1 = data["FullName_Nurse_PhuMe_1"].ToString();
            current_data.FullName_Nurse_PhuMe_2 = data["FullName_Nurse_PhuMe_2"].ToString();
            current_data.ThoiGianThucHien = data["ThoiGianThucHien"].ToObject<DateTime>();
            unitOfWork.ORDataRepository.Update(current_data);
            unitOfWork.Commit();
            return current_data;
        }

        #endregion
    }
}